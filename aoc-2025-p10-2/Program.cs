using System.Collections;
using Microsoft.EntityFrameworkCore;

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

        static async Task Main(string[] args)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            if (args.Length == 0)
            {
                Console.WriteLine("Usage: aoc-2025-p10-2 <estimator|worker|solver> <filepath> [database-connection-string]");
                return;
            }

            string mode = args[0].ToLower();
            string filePath = args.Length > 1 ? args[1] : "";
            string connectionString = args.Length > 2 ? args[2] : GetDefaultConnectionString();

            try
            {
                if (mode == "estimator")
                {
                    await RunEstimator(filePath, connectionString);
                }
                else if (mode == "worker")
                {
                    await RunWorker(connectionString);
                }
                else if (mode == "solver")
                {
                    await RunSolver(filePath);
                }
                else
                {
                    Console.WriteLine($"Unknown mode: {mode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            stopwatch.Stop();
            Console.WriteLine($"\n\nTotal execution time: {stopwatch.Elapsed.TotalSeconds:F2} seconds");
        }

        static async Task RunEstimator(string filePath, string connectionString)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                Console.WriteLine($"Error: File '{filePath}' not found.");
                return;
            }

            Console.WriteLine($"=== ESTIMATOR MODE ===");
            Console.WriteLine($"Loading machines from '{filePath}'...\n");

            string[] lines = File.ReadAllLines(filePath);
            var machines = new List<(int machineNumber, int[] target, int[][] buttons)>();

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line))
                    continue;

                var machine = ParseLine(line);
                if (machine != null)
                {
                    machines.Add((i, machine.Target, machine.Buttons));
                }
            }

            Console.WriteLine($"Successfully parsed {machines.Count} machines.\n");

            // Estimate durations and show breakdown
            Console.WriteLine("Estimating job durations:");
            long totalEstimatedSeconds = 0;
            foreach (var (machineNum, target, buttons) in machines)
            {
                long estimatedSeconds = JobEstimator.EstimateDurationSeconds(target, buttons);
                totalEstimatedSeconds += estimatedSeconds;
                Console.WriteLine($"  Machine {machineNum}: ~{estimatedSeconds} seconds ({estimatedSeconds / 60.0:F1} minutes)");
            }

            Console.WriteLine($"\nTotal estimated time: {totalEstimatedSeconds} seconds ({totalEstimatedSeconds / 3600.0:F1} hours)\n");

            // Create jobs
            Console.WriteLine("Creating jobs in database...");
            var jobs = JobEstimator.CreateJobsFromMachines(machines);

            // Save to database
            var options = new DbContextOptionsBuilder<JobDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            using (var context = new JobDbContext(options))
            {
                await context.Database.MigrateAsync();
                context.MachineJobs.AddRange(jobs);
                await context.SaveChangesAsync();
            }

            Console.WriteLine($"Successfully created {jobs.Count} jobs in database.");
            Console.WriteLine($"\nJobs ready for workers to process.");
        }

        static async Task RunWorker(string connectionString)
        {
            Console.WriteLine($"=== WORKER MODE ===");
            Console.WriteLine($"Worker starting, polling for jobs...\n");

            var options = new DbContextOptionsBuilder<JobDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            string workerId = Environment.MachineName;
            int jobsProcessed = 0;

            while (true)
            {
                // Create a fresh context for each job
                using (var context = new JobDbContext(options))
                {
                    var manager = new JobManager(context, workerId);
                    
                    var job = await manager.GetNextJobAsync();
                    if (job == null)
                    {
                        Console.WriteLine("No more jobs available. Worker exiting.");
                        break;
                    }

                    Console.WriteLine($"Processing Job {job.Id} (Machine {job.MachineNumber})...");
                    var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                    try
                    {
                        var target = System.Text.Json.JsonSerializer.Deserialize<int[]>(job.TargetData);
                        var buttons = System.Text.Json.JsonSerializer.Deserialize<int[][]>(job.ButtonsData);

                        var initialState = new MachineState { State = new int[target.Length] };
                        int stepsToTarget = FindPathToTarget(new Machine { Target = target, Buttons = buttons }, initialState);

                        stopwatch.Stop();

                        if (stepsToTarget >= 0)
                        {
                            await manager.ReportJobResultAsync(job.Id, stepsToTarget, true, stopwatch.ElapsedMilliseconds);
                            Console.WriteLine($"Job {job.Id} completed: {stepsToTarget} button presses ({stopwatch.ElapsedMilliseconds}ms)\n");
                        }
                        else
                        {
                            await manager.ReportJobResultAsync(job.Id, -1, false, stopwatch.ElapsedMilliseconds);
                            Console.WriteLine($"Job {job.Id} completed: No solution found ({stopwatch.ElapsedMilliseconds}ms)\n");
                        }

                        jobsProcessed++;
                    }
                    catch (Exception ex)
                    {
                        stopwatch.Stop();
                        await manager.ReportJobFailedAsync(job.Id, ex.Message);
                        Console.WriteLine($"Job {job.Id} failed: {ex.Message}\n");
                    }
                }
            }

            Console.WriteLine($"Worker processed {jobsProcessed} jobs.");
        }

        static async Task RunSolver(string filePath)
        {
            // Original solver logic
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                Console.WriteLine($"Error: File '{filePath}' not found.");
                return;
            }

            Console.WriteLine($"=== SOLVER MODE ===");
            Console.WriteLine($"Parsing {filePath}...\n");

            string[] lines = File.ReadAllLines(filePath);
            var machines = new List<Machine>();

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

            Console.WriteLine("Solving...");
            long totalSteps = 0;
            object lockObj = new object();
            int completedMachines = 0;

            Parallel.ForEach(Enumerable.Range(0, machines.Count), 
                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, i =>
                {
                    var machine = machines[i];
                    var initialState = new MachineState { State = new int[machine.Target.Length] };
                    int stepsToTarget = FindPathToTarget(machine, initialState);

                    if (stepsToTarget >= 0)
                    {
                        lock (lockObj)
                        {
                            completedMachines++;
                            totalSteps += stepsToTarget;
                            double percentage = (completedMachines * 100.0) / machines.Count;
                            Console.WriteLine($"Machine {i}: {stepsToTarget} button presses ({percentage:F1}% complete)");
                        }
                    }
                    else
                    {
                        lock (lockObj)
                        {
                            completedMachines++;
                            double percentage = (completedMachines * 100.0) / machines.Count;
                            Console.WriteLine($"Machine {i}: unreachable ({percentage:F1}% complete)");
                        }
                    }
                });

            Console.WriteLine($"\nTotal steps for all machines: {totalSteps}");
        }

        static string GetDefaultConnectionString()
        {
            return "Host=localhost;Database=aoc_2025;Username=postgres;Password=postgres";
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
