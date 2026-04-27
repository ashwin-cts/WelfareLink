using WelfareLink.Operations.API.Models;

namespace WelfareLink.Operations.API.Interfaces;

public interface IUserService
{
    Task LogUserCreationAsync(User user);
    Task LogUserUpdateAsync(User oldUser, User newUser);
}
