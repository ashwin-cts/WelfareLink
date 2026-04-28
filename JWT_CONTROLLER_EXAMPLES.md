# JWT Authorization Examples - Controller Implementations

This document provides practical examples of how to use JWT authentication in your controllers.

## Basic Protected Endpoint

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class CitizenController : ControllerBase
{
    private readonly ICitizenService _citizenService;
    private readonly ILogger<CitizenController> _logger;

    public CitizenController(ICitizenService citizenService, ILogger<CitizenController> logger)
    {
        _citizenService = citizenService;
        _logger = logger;
    }

    // Public endpoint - no authentication required
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] CitizenRegistration registration)
    {
        // Create new citizen account
        var citizen = await _citizenService.RegisterAsync(registration);
        return Ok(citizen);
    }

    // Protected endpoint - requires valid JWT token
    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCitizenDetails(int id)
    {
        // Extract current user info from token
        var userId = User.FindFirst("UserId")?.Value;

        _logger.LogInformation($"User {userId} accessing citizen {id}");

        var citizen = await _citizenService.GetCitizenAsync(id);

        if (citizen == null)
            return NotFound();

        return Ok(citizen);
    }

    // Protected endpoint - update own profile
    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] CitizenUpdate update)
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
        var username = User.FindFirst("Username")?.Value;

        _logger.LogInformation($"User {username} updating their profile");

        var updated = await _citizenService.UpdateProfileAsync(userId, update);
        return Ok(updated);
    }
}
```

## Role-Based Authorization

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class ApplicationController : ControllerBase
{
    private readonly IApplicationService _applicationService;

    public ApplicationController(IApplicationService applicationService)
    {
        _applicationService = applicationService;
    }

    // Any authenticated user can view their own applications
    [Authorize]
    [HttpGet("my-applications")]
    public async Task<IActionResult> GetMyApplications()
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
        var applications = await _applicationService.GetUserApplicationsAsync(userId);
        return Ok(applications);
    }

    // Only WelfareOfficers and ProgramManagers can approve applications
    [Authorize(Roles = "WelfareOfficer,ProgramManager")]
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> ApproveApplication(int id, [FromBody] ApprovalReason reason)
    {
        var officerId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
        var officerName = User.FindFirst("Username")?.Value;

        var result = await _applicationService.ApproveAsync(id, officerId, reason);
        return Ok(result);
    }

    // Only Compliance Officers can flag applications for review
    [Authorize(Roles = "ComplianceOfficer")]
    [HttpPost("{id}/flag-for-review")]
    public async Task<IActionResult> FlagForComplianceReview(int id, [FromBody] ComplianceNote note)
    {
        var complianceOfficerId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await _applicationService.FlagForReviewAsync(id, complianceOfficerId, note);
        return Ok(result);
    }

    // Only Admins can delete applications
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteApplication(int id)
    {
        var adminId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        await _applicationService.DeleteAsync(id);
        return NoContent();
    }
}
```

## Multiple Role Support

```csharp
[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    // Multiple roles can access this endpoint
    [Authorize(Roles = "Admin,ProgramManager,GovernmentAuditor")]
    [HttpGet("analytics")]
    public async Task<IActionResult> GetAnalyticsReport()
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        var report = await _reportService.GenerateAnalyticsAsync();
        return Ok(report);
    }

    // Only Government Auditors can access audit logs
    [Authorize(Roles = "GovernmentAuditor")]
    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogs()
    {
        var logs = await _reportService.GetAuditLogsAsync();
        return Ok(logs);
    }
}
```

## Conditional Logic Based on Roles

```csharp
[ApiController]
[Route("api/[controller]")]
public class BenefitController : ControllerBase
{
    private readonly IBenefitService _benefitService;

    public BenefitController(IBenefitService benefitService)
    {
        _benefitService = benefitService;
    }

    // Same endpoint, different data based on role
    [Authorize]
    [HttpGet("summary")]
    public async Task<IActionResult> GetBenefitSummary()
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        if (userRole == "Admin" || userRole == "ProgramManager")
        {
            // Admins/ProgramManagers see all benefits
            var allBenefits = await _benefitService.GetAllBenefitsAsync();
            return Ok(allBenefits);
        }
        else if (userRole == "Citizen")
        {
            // Citizens see only their benefits
            var userBenefits = await _benefitService.GetUserBenefitsAsync(userId);
            return Ok(userBenefits);
        }
        else if (userRole == "WelfareOfficer")
        {
            // Officers see benefits for their assigned citizens
            var assignedBenefits = await _benefitService.GetAssignedBenefitsAsync(userId);
            return Ok(assignedBenefits);
        }

        return Unauthorized();
    }
}
```

## Extracting User Claims

