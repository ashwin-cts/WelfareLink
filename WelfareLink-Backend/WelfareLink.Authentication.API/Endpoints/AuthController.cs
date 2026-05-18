using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WelfareLink.Authentication.API.Models;
using WelfareLink.Authentication.API.Services;

namespace WelfareLink.Authentication.API.Endpoints
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthenticationService authenticationService, IJwtService jwtService, ILogger<AuthController> logger)
        {
            _authenticationService = authenticationService;
            _jwtService = jwtService;
            _logger = logger;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest loginRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(loginRequest.Username) || 
                string.IsNullOrWhiteSpace(loginRequest.Password) ||
                string.IsNullOrWhiteSpace(loginRequest.UserType))
            {
                return BadRequest(new { error = "Username, password, and user type are required" });
            }

            try
            {
                _logger.LogInformation($"Login attempt for user: {loginRequest.Username} with userType: {loginRequest.UserType}");

                var user = await _authenticationService.ValidateUserAsync(
                    loginRequest.Username,
                    loginRequest.Password,
                    loginRequest.UserType
                );

                if (user == null)
                {
                    _logger.LogWarning($"User validation returned null for: {loginRequest.Username}");
                    return Unauthorized(new { error = "Invalid credentials or account is inactive" });
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning($"Login attempt for inactive user: {loginRequest.Username}");
                    return Unauthorized(new { error = "Invalid credentials or account is inactive" });
                }

                var token = _jwtService.GenerateToken(user);
                var expiryTime = _jwtService.GetTokenExpiry();

                var response = new LoginResponse
                {
                    Token = token,
                    Username = user.Username,
                    Role = user.Role,
                    FullName = user.FullName,
                    ExpiryTime = expiryTime
                };

                _logger.LogInformation($"Successful login for user: {user.Username} with role: {user.Role}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Login error for user {loginRequest.Username}: {ex.GetType().Name} - {ex.Message}. Stack: {ex.StackTrace}");
                return StatusCode(500, new { error = "An error occurred during login" });
            }
        }

        [HttpPost("validate")]
        public IActionResult ValidateToken()
        {
            return Ok(new { message = "Token is valid" });
        }
    }
}
