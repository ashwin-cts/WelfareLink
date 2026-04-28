# Global JWT Authorization Implementation Guide

## Overview
This document explains the JWT-based global authorization system implemented across all WelfareLink API projects. The system allows users to log in once through the Authentication.API and use the issued JWT token to access all six API projects.

## Architecture Flow

```
User Login Credentials
    ↓
[Authentication.API - Login Endpoint]
    ↓
Validates against User Database
    ↓
Issues JWT Token with Claims
    ↓
User receives Token with Expiry
    ↓
User includes Token in Authorization Header
    ↓
[All Other APIs]
    ↓
Validates Token using Shared JWT Configuration
    ↓
Extracts User Claims from Token
    ↓
Grants Access if Valid
```

## Key Components

### 1. JWT Configuration (Centralized)
- **File**: `WelfareLink.Authentication.API\Configuration\JwtConfiguration.cs`
- **Purpose**: Provides centralized extension methods for all APIs to configure JWT authentication
- **Benefits**: 
  - Single source of truth for JWT settings
  - Consistent configuration across all APIs
  - Easy maintenance and updates

### 2. Login Flow (Authentication.API)

#### Endpoint
- **URL**: `POST /api/auth/login`
- **Request Body**:
```json
{
    "username": "user@example.com",
    "password": "password123",
    "userType": "WelfareOfficer"
}
```

#### Response (Success)
```json
{
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "username": "user@example.com",
    "role": "WelfareOfficer",
    "fullName": "John Doe",
    "expiryTime": "2024-12-31T12:00:00Z"
}
```

#### Response (Failure)
```json
{
    "error": "Invalid credentials or account is inactive"
}
```

### 3. Token Validation (All APIs)

Each API validates the token using the same JWT configuration:

1. **Validates Signature**: Ensures token wasn't tampered with
2. **Validates Issuer**: Confirms token came from your authorization server
3. **Validates Audience**: Ensures token is intended for your APIs
4. **Validates Expiry**: Checks if token is still valid
5. **Extracts Claims**: Retrieves user information from the token

### 4. Token Claims Structure

The JWT token contains the following claims:

```csharp
new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),           // Subject (User ID)
new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),            // Username
new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),       // JWT ID
new Claim("UserId", user.UserId.ToString()),                             // User ID
new Claim("Username", user.Username),                                    // Username
new Claim(ClaimTypes.Role, user.Role),                                   // Role
new Claim("FullName", user.FullName),                                    // Full Name
new Claim("Email", user.Email)                                           // Email
```

## Configuration Settings

### appsettings.json

All API projects require the following configuration in their `appsettings.json`:

```json
{
  "JwtSettings": {
    "Secret": "your-super-secret-key-minimum-32-characters-for-security",
    "Issuer": "WelfareLinkAuthenticationServer",
    "Audience": "WelfareLinkAPIClients",
    "ExpiryMinutes": 60
  }
}
```

**Important**: 
- The `Secret` must be the same across all projects
- Use a strong secret (minimum 32 characters) in production
- Store secrets in Azure Key Vault or similar service
- Never commit secrets to version control

## API Projects Configured

The following API projects have JWT authorization configured:

1. **WelfareLink.Authentication.API** - Handles user login and token generation
2. **WelfareLink.BenifitEligiblity.API** - Validates tokens for benefit eligibility endpoints
3. **WelfareLink.UserManagement.API** - Validates tokens for user management endpoints
4. **WelfareLink.WApplicationSystem.API** - Validates tokens for welfare application endpoints
5. **WelfareLink.ComplianceAndAudit.API** - Validates tokens for compliance endpoints
6. **WelfareLink.AnalyticsReport.API** - Validates tokens for analytics endpoints
7. **WelfareLink.Operations.API** - Validates tokens for operations endpoints

## Implementation Steps for Each API

### Step 1: Update Program.cs

Replace the JWT configuration section with the centralized helper:

**Before:**
```csharp
// JWT Configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secret = jwtSettings["Secret"] ?? throw new InvalidOperationException("JwtSettings:Secret is not configured");
var key = Encoding.ASCII.GetBytes(secret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // ... token validation parameters ...
});

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});
```

**After:**
```csharp
// Add JWT Authentication & Authorization (Centralized Configuration)
builder.Services.AddJwtAuthenticationAndAuthorization(builder.Configuration);
```

And in the middleware configuration:

**Before:**
```csharp
app.UseAuthentication();
app.UseAuthorization();
```

**After:**
```csharp
app.UseJwtAuthenticationAndAuthorization();
```

### Step 2: Add appsettings.json Configuration

Ensure each API's `appsettings.json` contains the JWT settings:

```json
{
  "JwtSettings": {
    "Secret": "your-super-secret-key-minimum-32-characters-for-security",
    "Issuer": "WelfareLinkAuthenticationServer",
    "Audience": "WelfareLinkAPIClients",
    "ExpiryMinutes": 60
  }
}
```

### Step 3: Verify Global [Authorize] Attribute

With the centralized configuration, all endpoints automatically require authentication unless explicitly exempted with `[AllowAnonymous]`.

**Example Controller:**
```csharp
[ApiController]
[Route("api/[controller]")]
public class CitizenController : ControllerBase
{
    // This endpoint requires authentication
    [HttpGet("{id}")]
    public IActionResult GetCitizen(int id)
    {
        var userId = User.FindFirst("UserId")?.Value;
        // Access endpoint
    }

    // This endpoint is accessible without authentication
    [AllowAnonymous]
    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        // Public endpoint
    }
}
```

