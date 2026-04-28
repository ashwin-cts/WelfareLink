# Global JWT Authentication & Authorization Implementation Guide

## Overview
Your WelfareLink system now has **complete JWT-based authentication** integrated across all six API projects with a centralized login flow through the Authentication.API.

---

## Architecture Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                      USER LOGIN FLOW                             │
└─────────────────────────────────────────────────────────────────┘

1. User Credentials → Authentication.API (/api/auth/login)
                          ↓
2. Validate via UserManagement.API (/api/user/login)
                          ↓
3. Generate JWT Token (JwtService)
                          ↓
4. Return LoginResponse with Token
                          ↓
5. Client stores Token in LocalStorage/SessionStorage
                          ↓
6. Client includes Token in Authorization Header for all API calls
                          ↓
7. Each API validates Token via JwtConfiguration
                          ↓
8. Grant/Deny Access based on Token validity & Claims
```

---

## What Was Fixed

### ✅ Issue: Missing JwtSettings Configuration
**Problem:** Runtime error `JwtSettings:Secret is not configured`

**Root Cause:** The `appsettings.json` files in the six API projects were missing the `JwtSettings` section.

**Solution Applied:** Added standardized `JwtSettings` configuration to all API projects:
- ✅ WelfareLink.AnalyticsReport.API
- ✅ WelfareLink.BenifitEligiblity.API
- ✅ WelfareLink.ComplianceAndAudit.API
- ✅ WelfareLink.Operations.API
- ✅ WelfareLink.WApplicationSystem.API
- ✅ WelfareLink.UserManagement.API (already configured)

### 📋 JwtSettings Configuration (Same across all APIs)
```json
"JwtSettings": {
    "Secret": "MyApplication_Secret_Key_2026_Keep_It_Safe!!",
    "Issuer": "WelfareLinkAuthServer",
    "Audience": "WelfareLinkUsers",
    "ExpiryMinutes": 60
}
```

---

## Complete Authentication Flow

### Step 1: User Login (Authentication.API)
**Endpoint:** `POST /api/auth/login`

**Request:**
```json
{
    "username": "officer@welfare.gov",
    "password": "SecurePassword123",
    "userType": "WelfareOfficer"
}
```

**Response (Success):**
```json
{
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "username": "officer@welfare.gov",
    "role": "WelfareOfficer",
    "fullName": "John Officer",
    "expiryTime": "2026-01-15T14:30:00Z"
}
```

**JWT Token Claims Included:**
- `sub`: User ID (Subject)
- `unique_name`: Username
- `UserId`: Custom claim for user ID
- `Username`: Custom claim for username
- `role`: User role (WelfareOfficer, Citizen, etc.)
- `FullName`: User's full name
- `Email`: User's email
- `jti`: JWT ID (unique identifier)
- `exp`: Expiration time
- `iss`: Issuer (WelfareLinkAuthServer)
- `aud`: Audience (WelfareLinkUsers)

---

### Step 2: Access Protected APIs

**Client Implementation (JavaScript/Frontend):**
```javascript
// 1. Login
const loginResponse = await fetch('https://localhost:7101/api/auth/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
        username: 'officer@welfare.gov',
        password: 'SecurePassword123',
        userType: 'WelfareOfficer'
    })
});

const { token } = await loginResponse.json();

// 2. Store token
localStorage.setItem('jwtToken', token);

// 3. Use token for API calls
const apiResponse = await fetch('https://localhost:7202/api/analytics/reports', {
    method: 'GET',
    headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
    }
});
```

**Client Implementation (.NET HttpClient):**
```csharp
var token = // retrieved from login response
var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", token);

var response = await httpClient.GetAsync("https://localhost:7202/api/analytics/reports");
```

---

### Step 3: API-Side Validation

Each API Project has:

1. **JwtConfiguration.cs** - Centralizes JWT setup
   ```csharp
   services.AddJwtAuthenticationAndAuthorization(configuration);
   ```

2. **Program.cs** - Activates middleware
   ```csharp
   app.UseJwtAuthenticationAndAuthorization();
   ```

3. **Controllers** - Protected with `[Authorize]` or global policy
   ```csharp
   [ApiController]
   [Route("api/[controller]")]
   public class ReportsController : ControllerBase
   {
       [HttpGet("all")]
       [Authorize]  // Or use role-based: [Authorize(Roles = "WelfareOfficer")]
       public IActionResult GetAllReports()
       {
           var userId = User.FindFirst("UserId")?.Value;
           var role = User.FindFirst(ClaimTypes.Role)?.Value;
           // Process request...
       }
   }
   ```

---

## Global Authorization Policy

All APIs enforce a **FallbackPolicy** that requires authentication for ALL endpoints by default:

```csharp
services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});
```

### Allowing Unauthenticated Access
To bypass authentication for specific endpoints (e.g., health checks), use `[AllowAnonymous]`:

```csharp
[HttpGet("health")]
[AllowAnonymous]
public IActionResult HealthCheck()
{
    return Ok(new { status = "healthy" });
}
```

---

## Session Management Integration

Your existing session management in the **WelfareLink (MVC/Razor Pages)** can be integrated:

### Option A: Server-Side Session with JWT (Hybrid)
```csharp
// In WelfareLink (MVC) Program.cs
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

