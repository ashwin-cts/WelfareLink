using WelfareLink.AuditorManagement.API.Models;

namespace WelfareLink.AuditorManagement.API.Interfaces;

public interface IUserService
{
    Task LogUserCreationAsync(User user);
    Task LogUserUpdateAsync(User oldUser, User newUser);
}
