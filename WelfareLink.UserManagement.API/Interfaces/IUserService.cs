using WelfareLink.UserManagement.API.Models;

namespace WelfareLink.UserManagement.API.Interfaces;

public interface IUserService
{
    Task LogUserCreationAsync(User user);
    Task LogUserUpdateAsync(User oldUser, User newUser);
}
