using System.Collections;

namespace aoc_2025_p10
{
    internal class Program
    {
        class Machine
        {
            public BitArray Target { get; set; }
            public int[][] Buttons { get; set; }

            public override string ToString()
            {
                return $"Target: {string.Join("", Target.Cast<bool>().Select(b => b ? "1" : "0"))}, Buttons: {Buttons.Length} buttons";
            }
        }

        class MachineState
        {
            public BitArray State { get; set; }
            public List<(int buttonIndex, MachineState nextState)> Transitions { get; set; } = new();

            public override string ToString()
            {
                return string.Join("", State.Cast<bool>().Select(b => b ? "1" : "0"));
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
                    hash = hash * 2 + (State[i] ? 1 : 0);
                }
                return hash;
            }
        }

        static void Main(string[] args)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            if (args.Length == 0)
            {
                Console.WriteLine("Usage: aoc-2025-p10 <filepath>");
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

                    // Parse the line to extract BitArray and array of arrays
                    var machine = ParseLine(line);
                    if (machine != null)
                    {
                        machines.Add(machine);
                    }
                }

                Console.WriteLine($"Successfully parsed {machines.Count} machines.\n");

                // Display all machines
                Console.WriteLine("Machines:");
                for (int i = 0; i < machines.Count; i++)
                {
                    Console.WriteLine($"  {i}: {machines[i]}");
                }

                // Build state graphs for each machine
                Console.WriteLine("\n\nBuilding state graphs...");
                long totalSteps = 0;
                
                for (int i = 0; i < machines.Count; i++)
                {
                    var machine = machines[i];
                    var initialState = new MachineState
                    {
                        State = new BitArray(machine.Target.Length, false) // All bits start off
                    };

                    int stepsToTarget = FindPathToTarget(machine, initialState);
                    Console.WriteLine($"Machine {i}: {stepsToTarget} button presses needed to reach target");
                    
                    if (stepsToTarget >= 0)
                    {
                        totalSteps += stepsToTarget;
                    }
                }

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
            string[] tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            if (tokens.Length < 1)
                return null;

            try
            {
                // Parse Target BitArray from first token: [..##]
                string targetStr = tokens[0];
                if (!targetStr.StartsWith("[") || !targetStr.EndsWith("]"))
                    return null;

                // Extract content between brackets
                string content = targetStr.Substring(1, targetStr.Length - 2);
                BitArray target = new BitArray(content.Length);

                // . = true (1), # = false (0)
                for (int i = 0; i < content.Length; i++)
                {
                    target[i] = content[i] == '#';
                }

                // Parse Buttons (array of arrays of ints)
                // Remaining tokens should be button descriptions in format (num1,num2,...)
                var buttons = new List<int[]>();
                for (int i = 1; i < tokens.Length; i++)
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
            var visited = new Dictionary<string, int>(); // state -> steps to reach it
            var queue = new Queue<(MachineState state, int steps)>();
            queue.Enqueue((initialState, 0));
            visited[initialState.ToString()] = 0;

            while (queue.Count > 0)
            {
                var (currentState, steps) = queue.Dequeue();

                // Check if current state matches target
                if (StateMatchesTarget(currentState, machine.Target))
                {
                    return steps;
                }

                // Try pressing each button
                for (int buttonIndex = 0; buttonIndex < machine.Buttons.Length; buttonIndex++)
                {
                    var nextStateBits = new BitArray(currentState.State);
                    int[] button = machine.Buttons[buttonIndex];
                    
                    // Toggle all bits specified by the button array
                    foreach (int bitIndex in button)
                    {
                        if (bitIndex >= 0 && bitIndex < nextStateBits.Length)
                            nextStateBits[bitIndex] = !nextStateBits[bitIndex];
                    }

                    var nextState = new MachineState { State = nextStateBits };
                    string stateStr = nextState.ToString();

                    // Only add if we haven't seen this state before
                    if (!visited.ContainsKey(stateStr))
                    {
                        visited[stateStr] = steps + 1;
                        queue.Enqueue((nextState, steps + 1));
                    }
                }
            }

            // Target not reachable
            return -1;
        }

        static bool StateMatchesTarget(MachineState state, BitArray target)
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
