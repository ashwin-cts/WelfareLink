using Microsoft.AspNetCore.Mvc;
using WelfareLinkApi.Interfaces;
using WelfareLinkApi.Models;
using WelfareLinkApi.Utilities;

namespace WelfareLinkApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserApiController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository;
        private readonly IAuditLogService _auditLogService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserApiController(IUserService userService, IUserRepository userRepository, IAuditLogService auditLogService, IHttpContextAccessor httpContextAccessor)
        {
            _userService = userService;
            _userRepository = userRepository;
            _auditLogService = auditLogService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost]
        public async Task<ActionResult<User>> CreateUser([FromBody] User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var ipAddress = AuditLogHelper.GetClientIpAddress(HttpContext);
                var userAgent = AuditLogHelper.GetUserAgent(HttpContext);
                var currentUserId = _httpContextAccessor.HttpContext?.Session.GetInt32("UserId");

                // Add user to database
                var createdUser = await _userRepository.AddAsync(user);

                // Log the user creation action
                await _auditLogService.LogActionAsync(
                    userId: currentUserId,
                    action: "Create",
                    entityType: "User",
                    entityId: createdUser.UserId,
                    description: $"Created user '{createdUser.Username}' with role '{createdUser.Role}'",
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    status: "Success"
                );

                return CreatedAtAction(nameof(CreateUser), new { id = createdUser.UserId }, createdUser);
            }
            catch (Exception ex)
            {
                var ipAddress = AuditLogHelper.GetClientIpAddress(HttpContext);
                var userAgent = AuditLogHelper.GetUserAgent(HttpContext);
                var currentUserId = _httpContextAccessor.HttpContext?.Session.GetInt32("UserId");

                await _auditLogService.LogActionAsync(
                    userId: currentUserId,
                    action: "Create",
                    entityType: "User",
                    entityId: null,
                    description: $"Failed to create user '{user.Username}'",
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    status: "Failed"
                );

                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<User>> Login([FromBody] LoginRequest loginRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var ipAddress = AuditLogHelper.GetClientIpAddress(HttpContext);
            var userAgent = AuditLogHelper.GetUserAgent(HttpContext);

            try
            {
                var user = await _userRepository.GetByUsernameAndRoleAsync(loginRequest.Username, loginRequest.UserType);

                if (user == null || user.Password != loginRequest.Password)
                {
                    // Log failed login attempt
                    await _auditLogService.LogActionAsync(
                        userId: null,
                        action: "Login",
                        entityType: "User",
                        entityId: null,
                        description: $"Failed login attempt for user '{loginRequest.Username}' with role '{loginRequest.UserType}'",
                        ipAddress: ipAddress,
                        userAgent: userAgent,
                        status: "Failed"
                    );

                    return Unauthorized(new { error = "Invalid username or password" });
                }

                if (!user.IsActive)
                {
                    // Log blocked account login attempt
                    await _auditLogService.LogActionAsync(
                        userId: user.UserId,
                        action: "Login",
                        entityType: "User",
                        entityId: user.UserId,
                        description: $"Login attempt by blocked user '{user.Username}'",
                        ipAddress: ipAddress,
                        userAgent: userAgent,
                        status: "Failed"
                    );

                    return Unauthorized(new { error = "Your account is blocked. Please contact Admin." });
                }

                // Log successful login
                await _auditLogService.LogActionAsync(
                    userId: user.UserId,
                    action: "Login",
                    entityType: "User",
                    entityId: user.UserId,
                    description: $"User '{user.Username}' logged in with role '{user.Role}'",
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    status: "Success"
                );

                return Ok(user);
            }
            catch (Exception ex)
            {
                await _auditLogService.LogActionAsync(
                    userId: null,
                    action: "Login",
                    entityType: "User",
                    entityId: null,
                    description: $"Login error for user '{loginRequest.Username}': {ex.Message}",
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    status: "Failed"
                );

                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var ipAddress = AuditLogHelper.GetClientIpAddress(HttpContext);
            var userAgent = AuditLogHelper.GetUserAgent(HttpContext);
            var currentUserId = _httpContextAccessor.HttpContext?.Session.GetInt32("UserId");

            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    // Log failed read attempt
                    await _auditLogService.LogActionAsync(
                        userId: currentUserId,
                        action: "Read",
                        entityType: "User",
                        entityId: id,
                        description: $"Attempted to read non-existent user with ID {id}",
                        ipAddress: ipAddress,
                        userAgent: userAgent,
                        status: "Failed"
                    );

                    return NotFound(new { error = "User not found" });
                }

                // Log successful read
                await _auditLogService.LogActionAsync(
                    userId: currentUserId,
                    action: "Read",
                    entityType: "User",
                    entityId: id,
                    description: $"Retrieved user '{user.Username}' details",
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    status: "Success"
                );

                return Ok(user);
            }
            catch (Exception ex)
            {
                await _auditLogService.LogActionAsync(
                    userId: currentUserId,
                    action: "Read",
                    entityType: "User",
                    entityId: id,
                    description: $"Error reading user with ID {id}: {ex.Message}",
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    status: "Failed"
                );

                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}/profile")]
        public async Task<ActionResult<User>> UpdateProfile(int id, [FromBody] UpdateProfileRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var ipAddress = AuditLogHelper.GetClientIpAddress(HttpContext);
            var userAgent = AuditLogHelper.GetUserAgent(HttpContext);
            var currentUserId = _httpContextAccessor.HttpContext?.Session.GetInt32("UserId");

            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    await _auditLogService.LogActionAsync(
                        userId: currentUserId,
                        action: "Update",
                        entityType: "User",
                        entityId: id,
                        description: $"Attempted to update non-existent user with ID {id}",
                        ipAddress: ipAddress,
                        userAgent: userAgent,
                        status: "Failed"
                    );

                    return NotFound(new { error = "User not found" });
                }

                var oldUser = new User
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Password = user.Password,
                    Role = user.Role,
                    FullName = user.FullName,
                    Email = user.Email,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    CitizenId = user.CitizenId
                };

                user.FullName = request.FullName;
                user.Email = request.Email;

                await _userRepository.UpdateAsync(user);

                var changes = new List<string>();
                if (oldUser.FullName != user.FullName)
                    changes.Add($"FullName: {oldUser.FullName} → {user.FullName}");
                if (oldUser.Email != user.Email)
                    changes.Add($"Email: {oldUser.Email} → {user.Email}");

                // Log the update action
                await _auditLogService.LogActionAsync(
                    userId: currentUserId,
                    action: "Update",
                    entityType: "User",
                    entityId: id,
                    description: $"Updated profile for user '{user.Username}'",
                    oldValue: string.Join("; ", changes.Take(1)),
                    newValue: string.Join("; ", changes.Skip(1)),
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    status: "Success"
                );

                return Ok(user);
            }
            catch (Exception ex)
            {
                await _auditLogService.LogActionAsync(
                    userId: currentUserId,
                    action: "Update",
                    entityType: "User",
                    entityId: id,
                    description: $"Error updating user profile for ID {id}: {ex.Message}",
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    status: "Failed"
                );

                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}/password")]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var ipAddress = AuditLogHelper.GetClientIpAddress(HttpContext);
            var userAgent = AuditLogHelper.GetUserAgent(HttpContext);
            var currentUserId = _httpContextAccessor.HttpContext?.Session.GetInt32("UserId");

            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    await _auditLogService.LogActionAsync(
                        userId: currentUserId,
                        action: "Update",
                        entityType: "User",
                        entityId: id,
                        description: $"Attempted to change password for non-existent user with ID {id}",
                        ipAddress: ipAddress,
                        userAgent: userAgent,
                        status: "Failed"
                    );

                    return NotFound(new { error = "User not found" });
                }

                if (user.Password != request.CurrentPassword)
                {
                    await _auditLogService.LogActionAsync(
                        userId: currentUserId,
                        action: "Update",
                        entityType: "User",
                        entityId: id,
                        description: $"Failed password change attempt for user '{user.Username}' - incorrect current password",
                        ipAddress: ipAddress,
                        userAgent: userAgent,
                        status: "Failed"
                    );

                    return BadRequest(new { error = "Current password is incorrect" });
                }

                user.Password = request.NewPassword;
                await _userRepository.UpdateAsync(user);

                // Log the password change action
                await _auditLogService.LogActionAsync(
                    userId: currentUserId,
                    action: "Update",
                    entityType: "User",
                    entityId: id,
                    description: $"User '{user.Username}' changed password",
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    status: "Success"
                );

                return Ok(new { message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                await _auditLogService.LogActionAsync(
                    userId: currentUserId,
                    action: "Update",
                    entityType: "User",
                    entityId: id,
                    description: $"Error changing password for user ID {id}: {ex.Message}",
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    status: "Failed"
                );

                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}/block")]
        public async Task<IActionResult> BlockUser(int id)
        {
            var ipAddress = AuditLogHelper.GetClientIpAddress(HttpContext);
            var userAgent = AuditLogHelper.GetUserAgent(HttpContext);
            var currentUserId = _httpContextAccessor.HttpContext?.Session.GetInt32("UserId");

            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    await _auditLogService.LogActionAsync(
                        userId: currentUserId,
                        action: "Update",
                        entityType: "User",
                        entityId: id,
                        description: $"Attempted to block non-existent user with ID {id}",
                        ipAddress: ipAddress,
                        userAgent: userAgent,
                        status: "Failed"
                    );

                    return NotFound(new { error = "User not found" });
                }

                if (!user.IsActive)
                {
                    return BadRequest(new { error = "User is already blocked" });
                }

                user.IsActive = false;
                await _userRepository.UpdateAsync(user);

                // Log the block action
                await _auditLogService.LogActionAsync(
                    userId: currentUserId,
                    action: "Update",
                    entityType: "User",
                    entityId: id,
                    description: $"User '{user.Username}' with role '{user.Role}' was blocked",
                    oldValue: $"IsActive: true",
                    newValue: $"IsActive: false",
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    status: "Success"
                );

                return Ok(new { message = "User blocked successfully" });
            }
            catch (Exception ex)
            {
                await _auditLogService.LogActionAsync(
                    userId: currentUserId,
                    action: "Update",
                    entityType: "User",
                    entityId: id,
                    description: $"Error blocking user with ID {id}: {ex.Message}",
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    status: "Failed"
                );

                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}/unblock")]
        public async Task<IActionResult> UnblockUser(int id)
        {
            var ipAddress = AuditLogHelper.GetClientIpAddress(HttpContext);
            var userAgent = AuditLogHelper.GetUserAgent(HttpContext);
            var currentUserId = _httpContextAccessor.HttpContext?.Session.GetInt32("UserId");

            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    await _auditLogService.LogActionAsync(
                        userId: currentUserId,
                        action: "Update",
                        entityType: "User",
                        entityId: id,
                        description: $"Attempted to unblock non-existent user with ID {id}",
                        ipAddress: ipAddress,
                        userAgent: userAgent,
                        status: "Failed"
                    );

                    return NotFound(new { error = "User not found" });
                }

                if (user.IsActive)
                {
                    return BadRequest(new { error = "User is already active" });
                }

                user.IsActive = true;
                await _userRepository.UpdateAsync(user);

                // Log the unblock action
                await _auditLogService.LogActionAsync(
                    userId: currentUserId,
                    action: "Update",
                    entityType: "User",
                    entityId: id,
                    description: $"User '{user.Username}' with role '{user.Role}' was unblocked",
                    oldValue: $"IsActive: false",
                    newValue: $"IsActive: true",
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    status: "Success"
                );

                return Ok(new { message = "User unblocked successfully" });
            }
            catch (Exception ex)
            {
                await _auditLogService.LogActionAsync(
                    userId: currentUserId,
                    action: "Update",
                    entityType: "User",
                    entityId: id,
                    description: $"Error unblocking user with ID {id}: {ex.Message}",
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    status: "Failed"
                );

                return BadRequest(new { error = ex.Message });
            }
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class UpdateProfileRequest
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
    }

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
