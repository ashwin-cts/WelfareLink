// IMPORTANT: How to Use [AllowAnonymous] in Protected API Endpoints
// ===================================================================
// 
// With global JWT authorization enabled, ALL API endpoints require a valid JWT token by default.
// Use [AllowAnonymous] ONLY on endpoints that should be publicly accessible without authentication.
//
// EXAMPLE USAGE:
// ==============
//
// In your controller, use it like this:
//
//    [ApiController]
//    [Route("api/[controller]")]
//    public class YourController : ControllerBase
//    {
//        // This endpoint requires JWT token (because of global [Authorize])
//        [HttpGet("protected")]
//        public async Task<IActionResult> GetProtected()
//        {
//            return Ok("You need a valid JWT token to access this");
//        }
//
//        // This endpoint is accessible without JWT token
//        [AllowAnonymous]
//        [HttpGet("public")]
//        public async Task<IActionResult> GetPublic()
//        {
//            return Ok("Public endpoint - no JWT required");
//        }
//    }
//
// COMMON SCENARIOS:
// =================
// 1. Authentication endpoints (login, register) - Use [AllowAnonymous]
// 2. Health check endpoints - Use [AllowAnonymous]
// 3. Documentation/Swagger endpoints - Already excluded by default
// 4. All other endpoints - Protected by default with global [Authorize]
//
// MIGRATION NOTES:
// ================
// - Session-based authentication has been replaced with JWT
// - No more builder.Services.AddSession() in API projects
// - Session support remains in the main Razor Pages project (WelfareLink)
// - All API calls must now include Authorization header: "Bearer <token>"
//
// TESTING:
// ========
// When testing API endpoints with Postman or similar tools:
//
// 1. First, get a token from the Authentication API:
//    POST https://localhost:7001/api/authentication/login
//    Body: { "username": "user", "password": "pass" }
//
// 2. Copy the JWT token from the response
//
// 3. For subsequent API calls, add the Authorization header:
//    Key: Authorization
//    Value: Bearer <your_jwt_token>
