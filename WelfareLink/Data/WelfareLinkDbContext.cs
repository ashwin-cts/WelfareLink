using Microsoft.EntityFrameworkCore;
using WelfareLink.Models;

namespace WelfareLink.Data
{
    public class WelfareLinkDbContext : DbContext
    {
        public WelfareLinkDbContext(DbContextOptions<WelfareLinkDbContext> options) : base(options) { }

        public WelfareLinkDbContext() { }

        // Main entities
        public DbSet<User> Users { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Citizen> Citizens { get; set; }
        public DbSet<CitizenDocument> CitizenDocuments { get; set; }
        public DbSet<WelfareApplication> WelfareApplications { get; set; }
        public DbSet<WelfareProgram> WelfarePrograms { get; set; }
        public DbSet<Benefit> Benefits { get; set; }
        public DbSet<Disbursement> Disbursements { get; set; }
        public DbSet<EligibilityCheck> EligibilityChecks { get; set; }
        public DbSet<Resource> Resources { get; set; }
        public DbSet<ComplainceRecord> ComplainceRecords { get; set; }
        public DbSet<Report> Reports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure shadow properties for password hash if needed
            modelBuilder.Entity<User>()
                .Property<string>("PasswordHash");

            modelBuilder.Entity<User>()
                .Property<string>("Status");

            modelBuilder.Entity<User>()
                .Property<DateTime>("CreatedDate");
        }
    }
}
