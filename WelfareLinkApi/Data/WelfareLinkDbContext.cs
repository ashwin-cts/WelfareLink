using Microsoft.EntityFrameworkCore;
using WelfareLinkApi.Models;

namespace WelfareLinkApi.Data
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
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Audit> Audits { get; set; }
        public DbSet<ComplainceRecord> ComplianceRecords { get; set; }

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

            // Prevent cascade delete on Audit → User / WelfareProgram
            modelBuilder.Entity<Audit>()
                .HasOne(a => a.AuditedByUser)
                .WithMany()
                .HasForeignKey(a => a.AuditedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Audit>()
                .HasOne(a => a.WelfareProgram)
                .WithMany()
                .HasForeignKey(a => a.ProgramID)
                .OnDelete(DeleteBehavior.SetNull);

            // Prevent cascade delete on ComplianceRecord → User
            modelBuilder.Entity<ComplainceRecord>()
                .HasOne(r => r.RaisedByUser)
                .WithMany()
                .HasForeignKey(r => r.RaisedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ComplainceRecord>()
                .HasOne(r => r.ResolvedByUser)
                .WithMany()
                .HasForeignKey(r => r.ResolvedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // AuditLog → User
            modelBuilder.Entity<AuditLog>()
                .HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        }

        


    }
}
