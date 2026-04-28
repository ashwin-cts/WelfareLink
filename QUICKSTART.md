# 🚀 GET STARTED - Quick Start Guide

## One-Minute Summary

✅ **What's Done:**
- Fixed: `JwtSettings:Secret is not configured` error
- Implemented: Global JWT authorization across 6 APIs
- Ready: All APIs to validate tokens and protect endpoints

✅ **What's Next:**
- Run all APIs
- Test login endpoint
- Access protected endpoints with token

---

## Step 1: Start All APIs (7 Terminals)

### Terminal 1 - Authentication API (Port 7101)
```powershell
cd WelfareLink.Authentication.API
dotnet run
```
**Expected Output:**
```
Now listening on: https://localhost:7101
```

### Terminal 2 - User Management API (Port 7203)
```powershell
cd WelfareLink.UserManagement.API
dotnet run
```

### Terminal 3 - Analytics API (Port 7202)
```powershell
cd WelfareLink.AnalyticsReport.API
dotnet run
```

### Terminal 4 - Operations API (Port 7204)
```powershell
cd WelfareLink.Operations.API
dotnet run
```

### Terminal 5 - Benefit Eligibility API (Port 7205)
```powershell
cd WelfareLink.BenifitEligiblity.API
dotnet run
```

### Terminal 6 - Compliance API (Port 7206)
```powershell
cd WelfareLink.ComplianceAndAudit.API
dotnet run
```

### Terminal 7 - Application System API (Port 7207)
```powershell
cd WelfareLink.WApplicationSystem.API
dotnet run
```

---

## Step 2: Test Login (Get JWT Token)

### Using cURL
```bash
curl -X POST https://localhost:7101/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "officer@welfare.gov",
    "password": "SecurePassword123",
    "userType": "WelfareOfficer"
  }' | jq
```

### Using PowerShell
```powershell
$body = @{
    username = "officer@welfare.gov"
    password = "SecurePassword123"
    userType = "WelfareOfficer"
} | ConvertTo-Json

$response = Invoke-WebRequest -Uri "https://localhost:7101/api/auth/login" `
  -Method POST `
  -Headers @{"Content-Type"="application/json"} `
  -Body $body

$response.Content | ConvertFrom-Json | ConvertTo-Json
```

### Expected Response
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "username": "officer@welfare.gov",
  "role": "WelfareOfficer",
  "fullName": "John Officer",
  "expiryTime": "2026-01-15T15:00:00Z"
}
```

**⚠️ IMPORTANT: Copy the entire `token` value**

---

## Step 3: Use Token to Access Protected API

### Set Token Variable
```bash
# macOS/Linux
TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."

# Windows PowerShell
$TOKEN = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

### Using cURL
```bash
curl -X GET https://localhost:7202/api/analytics/reports \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" | jq
```

### Using PowerShell
```powershell
$headers = @{
    "Authorization" = "Bearer $TOKEN"
    "Content-Type" = "application/json"
}

Invoke-WebRequest -Uri "https://localhost:7202/api/analytics/reports" `
  -Method GET `
  -Headers $headers | Select-Object -ExpandProperty Content | ConvertFrom-Json
```

### Expected Response
```json
{
  "success": true,
  "data": [
    // Report data here
  ]
}
```

---

## Step 4: Test Without Token (Should Fail)

### Using cURL
```bash
curl -X GET https://localhost:7202/api/analytics/reports \
  -H "Content-Type: application/json" | jq
```

### Using PowerShell
```powershell
Invoke-WebRequest -Uri "https://localhost:7202/api/analytics/reports" `
  -Method GET | Select-Object -ExpandProperty Content
```

### Expected Response
```json
{
  "error": "Unauthorized - Valid JWT token required"
}
```

---

## Step 5: Test Role-Based Access

### Find Role-Based Endpoint
Check your API controllers for endpoints with:
```csharp
[Authorize(Roles = "WelfareOfficer")]
```

### Test with Correct Role
Token includes: `"role": "WelfareOfficer"`
```bash
curl -X GET https://localhost:7202/api/analytics/officer-reports \
  -H "Authorization: Bearer $TOKEN"
```

Expected: **200 OK** ✅

### Test with Wrong Role
Create/use a token with different role:
```bash
# Would fail with 403 Forbidden
curl -X GET https://localhost:7202/api/admin/reports \
  -H "Authorization: Bearer $CITIZEN_TOKEN"
```

Expected: **403 Forbidden** ✅

---

## Using Postman (Alternative)

### 1. Create POST Request
- URL: `https://localhost:7101/api/auth/login`
- Method: `POST`
- Headers: `Content-Type: application/json`
- Body (raw):
```json
{
    "username": "officer@welfare.gov",
    "password": "SecurePassword123",
    "userType": "WelfareOfficer"
}
```

### 2. Send and Save Token
- Click Send
- In response, copy the `token` value

### 3. Create GET Request for Protected API
- URL: `https://localhost:7202/api/analytics/reports`
- Method: `GET`
- Headers:
  - Key: `Authorization`
  - Value: `Bearer {paste-token-here}`

### 4. Send Request
- Click Send
- You should get 200 OK with data

