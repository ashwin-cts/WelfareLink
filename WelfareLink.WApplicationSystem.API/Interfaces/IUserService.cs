using WelfareLink.WApplicationSystem.API.Models;

namespace WelfareLink.WApplicationSystem.API.Interfaces;

public interface IUserService
{
    Task LogUserCreationAsync(User user);
    Task LogUserUpdateAsync(User oldUser, User newUser);
}
