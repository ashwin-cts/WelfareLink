using WelfareLink.Data;
using WelfareLink.Interfaces;
using WelfareLink.Models;

namespace WelfareLink.Repositories;

public class WelfareProgramRepository : Repository<WelfareProgram>, IWelfareProgramRepository
{
    public WelfareProgramRepository(WelfareLinkDbContext context) : base(context)
    {
    }
}
