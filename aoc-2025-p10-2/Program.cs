using System.Collections;

namespace aoc_2025_p10_2
{
    internal class Program
    {
        class Machine
        {
            public int[] Target { get; set; }
            public int[][] Buttons { get; set; }

            public override string ToString()
            {
                return $"Target: [{string.Join(", ", Target)}], Buttons: {Buttons.Length} buttons";
            }
        }

        class MachineState
        {
            public int[] State { get; set; }
            public List<(int buttonIndex, MachineState nextState)> Transitions { get; set; } = new();

            public override string ToString()
            {
                return string.Join(",", State);
            }

            public override bool Equals(object obj)
            {
                if (obj is not MachineState other)
                    return false;
                if (State.Length != other.State.Length)
                    return false;
                for (int i = 0; i < State.Length; i++)
                {
                    if (State[i] != other.State[i])
                        return false;
                }
                return true;
            }

            public override int GetHashCode()
            {
                int hash = 0;
                for (int i = 0; i < State.Length; i++)
                {
                    hash = hash * 31 + State[i].GetHashCode();
                }
                return hash;
            }
        }

        static void Main(string[] args)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            if (args.Length == 0)
            {
                Console.WriteLine("Usage: aoc-2025-p10-2 <filepath>");
                return;
            }

            string filePath = args[0];

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Error: File '{filePath}' not found.");
                return;
            }

            try
            {
                string[] lines = File.ReadAllLines(filePath);
                var machines = new List<Machine>();

                Console.WriteLine($"Parsing {lines.Length} lines from '{filePath}'...\n");

                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line))
                        continue;

                    var machine = ParseLine(line);
                    if (machine != null)
                    {
                        machines.Add(machine);
                    }
                }

                Console.WriteLine($"Successfully parsed {machines.Count} machines.\n");

                Console.WriteLine("Machines:");
                for (int i = 0; i < machines.Count; i++)
                {
                    Console.WriteLine($"  {i}: {machines[i]}");
                }

                Console.WriteLine("\n\nFinding shortest paths...");
                long totalSteps = 0;
                object lockObj = new object();
                int completedMachines = 0;

                Parallel.ForEach(Enumerable.Range(0, machines.Count), new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, i =>
                {
                    var machine = machines[i];
                    var initialState = new MachineState
                    {
                        State = new int[machine.Target.Length]
                    };

                    int stepsToTarget = FindPathToTarget(machine, initialState);

                    if (stepsToTarget >= 0)
                    {
                        lock (lockObj)
                        {
                            completedMachines++;
                            totalSteps += stepsToTarget;
                            double percentage = (completedMachines * 100.0) / machines.Count;
                            Console.WriteLine($"Machine {i}: {stepsToTarget} button presses needed to reach target ({percentage:F1}% complete)");
                        }
                    }
                    else
                    {
                        lock (lockObj)
                        {
                            completedMachines++;
                            double percentage = (completedMachines * 100.0) / machines.Count;
                            Console.WriteLine($"Machine {i}: {stepsToTarget} - unreachable ({percentage:F1}% complete)");
                        }
                    }
                });

                Console.WriteLine($"\nTotal steps for all machines: {totalSteps}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            stopwatch.Stop();
            Console.WriteLine($"\n\nTotal execution time: {stopwatch.Elapsed.TotalSeconds:F2} seconds");
        }

        static Machine ParseLine(string line)
        {
            // Find and extract the target from curly braces at the end
            int lastBraceIndex = line.LastIndexOf('}');
            int firstBraceIndex = line.LastIndexOf('{');

            if (firstBraceIndex == -1 || lastBraceIndex == -1 || firstBraceIndex >= lastBraceIndex)
                return null;

            string targetStr = line.Substring(firstBraceIndex + 1, lastBraceIndex - firstBraceIndex - 1);
            string remainingLine = line.Substring(0, firstBraceIndex).Trim();

            string[] tokens = remainingLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length < 1)
                return null;

            try
            {
                // Parse Target int array from {num1, num2, ...}
                string[] targetParts = targetStr.Split(',');
                var targetValues = new List<int>();

                foreach (string part in targetParts)
                {
                    if (int.TryParse(part.Trim(), out int num))
                    {
                        targetValues.Add(num);
                    }
                    else
                    {
                        return null;
                    }
                }

                int[] target = targetValues.ToArray();

                // Parse Buttons (array of arrays of ints)
                // Remaining tokens should be button descriptions in format (num1,num2,...)
                var buttons = new List<int[]>();
                for (int i = 0; i < tokens.Length; i++)
                {
                    string buttonStr = tokens[i];

                    // Parse the button numbers in format (num1,num2,...)
                    if (buttonStr.StartsWith("(") && buttonStr.EndsWith(")"))
                    {
                        string nums = buttonStr.Substring(1, buttonStr.Length - 2);
                        string[] numParts = nums.Split(',');
                        var buttonValues = new List<int>();

                        bool allParsed = true;
                        foreach (string part in numParts)
                        {
                            if (int.TryParse(part.Trim(), out int num))
                            {
                                buttonValues.Add(num);
                            }
                            else
                            {
                                allParsed = false;
                                break;
                            }
                        }

                        if (allParsed && buttonValues.Count > 0)
                        {
                            buttons.Add(buttonValues.ToArray());
                        }
                    }
                }

                return new Machine
                {
                    Target = target,
                    Buttons = buttons.ToArray()
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing line: {ex.Message}");
                return null;
            }
        }

        static int FindPathToTarget(Machine machine, MachineState initialState)
        {
            var visited = new Dictionary<string, int>();
            var queue = new Queue<(MachineState state, int steps)>();
            queue.Enqueue((initialState, 0));
            visited[initialState.ToString()] = 0;

            while (queue.Count > 0)
            {
                var (currentState, steps) = queue.Dequeue();

                if (StateMatchesTarget(currentState, machine.Target))
                {
                    return steps;
                }

                for (int buttonIndex = 0; buttonIndex < machine.Buttons.Length; buttonIndex++)
                {
                    var nextState = new int[currentState.State.Length];
                    Array.Copy(currentState.State, nextState, currentState.State.Length);

                    int[] button = machine.Buttons[buttonIndex];

                    foreach (int index in button)
                    {
                        if (index >= 0 && index < nextState.Length)
                            nextState[index]++;
                    }

                    // Prune: skip if any state value exceeds target
                    bool exceedsTarget = false;
                    for (int i = 0; i < nextState.Length; i++)
                    {
                        if (nextState[i] > machine.Target[i])
                        {
                            exceedsTarget = true;
                            break;
                        }
                    }

                    if (exceedsTarget)
                        continue;

                    var nextMachineState = new MachineState { State = nextState };
                    string stateStr = nextMachineState.ToString();

                    if (!visited.ContainsKey(stateStr))
                    {
                        visited[stateStr] = steps + 1;
                        queue.Enqueue((nextMachineState, steps + 1));
                    }
                }
            }

            return -1;
        }

        static bool StateMatchesTarget(MachineState state, int[] target)
        {
            if (state.State.Length != target.Length)
                return false;
            for (int i = 0; i < state.State.Length; i++)
            {
                if (state.State[i] != target[i])
                    return false;
            }
            return true;
        }
    }
}
