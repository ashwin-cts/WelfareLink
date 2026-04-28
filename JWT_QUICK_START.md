# JWT Authentication - Quick Start Guide

## 🚀 Getting Started

### 1. Start the Services

Start all API projects:
```bash
# Terminal 1 - Authentication API (Port 7200)
cd WelfareLink.Authentication.API
dotnet run

# Terminal 2 - UserManagement API (Port 7203)
cd WelfareLink.UserManagement.API
dotnet run

# Terminal 3+ - Other APIs as needed
cd WelfareLink.Operations.API
dotnet run
```

### 2. Get a JWT Token

**Using Postman or curl:**

```bash
curl -X POST https://localhost:7200/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "password123",
    "userType": "Admin"
  }'
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "username": "admin",
  "role": "Admin",
  "fullName": "Administrator",
  "expiryTime": "2026-03-27T12:00:00Z"
}
```

### 3. Use the Token

Copy the token and add it to your request headers:

```bash
curl -X GET https://localhost:7203/api/user/1 \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

### 4. Protect Your Endpoints

Add the `[Authorize]` attribute to your controllers:

```csharp
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
public class ResourceController : ControllerBase
{
    [Authorize]
    [HttpGet("{id}")]
    public IActionResult GetResource(int id)
    {
        // Extract user info from token
        var userId = User.FindFirst("UserId")?.Value;
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        return Ok(new { resourceId = id, userId, userRole = role });
    }

    [Authorize(Roles = "Admin,ProgramManager")]
    [HttpDelete("{id}")]
    public IActionResult DeleteResource(int id)
    {
        // Only Admin or ProgramManager can delete
        return Ok(new { deleted = true });
    }
}
```

## 🧪 Testing with Postman

### Step 1: Create Login Request

- **Method:** POST
- **URL:** `https://localhost:7200/api/auth/login`
- **Headers:**
  - `Content-Type: application/json`
- **Body (JSON):**
```json
{
  "username": "admin",
  "password": "password123",
  "userType": "Admin"
}
```

### Step 2: Save Token as Variable

Click on the token response and use Postman's test script:

```javascript
var jsonData = pm.response.json();
pm.environment.set("jwt_token", jsonData.token);
```

### Step 3: Use Token in Protected Requests

Add to Headers tab:
- **Key:** `Authorization`
- **Value:** `Bearer {{jwt_token}}`

## 📋 User Types

Use these values in the `userType` field:
- `Citizen`
- `WelfareOfficer`
- `ProgramManager`
- `ComplianceOfficer`
- `GovernmentAuditor`
- `Admin`

## ⚙️ Configuration

### JWT Expiry

Edit `appsettings.json` to change token expiry:

```json
"JwtSettings": {
  "ExpiryMinutes": 120  // Change from 60 to 120 minutes
}
```

### Secret Key

⚠️ **Security:** Change the secret in production:

```json
"JwtSettings": {
  "Secret": "Your-Very-Long-And-Secure-Secret-Key-Here"
}
```

## 🔍 Debugging Token

1. Copy your JWT token
2. Go to [https://jwt.io](https://jwt.io)
3. Paste token in the "Encoded" section
4. View the decoded claims

## 🐛 Troubleshooting

### Problem: 401 Unauthorized on Protected Endpoint

**Check:**
- [ ] Token is included in Authorization header
- [ ] Format is `Bearer <token>` (note the space)
- [ ] Token has not expired
- [ ] Secret matches in appsettings.json

### Problem: Login Returns 500 Error

**Check:**
- [ ] UserManagement API is running
- [ ] Credentials exist in database
- [ ] No exceptions in UserManagement API logs

### Problem: Token Works on One API but Not Another

**Check:**
- [ ] All APIs have same JWT Secret in appsettings.json
- [ ] All APIs have same Issuer and Audience
- [ ] All APIs have JWT authentication configured in Program.cs

## 📚 Full Documentation

See these files for complete documentation:
- `JWT_IMPLEMENTATION_GUIDE.md` - Full implementation guide
- `JWT_IMPLEMENTATION_SUMMARY.md` - Complete summary
- `JWT_PROGRAM_CS_TEMPLATE.md` - Program.cs template

## 🎯 Next Steps

1. **Test Authentication API:** Login and get a token
2. **Test Validation:** Use token on protected endpoints
3. **Add Authorization:** Protect your endpoints with [Authorize]
4. **Implement Refresh:** Consider token refresh for long sessions
5. **Monitor:** Add logging for authentication failures

## 💡 Tips

- **Token Storage:** For web apps, store in secure cookies (HttpOnly)
- **Token Refresh:** Implement refresh tokens for better UX
- **Rate Limiting:** Add rate limiting on login endpoint
- **Monitoring:** Log all authentication failures for security
- **Testing:** Use Postman collections for automated testing

---

**Questions?** Refer to JWT_IMPLEMENTATION_GUIDE.md for detailed information.
