using WelfareLink.Authentication.API.Models;

namespace WelfareLink.Authentication.API.Services
{
    public interface IAuthenticationService
    {
        Task<AuthUser?> ValidateUserAsync(string username, string password, string userType);
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<AuthenticationService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AuthUser?> ValidateUserAsync(string username, string password, string userType)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("UserManagement");

                var loginRequest = new
                {
                    username,
                    password,
                    userType
                };

                var response = await client.PostAsJsonAsync("api/user/login", loginRequest);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var content = System.Text.Json.JsonSerializer.Deserialize<AuthUser>(json, 
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return content;
                }

                _logger.LogWarning($"Login validation failed for user: {username}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error validating user {username}: {ex.Message}");
                return null;
            }
        }
    }
}
