using WelfareLink.AnalyticsReport.API.Models;

namespace WelfareLink.AnalyticsReport.API.Interfaces;

public interface IUserService
{
    Task LogUserCreationAsync(User user);
    Task LogUserUpdateAsync(User oldUser, User newUser);
}
