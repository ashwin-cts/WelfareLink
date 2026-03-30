using Microsoft.EntityFrameworkCore;
using WelfareLink.Models;

namespace WelfareLink.Data
{
    public class WelfareLinkDbContext:DbContext
    {

        public WelfareLinkDbContext(DbContextOptions<WelfareLinkDbContext> options) : base(options) { }

        public WelfareLinkDbContext() { }


        public DbSet<Benefit> Benefits { get; set; }
        public DbSet<Disbursement> Disbursements { get; set; }

        public DbSet<WelfareProgram> Programs { get; set; }
        public DbSet<Resource> Resources { get; set; }
        public DbSet<WelfareApplication> WelfareApplications { get; set; }
        public DbSet<EligibilityCheck> EligibilityChecks { get; set; }
        public DbSet<Citizen> Citizens { get; set; }

        public DbSet<CitizenDocument> CitizenDocuments { get; set; }

        public DbSet<WelfareApplicationDocument> WelfareApplicationDocuments { get; set; }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Prevent cascade delete cycles on WelfareApplicationDocument
            modelBuilder.Entity<WelfareApplicationDocument>()
                .HasOne(d => d.WelfareApplication)
                .WithMany(a => a.ApplicationDocuments)
                .HasForeignKey(d => d.ApplicationID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WelfareApplicationDocument>()
                .HasOne(d => d.CitizenDocument)
                .WithMany()
                .HasForeignKey(d => d.DocumentID)
                .OnDelete(DeleteBehavior.Restrict);
        }

        


    }
}
