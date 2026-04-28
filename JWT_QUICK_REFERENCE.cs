/*
 * QUICK REFERENCE: JWT Authentication Configuration
 * 
 * This document provides a quick reference for JWT settings across all API projects
 */

// ============================================================================
// STANDARD JWT SETTINGS (Applied to all 6 API Projects)
// ============================================================================

{
  "JwtSettings": {
    "Secret": "MyApplication_Secret_Key_2026_Keep_It_Safe!!",
    "Issuer": "WelfareLinkAuthServer",
    "Audience": "WelfareLinkUsers",
    "ExpiryMinutes": 60
  }
}

// ============================================================================
// API PROJECTS CONFIGURED
// ============================================================================

API Projects with JWT enabled:
  ✅ WelfareLink.Authentication.API      (Login/Token Generation)
  ✅ WelfareLink.UserManagement.API      (User Validation)
  ✅ WelfareLink.BenifitEligiblity.API   (Benefit Operations)
  ✅ WelfareLink.AnalyticsReport.API     (Analytics/Reports)
  ✅ WelfareLink.ComplianceAndAudit.API  (Compliance & Audit)
  ✅ WelfareLink.Operations.API          (Operations)
  ✅ WelfareLink.WApplicationSystem.API  (Application System)

// ============================================================================
// AUTHENTICATION FLOW ENDPOINTS
// ============================================================================

Authentication.API:
  POST   /api/auth/login        - User login (credentials) → JWT Token
  POST   /api/auth/validate     - Validate existing token

UserManagement.API:
  POST   /api/user/login        - Internal validation of user credentials

All Other APIs:
  [Protected] - All endpoints require valid JWT token in Authorization header

// ============================================================================
// TOKEN STRUCTURE (JWT Claims)
// ============================================================================

Header:
{
  "alg": "HS256",
  "typ": "JWT"
}

Payload (Claims):
{
  "sub": "123",                           // Subject (User ID)
  "unique_name": "officer@welfare.gov",   // Username
  "UserId": "123",                        // Custom: User ID
  "Username": "officer@welfare.gov",      // Custom: Username
  "role": "WelfareOfficer",               // Role
  "FullName": "John Officer",             // Full Name
  "Email": "john@welfare.gov",            // Email
  "jti": "abc-def-ghi",                   // JWT ID
  "exp": 1672531200,                      // Expiration timestamp
  "iss": "WelfareLinkAuthServer",          // Issuer
  "aud": "WelfareLinkUsers"                // Audience
}

Signature:
  HMACSHA256(base64UrlEncode(header) + "." + base64UrlEncode(payload), secret)

// ============================================================================
// USAGE PATTERNS
// ============================================================================

// 1. LOGIN - Get JWT Token
POST /api/auth/login
Content-Type: application/json

{
  "username": "officer@welfare.gov",
  "password": "SecurePassword123",
  "userType": "WelfareOfficer"
}

Response:
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "username": "officer@welfare.gov",
  "role": "WelfareOfficer",
  "fullName": "John Officer",
  "expiryTime": "2026-01-15T14:30:00Z"
}

// 2. API CALL - Use JWT Token
GET /api/analytics/reports
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

Response: 200 OK with data

// 3. INVALID TOKEN - No Authorization Header
GET /api/analytics/reports

Response: 401 Unauthorized
{
  "error": "Unauthorized - Valid JWT token required"
}

// ============================================================================
// CONTROLLER USAGE
// ============================================================================

// Require Authentication
[Authorize]
[HttpGet("protected")]
public IActionResult ProtectedEndpoint() { }

// Require Specific Role
[Authorize(Roles = "WelfareOfficer,Admin")]
[HttpGet("admin-only")]
public IActionResult AdminOnly() { }

// Allow Anonymous
[AllowAnonymous]
[HttpGet("public")]
public IActionResult PublicEndpoint() { }