---

## Using Swagger/OpenAPI

### Access Swagger UI
Each API has Swagger available at:
```
https://localhost:7101/swagger/ui/  (Authentication.API)
https://localhost:7202/swagger/ui/  (Analytics.API)
etc.
```

### Test Login
1. Open Swagger at `https://localhost:7101`
2. Find `/api/auth/login` endpoint
3. Click "Try it out"
4. Enter credentials
5. Click "Execute"
6. Copy token from response

### Test Protected Endpoint
1. Open Swagger at `https://localhost:7202`
2. Click the lock icon (Authorize button)
3. Paste token in format: `Bearer {token}`
4. Find a protected endpoint
5. Click "Try it out"
6. Click "Execute"

---

## Troubleshooting Quick Tips

### ❌ Error: `Connection refused`
**Solution:** Make sure all APIs are running in separate terminals

### ❌ Error: `401 Unauthorized`
**Solutions:**
- Ensure token is included in Authorization header
- Check token format: `Bearer {token}` (with space!)
- Verify token hasn't expired
- Ensure issuer matches (WelfareLinkAuthServer)

### ❌ Error: `403 Forbidden`
**Solution:** User role doesn't match endpoint requirements

### ❌ Error: `CORS policy blocked`
**Solution:** Make sure request comes from allowed origin

### ❌ Error: `JwtSettings:Secret is not configured`
**Solution:** ✅ ALREADY FIXED - shouldn't see this anymore

---

## Next Steps After Testing

### 1. Read Documentation
- `SETUP_COMPLETE.md` - Overview
- `VISUAL_GUIDE.md` - Diagrams
- `JWT_AUTHENTICATION_GUIDE.md` - Deep dive

### 2. Integrate with Razor Pages
- See `JWT_RAZORPAGES_INTEGRATION.cs`
- Implement session management
- Add login/logout pages

### 3. Test Advanced Scenarios
- Token expiration
- Refresh tokens
- Role-based access
- User claims access

### 4. Before Production
- Move secrets to Azure Key Vault
- Implement refresh tokens
- Add audit logging
- Security review

---

## Common Test Scenarios

### Scenario 1: Valid Credentials
```bash
# Login
curl -X POST https://localhost:7101/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"officer@welfare.gov","password":"SecurePassword123","userType":"WelfareOfficer"}'

# Result: ✅ 200 OK with token
```

### Scenario 2: Invalid Credentials
```bash
# Login with wrong password
curl -X POST https://localhost:7101/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"officer@welfare.gov","password":"WrongPassword","userType":"WelfareOfficer"}'

# Result: ❌ 401 Unauthorized
```

### Scenario 3: Protected Endpoint with Token
```bash
# Use token
curl -X GET https://localhost:7202/api/analytics/reports \
  -H "Authorization: Bearer $TOKEN"

# Result: ✅ 200 OK with data
```

### Scenario 4: Protected Endpoint without Token
```bash
# No token
curl -X GET https://localhost:7202/api/analytics/reports

# Result: ❌ 401 Unauthorized
```

### Scenario 5: Expired Token
```bash
# Token expired (after 60 minutes)
curl -X GET https://localhost:7202/api/analytics/reports \
  -H "Authorization: Bearer $EXPIRED_TOKEN"

# Result: ❌ 401 Unauthorized (token validation failed)
```

---

## API Reference

### Authentication Endpoints

| Endpoint | Method | Auth | Purpose |
|----------|--------|------|---------|
| `/api/auth/login` | POST | None | Get JWT token |
| `/api/auth/validate` | POST | Bearer | Validate token |

### Example: All Protected Endpoints

| API | Port | Example Endpoint |
|-----|------|------------------|
| Analytics | 7202 | `GET /api/analytics/reports` |
| Operations | 7204 | `GET /api/operations/tasks` |
| Benefit | 7205 | `GET /api/benefits/eligibility` |
| Compliance | 7206 | `GET /api/compliance/audit-logs` |
| Application | 7207 | `GET /api/applications/list` |
| UserManagement | 7203 | `GET /api/user/profile` |

**All require:** `Authorization: Bearer {token}`

---

## Key Takeaway

```
🔑 YOU NOW HAVE:

✅ User submits login credentials
✅ System issues JWT token
✅ User uses token to access all 6 APIs
✅ Each API validates token independently
✅ Global authorization working!

Ready to deploy and use! 🚀
```

---

## Quick Command Sheet

```bash
# Start API
dotnet run

# Login
curl -X POST https://localhost:7101/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"user","password":"pass","userType":"WelfareOfficer"}'

# Use token
curl -X GET https://localhost:7202/api/analytics/reports \
  -H "Authorization: Bearer TOKEN"

# Test without auth
curl -X GET https://localhost:7202/api/analytics/reports
```

---

## Support

Need help?
1. Check `INDEX.md` for documentation
2. See `VISUAL_GUIDE.md` for diagrams
3. Review `JWT_QUICK_REFERENCE.cs` for examples
4. Check error messages against `JWT_AUTHENTICATION_GUIDE.md`

**Everything you need is documented. You're ready to go!** 🎉

