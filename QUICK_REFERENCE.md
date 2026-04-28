# JWT Authorization - Quick Reference

## Login Flow
```
1. POST /api/auth/login
   {
     "username": "user@example.com",
     "password": "password123",
     "userType": "WelfareOfficer"
   }

2. Response:
   {
     "token": "eyJhbGc...",
     "username": "user@example.com",
     "role": "WelfareOfficer",
     "fullName": "John Doe",
     "expiryTime": "2024-12-31T12:00:00Z"
   }

3. Use token in subsequent requests:
   GET /api/citizen/123
   Authorization: Bearer eyJhbGc...
```

## Configuration (appsettings.json)
```json
{
  "JwtSettings": {
    "Secret": "your-secret-key-minimum-32-characters",
    "Issuer": "WelfareLinkAuthenticationServer",
    "Audience": "WelfareLinkAPIClients",
    "ExpiryMinutes": 60
  }
}
```

## Controller Usage

### Extract JWT Claims
```csharp
using WelfareLink.Authentication.API.Utilities;

[HttpGet("{id}")]
public IActionResult GetResource(int id)
{
    var userId = JwtClaimsHelper.GetUserId(User);
    var username = JwtClaimsHelper.GetUsername(User);
    var role = JwtClaimsHelper.GetRole(User);

    // Your logic here
}
```

### Role-Based Authorization
```csharp
// Admin only
[Authorize(Roles = "Admin")]
public IActionResult AdminPanel() { }

// Multiple roles
[Authorize(Roles = "Admin,Manager")]
public IActionResult ManagementPanel() { }

// Public endpoint (no auth)
[AllowAnonymous]
public IActionResult PublicInfo() { }
```

## Error Responses

### 401 - No Token
```json
{
    "error": "Unauthorized - Valid JWT token required"
}
```

### 401 - Invalid Token
```json
{
    "error": "Token validation failed",
    "details": "The token is not valid..."
}
```

### 403 - Insufficient Permissions
```json
{
    "error": "Forbidden - Insufficient permissions"
}
```

## Testing with Postman

1. **Create Login Request**
   - POST: `https://localhost:7101/api/auth/login`
   - Body: JSON with username/password/userType
   - Copy token from response

2. **Create Protected Request**
   - GET: `https://localhost:7102/api/citizen/1`
   - Add header: `Authorization: Bearer {token}`

## Program.cs Setup
```csharp
// Add to services
builder.Services.AddJwtAuthenticationAndAuthorization(builder.Configuration);

// Add to middleware
app.UseJwtAuthenticationAndAuthorization();
```

## Supported User Types
- Admin
- Manager
- WelfareOfficer
- ComplianceOfficer
- ProgramManager
- GovernmentAuditor
- Citizen

## JWT Token Claims
| Claim | Example |
|-------|---------|
| sub | 1 |
| name | john.doe |
| jti | 550e8400-e29b-41d4-a716-446655440000 |
| UserId | 1 |
| Username | john.doe |
| role | WelfareOfficer |
| FullName | John Doe |
| Email | john@example.com |
| iat | 1703001200 |
| exp | 1703004800 |
| iss | WelfareLinkAuthenticationServer |
| aud | WelfareLinkAPIClients |

## API Projects Protected
- ✅ WelfareLink.Authentication.API
- ✅ WelfareLink.UserManagement.API
- ✅ WelfareLink.WApplicationSystem.API
- ✅ WelfareLink.ComplianceAndAudit.API
- ✅ WelfareLink.AnalyticsReport.API
- ✅ WelfareLink.Operations.API
- ✅ WelfareLink.BenefitEligiblity.API

## Helper Methods

```csharp
// Get specific claims
JwtClaimsHelper.GetUserId(User)        // int?
JwtClaimsHelper.GetUsername(User)      // string?
JwtClaimsHelper.GetRole(User)          // string?
JwtClaimsHelper.GetEmail(User)         // string?
JwtClaimsHelper.GetFullName(User)      // string?
JwtClaimsHelper.GetJti(User)           // string?

// Check roles
JwtClaimsHelper.HasRole(User, "Admin")
JwtClaimsHelper.HasAnyRole(User, "Admin", "Manager")
JwtClaimsHelper.HasAllRoles(User, "Admin", "Manager")

// Get all claims
JwtClaimsHelper.GetAllClaims(User)     // Dictionary<string, string>
```

## Security Checklist

- [ ] Update appsettings.json with strong secret
- [ ] Secret is same across all APIs
- [ ] Using HTTPS in production
- [ ] Secrets stored in Key Vault (not in code)
- [ ] Token expiry is reasonable (30-60 min)
- [ ] CORS configured for trusted origins only
- [ ] Logging implemented for auth failures
- [ ] Rate limiting configured
- [ ] Token refresh implemented (optional)

## Common Issues

**401 on all requests:**
- Check JWT secret is identical in all projects
- Verify token sent in header: `Authorization: Bearer {token}`
- Check token hasn't expired

**500 error on login:**
- Check database connection string
- Verify user exists in database
- Check user is marked as active

**Token validation failed:**
- Verify Issuer/Audience match config
- Check system time is synchronized
- Look at specific error message

## Files Created
- `JwtConfiguration.cs` in each API project
- `JwtClaimsHelper.cs` for extracting claims
- `ExampleProtectedController.cs` for usage examples
- Implementation guides and this quick reference