```csharp
[ApiController]
[Route("api/[controller]")]
public class UserInfoController : ControllerBase
{
    [Authorize]
    [HttpGet("my-info")]
    public IActionResult GetMyInfo()
    {
        // Extract all user information from JWT claims
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

    // Alternative: Using User.Identity
    [Authorize]
    [HttpGet("current-user")]
    public IActionResult GetCurrentUser()
    {
        var userName = User.Identity?.Name;
        var isAuthenticated = User.Identity?.IsAuthenticated;

        var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();

        return Ok(new
        {
            userName,
            isAuthenticated,
            claims
        });
    }
}
```

## Handling Missing or Invalid Tokens

```csharp
[ApiController]
[Route("api/[controller]")]
public class SecureController : ControllerBase
{
    [Authorize]
    [HttpGet]
    public IActionResult GetSecureData()
    {
        // If we reach here, token is valid
        // If token is missing or invalid, ASP.NET Core returns 401 automatically

        return Ok(new { message = "This is secure data" });
    }
}
```

Response when accessing without token:
```json
{
    "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
    "title": "Unauthorized",
    "status": 401,
    "traceId": "..."
}
```

## Custom Authorization Policy

```csharp
// In Program.cs
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("MustBeCitizen", policy =>
        policy.RequireRole("Citizen"))
    .AddPolicy("MustBeOfficer", policy =>
        policy.RequireRole("WelfareOfficer", "ProgramManager"))
    .AddPolicy("MustBeAuditor", policy =>
        policy.RequireRole("GovernmentAuditor", "Admin"));

// In Controller
[ApiController]
[Route("api/[controller]")]
public class ProgramController : ControllerBase
{
    [Authorize(Policy = "MustBeOfficer")]
    [HttpPost("enroll")]
    public async Task<IActionResult> EnrollCitizen([FromBody] CitizenEnrollment enrollment)
    {
        // Only WelfareOfficer or ProgramManager can enroll citizens
        return Ok();
    }

    [Authorize(Policy = "MustBeAuditor")]
    [HttpGet("audit-report")]
    public async Task<IActionResult> GetAuditReport()
    {
        // Only Auditors and Admins can access
        return Ok();
    }
}
```

## Logging User Actions

```csharp
[ApiController]
[Route("api/[controller]")]
public class DisbursementController : ControllerBase
{
    private readonly IDisbursementService _disbursementService;
    private readonly IAuditLogger _auditLogger;
    private readonly ILogger<DisbursementController> _logger;

    public DisbursementController(
        IDisbursementService disbursementService,
        IAuditLogger auditLogger,
        ILogger<DisbursementController> logger)
    {
        _disbursementService = disbursementService;
        _auditLogger = auditLogger;
        _logger = logger;
    }

    [Authorize(Roles = "WelfareOfficer,ProgramManager,Admin")]
    [HttpPost("process")]
    public async Task<IActionResult> ProcessDisbursement([FromBody] DisbursementRequest request)
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
        var username = User.FindFirst("Username")?.Value;
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        _logger.LogInformation($"User {username} ({role}) processing disbursement");

        try
        {
            var result = await _disbursementService.ProcessAsync(request);

            // Log successful action
            await _auditLogger.LogActionAsync(
                userId: userId,
                action: "Process Disbursement",
                resourceId: result.DisbursementId,
                status: "Success"
            );

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error processing disbursement: {ex.Message}");

            // Log failed action
            await _auditLogger.LogActionAsync(
                userId: userId,
                action: "Process Disbursement",
                resourceId: null,
                status: "Failed",
                details: ex.Message
            );

            return BadRequest(new { error = ex.Message });
        }
    }
}
```

## Combining Multiple Authorization Requirements

```csharp
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    // Combine role-based and policy-based authorization
    [Authorize(Roles = "Admin")]
    [Authorize(Policy = "MustHaveFullAccess")]
    [HttpDelete("user/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        // Must be Admin AND pass MustHaveFullAccess policy
        return Ok();
    }
}
```

## Best Practices Demonstrated

1. ✅ **Always extract user ID:** Use `User.FindFirst("UserId")?.Value`
2. ✅ **Verify ownership:** Check if user ID matches resource owner
3. ✅ **Log actions:** Record who did what and when
4. ✅ **Handle missing claims:** Use null coalescing operator (??)
5. ✅ **Use appropriate roles:** Match roles to actual permissions
6. ✅ **Test authorization:** Test with different role combinations
7. ✅ **Return appropriate codes:** 401 for auth, 403 for authorization failure

---

**Related Files:**
- JWT_IMPLEMENTATION_GUIDE.md
- JWT_QUICK_START.md
- JWT_IMPLEMENTATION_SUMMARY.md
