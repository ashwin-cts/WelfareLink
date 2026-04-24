using WelfareLinkApi.Models;

namespace WelfareLinkApi.Interfaces;

public interface IUserService
{
    Task LogUserCreationAsync(User user);
    Task LogUserUpdateAsync(User oldUser, User newUser);
}
