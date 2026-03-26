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
    }
}
