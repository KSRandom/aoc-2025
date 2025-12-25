namespace aoc_2025_p10_2
{
    public class JobEstimator
    {
        /// <summary>
        /// Estimates the duration of solving a machine in seconds.
        /// Goal: each job should take approximately 3600 seconds (1 hour)
        /// </summary>
        public static long EstimateDurationSeconds(int[] target, int[][] buttons)
        {
            // Calculate the search space size
            long searchSpaceSize = CalculateSearchSpaceSize(target, buttons);
            
            // Empirical: We can process roughly 1,000,000 states per second
            // This is a rough estimate based on typical brute force search performance
            long estimatedStates = searchSpaceSize;
            long estimatedSeconds = Math.Max(1, estimatedStates / 1_000_000);
            
            // Target 3600 seconds per job
            long targetSeconds = 3600;
            
            return Math.Min(estimatedSeconds, targetSeconds);
        }

        /// <summary>
        /// Calculates the approximate search space size (total combinations to try)
        /// </summary>
        private static long CalculateSearchSpaceSize(int[] target, int[][] buttons)
        {
            if (buttons.Length == 0)
                return 1;

            // For each button, estimate max presses needed
            long totalCombinations = 1;
            
            foreach (var button in buttons)
            {
                long maxForThisButton = long.MaxValue;
                
                // Find the constraint limiting this button
                foreach (int idx in button)
                {
                    if (idx >= 0 && idx < target.Length)
                    {
                        int timesInButton = button.Count(x => x == idx);
                        long maxForThisIndex = target[idx] / timesInButton;
                        maxForThisButton = Math.Min(maxForThisButton, maxForThisIndex);
                    }
                }
                
                maxForThisButton = (maxForThisButton == long.MaxValue) ? 100 : maxForThisButton;
                totalCombinations *= (maxForThisButton + 1);
                
                // Prevent overflow
                if (totalCombinations > long.MaxValue / 1000)
                    return long.MaxValue;
            }
            
            return totalCombinations;
        }

        /// <summary>
        /// Splits a list of machines into jobs that each take approximately one hour
        /// </summary>
        public static List<MachineJob> CreateJobsFromMachines(
            List<(int machineNumber, int[] target, int[][] buttons)> machines)
        {
            var jobs = new List<MachineJob>();
            
            foreach (var (machineNumber, target, buttons) in machines)
            {
                long estimatedSeconds = EstimateDurationSeconds(target, buttons);
                
                var job = new MachineJob
                {
                    MachineNumber = machineNumber,
                    TargetData = System.Text.Json.JsonSerializer.Serialize(target),
                    ButtonsData = System.Text.Json.JsonSerializer.Serialize(buttons),
                    EstimatedDurationSeconds = estimatedSeconds,
                    Status = JobStatus.Pending
                };
                
                jobs.Add(job);
            }
            
            return jobs;
        }
    }
}
