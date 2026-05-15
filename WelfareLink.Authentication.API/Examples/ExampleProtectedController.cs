using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WelfareLink.Authentication.API.Utilities;

namespace WelfareLink.Authentication.API.Examples
{
    /// <summary>
    /// Example controller demonstrating JWT authentication and authorization usage.
    /// This shows best practices for using JWT tokens in your API endpoints.
    /// 
    /// Note: This is an example file. In production, follow your actual controller structure.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Global authorization - all endpoints require valid JWT by default
    public class ExampleProtectedController : ControllerBase
    {
        private readonly ILogger<ExampleProtectedController> _logger;

        public ExampleProtectedController(ILogger<ExampleProtectedController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Example: Protected endpoint that requires authentication.
        /// Any valid JWT token grants access.
        /// </summary>
        [HttpGet("user-info")]
        public IActionResult GetUserInfo()
        {
            // Extract user information from JWT claims
            var userId = JwtClaimsHelper.GetUserId(User);
            var username = JwtClaimsHelper.GetUsername(User);
            var role = JwtClaimsHelper.GetRole(User);
            var email = JwtClaimsHelper.GetEmail(User);

            if (userId == null)
            {
                return Unauthorized(new { error = "User ID not found in token" });
            }

            _logger.LogInformation($"User {username} (ID: {userId}) accessed their info");

            return Ok(new
            {
                userId,
                username,
                role,
                email,
                accessedAt = DateTime.Now
            });
        }

        /// <summary>
        /// Example: Protected endpoint restricted to specific role.
        /// Only users with "Admin" role can access.
        /// </summary>
        [HttpGet("admin-panel")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAdminPanel()
        {
            var username = JwtClaimsHelper.GetUsername(User);
            _logger.LogInformation($"Admin {username} accessed admin panel");

            return Ok(new
            {
                message = "Welcome to Admin Panel",
                username
            });
        }

        /// <summary>
        /// Example: Protected endpoint allowing multiple roles.
        /// Can be accessed by both Admin and Manager.
        /// </summary>
        [HttpGet("reports")]
        [Authorize(Roles = "Admin,Manager,ComplianceOfficer")]
        public IActionResult GetReports()
        {
            var username = JwtClaimsHelper.GetUsername(User);
            var role = JwtClaimsHelper.GetRole(User);

            _logger.LogInformation($"User {username} with role {role} accessed reports");

            return Ok(new
            {
                message = "Reports data",
                username,
                role
            });
        }

        /// <summary>
        /// Example: Public endpoint accessible without authentication.
        /// Use [AllowAnonymous] to override global authorization requirement.
        /// </summary>
        [HttpGet("public-info")]
        [AllowAnonymous]
        public IActionResult GetPublicInfo()
        {
            return Ok(new
            {
                message = "This is public information",
                timestamp = DateTime.Now
            });
        }

        /// <summary>
        /// Example: Protected endpoint with custom authorization logic.
        /// Demonstrates manual claim checking for complex authorization.
        /// </summary>
        [HttpGet("resource/{resourceId}")]
        public IActionResult GetResource(int resourceId)
        {
            var userId = JwtClaimsHelper.GetUserId(User);

            if (userId == null)
            {
                return Unauthorized(new { error = "User ID required" });
            }

            // Example: Check if user owns the resource
            // In real implementation, query database
            var isOwner = CheckResourceOwnership(resourceId, userId.Value);

            if (!isOwner)
            {
                _logger.LogWarning($"Unauthorized access attempt by user {userId} to resource {resourceId}");
                return Forbid();
            }

            return Ok(new
            {
                resourceId,
                message = "Resource data",
                ownerId = userId
            });
        }

        /// <summary>
        /// Example: Protected endpoint with role-based resource access.
        /// Demonstrates combining role authorization with resource ownership.
        /// </summary>
        [HttpPost("resource")]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult CreateResource([FromBody] CreateResourceRequest request)
        {
            var userId = JwtClaimsHelper.GetUserId(User);
            var username = JwtClaimsHelper.GetUsername(User);
            var role = JwtClaimsHelper.GetRole(User);

            if (userId == null)
            {
                return BadRequest(new { error = "User identification failed" });
            }

            _logger.LogInformation(
                $"User {username} (ID: {userId}, Role: {role}) created resource: {request.Name}");

            // In real implementation, save to database
            return CreatedAtAction(nameof(GetResource), new { resourceId = 1 }, new
            {
                id = 1,
                name = request.Name,
                createdBy = userId,
                createdAt = DateTime.Now
            });
        }

        /// <summary>
        /// Example: Protected endpoint that shows all JWT claims.
        /// Useful for debugging authentication issues.
        /// </summary>
        [HttpGet("debug/all-claims")]
        [Authorize(Roles = "Admin")] // Only admins can see all claims
        public IActionResult GetAllClaims()
        {
            var allClaims = JwtClaimsHelper.GetAllClaims(User);

            return Ok(new
            {
                message = "All JWT claims",
                claims = allClaims,
                claimCount = allClaims.Count
            });
        }

        /// <summary>
        /// Example: Protected endpoint that demonstrates role checking.
        /// </summary>
        [HttpGet("my-permissions")]
        public IActionResult GetMyPermissions()
        {
            var username = JwtClaimsHelper.GetUsername(User);
            var hasAdminRole = JwtClaimsHelper.HasRole(User, "Admin");
            var hasManagerRole = JwtClaimsHelper.HasRole(User, "Manager");
            var isCitizen = JwtClaimsHelper.HasRole(User, "Citizen");

            var permissions = new List<string>();
            if (hasAdminRole) permissions.Add("Full system access");
            if (hasManagerRole) permissions.Add("Manage welfare programs");
            if (isCitizen) permissions.Add("View personal application status");

            return Ok(new
            {
                username,
                roles = new
                {
                    admin = hasAdminRole,
                    manager = hasManagerRole,
                    citizen = isCitizen
                },
                permissions
            });
        }

        #region Helper Methods

        /// <summary>
        /// Helper method to check resource ownership.
        /// In real implementation, query database.
        /// </summary>
        private bool CheckResourceOwnership(int resourceId, int userId)
        {
            // TODO: Implement actual database check
            // Example logic: SELECT WHERE resourceId = @resourceId AND ownerId = @userId
            return true;
        }

        #endregion
    }

    /// <summary>
    /// Request model for creating resources.
    /// </summary>
    public class CreateResourceRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
