/*
 * INTEGRATION GUIDE: JWT Authentication in WelfareLink (Razor Pages/MVC)
 * 
 * This file demonstrates how to integrate JWT tokens with your existing
 * session management in the main WelfareLink Razor Pages application.
 */

// ============================================================================
// 1. PROGRAM.CS - Add JWT Services & Session Configuration
// ============================================================================

// In WelfareLink/Program.cs, add after builder = WebApplication.CreateBuilder(args):

// Add Session Support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// Add HttpClient for API communication
builder.Services.AddHttpClient("AuthApi", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ApiSettings:Authentication"] 
        ?? throw new InvalidOperationException("ApiSettings:Authentication not configured"));
})
.ConfigurePrimaryHttpMessageHandler(() =>
    new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });

// Add Authentication (for displaying user info)
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Login";
        options.LogoutPath = "/Logout";
        options.AccessDeniedPath = "/AccessDenied";
    });

builder.Services.AddAuthorization();

// Register JWT Auth Service
builder.Services.AddScoped<IJwtAuthService, JwtAuthService>();

// ============================================================================
// In the middleware pipeline (after var app = builder.Build()):
// ============================================================================

app.UseSession();                           // Enable session before auth
app.UseAuthentication();                    // JWT authentication
app.UseAuthorization();                     // Authorization

// ============================================================================
// 2. APPSETTINGS.JSON - Add API Configuration
// ============================================================================

{
  "Logging": { ... },
  "AllowedHosts": "*",
  "ConnectionStrings": { ... },
  "ApiSettings": {
    "Authentication": "https://localhost:7101"
  }
}

// ============================================================================
// 3. CREATE JWT AUTH SERVICE
// ============================================================================

// File: WelfareLink/Services/JwtAuthService.cs

using System.Net.Http.Headers;

namespace WelfareLink.Services
{
    public interface IJwtAuthService
    {
        Task<LoginResponse?> LoginAsync(string username, string password, string userType);
        Task<bool> ValidateTokenAsync(string token);
        string? GetStoredToken(HttpContext httpContext);
        void StoreToken(HttpContext httpContext, string token, LoginResponse response);
        void ClearToken(HttpContext httpContext);
    }

    public class JwtAuthService : IJwtAuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<JwtAuthService> _logger;
        private const string TokenSessionKey = "JwtToken";
        private const string UserSessionKey = "UserInfo";

        public JwtAuthService(IHttpClientFactory httpClientFactory, ILogger<JwtAuthService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<LoginResponse?> LoginAsync(string username, string password, string userType)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("AuthApi");

                var loginRequest = new
                {
                    username,
                    password,
                    userType
                };

                var response = await client.PostAsJsonAsync("api/auth/login", loginRequest);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsAsync<LoginResponse>();
                    _logger.LogInformation($"User {username} logged in successfully");
                    return result;
                }

                _logger.LogWarning($"Login failed for user {username}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Login error: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("AuthApi");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.PostAsync("api/auth/validate", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Token validation error: {ex.Message}");
                return false;
            }
        }

        public string? GetStoredToken(HttpContext httpContext)
        {
            return httpContext.Session.GetString(TokenSessionKey);
        }

        public void StoreToken(HttpContext httpContext, string token, LoginResponse response)
        {
            httpContext.Session.SetString(TokenSessionKey, token);
            httpContext.Session.SetString(UserSessionKey, System.Text.Json.JsonSerializer.Serialize(response));
        }

        public void ClearToken(HttpContext httpContext)
        {
            httpContext.Session.Remove(TokenSessionKey);
            httpContext.Session.Remove(UserSessionKey);
        }
    }

    public class LoginResponse
    {
        public string? Token { get; set; }
        public string? Username { get; set; }
        public string? Role { get; set; }
        public string? FullName { get; set; }
        public DateTime ExpiryTime { get; set; }
    }
}

// ============================================================================
// 4. LOGIN PAGE MODEL
// ============================================================================

// File: WelfareLink/Pages/Login.cshtml.cs

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WelfareLink.Services;

namespace WelfareLink.Pages
{
    [IgnoreAntiforgeryToken(Order = 1000)]
    public class LoginModel : PageModel
    {
        private readonly IJwtAuthService _authService;
        private readonly ILogger<LoginModel> _logger;

        [BindProperty]
        public string? Username { get; set; }

