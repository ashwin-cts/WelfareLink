using Microsoft.EntityFrameworkCore;
using WelfareLink.Models;

namespace WelfareLink.Data
{
    public class WelfareLinkDbContext : DbContext
    {
        public WelfareLinkDbContext(DbContextOptions<WelfareLinkDbContext> options) : base(options) { }

        public WelfareLinkDbContext() { }
<<<<<<< HEAD

        // DbSets for all entities
        public DbSet<User> Users { get; set; }
        public DbSet<Citizen> Citizens { get; set; }
        public DbSet<CitizenDocument> CitizenDocuments { get; set; }
        public DbSet<WelfareProgram> WelfarePrograms { get; set; }
        public DbSet<WelfareApplication> WelfareApplications { get; set; }
        public DbSet<EligibilityCheck> EligibilityChecks { get; set; }
        public DbSet<Benefit> Benefits { get; set; }
        public DbSet<Disbursement> Disbursements { get; set; }
        public DbSet<Audit> Audits { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<ComplianceRecord> ComplianceRecords { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Resource> Resources { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User-Audit relationship
            modelBuilder.Entity<Audit>()
                .HasOne(a => a.Officer)
                .WithMany(u => u.ConductedAudits)
                .HasForeignKey(a => a.OfficerID)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Citizen-CitizenDocument relationship
            modelBuilder.Entity<CitizenDocument>()
                .HasOne(cd => cd.Citizen)
                .WithMany(c => c.Documents)
                .HasForeignKey(cd => cd.CitizenID)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Citizen-WelfareApplication relationship
            modelBuilder.Entity<WelfareApplication>()
                .HasOne(wa => wa.Citizen)
                .WithMany(c => c.Applications)
                .HasForeignKey(wa => wa.CitizenID)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure WelfareProgram-WelfareApplication relationship
            modelBuilder.Entity<WelfareApplication>()
                .HasOne(wa => wa.WelfareProgram)
                .WithMany(wp => wp.Applications)
                .HasForeignKey(wa => wa.ProgramID)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure WelfareApplication-EligibilityCheck relationship
            modelBuilder.Entity<EligibilityCheck>()
                .HasOne(ec => ec.WelfareApplication)
                .WithMany(wa => wa.EligibilityChecks)
                .HasForeignKey(ec => ec.ApplicationID)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure WelfareApplication-Benefit relationship
            modelBuilder.Entity<Benefit>()
                .HasOne(b => b.WelfareApplication)
                .WithMany(wa => wa.Benefits)
                .HasForeignKey(b => b.ApplicationID)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure WelfareProgram-Benefit relationship
            modelBuilder.Entity<Benefit>()
                .HasOne(b => b.WelfareProgram)
                .WithMany(wp => wp.Benefits)
                .HasForeignKey(b => b.ProgramID)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Benefit-Disbursement relationship
            modelBuilder.Entity<Disbursement>()
                .HasOne(d => d.Benefit)
                .WithMany(b => b.Disbursements)
                .HasForeignKey(d => d.BenefitID)
                .OnDelete(DeleteBehavior.Cascade);
        }
=======
         public DbSet<ComplainceRecord>ComplainceRecords{ get; set; }
         public DbSet<Audit> Audits{ get; set; }
>>>>>>> 67010b637ee5fae89ead73a246ac714beea4c426
    }
}
