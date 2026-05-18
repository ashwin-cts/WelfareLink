using System.Security.Claims;

namespace WelfareLink.Authentication.API.Utilities
{
    /// <summary>
    /// Helper utility for extracting JWT claims from the authenticated user context.
    /// Use this in controllers to access user information from the JWT token.
    /// </summary>
    public static class JwtClaimsHelper
    {
        /// <summary>
        /// Gets the User ID from the JWT claims.
        /// </summary>
        public static int? GetUserId(ClaimsPrincipal user)
        {
            var userIdClaim = user?.FindFirst("UserId")?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        /// <summary>
        /// Gets the Username from the JWT claims.
        /// </summary>
        public static string? GetUsername(ClaimsPrincipal user)
        {
            return user?.FindFirst(ClaimTypes.Name)?.Value 
                ?? user?.FindFirst("Username")?.Value
                ?? user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        /// <summary>
        /// Gets the User Role from the JWT claims.
        /// </summary>
        public static string? GetRole(ClaimsPrincipal user)
        {
            return user?.FindFirst(ClaimTypes.Role)?.Value;
        }

        /// <summary>
        /// Gets the Full Name from the JWT claims.
        /// </summary>
        public static string? GetFullName(ClaimsPrincipal user)
        {
            return user?.FindFirst("FullName")?.Value;
        }

        /// <summary>
        /// Gets the Email from the JWT claims.
        /// </summary>
        public static string? GetEmail(ClaimsPrincipal user)
        {
            return user?.FindFirst("Email")?.Value;
        }

        /// <summary>
        /// Gets the JWT ID (Jti) from the JWT claims.
        /// Useful for token tracking and revocation.
        /// </summary>
        public static string? GetJti(ClaimsPrincipal user)
        {
            return user?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? user?.FindFirst("jti")?.Value;
        }

        /// <summary>
        /// Gets all claims from the authenticated user.
        /// Useful for debugging or logging.
        /// </summary>
        public static Dictionary<string, string> GetAllClaims(ClaimsPrincipal user)
        {
            var claims = new Dictionary<string, string>();
            foreach (var claim in user?.Claims ?? Enumerable.Empty<Claim>())
            {
                claims[claim.Type] = claim.Value;
            }
            return claims;
        }

        /// <summary>
        /// Checks if the user has a specific role.
        /// </summary>
        public static bool HasRole(ClaimsPrincipal user, string role)
        {
            return user?.IsInRole(role) ?? false;
        }

        /// <summary>
        /// Checks if the user has any of the specified roles.
        /// </summary>
        public static bool HasAnyRole(ClaimsPrincipal user, params string[] roles)
        {
            return roles.Any(role => user?.IsInRole(role) ?? false);
        }

        /// <summary>
        /// Checks if the user has all of the specified roles.
        /// </summary>
        public static bool HasAllRoles(ClaimsPrincipal user, params string[] roles)
        {
            return roles.All(role => user?.IsInRole(role) ?? false);
        }
    }
}