## Using the JWT Token

### Client-Side Implementation

#### 1. Login to Get Token
```csharp
var loginRequest = new
{
    username = "user@example.com",
    password = "password123",
    userType = "WelfareOfficer"
};

var response = await httpClient.PostAsJsonAsync(
    "https://api.welfarelink.com/api/auth/login", 
    loginRequest);

if (response.IsSuccessStatusCode)
{
    var loginResponse = await response.Content.ReadAsAsync<LoginResponse>();
    var token = loginResponse.Token;
    // Store token (localStorage, SessionStorage, or secure cookie)
}
```

#### 2. Use Token for Subsequent Requests
```csharp
var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", token);

// Now make requests to any of the APIs
var response = await httpClient.GetAsync(
    "https://api.welfarelink.com/api/citizen/123");
```

### JavaScript/TypeScript Client Example

```typescript
// Login
async function login(username: string, password: string, userType: string) {
    const response = await fetch('https://api.welfarelink.com/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username, password, userType })
    });

    const data = await response.json();
    if (response.ok) {
        localStorage.setItem('jwtToken', data.token);
        localStorage.setItem('expiryTime', data.expiryTime);
        return true;
    }
    return false;
}

// Make authenticated request
async function getProtectedResource(url: string) {
    const token = localStorage.getItem('jwtToken');
    const response = await fetch(url, {
        headers: {
            'Authorization': `Bearer ${token}`
        }
    });

    return await response.json();
}
```

## Error Handling

### Token Validation Errors

The JWT middleware returns consistent error responses:

#### 401 - Unauthorized (No Token)
```json
{
    "error": "Unauthorized - Valid JWT token required"
}
```

#### 401 - Token Validation Failed
```json
{
    "error": "Token validation failed",
    "details": "The token is not valid before: 'yyyy-MM-ddThh:mm:ssZ'"
}
```

#### 401 - Token Expired
```json
{
    "error": "Token validation failed",
    "details": "The token is not valid before: 'yyyy-MM-ddThh:mm:ssZ'"
}
```

## Security Best Practices

1. **Secret Management**
   - Store JWT secret in Azure Key Vault
   - Never hardcode secrets in appsettings.json
   - Use different secrets for different environments

2. **Token Storage (Client)**
   - For web applications: Use HttpOnly cookies
   - For SPAs: Use in-memory storage or secure SessionStorage
   - Never store tokens in localStorage in production

3. **HTTPS Only**
   - Always use HTTPS in production
   - Set secure flag on cookies
   - Use strict transport security headers

4. **Token Expiry**
   - Keep token expiry short (30-60 minutes)
   - Implement refresh token mechanism for longer sessions
   - Log token expiry to audit logs

5. **CORS Configuration**
   - Limit CORS to trusted origins only
   - Don't use wildcard (*) in production
   - Require credentials for cross-origin requests

## Token Refresh (Optional Enhancement)

For better security, implement refresh token mechanism:

```csharp
[HttpPost("refresh")]
[AllowAnonymous]
public IActionResult RefreshToken([FromBody] string refreshToken)
{
    // Validate refresh token
    var user = _tokenService.ValidateRefreshToken(refreshToken);
    if (user == null)
        return Unauthorized(new { error = "Invalid refresh token" });

    // Generate new access token
    var newToken = _jwtService.GenerateToken(user);
    return Ok(new { token = newToken });
}
```

## Troubleshooting

### Issue: 401 Unauthorized on all requests

**Solution:**
1. Verify JWT secret is the same across all projects
2. Check token expiry in appsettings.json
3. Ensure token is sent in Authorization header: `Authorization: Bearer {token}`
4. Verify JwtSettings section exists in appsettings.json

### Issue: Token works on one API but not another

**Solution:**
1. Check Issuer and Audience match across all projects
2. Verify all projects have same JWT secret
3. Check local time synchronization (clock skew can cause issues)

### Issue: 500 Internal Server Error on Login

**Solution:**
1. Verify authentication service can reach user database
2. Check database connection string in appsettings.json
3. Review logs for specific error details
4. Ensure user exists and is active in database

## Migration from Session-Based to JWT

If you're migrating from session-based authentication:

1. **Keep both systems during transition**: Support old session-based endpoints while adding JWT endpoints
2. **Gradual rollout**: Move one API at a time
3. **Monitor logs**: Watch for authentication failures
4. **User education**: Document new login/token usage process
5. **Deprecate old endpoints**: Set deprecation date for session-based endpoints

## Testing the Implementation

### Manual Testing with cURL

```bash
# Login
curl -X POST https://localhost:7101/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123","userType":"Admin"}'

# Use token to access protected endpoint
curl -X GET https://localhost:7102/api/citizen/1 \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

### Using Postman

1. **Create Login Request**
   - Method: POST
   - URL: `https://localhost:7101/api/auth/login`
   - Body (JSON):
   ```json
   {
       "username": "admin",
       "password": "admin123",
       "userType": "Admin"
   }
   ```

2. **Extract Token**
   - Copy token from response

3. **Create Protected Request**
   - Method: GET
   - URL: `https://localhost:7102/api/citizen/1`
   - Headers: `Authorization: Bearer {paste_token_here}`

## Conclusion

The JWT-based authorization system provides:
- ✅ Stateless authentication across all APIs
- ✅ Secure token-based access control
- ✅ Centralized configuration and management
- ✅ Easy scaling to multiple servers
- ✅ Better integration with microservices

For questions or issues, refer to the implementation files or contact the development team.
