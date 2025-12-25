using Microsoft.EntityFrameworkCore;

namespace aoc_2025_p10_2
{
    public class JobDbContext : DbContext
    {
        public DbSet<MachineJob> MachineJobs { get; set; }
        public DbSet<JobResult> JobResults { get; set; }

        public JobDbContext(DbContextOptions<JobDbContext> options) : base(options)
        {
        }

        // Parameterless constructor for EF Core migrations
        public JobDbContext()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") ??
                    "Host=localhost;Database=aoc_2025;Username=postgres;Password=postgres";
                optionsBuilder.UseNpgsql(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<MachineJob>()
                .HasIndex(j => j.Status);
            
            modelBuilder.Entity<MachineJob>()
                .HasIndex(j => j.EstimatedDurationSeconds);
        }
    }

    public class MachineJob
    {
        public int Id { get; set; }
        public int MachineNumber { get; set; }
        public string TargetData { get; set; } // JSON serialized target
        public string ButtonsData { get; set; } // JSON serialized buttons
        public JobStatus Status { get; set; } = JobStatus.Pending;
        public long EstimatedDurationSeconds { get; set; } // Estimated time to run
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? WorkerId { get; set; }
        
        public virtual JobResult? Result { get; set; }
    }

    public class JobResult
    {
        public int Id { get; set; }
        public int MachineJobId { get; set; }
        public long ButtonPresses { get; set; }
        public bool Found { get; set; }
        public long ActualDurationMilliseconds { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public virtual MachineJob MachineJob { get; set; }
    }

    public enum JobStatus
    {
        Pending = 0,
        Running = 1,
        Completed = 2,
        Failed = 3
    }
}
