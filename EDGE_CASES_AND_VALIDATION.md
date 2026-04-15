# Edge Cases & Validation Checklist

## 🔍 Compliance Engine Edge Cases

### Max Benefit Validation
- ✅ If `MaxBenefitPerCitizen = 0`, check is disabled (default)
- ✅ Handles currency precision (decimal calculations)
- ✅ Excludes "Failed" and "Cancelled" benefits from calculation
- ✅ Prevents duplicate compliance records for same violation
- ✅ Tracks which benefits caused the violation

**Edge Cases to Handle**:
- Multiple applications for same citizen in same program → Sum all benefits
- Partially disbursed benefits → Count full allocation amount
- Benefit status transitions → Re-check on status change
- Program changes to MaxBenefitPerCitizen → Re-validate existing benefits

### Disbursement Delay Check
- ✅ Checks benefits created > 2 days ago in "Pending" or "InProgress" status
- ✅ Marks as CRITICAL priority
- ✅ Prevents duplicate flagging

**Edge Cases to Handle**:
- What if benefit created 2.5 days ago but completed now → Should not flag
- Multiple disbursements with different dates → Use earliest date
- Zero amount benefits → Should not be flagged
- Weekend/holiday considerations → May need configurable delay period

---

## 🗃️ Database & Data Integrity

### Circular References (Already Fixed ✅)
- ✅ Benefit ↔ Disbursement → [JsonIgnore] on both directions
- ✅ Benefit ↔ WelfareApplication → [JsonIgnore] on collections
- ✅ Citizen ↔ CitizenDocument → [JsonIgnore] on both
- ✅ WelfareProgram ↔ Resource → [JsonIgnore] on both
- ✅ WelfareProgram ↔ WelfareApplication → [JsonIgnore]
- ✅ WelfareApplication ↔ EligibilityCheck → [JsonIgnore]
- ✅ WelfareApplication ↔ Document → [JsonIgnore] on both

### Null Value Handling
- ✅ `EntityId` in AuditLog is now nullable
- ✅ `IPAddress`, `UserAgent`, `OldValue`, `NewValue` are nullable
- ✅ Specific IDs in ComplainceRecord (BenefitID, DisbursementID, etc.) are nullable
- ✅ Navigation properties use safe null coalescing

### Foreign Key Constraints
```sql
-- All properly configured with OnDelete behavior
ALTER TABLE ComplianceRecords
  ADD CONSTRAINT FK_ComplianceRecords_Users_RaisedByUserId
  FOREIGN KEY (RaisedByUserId) REFERENCES Users(UserId) ON DELETE NO ACTION;

ALTER TABLE ComplianceRecords
  ADD CONSTRAINT FK_ComplianceRecords_Users_ResolvedByUserId
  FOREIGN KEY (ResolvedByUserId) REFERENCES Users(UserId) ON DELETE NO ACTION;
```

---

## 🔐 Audit Logging Edge Cases

### User Action Logging
**Case 1: Account Creation**
- Logs: Admin/Manager creates new user account
- Captures: Username, role, initial status
- Risk: Account created with wrong role → Audit shows initial role

**Case 2: Account Deletion**
- Logs: Who deleted, when, which user
- Risk: Cannot re-create same username immediately (soft delete recommended)

**Case 3: Profile Edit**
- Logs: All field changes with before/after values
- Risk: Password field should NOT be logged in clear text
- Solution: Log only "Password Changed" without value

**Case 4: Benefit Allocation**
- Logs: Officer, citizen, amount, program, timestamp
- Risk: If amount is maliciously changed → Audit trail proves it
- Good for compliance

**Case 5: Disbursement Actions**
- Logs: Disbursement date, amount, status changes
- Risk: If partially reversed → Log shows complete history

### Special Logging Scenarios

**Batch Operations**:
- If multiple benefits allocated at once → Each logged separately
- If system auto-creates records → Log as "SYSTEM" or "AUTO"

**Failed Operations**:
- Log with Status = "Failed"
- Include error message in Description
- Example: `Status: Failed, Error: Insufficient funds`

---

## ⚠️ Data Validation Rules

### WelfareProgram
```csharp
// Rule 1: MaxBenefitPerCitizen must be >= 0
if (MaxBenefitPerCitizen < 0) throw new ValidationException();

// Rule 2: If MaxBenefitPerCitizen is set, must be <= Budget
if (MaxBenefitPerCitizen > Budget) 
    throw new ValidationException("Max benefit cannot exceed program budget");

// Rule 3: EndDate must be after StartDate
if (EndDate <= StartDate) throw new ValidationException();

// Rule 4: Budget must be positive
if (Budget <= 0) throw new ValidationException();
```

### Benefit
```csharp
// Rule 1: Amount must be positive
if (Amount <= 0) throw new ValidationException();

// Rule 2: Cannot add benefit if program MaxBenefitPerCitizen would be exceeded
decimal totalForCitizen = GetTotalBenefitsForCitizen(citizen, program);
if (totalForCitizen + Amount > program.MaxBenefitPerCitizen && program.MaxBenefitPerCitizen > 0)
    throw new ValidationException("Would exceed maximum benefit");

// Rule 3: Date should not be in future
if (Date > DateTime.UtcNow) throw new ValidationException();

// Rule 4: Application must exist and be approved
var app = GetApplication(ApplicationID);
if (app?.Status != "Approved") throw new ValidationException();
```

### Disbursement
```csharp
// Rule 1: Amount must not exceed benefit amount
var benefit = GetBenefit(BenefitID);
decimal totalDisbursed = GetTotalDisbursed(BenefitID);
if (totalDisbursed + Amount > benefit.Amount)
    throw new ValidationException("Disbursement exceeds allocation");

// Rule 2: Amount must be positive
if (Amount <= 0) throw new ValidationException();

// Rule 3: Date should not be before benefit date
if (Date < benefit.Date) throw new ValidationException();
```

