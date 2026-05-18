using WelfareLink.Authentication.API.Models;
using System.Text.Json.Serialization;

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
            HttpResponseMessage? response = null;
            try
            {
                var client = _httpClientFactory.CreateClient("UserManagement");

                var loginRequest = new
                {
                    username,
                    password,
                    userType
                };

                _logger.LogInformation($"Sending login request to UserManagement API for user: {username}");
                response = await client.PostAsJsonAsync("api/userapi/login", loginRequest);

                if (response.IsSuccessStatusCode)
                {
                    // Read response with proper buffering
                    var json = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(json))
                    {
                        _logger.LogWarning($"Empty response from UserManagement API for user: {username}");
                        return null;
                    }

                    _logger.LogInformation($"Response received from UserManagement API: {json.Substring(0, Math.Min(200, json.Length))}");

                    var options = new System.Text.Json.JsonSerializerOptions 
                    { 
                        PropertyNameCaseInsensitive = true,
                        WriteIndented = false,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                    };

                    var userResponse = System.Text.Json.JsonSerializer.Deserialize<AuthUser>(json, options);

                    if (userResponse == null)
                    {
                        _logger.LogWarning($"Failed to deserialize user response for user: {username}. Raw JSON: {json}");
                        return null;
                    }

                    _logger.LogInformation($"Successfully deserialized user: {userResponse.Username}, UserId: {userResponse.UserId}, IsActive: {userResponse.IsActive}");
                    return userResponse;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Login validation failed for user: {username}. Status: {response.StatusCode}, Response: {errorContent}");
                    return null;
                }
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError($"Request timeout validating user {username}: {ex.Message}. Stack: {ex.StackTrace}");
                return null;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"HTTP error validating user {username}: {ex.Message}. Inner exception: {ex.InnerException?.Message}. Stack: {ex.StackTrace}");
                return null;
            }
            catch (System.Text.Json.JsonException ex)
            {
                _logger.LogError($"JSON deserialization error validating user {username}: {ex.Message}. Stack: {ex.StackTrace}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error validating user {username}: {ex.GetType().Name} - {ex.Message}. Stack: {ex.StackTrace}");
                return null;
            }
            finally
            {
                // Properly dispose of response
                response?.Dispose();
            }
        }
    }
}
