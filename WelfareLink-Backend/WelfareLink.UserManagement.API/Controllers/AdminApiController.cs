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
    // Base Rule: ONLY the Admin role can access the Admin Controller endpoints
    [Authorize(Roles = "Admin")]
    public class AdminApiController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository;
        private readonly IAuditLogService _auditLogService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdminApiController(IUserService userService, IUserRepository userRepository, IAuditLogService auditLogService, IHttpContextAccessor httpContextAccessor)
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

        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            var ipAddress = AuditLogHelper.GetClientIpAddress(HttpContext);
            var userAgent = AuditLogHelper.GetUserAgent(HttpContext);

            // FIXED: Using JWT Claims instead of Session
            var currentUserId = GetCurrentUserId();

            try
            {
                var users = await _userRepository.GetAllAsync();
                var filteredUsers = users.Where(u => u.UserId != currentUserId).ToList();

                // Log the action
                await _auditLogService.LogActionAsync(
                    userId: currentUserId,
                    action: "Read",
                    entityType: "User",
                    entityId: null,
                    description: $"Retrieved list of all users (total: {filteredUsers.Count})",
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    status: "Success"
                );

                return Ok(filteredUsers);
            }
            catch (Exception ex)
            {
                await _auditLogService.LogActionAsync(
                    userId: currentUserId,
                    action: "Read",
                    entityType: "User",
                    entityId: null,
                    description: $"Error retrieving user list: {ex.Message}",
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    status: "Failed"
                );

                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("create-officer")]
        public async Task<ActionResult<User>> CreateOfficer([FromBody] User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var ipAddress = AuditLogHelper.GetClientIpAddress(HttpContext);
            var userAgent = AuditLogHelper.GetUserAgent(HttpContext);

            // FIXED: Using JWT Claims instead of Session
            var currentUserId = GetCurrentUserId();

            try
            {
                // Validate that the role is not Citizen
                if (user.Role == "Citizen")
                {
                    await _auditLogService.LogActionAsync(
                        userId: currentUserId,
                        action: "Create",
                        entityType: "User",
                        entityId: null,
                        description: $"Attempted to create citizen through officer creation endpoint with username '{user.Username}'",
                        ipAddress: ipAddress,
                        userAgent: userAgent,
                        status: "Failed"
                    );

                    return BadRequest(new { error = "Cannot create citizen through this endpoint" });
                }

                // Check if username already exists
                var existingUser = await _userRepository.GetByUsernameAsync(user.Username);
                if (existingUser != null)
                {
                    await _auditLogService.LogActionAsync(
                        userId: currentUserId,
                        action: "Create",
                        entityType: "User",
                        entityId: null,
                        description: $"Attempted to create officer with duplicate username '{user.Username}'",
                        ipAddress: ipAddress,
                        userAgent: userAgent,
                        status: "Failed"
                    );

                    return BadRequest(new { error = "Username already exists" });
                }

                user.IsActive = true;
                user.CreatedAt = DateTime.Now;

                // Create the user
                var createdUser = await _userRepository.AddAsync(user);

                // Log the successful creation
                await _auditLogService.LogActionAsync(
                    userId: currentUserId,
                    action: "Create",
                    entityType: "User",
                    entityId: createdUser.UserId,
                    description: $"Created officer user '{createdUser.Username}' with role '{createdUser.Role}'",
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    status: "Success"
                );

                return CreatedAtAction(nameof(CreateOfficer), new { id = createdUser.UserId }, createdUser);
            }
            catch (Exception ex)
            {
                await _auditLogService.LogActionAsync(
                    userId: currentUserId,
                    action: "Create",
                    entityType: "User",
                    entityId: null,
                    description: $"Error creating officer user '{user.Username}': {ex.Message}",
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    status: "Failed"
                );

                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("create-admin")]
        public async Task<ActionResult<User>> CreateAdmin([FromBody] User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var ipAddress = AuditLogHelper.GetClientIpAddress(HttpContext);
            var userAgent = AuditLogHelper.GetUserAgent(HttpContext);

            // FIXED: Using JWT Claims instead of Session
            var currentUserId = GetCurrentUserId();

            try
            {
                // Check if username already exists
                var existingUser = await _userRepository.GetByUsernameAsync(user.Username);
                if (existingUser != null)
                {
                    await _auditLogService.LogActionAsync(
                        userId: currentUserId,
                        action: "Create",
                        entityType: "User",
                        entityId: null,
                        description: $"Attempted to create admin with duplicate username '{user.Username}'",
                        ipAddress: ipAddress,
                        userAgent: userAgent,
                        status: "Failed"
                    );

                    return BadRequest(new { error = "Username already exists" });
                }

                user.Role = "Admin";
                user.IsActive = true;
                user.CreatedAt = DateTime.Now;

                // Create the admin user
                var createdUser = await _userRepository.AddAsync(user);

                // Log the successful creation
                await _auditLogService.LogActionAsync(
                    userId: currentUserId,
                    action: "Create",
                    entityType: "User",
                    entityId: createdUser.UserId,
                    description: $"Created admin user '{createdUser.Username}'",
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    status: "Success"
                );

                return CreatedAtAction(nameof(CreateAdmin), new { id = createdUser.UserId }, createdUser);
            }
            catch (Exception ex)
            {
                await _auditLogService.LogActionAsync(
                    userId: currentUserId,
                    action: "Create",
                    entityType: "User",
                    entityId: null,
                    description: $"Error creating admin user '{user.Username}': {ex.Message}",
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    status: "Failed"
                );

                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{userId}/block")]
        public async Task<IActionResult> BlockUser(int userId)
        {
            var ipAddress = AuditLogHelper.GetClientIpAddress(HttpContext);
            var userAgent = AuditLogHelper.GetUserAgent(HttpContext);

            // FIXED: Using JWT Claims instead of Session
            var currentUserId = GetCurrentUserId();

            try
            {
                // Prevent blocking own account
                if (userId == currentUserId)
                {
                    await _auditLogService.LogActionAsync(
                        userId: currentUserId,
                        action: "Update",
                        entityType: "User",
                        entityId: userId,
                        description: $"Attempted to block own account",
                        ipAddress: ipAddress,
                        userAgent: userAgent,
                        status: "Failed"
                    );

                    return BadRequest(new { error = "You cannot block your own account" });
                }

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    await _auditLogService.LogActionAsync(
                        userId: currentUserId,
                        action: "Update",
                        entityType: "User",
                        entityId: userId,
                        description: $"Attempted to block non-existent user with ID {userId}",
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
                    entityId: userId,
                    description: $"User '{user.Username}' with role '{user.Role}' was blocked",
                    oldValue: "IsActive: true",
                    newValue: "IsActive: false",
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
                    entityId: userId,
                    description: $"Error blocking user with ID {userId}: {ex.Message}",
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    status: "Failed"
                );

                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{userId}/unblock")]
        public async Task<IActionResult> UnblockUser(int userId)
        {
            var ipAddress = AuditLogHelper.GetClientIpAddress(HttpContext);
            var userAgent = AuditLogHelper.GetUserAgent(HttpContext);

            // FIXED: Using JWT Claims instead of Session
            var currentUserId = GetCurrentUserId();

            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    await _auditLogService.LogActionAsync(
                        userId: currentUserId,
                        action: "Update",
                        entityType: "User",
                        entityId: userId,
                        description: $"Attempted to unblock non-existent user with ID {userId}",
                        ipAddress: ipAddress,
                        userAgent: userAgent,
                        status: "Failed"
                    );

                    return NotFound(new { error = "User not found" });
                }

                if (user.IsActive)
                {
                    return BadRequest(new { error = "User is already unblocked" });
                }

                user.IsActive = true;
                await _userRepository.UpdateAsync(user);

                // Log the unblock action
                await _auditLogService.LogActionAsync(
                    userId: currentUserId,
                    action: "Update",
                    entityType: "User",
                    entityId: userId,
                    description: $"User '{user.Username}' with role '{user.Role}' was unblocked",
                    oldValue: "IsActive: false",
                    newValue: "IsActive: true",
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
                    entityId: userId,
                    description: $"Error unblocking user with ID {userId}: {ex.Message}",
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    status: "Failed"
                );

                return BadRequest(new { error = ex.Message });
            }
        }
    }
}