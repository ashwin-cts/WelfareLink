using Microsoft.EntityFrameworkCore;
using WelfareLink.Models;

namespace WelfareLink.Data
{
    public class WelfareLinkDbContext:DbContext
    {

        public WelfareLinkDbContext(DbContextOptions<WelfareLinkDbContext> options) : base(options) { }

        public WelfareLinkDbContext() { }

        // DbSets for entities
      
        public DbSet<Audit> Audits { get; set; } = null!;
        public DbSet<ComplainceRecord> ComplainceRecords { get; set; } = null!;
    }
}