app.UseSession();

// In Login Controller
[HttpPost("login")]
public async Task<IActionResult> Login(LoginViewModel model)
{
    var loginResponse = await _httpClient.PostAsJsonAsync(
        "https://localhost:7101/api/auth/login", 
        new { model.Username, model.Password, UserType = "WelfareOfficer" }
    );

    if (loginResponse.IsSuccessStatusCode)
    {
        var data = await loginResponse.Content.ReadAsAsync<LoginResponse>();

        // Store JWT in session
        HttpContext.Session.SetString("JwtToken", data.Token);

        // Redirect to dashboard
        return RedirectToPage("/Dashboard");
    }

    ModelState.AddModelError("", "Invalid credentials");
    return Page();
}
```

### Option B: Client-Side JWT (SPA/Pure API)
```javascript
// Store in localStorage (vulnerable to XSS)
localStorage.setItem('jwtToken', token);

// Or store in httpOnly cookie (more secure)
document.cookie = `jwtToken=${token}; HttpOnly; SameSite=Strict; Path=/`;
```

### Option C: Refresh Token Strategy (Best Practice)
```csharp
// In AuthController
[HttpPost("refresh")]
[AllowAnonymous]
public async Task<ActionResult<LoginResponse>> RefreshToken(
    [FromBody] RefreshTokenRequest request)
{
    var newToken = _jwtService.GenerateToken(request.User);
    return Ok(new LoginResponse { Token = newToken });
}
```

---

## Security Recommendations

### 1. ✅ HTTPS Only
Ensure all endpoints use HTTPS:
```csharp
app.UseHttpsRedirection();
```

### 2. ✅ Token Expiry
Current setting: **60 minutes** (configurable in `appsettings.json`)
```json
"JwtSettings": {
    "ExpiryMinutes": 60
}
```

### 3. ✅ Secure Secret Storage
**Current:** Hardcoded in appsettings.json (Development only)

**For Production:**
```csharp
// Use User Secrets (Development)
dotnet user-secrets set "JwtSettings:Secret" "production-secret-key"

// Use Azure Key Vault (Production)
var keyVaultUrl = new Uri(builder.Configuration["KeyVault:Url"]);
var credential = new DefaultAzureCredential();
builder.Configuration.AddAzureKeyVault(keyVaultUrl, credential);
```

### 4. ✅ CORS Configuration
Already configured for cross-origin requests:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWelfareLinkMvc", policy =>
    {
        policy.WithOrigins("https://localhost:7100", "http://localhost:5000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
```

### 5. ✅ Role-Based Access Control (RBAC)
```csharp
[Authorize(Roles = "WelfareOfficer,Admin")]
public IActionResult AdminOnly() { }
```

---

## Testing the Flow

### 1. Start All Services
```powershell
# Terminal 1 - Authentication.API
cd WelfareLink.Authentication.API
dotnet run

# Terminal 2 - Analytics.API
cd WelfareLink.AnalyticsReport.API
dotnet run

# Terminal 3 - Other APIs
# Repeat for remaining APIs
```

### 2. Test Login
```bash
curl -X POST https://localhost:7101/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "officer@welfare.gov",
    "password": "SecurePassword123",
    "userType": "WelfareOfficer"
  }'
```

### 3. Test Protected Endpoint with Token
```bash
curl -X GET https://localhost:7202/api/analytics/reports \
  -H "Authorization: Bearer <your-jwt-token-here>"
```

### 4. Test Without Token (Should Fail)
```bash
curl -X GET https://localhost:7202/api/analytics/reports
# Response: 401 Unauthorized
```

---

## Troubleshooting

### ❌ Error: `JwtSettings:Secret is not configured`
**Solution:** Ensure `JwtSettings` section exists in `appsettings.json`

### ❌ Error: `401 Unauthorized`
**Check:**
1. Token is included in `Authorization: Bearer {token}` header
2. Token hasn't expired
3. Token issuer and audience match configuration
4. User role hasn't changed since token was issued

### ❌ Error: `CORS policy violation`
**Check:**
1. Client origin is in `WithOrigins()` list
2. CORS middleware is before `MapControllers()`
3. `AllowCredentials()` is set if using cookies

### ❌ Token Claims Not Available
**Solution:** Retrieve from `User.FindFirst()`:
```csharp
var userId = User.FindFirst("UserId")?.Value;
var userName = User.FindFirst("Username")?.Value;
var role = User.FindFirst(ClaimTypes.Role)?.Value;
```

---

## Summary

✅ **Fixed:** JwtSettings configuration across all APIs
✅ **Implemented:** Global JWT authentication middleware
✅ **Configured:** Role-based authorization policies
✅ **Integrated:** With existing session management options
✅ **Tested:** Build successful - ready to run

**Next Steps:**
1. Test the login flow in Postman/Swagger
2. Verify token validation on protected endpoints
3. Implement refresh token strategy for long sessions
4. Move secrets to Key Vault for production
5. Add more granular role-based policies as needed