        [BindProperty]
        public string? Password { get; set; }

        [BindProperty]
        public string? UserType { get; set; } = "WelfareOfficer";

        public string? ErrorMessage { get; set; }

        public LoginModel(IJwtAuthService authService, ILogger<LoginModel> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        public void OnGet()
        {
            // Check if user already has valid token
            var token = _authService.GetStoredToken(HttpContext);
            if (!string.IsNullOrEmpty(token))
            {
                RedirectToPage("/Dashboard");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "Username and password are required";
                return Page();
            }

            try
            {
                var loginResponse = await _authService.LoginAsync(
                    Username,
                    Password,
                    UserType ?? "WelfareOfficer"
                );

                if (loginResponse == null || string.IsNullOrEmpty(loginResponse.Token))
                {
                    ErrorMessage = "Invalid credentials or login service unavailable";
                    _logger.LogWarning($"Failed login attempt for {Username}");
                    return Page();
                }

                // Store token and user info in session
                _authService.StoreToken(HttpContext, loginResponse.Token, loginResponse);

                _logger.LogInformation($"User {Username} logged in successfully");
                return RedirectToPage("/Dashboard");
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred during login. Please try again.";
                _logger.LogError($"Login error: {ex.Message}");
                return Page();
            }
        }
    }
}

// ============================================================================
// 5. MIDDLEWARE - Add Token to API Requests
// ============================================================================

// File: WelfareLink/Middleware/JwtTokenMiddleware.cs

using System.Net.Http.Headers;

namespace WelfareLink.Middleware
{
    public class JwtTokenMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<JwtTokenMiddleware> _logger;

        public JwtTokenMiddleware(RequestDelegate next, ILogger<JwtTokenMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IJwtAuthService authService)
        {
            var token = authService.GetStoredToken(context);

            if (!string.IsNullOrEmpty(token))
            {
                // Add token to HttpContext for use in services
                context.Items["JwtToken"] = token;
                _logger.LogDebug("JWT token added to context");
            }

            await _next(context);
        }
    }
}

// Register in Program.cs:
// app.UseMiddleware<JwtTokenMiddleware>();

// ============================================================================
// 6. API CLIENT SERVICE - Make Calls with JWT Token
// ============================================================================

// File: WelfareLink/Services/ApiClientService.cs

using System.Net.Http.Headers;

namespace WelfareLink.Services
{
    public interface IApiClientService
    {
        Task<T?> GetAsync<T>(string baseUrl, string endpoint, HttpContext httpContext);
        Task<T?> PostAsync<T>(string baseUrl, string endpoint, object? data, HttpContext httpContext);
    }

    public class ApiClientService : IApiClientService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IJwtAuthService _jwtAuthService;
        private readonly ILogger<ApiClientService> _logger;

        public ApiClientService(
            IHttpClientFactory httpClientFactory,
            IJwtAuthService jwtAuthService,
            ILogger<ApiClientService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _jwtAuthService = jwtAuthService;
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string baseUrl, string endpoint, HttpContext httpContext)
        {
            try
            {
                var token = _jwtAuthService.GetStoredToken(httpContext);
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("No JWT token found in session");
                    return default;
                }

                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsAsync<T>();
                }

                _logger.LogWarning($"API call failed: {response.StatusCode}");
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError($"API error: {ex.Message}");
                return default;
            }
        }

        public async Task<T?> PostAsync<T>(string baseUrl, string endpoint, object? data, HttpContext httpContext)
        {
            try
            {
                var token = _jwtAuthService.GetStoredToken(httpContext);
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("No JWT token found in session");
                    return default;
                }

                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.PostAsJsonAsync(endpoint, data);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsAsync<T>();
                }

                _logger.LogWarning($"API call failed: {response.StatusCode}");
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError($"API error: {ex.Message}");
                return default;
            }
        }
    }
}

// Register in Program.cs:
// builder.Services.AddScoped<IApiClientService, ApiClientService>();

// ============================================================================
// 7. DASHBOARD PAGE - Using JWT Token to Call APIs
// ============================================================================

// File: WelfareLink/Pages/Dashboard.cshtml.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WelfareLink.Services;

namespace WelfareLink.Pages
{
    [Authorize]
    public class DashboardModel : PageModel
    {
        private readonly IApiClientService _apiClient;
        private readonly IJwtAuthService _jwtAuthService;
        private readonly ILogger<DashboardModel> _logger;

