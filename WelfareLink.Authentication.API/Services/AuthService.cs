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

                using (var response = await client.PostAsJsonAsync("api/user/login", loginRequest))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        using (var content = response.Content)
                        {
                            var json = await content.ReadAsStringAsync();

                            if (string.IsNullOrEmpty(json))
                            {
                                _logger.LogWarning($"Empty response from UserManagement API for user: {username}");
                                return null;
                            }

                            _logger.LogInformation($"Response received from UserManagement API: {json.Substring(0, Math.Min(100, json.Length))}...");

                            var options = new System.Text.Json.JsonSerializerOptions 
                            { 
                                PropertyNameCaseInsensitive = true,
                                WriteIndented = false
                            };

                            var userResponse = System.Text.Json.JsonSerializer.Deserialize<AuthUser>(json, options);

                            if (userResponse == null)
                            {
                                _logger.LogWarning($"Failed to deserialize user response for user: {username}");
                                return null;
                            }

                            return userResponse;
                        }
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogWarning($"Login validation failed for user: {username}. Status: {response.StatusCode}, Response: {errorContent}");
                        return null;
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"HTTP error validating user {username}: {ex.Message}. Inner exception: {ex.InnerException?.Message}");
                return null;
            }
            catch (System.Text.Json.JsonException ex)
            {
                _logger.LogError($"JSON deserialization error validating user {username}: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error validating user {username}: {ex.GetType().Name} - {ex.Message}. Stack: {ex.StackTrace}");
                return null;
            }
        }
    }
}