### ComplainceRecord
```csharp
// Rule 1: Priority must be valid
var validPriorities = new[] { "Low", "Medium", "High", "Critical" };
if (!validPriorities.Contains(Priority))
    throw new ValidationException();

// Rule 2: ViolationType must be specified
if (string.IsNullOrWhiteSpace(ViolationType))
    throw new ValidationException();

// Rule 3: Description required
if (Description.Length < 10)
    throw new ValidationException("Description must be at least 10 characters");
```

---

## 🚨 Error Handling Scenarios

### Scenario 1: Compliance Check During Allocation
**Current Flow**:
```
1. User allocates benefit (Rs. 5000)
2. Service checks: Total for citizen = Rs. 6500, MaxBenefit = Rs. 5000
3. System should either:
   a) Reject allocation with error message
   b) Accept but auto-flag compliance issue
```
**Recommendation**: Option (a) - Prevent over-allocation upfront

### Scenario 2: Disbursement Delay Check Finds Delayed Items
**Current Flow**:
```
1. Background job runs daily at 2 AM
2. Finds 50 benefits delayed >2 days
3. Creates 50 compliance records
4. Should notify compliance officer
```
**Enhancement Needed**: Send email/notification to officer

### Scenario 3: User Deleted Before Audit Can Complete
**Current Flow**:
```
1. User performs action (Status = Success)
2. User is deleted
3. Audit log has UserId foreign key
```
**Protection**: FK has OnDelete.SetNull → Log remains but User = null
**Better**: Use username snapshot in description field

### Scenario 4: Concurrent Compliance Record Creation
**Current Flow**:
```
1. Thread A checks benefit and finds violation
2. Thread B checks SAME benefit simultaneously
3. Both create compliance records
```
**Solution**: Lock or check for existing record in transaction
**Current Implementation**: Checks existing record before creating ✅

---

## 🎯 Testing Scenarios

### Test Case 1: Max Benefit Exceeded
```csharp
[Test]
public async Task CheckMaxBenefitCompliance_ExceededAmount_CreatesComplianceRecord()
{
    // Arrange
    var program = CreateProgram(maxBenefit: 5000);
    var citizen = CreateCitizen();
    var app = CreateApplication(citizen, program);
    var benefit1 = CreateBenefit(app, 3000);
    var benefit2 = CreateBenefit(app, 2500); // This exceeds max

    // Act
    await _complianceService.CheckMaxBenefitComplianceAsync(benefit2.BenefitID);

    // Assert
    var record = await _context.ComplianceRecords
        .FirstAsync(c => c.BenefitID == benefit2.BenefitID);
    Assert.AreEqual("MaxBenefitExceeded", record.ViolationType);
    Assert.AreEqual("High", record.Priority);
}
```

### Test Case 2: Disbursement Delay Flagged
```csharp
[Test]
public async Task CheckDisbursementDelayCompliance_OlderThan2Days_FlagsCritical()
{
    // Arrange
    var benefit = CreateBenefit(amount: 1000);
    benefit.Date = DateTime.UtcNow.AddDays(-3); // 3 days old
    benefit.Status = "Pending";

    // Act
    await _complianceService.CheckDisbursementDelayComplianceAsync();

    // Assert
    var record = await _context.ComplianceRecords
        .FirstAsync(c => c.BenefitID == benefit.BenefitID);
    Assert.AreEqual("DisbursementDelayed", record.ViolationType);
    Assert.AreEqual("Critical", record.Priority);
}
```

### Test Case 3: Audit Log Captures Changes
```csharp
[Test]
public async Task LogUserAction_CapturesAllFields()
{
    // Act
    await _auditLogService.LogUserActionAsync(
        userId: 5,
        action: "UPDATE",
        entityType: "User",
        entityId: 10,
        description: "Profile updated",
        oldValue: "Status: Active",
        newValue: "Status: Inactive",
        ipAddress: "192.168.1.1",
        userAgent: "Mozilla/5.0..."
    );

    // Assert
    var log = await _context.AuditLogs
        .FirstAsync(l => l.EntityId == 10);
    Assert.AreEqual(5, log.UserId);
    Assert.AreEqual("Status: Active", log.OldValue);
    Assert.AreEqual("Status: Inactive", log.NewValue);
    Assert.AreEqual("192.168.1.1", log.IPAddress);
}
```

---

## 📱 API Response Edge Cases

### Empty Results
```json
GET /api/complianceofficerdashboard/issues
{
  "data": [],
  "message": "No open compliance issues found"
}
```

### Pagination Beyond Max
```json
GET /api/auditordashboard/system-logs?pageNumber=999&pageSize=50
{
  "logs": [],
  "pagination": {
    "totalRecords": 1250,
    "pageNumber": 999,
    "pageSize": 50,
    "totalPages": 25
  }
}
```

### Invalid Entity Type
```json
GET /api/auditordashboard/entity-changes/InvalidType/5
HTTP 404: Entity type not recognized
```

---

## ✨ Recommended Enhancements

1. **Configurable Delay Period**: Make 2-day check configurable via appsettings
2. **Email Notifications**: Alert officers of critical compliance issues
3. **Soft Deletes**: For audit trail preservation (don't physically delete)
4. **Batch Compliance Checks**: Schedule as background job
5. **Compliance Dashboard UI**: Real-time alerts with colors/icons
6. **Export Reports**: Generate PDF/Excel compliance reports
7. **Role-Based Filters**: Officers see only their raised issues
8. **Dashboard Caching**: Cache static metrics for performance
9. **Search & Filters**: Full-text search on compliance descriptions
10. **Remediation Tracking**: Track how issues were resolved
