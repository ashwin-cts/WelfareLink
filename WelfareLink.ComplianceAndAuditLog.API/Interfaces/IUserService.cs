using WelfareLink.ComplianceAndAuditLog.API.Models;

namespace WelfareLink.ComplianceAndAuditLog.API.Interfaces;

public interface IUserService
{
    Task LogUserCreationAsync(User user);
    Task LogUserUpdateAsync(User oldUser, User newUser);
}
