using WelfareLink.ProgramManagement.API.Models;

namespace WelfareLink.ProgramManagement.API.Interfaces;

public interface IUserService
{
    Task LogUserCreationAsync(User user);
    Task LogUserUpdateAsync(User oldUser, User newUser);
}
