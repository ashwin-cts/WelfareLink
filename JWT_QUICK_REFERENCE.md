# JWT Authentication - Quick Reference Card

## 🔐 Quick Reference Guide

### Login Endpoint
```
POST /api/auth/login
Content-Type: application/json

Request:
{
  "username": "john_doe",
  "password": "securePassword123",
  "userType": "Citizen"
}

Response (200 OK):
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "username": "john_doe",
  "role": "Citizen",
  "fullName": "John Doe",
  "expiryTime": "2026-03-27T11:30:00Z"
}

Error (401 Unauthorized):
{
  "error": "Invalid credentials or account is inactive"
}
```

---

### Using Token on Protected Endpoint
```
GET /api/resource/123
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

Success (200 OK):
{ /* resource data */ }

Error (401 Unauthorized):
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401
}
```

---

## 🛡️ Protecting Endpoints

### Basic Protection (Any Authenticated User)
```csharp
[Authorize]
[HttpGet("{id}")]
public IActionResult GetResource(int id)
{
    var userId = User.FindFirst("UserId")?.Value;
    return Ok(new { userId });
}
```

### Role-Based Protection
```csharp
[Authorize(Roles = "Admin")]
[HttpDelete("{id}")]
public IActionResult DeleteResource(int id)
{
    return Ok();
}

// Multiple roles
[Authorize(Roles = "Admin,ProgramManager")]
[HttpPut("{id}")]
public IActionResult UpdateResource(int id, [FromBody] Resource data)
{
    return Ok();
}
```

---

## 📋 User Roles

| Role | Value | Use Case |
|------|-------|----------|
| Citizens | `Citizen` | End users |
| Officers | `WelfareOfficer` | Process applications |
| Managers | `ProgramManager` | Program oversight |
| Compliance | `ComplianceOfficer` | Compliance review |
| Auditors | `GovernmentAuditor` | External audit |
| System | `Admin` | Full access |

---

## 🔑 Extracting User Info

```csharp
// Get single claim
var userId = User.FindFirst("UserId")?.Value;
var username = User.FindFirst("Username")?.Value;
var role = User.FindFirst(ClaimTypes.Role)?.Value;
var fullName = User.FindFirst("FullName")?.Value;
var email = User.FindFirst("Email")?.Value;

// Get all claims
var allClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();

// Check if authenticated
var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
```

---

## ⚙️ Configuration (appsettings.json)

```json
{
  "JwtSettings": {
    "Secret": "MyApplication_Secret_Key_2026_Keep_It_Safe!!",
    "Issuer": "WelfareLinkAuthServer",
    "Audience": "WelfareLinkUsers",
    "ExpiryMinutes": 60
  }
}
```

---

## 🚀 Testing with cURL

### Get Token
```bash
curl -X POST https://localhost:7200/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "password123",
    "userType": "Admin"
  }' \
  --insecure
```

### Use Token
```bash
TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."

curl -X GET https://localhost:7203/api/user/1 \
  -H "Authorization: Bearer $TOKEN" \
  --insecure
```

### Without Token (Should Fail)
```bash
curl -X GET https://localhost:7203/api/user/1 \
  --insecure
# Returns 401 Unauthorized
```

---

## 🎯 Authorization Matrix

| Endpoint | Public | Citizen | Officer | Manager | Auditor | Admin |
|----------|:------:|:-------:|:-------:|:-------:|:-------:|:-----:|
| POST /login | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| GET /profile | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| GET /my-benefits | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| POST /apply | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| GET /all-benefits | ❌ | ❌ | ✅ | ✅ | ✅ | ✅ |
| POST /approve | ❌ | ❌ | ✅ | ✅ | ❌ | ✅ |
| DELETE /user | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ |

---

## 🔍 Debugging Token

1. Copy token from response
2. Go to [https://jwt.io](https://jwt.io)
3. Paste in "Encoded" section
4. View decoded claims

**Token Structure:**
```
Header.Payload.Signature

Header: {
  "alg": "HS256",
  "typ": "JWT"
}

Payload: {
  "sub": "1",
  "UserId": "1",
  "Username": "john_doe",
  "role": "Citizen",
  "exp": 1743209400
}

Signature: HmacSHA256(base64(header) + "." + base64(payload), secret)
```

---

## ⚡ Performance

| Operation | Time | Notes |
|-----------|------|-------|
| Login | 50-100ms | Includes credential validation |
| Token Generation | 5-10ms | Cryptographic operation |
| Token Validation | <5ms | Signature verification only |
| Authorization Check | <1ms | No DB calls |

---

## 🐛 Common Issues

| Issue | Cause | Solution |
|-------|-------|----------|
| 401 Unauthorized | Missing/invalid token | Add `Authorization: Bearer <token>` header |
| 403 Forbidden | Insufficient role | Use authorized user role |
| 500 on Login | UserManagement API down | Check UserManagement API is running |
| Token mismatch | Different secrets | Ensure same JWT Secret in all configs |
| Expired token | > ExpiryMinutes passed | Get new token from login endpoint |

---

## 📚 Complete Documentation

- **JWT_QUICK_START.md** - 5-minute setup
- **JWT_IMPLEMENTATION_GUIDE.md** - Complete reference
- **JWT_CONTROLLER_EXAMPLES.md** - Code examples
- **JWT_IMPLEMENTATION_SUMMARY.md** - Full summary
- **JWT_IMPLEMENTATION_STATUS.md** - Current status

---

## 🚀 Quick Start (3 Steps)

1. **Start APIs**
   ```bash
   cd WelfareLink.Authentication.API && dotnet run
   cd WelfareLink.UserManagement.API && dotnet run
   ```

2. **Get Token**
   ```bash
   curl -X POST https://localhost:7200/api/auth/login \
     -H "Content-Type: application/json" \
     -d '{"username":"admin","password":"password123","userType":"Admin"}'
   ```

3. **Use Token**
   ```bash
   curl -H "Authorization: Bearer <TOKEN>" \
     https://localhost:7203/api/user/1
   ```

---

## 📖 API Ports

| Service | Port | Endpoint |
|---------|------|----------|
| Authentication API | 7200 | `https://localhost:7200/api/auth` |
| UserManagement API | 7203 | `https://localhost:7203/api/user` |
| Operations API | 7114 | `https://localhost:7114/api/*` |
| Benefits API | 7029 | `https://localhost:7029/api/*` |
| Application API | 7143 | `https://localhost:7143/api/*` |
| Analytics API | 7129 | `https://localhost:7129/api/*` |
| Compliance API | 7255 | `https://localhost:7255/api/*` |

---

## ✅ Checklist

Before deploying:
- [ ] All APIs running
- [ ] Login endpoint returns token
- [ ] Token validates on protected endpoints
- [ ] Role-based authorization working
- [ ] Error handling in place
- [ ] Monitoring configured
- [ ] Secrets secured
- [ ] Team trained

---

**Print this card for quick reference during development!**
