using Microsoft.AspNetCore.Authorization; // ADDED FOR AUTHORIZATION
using System.Security.Claims; // ADDED FOR JWT CLAIM EXTRACTION
using Microsoft.AspNetCore.Mvc;
using WelfareLink.UserManagement.API.Interfaces;
using WelfareLink.UserManagement.API.Models;
using WelfareLink.UserManagement.API.Utilities;

namespace WelfareLink.UserManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // Base Rule: Any valid logged-in user can access general endpoints (like GetUser, UpdateProfile, ChangePassword)
    [Authorize]
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

        // Helper method to securely extract UserId from JWT token
        private int? GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? _httpContextAccessor.HttpContext?.User.FindFirst("UserId")?.Value;

            if (int.TryParse(userIdClaim, out int id))
            {
                return id;
            }
            return null;
        }

        [HttpPost]
        // OVERRIDE: Only Admins can directly create raw system users here
        [Authorize(Roles = "Admin")]
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

                // FIXED: Using JWT Claims instead of Session
                var currentUserId = GetCurrentUserId();

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
                var currentUserId = GetCurrentUserId();

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
        // OVERRIDE: Crucial! Users don't have a token when they are trying to log in.
        [AllowAnonymous]
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
                // Normalize input - trim and handle case sensitivity
                var normalizedUsername = loginRequest.Username?.Trim() ?? "";
                var normalizedPassword = loginRequest.Password?.Trim() ?? "";
                var normalizedUserType = loginRequest.UserType?.Trim() ?? "";

                var user = await _userRepository.GetByUsernameAndRoleAsync(normalizedUsername, normalizedUserType);

                if (user == null)
                {
                    // User not found
                    await _auditLogService.LogActionAsync(
                        userId: null,
                        action: "Login",
                        entityType: "User",
                        entityId: null,
                        description: $"Failed login attempt - user not found for '{normalizedUsername}' with role '{normalizedUserType}'",
                        ipAddress: ipAddress,
                        userAgent: userAgent,
                        status: "Failed"
                    );

                    return Unauthorized(new { error = "Invalid username or password" });
                }

                // Trim password from database and compare
                var storedPassword = user.Password?.Trim() ?? "";

                if (!string.Equals(storedPassword, normalizedPassword, StringComparison.Ordinal))
                {
                    // Password mismatch - log for debugging
                    await _auditLogService.LogActionAsync(
                        userId: null,
                        action: "Login",
                        entityType: "User",
                        entityId: null,
                        description: $"Failed login attempt - password mismatch for user '{normalizedUsername}' with role '{normalizedUserType}'",
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
        // Falls back to Base Rule: Any valid token can access
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var ipAddress = AuditLogHelper.GetClientIpAddress(HttpContext);
            var userAgent = AuditLogHelper.GetUserAgent(HttpContext);
            var currentUserId = GetCurrentUserId();

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
        // Falls back to Base Rule: Any valid token can access
        public async Task<ActionResult<User>> UpdateProfile(int id, [FromBody] UpdateProfileRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var ipAddress = AuditLogHelper.GetClientIpAddress(HttpContext);
            var userAgent = AuditLogHelper.GetUserAgent(HttpContext);
            var currentUserId = GetCurrentUserId();

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
        // Falls back to Base Rule: Any valid token can access
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var ipAddress = AuditLogHelper.GetClientIpAddress(HttpContext);
            var userAgent = AuditLogHelper.GetUserAgent(HttpContext);
            var currentUserId = GetCurrentUserId();

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
        // OVERRIDE: Only Admins can block users
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BlockUser(int id)
        {
            var ipAddress = AuditLogHelper.GetClientIpAddress(HttpContext);
            var userAgent = AuditLogHelper.GetUserAgent(HttpContext);
            var currentUserId = GetCurrentUserId();

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
        // OVERRIDE: Only Admins can unblock users
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UnblockUser(int id)
        {
            var ipAddress = AuditLogHelper.GetClientIpAddress(HttpContext);
            var userAgent = AuditLogHelper.GetUserAgent(HttpContext);
            var currentUserId = GetCurrentUserId();

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

        // DEBUG: Remove this endpoint after troubleshooting
        [HttpPost("debug/check-credentials")]
        [AllowAnonymous]
        public async Task<IActionResult> DebugCheckCredentials([FromBody] LoginRequest request)
        {
            var username = request.Username?.Trim() ?? "";
            var password = request.Password?.Trim() ?? "";
            var role = request.UserType?.Trim() ?? "";

            var user = await _userRepository.GetByUsernameAndRoleAsync(username, role);

            if (user == null)
            {
                return Ok(new
                {
                    message = "User not found",
                    username,
                    role,
                    userExists = false
                });
            }

            var storedPassword = user.Password?.Trim() ?? "";
            var passwordMatch = string.Equals(storedPassword, password, StringComparison.Ordinal);

            return Ok(new
            {
                message = "Debug info",
                userExists = true,
                username = user.Username,
                role = user.Role,
                isActive = user.IsActive,
                passwordMatch,
                submittedPasswordLength = password.Length,
                storedPasswordLength = storedPassword.Length,
                submittedPassword = password,
                storedPassword,
                hint = !passwordMatch ? $"Password mismatch! Submitted: '{password}' vs Stored: '{storedPassword}'" : "Password matches!"
            });
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