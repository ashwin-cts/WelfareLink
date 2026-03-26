using Microsoft.EntityFrameworkCore;
using WelfareLink.Models;

namespace WelfareLink.Data
{
    public class WelfareLinkDbContext:DbContext
    {

        public WelfareLinkDbContext(DbContextOptions<WelfareLinkDbContext> options) : base(options) { }

        public WelfareLinkDbContext() { }
        public DbSet<WelfareApplication> WelfareApplications { get; set; }
        public DbSet<EligibilityCheck> EligibilityChecks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure WelfareApplication - EligibilityCheck relationship
            modelBuilder.Entity<EligibilityCheck>()
                .HasOne(e => e.WelfareApplication)
                .WithMany()
                .HasForeignKey(e => e.ApplicationID)
                .OnDelete(DeleteBehavior.Restrict); // Changed from CASCADE to RESTRICT to prevent accidental deletions
        }
    }
}