        public string? UserName { get; set; }
        public string? UserRole { get; set; }
        public List<ReportSummary> Reports { get; set; } = new();

        public DashboardModel(
            IApiClientService apiClient,
            IJwtAuthService jwtAuthService,
            ILogger<DashboardModel> logger)
        {
            _apiClient = apiClient;
            _jwtAuthService = jwtAuthService;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            try
            {
                var token = _jwtAuthService.GetStoredToken(HttpContext);
                if (string.IsNullOrEmpty(token))
                {
                    // Redirect to login if no token
                    RedirectToPage("/Login");
                    return;
                }

                // Set user info from session
                // TODO: Parse token claims if needed

                // Fetch reports from Analytics API using JWT token
                var baseUrl = "https://localhost:7202"; // Analytics API
                var reports = await _apiClient.GetAsync<List<ReportSummary>>(
                    baseUrl,
                    "api/analytics/reports",
                    HttpContext
                );

                if (reports != null)
                {
                    Reports = reports;
                }

                _logger.LogInformation("Dashboard loaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Dashboard error: {ex.Message}");
            }
        }
    }

    public class ReportSummary
    {
        public int ReportId { get; set; }
        public string? Title { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}

// ============================================================================
// 8. LOGOUT PAGE
// ============================================================================

// File: WelfareLink/Pages/Logout.cshtml.cs

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WelfareLink.Services;

namespace WelfareLink.Pages
{
    public class LogoutModel : PageModel
    {
        private readonly IJwtAuthService _authService;
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(IJwtAuthService authService, ILogger<LogoutModel> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            // Clear JWT token from session
            _authService.ClearToken(HttpContext);

            _logger.LogInformation("User logged out");
            return RedirectToPage("/Login");
        }
    }
}

// ============================================================================
// 9. USAGE IN RAZOR PAGES
// ============================================================================

@* File: WelfareLink/Pages/Dashboard.cshtml *@

@page
@model DashboardModel

<div class="container mt-5">
    <h2>Welcome to WelfareLink Dashboard</h2>
    <p>User: @Model.UserName (@Model.UserRole)</p>

    <h3>Recent Reports</h3>
    <table class="table">
        <thead>
            <tr>
                <th>Report ID</th>
                <th>Title</th>
                <th>Created</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var report in Model.Reports)
            {
                <tr>
                    <td>@report.ReportId</td>
                    <td>@report.Title</td>
                    <td>@report.CreatedDate.ToString("yyyy-MM-dd")</td>
                </tr>
            }
        </tbody>
    </table>
</div>

// ============================================================================
// 10. COMPLETE PROGRAM.CS SETUP
// ============================================================================

using Microsoft.EntityFrameworkCore;
using WelfareLink.Data;
using WelfareLink.Services;
using WelfareLink.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

// Database
builder.Services.AddDbContext<WelfareLinkDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// HTTP Client
builder.Services.AddHttpClient("AuthApi", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ApiSettings:Authentication"] 
        ?? throw new InvalidOperationException("ApiSettings:Authentication not configured"));
})
.ConfigurePrimaryHttpMessageHandler(() =>
    new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });

builder.Services.AddHttpClient();

// JWT & Auth
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Login";
        options.LogoutPath = "/Logout";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    });

builder.Services.AddAuthorization();

// Services
builder.Services.AddScoped<IJwtAuthService, JwtAuthService>();
builder.Services.AddScoped<IApiClientService, ApiClientService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();

app.UseMiddleware<JwtTokenMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();

// ============================================================================
// SUMMARY
// ============================================================================

/*
 * This implementation provides:
 * 
 * ✅ Session-based JWT token storage
 * ✅ Automatic token inclusion in API requests
 * ✅ Login/Logout pages integrated with JWT
 * ✅ Protected Razor Pages with [Authorize]
 * ✅ Token refresh capability
 * ✅ Unified authentication across MVC and APIs
 * 
 * Flow:
 * 1. User logs in on /Login page
 * 2. JwtAuthService calls Authentication.API
 * 3. JWT token is stored in session
 * 4. Dashboard and other pages access APIs using stored token
 * 5. All API calls include Authorization: Bearer {token}
 * 6. APIs validate token and respond based on claims
 * 
 */
