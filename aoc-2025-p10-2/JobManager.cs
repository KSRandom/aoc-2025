using Microsoft.EntityFrameworkCore;

namespace aoc_2025_p10_2
{
    public class JobManager
    {
        private readonly JobDbContext _context;
        private readonly string _workerId;

        public JobManager(JobDbContext context, string workerId = "default")
        {
            _context = context;
            _workerId = workerId;
        }

        /// <summary>
        /// Gets the next available job to process using database-level locking
        /// Retries multiple times in case of contention from other workers
        /// </summary>
        public async Task<MachineJob?> GetNextJobAsync()
        {
            const int maxRetries = 5;
            var random = new Random();

            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                // Use a transaction with serializable isolation to prevent race conditions
                using (var transaction = await _context.Database.BeginTransactionAsync(
                    System.Data.IsolationLevel.Serializable))
                {
                    try
                    {
                        var job = await _context.MachineJobs
                            .Where(j => j.Status == JobStatus.Pending)
                            .OrderBy(j => j.EstimatedDurationSeconds) // Process easier jobs first
                            .FirstOrDefaultAsync();

                        if (job != null)
                        {
                            job.Status = JobStatus.Running;
                            job.StartedAt = DateTime.UtcNow;
                            job.WorkerId = _workerId;
                            await _context.SaveChangesAsync();
                            await transaction.CommitAsync();
                            return job;
                        }

                        await transaction.CommitAsync();
                        
                        // No job found on this attempt
                        // On the last attempt, check if there are truly no pending jobs
                        if (attempt == maxRetries - 1)
                        {
                            var pendingCount = await _context.MachineJobs
                                .CountAsync(j => j.Status == JobStatus.Pending);
                            
                            if (pendingCount == 0)
                            {
                                // Confirmed: no pending jobs available
                                return null;
                            }
                        }
                        
                        // Jobs exist but we couldn't claim one - likely other workers grabbed them
                        // Wait and retry with random backoff
                        if (attempt < maxRetries - 1)
                        {
                            int delaySeconds = random.Next(1, 6); // Random delay between 1-5 seconds
                            await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                        }
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        
                        // Only log and rethrow on final attempt
                        if (attempt == maxRetries - 1)
                        {
                            Console.WriteLine($"Error acquiring job after {maxRetries} attempts: {ex.Message}");
                            throw;
                        }
                        
                        // On intermediate attempts, retry with random backoff
                        if (attempt < maxRetries - 1)
                        {
                            int delaySeconds = random.Next(1, 6); // Random delay between 1-5 seconds
                            await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                        }
                    }
                }
            }

            // Should not reach here, but return null as fallback
            return null;
        }

        /// <summary>
        /// Reports the result of a completed job
        /// </summary>
        public async Task ReportJobResultAsync(int jobId, long buttonPresses, bool found, long durationMilliseconds)
        {
            var job = await _context.MachineJobs.FindAsync(jobId);
            if (job == null)
                throw new InvalidOperationException($"Job {jobId} not found");

            job.Status = JobStatus.Completed;
            job.CompletedAt = DateTime.UtcNow;

            var result = new JobResult
            {
                MachineJobId = jobId,
                ButtonPresses = buttonPresses,
                Found = found,
                ActualDurationMilliseconds = durationMilliseconds
            };

            _context.JobResults.Add(result);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Reports a failed job
        /// </summary>
        public async Task ReportJobFailedAsync(int jobId, string errorMessage)
        {
            var job = await _context.MachineJobs.FindAsync(jobId);
            if (job == null)
                throw new InvalidOperationException($"Job {jobId} not found");

            job.Status = JobStatus.Failed;
            job.CompletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            Console.WriteLine($"Job {jobId} failed: {errorMessage}");
        }

        /// <summary>
        /// Gets statistics about job progress
        /// </summary>
        public async Task<JobStats> GetStatsAsync()
        {
            var pending = await _context.MachineJobs.CountAsync(j => j.Status == JobStatus.Pending);
            var running = await _context.MachineJobs.CountAsync(j => j.Status == JobStatus.Running);
            var completed = await _context.MachineJobs.CountAsync(j => j.Status == JobStatus.Completed);
            var failed = await _context.MachineJobs.CountAsync(j => j.Status == JobStatus.Failed);

            return new JobStats
            {
                Pending = pending,
                Running = running,
                Completed = completed,
                Failed = failed
            };
        }
    }

    public class JobStats
    {
        public int Pending { get; set; }
        public int Running { get; set; }
        public int Completed { get; set; }
        public int Failed { get; set; }

        public int Total => Pending + Running + Completed + Failed;
        public double PercentComplete => Total > 0 ? (Completed * 100.0) / Total : 0;
    }
}
