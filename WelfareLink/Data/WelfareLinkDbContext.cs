using Microsoft.EntityFrameworkCore;

namespace WelfareLink.Data
{
    public class WelfareLinkDbContext:DbContext
    {

        public WelfareLinkDbContext(DbContextOptions<WelfareLinkDbContext> options) : base(options) { }

        public WelfareLinkDbContext() { }
         public DbSet<ComplainceRecord>ComplainceRecords{ get; set; }
 public DbSet<Audit> Audits{ get; set; }
    }
}
