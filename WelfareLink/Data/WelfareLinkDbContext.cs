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


    }
}
