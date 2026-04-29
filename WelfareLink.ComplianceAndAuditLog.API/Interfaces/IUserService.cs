using WelfareLink.ComplianceAndAudit.API.Models;

namespace WelfareLink.ComplianceAndAudit.API.Interfaces;

public interface IUserService
{
    Task LogUserCreationAsync(User user);
    Task LogUserUpdateAsync(User oldUser, User newUser);
}
