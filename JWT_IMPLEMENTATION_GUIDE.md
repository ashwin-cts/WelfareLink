# JWT Authentication Implementation Guide

## Overview
This document describes the centralized JWT authentication system implemented for the WelfareLink application. The system uses a dedicated Authentication API to issue JWT tokens and validates them across all other API projects.

## Architecture

### Components

1. **WelfareLink.Authentication.API** (JWT Issuance Service)
   - Centralized authentication service
   - Accepts login credentials
   - Validates credentials via UserManagement API
   - Issues JWT tokens with role-based claims
   - Only this API generates tokens

2. **WelfareLink.UserManagement.API** (Credential Validation)
   - Contains user database and credential validation logic
   - Provides `/api/user/login` endpoint for credential validation
   - Does NOT generate tokens (validation only)
   - Other APIs validate tokens from Authentication API

3. **Other API Projects** (Token Validation Only)
   - WelfareLink.Operations.API
   - WelfareLink.BenifitEligiblity.API
   - WelfareLink.WApplicationSystem.API
   - WelfareLink.AnalyticsReport.API
   - WelfareLink.ComplianceAndAudit.API
   - Each validates JWT tokens before allowing access to protected endpoints

## Configuration

### JWT Settings (appsettings.json)

All API projects use the same JWT configuration:

```json
"JwtSettings": {
    "Secret": "MyApplication_Secret_Key_2026_Keep_It_Safe!!",
    "Issuer": "WelfareLinkAuthServer",
    "Audience": "WelfareLinkUsers",
    "ExpiryMinutes": 60
}
```

**Important:** Keep the Secret key secure and consistent across all projects.

### User Roles

The system supports the following user roles in JWT claims:

- **Citizen** - End users of welfare programs
- **WelfareOfficer** - Staff managing citizen applications
- **ProgramManager** - Program administrators
- **ComplianceOfficer** - Compliance monitoring
- **GovernmentAuditor** - Government auditing
- **Admin** - System administrators

## API Flows

### 1. Login Flow (Authentication API)

**Request:**
```http
POST /api/auth/login
Content-Type: application/json

{
    "username": "john_doe",
    "password": "securePassword123",
    "userType": "Citizen"
}
```

**Response:**
```json
{
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "username": "john_doe",
    "role": "Citizen",
    "fullName": "John Doe",
    "expiryTime": "2026-03-27T11:30:00Z"
}
```

**Process:**
1. Authentication API receives login request
2. Calls UserManagement API to validate credentials
3. If credentials are valid and account is active, generates JWT token
4. Returns token with user metadata

### 2. Protected Resource Access (Any API)

**Request:**
```http
GET /api/resource
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Process:**
1. API validates JWT token using configured secret key
2. Verifies issuer and audience match configuration
3. Extracts user claims (UserId, Username, Role, FullName)
4. Allows access if token is valid and not expired
5. Returns 401 Unauthorized if token is missing or invalid

## Securing Endpoints

### Adding Authorization to Controllers

Use the `[Authorize]` attribute to protect endpoints:

```csharp
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
public class CitizenController : ControllerBase
{
    // Public endpoint - no token required
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // Call Authentication API
    }

    // Protected endpoint - token required
    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCitizen(int id)
    {
        var userId = User.FindFirst("UserId")?.Value;
        // Return citizen data
    }

    // Protected endpoint - specific role required
    [Authorize(Roles = "Admin,ProgramManager")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCitizen(int id, [FromBody] Citizen citizen)
    {
        // Update citizen data
    }
}
```

### Role-Based Authorization

Restrict endpoints to specific user roles:

```csharp
[Authorize(Roles = "Admin")]
[HttpDelete("{id}")]
public async Task<IActionResult> DeleteUser(int id)
{
    // Admin-only operation
}

