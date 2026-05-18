using WelfareLink.CitizenManagement.API.Models;

namespace WelfareLink.CitizenManagement.API.Interfaces;

public interface IUserService
{
    Task LogUserCreationAsync(User user);
    Task LogUserUpdateAsync(User oldUser, User newUser);
}