// Access User Claims
[Authorize]
[HttpGet("my-data")]
public IActionResult GetMyData()
{
  var userId = User.FindFirst("UserId")?.Value;
  var username = User.FindFirst("Username")?.Value;
  var role = User.FindFirst(ClaimTypes.Role)?.Value;
  var email = User.FindFirst("Email")?.Value;

  return Ok(new { userId, username, role, email });
}

// ============================================================================
// CONFIGURATION FILES
// ============================================================================

All Projects:
  appsettings.json              - Development configuration
  appsettings.Development.json  - Dev-specific overrides
  appsettings.Production.json   - Production configuration (use Key Vault)

Middleware Stack (Program.cs):
  1. app.UseHttpsRedirection()
  2. app.UseCors()
  3. app.UseAuthentication()           ← JWT token validation
  4. app.UseAuthorization()            ← Role/Policy enforcement
  5. app.MapControllers()

// ============================================================================
// SECURITY CONSIDERATIONS
// ============================================================================

Token Expiry:        60 minutes (configurable)
Algorithm:           HMACSHA256
Validation:          Signature, Issuer, Audience, Lifetime
Storage (Client):    localStorage (XSS risk) or httpOnly cookie (safer)
Transport:           HTTPS only
CORS:                Restricted to known origins

// ============================================================================
// PRODUCTION DEPLOYMENT
// ============================================================================

1. Move secret to Azure Key Vault:
   var keyVaultUrl = new Uri(config["KeyVault:Url"]);
   builder.Configuration.AddAzureKeyVault(keyVaultUrl, credential);

2. Increase token expiry for production scenarios:
   "ExpiryMinutes": 480  // 8 hours

3. Implement refresh token endpoint:
   POST /api/auth/refresh
   Body: { "token": "current-token" }

4. Enable token blacklisting for logout:
   POST /api/auth/logout
   Body: { "token": "current-token" }

5. Monitor token usage and failed validations in logs

// ============================================================================
// COMMON ISSUES & SOLUTIONS
// ============================================================================

Issue: JwtSettings:Secret is not configured
Fix:   Add "JwtSettings" section to appsettings.json

Issue: 401 Unauthorized on valid token
Fix:   Check token hasn't expired, issuer matches, audience matches

Issue: CORS policy blocked the request
Fix:   Verify origin in AddCors() policy, ensure CORS before MapControllers()

Issue: Claims not available in controller
Fix:   Use User.FindFirst("ClaimName")?.Value to retrieve custom claims

Issue: Token expires too quickly
Fix:   Increase "ExpiryMinutes" in JwtSettings, implement refresh tokens

// ============================================================================
// TESTING CHECKLIST
// ============================================================================

[ ] Start all API projects
[ ] Test login endpoint with valid credentials
[ ] Test login endpoint with invalid credentials
[ ] Copy token from login response
[ ] Test protected endpoint with token in Authorization header
[ ] Test protected endpoint without token (should get 401)
[ ] Test protected endpoint with expired token
[ ] Test role-based endpoint with correct role
[ ] Test role-based endpoint with wrong role (should get 403)
[ ] Test [AllowAnonymous] endpoint without token
[ ] Verify claims are accessible in controller
[ ] Test CORS from different origin
[ ] Verify token validation logs in output

// ============================================================================
// REFERENCES
// ============================================================================

Files Modified:
  ✅ WelfareLink.AnalyticsReport.API/appsettings.json
  ✅ WelfareLink.BenifitEligiblity.API/appsettings.json
  ✅ WelfareLink.ComplianceAndAudit.API/appsettings.json
  ✅ WelfareLink.Operations.API/appsettings.json
  ✅ WelfareLink.WApplicationSystem.API/appsettings.json
  ✅ WelfareLink.UserManagement.API/appsettings.json (already had JwtSettings)

Existing Files (No changes needed):
  - WelfareLink.Authentication.API/Program.cs
  - WelfareLink.Authentication.API/Services/JwtService.cs
  - WelfareLink.Authentication.API/Services/AuthService.cs
  - WelfareLink.Authentication.API/Models/AuthModels.cs
  - WelfareLink.AnalyticsReport.API/Configuration/JwtConfiguration.cs
  - All other API JwtConfiguration files

*/