[Authorize(Roles = "WelfareOfficer,ProgramManager")]
[HttpPost("approve")]
public async Task<IActionResult> ApproveApplication(int applicationId)
{
    // Officer-only operation
}
```

## Extracting User Information from JWT

Access user claims in your controllers:

```csharp
[Authorize]
[HttpGet("profile")]
public IActionResult GetUserProfile()
{
    var userId = User.FindFirst("UserId")?.Value;
    var username = User.FindFirst("Username")?.Value;
    var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
    var fullName = User.FindFirst("FullName")?.Value;
    var email = User.FindFirst("Email")?.Value;

    return Ok(new
    {
        userId,
        username,
        role,
        fullName,
        email
    });
}
```

## JWT Token Structure

### Header
```json
{
    "alg": "HS256",
    "typ": "JWT"
}
```

### Payload (Claims)
```json
{
    "sub": "1",
    "unique_name": "john_doe",
    "jti": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "UserId": "1",
    "Username": "john_doe",
    "role": "Citizen",
    "FullName": "John Doe",
    "Email": "john.doe@example.com",
    "iss": "WelfareLinkAuthServer",
    "aud": "WelfareLinkUsers",
    "exp": 1743209400,
    "iat": 1743205800
}
```

### Security Algorithm
- **Algorithm:** HMAC SHA-256 (HS256)
- **Secret:** 256+ character strong password stored in appsettings.json

## Configuration Steps for New Projects

If adding a new API project:

1. **Add NuGet Packages:**
   ```bash
   dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 10.0.0
   dotnet add package System.IdentityModel.Tokens.Jwt --version 8.4.1
   dotnet add package Microsoft.IdentityModel.Tokens --version 8.4.1
   ```

2. **Update appsettings.json:**
   ```json
   "JwtSettings": {
       "Secret": "MyApplication_Secret_Key_2026_Keep_It_Safe!!",
       "Issuer": "WelfareLinkAuthServer",
       "Audience": "WelfareLinkUsers",
       "ExpiryMinutes": 60
   }
   ```

3. **Update Program.cs:**
   ```csharp
   using System.Text;
   using Microsoft.AspNetCore.Authentication.JwtBearer;
   using Microsoft.IdentityModel.Tokens;

   var builder = WebApplication.CreateBuilder(args);

   // JWT Configuration
   var jwtSettings = builder.Configuration.GetSection("JwtSettings");
   var secret = jwtSettings["Secret"];
   var key = Encoding.ASCII.GetBytes(secret);

   builder.Services.AddAuthentication(options =>
   {
       options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
       options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
   })
   .AddJwtBearer(options =>
   {
       options.TokenValidationParameters = new TokenValidationParameters
       {
           ValidateIssuerSigningKey = true,
           IssuerSigningKey = new SymmetricSecurityKey(key),
           ValidateIssuer = true,
           ValidateAudience = true,
           ValidIssuer = jwtSettings["Issuer"],
           ValidAudience = jwtSettings["Audience"],
           ClockSkew = TimeSpan.Zero
       };
   });

   var app = builder.Build();

   app.UseAuthentication();
   app.UseAuthorization();

   app.MapControllers();
   app.Run();
   ```

4. **Add [Authorize] Attributes:** Protect sensitive endpoints with `[Authorize]` attribute

## Security Best Practices

### Secrets Management

1. **Development:** Use appsettings.json (as configured)
2. **Staging/Production:** Use Azure Key Vault or environment variables

   ```csharp
   var secret = Environment.GetEnvironmentVariable("JWT_SECRET") 
       ?? configuration["JwtSettings:Secret"];
   ```

3. **Never commit secrets** to version control

### Token Expiry

- **Current Setting:** 60 minutes
- **Recommendation:** Adjust based on your security requirements
  - Short-lived: 15-30 minutes (high security)
  - Standard: 60 minutes (balance)
  - Long-lived: 24 hours (convenience, lower security)

### HTTPS Enforcement

- Always use HTTPS in production
- JWT tokens should never be transmitted over HTTP
- Use secure cookies for token storage in browsers

### Token Validation

- All APIs validate tokens consistently
- Invalid or expired tokens are rejected with 401 Unauthorized
- Tokens cannot be reused if valid format check fails

## Troubleshooting

### Issue: 401 Unauthorized on Protected Endpoint

**Causes:**
- Token not included in Authorization header
- Token is expired
- Token signature doesn't match (different secret)
- Issuer or Audience mismatch

**Solution:**
1. Verify token is included: `Authorization: Bearer <token>`
2. Check token expiry: Decode JWT on jwt.io
3. Ensure same secret in appsettings.json across all projects
4. Verify Issuer and Audience match configuration

### Issue: Invalid Token Signature

**Causes:**
- Different JWT_SECRET between Authentication API and other APIs
- Secret modified without updating all projects

**Solution:**
1. Ensure all projects use identical JWT settings
2. Restart all API services after changing configuration
3. Regenerate tokens if secret was changed

### Issue: Token Generated but Cannot Access Protected Resources

**Causes:**
- Authentication API generates token but other APIs can't validate it
- CORS policy blocking token transmission
- Authorization middleware not configured

**Solution:**
1. Verify JWT configuration is identical across projects
2. Check CORS policy allows Authorization header
3. Ensure `app.UseAuthentication()` comes before `app.UseAuthorization()` in Program.cs
4. Verify `[Authorize]` attribute is on the endpoint

## Testing

### Using Postman or cURL

1. **Get Token:**
   ```bash
   curl -X POST https://localhost:7200/api/auth/login \
     -H "Content-Type: application/json" \
     -d '{"username":"john","password":"pass123","userType":"Citizen"}'
   ```

2. **Use Token:**
   ```bash
   curl -X GET https://localhost:7203/api/citizen/1 \
     -H "Authorization: Bearer <token>"
   ```

## Migration Notes

### From Session-Based to JWT

- Old system used session-based authentication
- New system uses stateless JWT tokens
- Both can coexist during transition period
- Sessions can be phased out as clients migrate to JWT

### Backward Compatibility

- User login endpoint still available in UserManagement API
- Authentication API provides new JWT-based login
- Client applications should migrate to Authentication API endpoint

## References

- [JWT Introduction](https://jwt.io/introduction)
- [Microsoft JWT Documentation](https://docs.microsoft.com/en-us/dotnet/api/system.identitymodel.tokens.jwt)
- [ASP.NET Core Authentication](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/)
