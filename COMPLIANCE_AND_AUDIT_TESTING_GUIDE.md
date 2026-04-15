# WelfareLink Compliance & Audit System - Testing Guide

## 🧪 Quick Testing Guide

### Prerequisites
- WelfareLinkApi running on `https://localhost:7210` (or configured URL)
- Postman or similar API testing tool
- Sample data with:
  - At least 1 Welfare Program with `MaxBenefitPerCitizen` set
  - At least 1 Citizen
  - At least 1 Welfare Application

---

## Test Case 1: Auto-Detect Max Benefit Violation

### Setup
1. Create a Program with `MaxBenefitPerCitizen = 5000`
2. Create a Welfare Application for Citizen
3. Get ApplicationID and CitizenID

### Test Steps

**Step 1: Allocate First Benefit (Within Limit)**
```bash
POST /api/BenefitApiController
{
  "applicationID": 1,
  "type": "Cash",
  "amount": 3000,
  "date": "2025-03-26",
  "status": "Allocated"
}
```

**Step 2: Check Compliance (Should be Clean)**
```bash
GET /api/ComplianceOfficerDashboardApi/issues
```
Expected: No issues for this citizen

**Step 3: Allocate Second Benefit (Will Exceed Limit: 3000 + 3000 = 6000 > 5000)**
```bash
POST /api/BenefitApiController
{
  "applicationID": 1,
  "type": "Cash",
  "amount": 3000,
  "date": "2025-03-26",
  "status": "Allocated"
}
```

**Step 4: Check Compliance (Should Show Violation)**
```bash
GET /api/ComplianceOfficerDashboardApi/issues/filtered?violationType=MaxBenefitExceeded
```
Expected Response:
```json
{
  "success": true,
  "count": 1,
  "data": [
    {
      "recordID": 1,
      "violationType": "MaxBenefitExceeded",
      "priority": "High",
      "status": "Open",
      "description": "Citizen 5 total benefit (Rs. 6000) exceeds max allowed (Rs. 5000) in program Education Support"
    }
  ]
}
```

---

## Test Case 2: Compliance Officer Filters Issues

### Test Steps

**Get Critical Issues Only**
```bash
GET /api/ComplianceOfficerDashboardApi/issues/filtered?priority=Critical&status=Open
```

**Get Max Benefit Violations**
```bash
GET /api/ComplianceOfficerDashboardApi/issues/filtered?violationType=MaxBenefitExceeded
```

**Get Issues for Specific Citizen**
```bash
GET /api/ComplianceOfficerDashboardApi/issues/filtered?citizenID=5
```

**Get Compliance History**
```bash
GET /api/ComplianceOfficerDashboardApi/history?citizenID=5
```

---

## Test Case 3: Get Pending Items

### Test Steps

**Get Pending Benefits**
```bash
GET /api/ComplianceOfficerDashboardApi/pending-benefits
```
Expected: Benefits in "Pending" or "InProgress" status

**Get Pending Disbursements**
```bash
GET /api/ComplianceOfficerDashboardApi/pending-disbursements
```
Expected: Disbursements in "Pending" or "InProgress" status

---

## Test Case 4: Compliance Officer Resolves an Issue

### Test Steps

**Step 1: Identify an Open Issue**
```bash
GET /api/ComplianceOfficerDashboardApi/issues
```
Note the `recordID` of an issue, e.g., recordID = 1

**Step 2: Resolve the Issue**
```bash
POST /api/ComplianceOfficerDashboardApi/resolve/1
Content-Type: application/json

{
  "resolvedByUserId": 5,
  "notes": "Data verified. Exception approved by Program Director."
}
```

**Step 3: Verify Resolution**
```bash
GET /api/ComplianceOfficerDashboardApi/history?benefitID=1
```
Expected: Record shows `status: "Resolved"` with notes and timestamp

---

## Test Case 5: Compliance Officer Flags Officer

### Test Steps

**Flag a Welfare Officer**
```bash
POST /api/ComplianceOfficerDashboardApi/flag-officer
Content-Type: application/json

{
  "officerID": 3,
  "complianceRecordID": null,
  "reason": "Repeated violations of max benefit allocation policy",
  "flaggedByUserId": 5
}
```

**Verify Flag**
```bash
GET /api/ComplianceOfficerDashboardApi/issues/filtered?violationType=OfficerFlagged
```

---

## Test Case 6: Auditor Views Budget Tracking

### Test Steps

**Get Program-Specific Audit Report**
```bash
GET /api/AuditorDashboardApi/program-report/1
```
Expected Response:
```json
{
  "success": true,
  "data": {
    "programID": 1,
    "programTitle": "Education Support",
    "totalBudget": 100000,
    "allocatedBenefits": 45000,
    "disbursedAmount": 42000,
    "remainingBudget": 55000,
    "budgetUtilizationPercentage": 45.0,
    "pendingBenefitsCount": 3,
    "pendingDisbursementsCount": 2,
    "totalApplications": 20,
    "approvedApplications": 18
  }
}
```

**Get Comprehensive Budget Tracking**
```bash
GET /api/AuditorDashboardApi/budget-tracking-enhanced
```

**Get Money Flow Analysis**
```bash
GET /api/AuditorDashboardApi/money-flow/1
```

---

## Test Case 7: Auditor Manages Resources

### Test Steps

**View Pending Resources**
```bash
GET /api/AuditorDashboardApi/pending-resources
```

**Approve a Resource**
```bash
POST /api/AuditorDashboardApi/approve-resource/3
Content-Type: application/json

{
  "notes": "Verified with program officer. Adequate for beneficiary count."
}
```

**Flag Resource as Insufficient**
```bash
POST /api/AuditorDashboardApi/flag-resource/3
Content-Type: application/json

{
  "reason": "Only 50 resources for 200 expected beneficiaries. Recommend increasing allocation."
}
```

---

## Test Case 8: Auditor Views Audit Findings

### Test Steps

**Get Open Findings**
```bash
GET /api/AuditorDashboardApi/open-audit-findings
```

**Get Program-Specific Findings**
```bash
GET /api/AuditorDashboardApi/open-audit-findings?programID=1
```

**Close a Finding**
```bash
POST /api/AuditorDashboardApi/close-audit-finding/5
Content-Type: application/json

{
  "resolutionNotes": "Resource allocation increased from 50 to 200 units. Verified with beneficiary list."
}
```

---

## Test Case 9: Audit Trail Tracking

### Test Steps

**Get Program Audit Trail**
```bash
GET /api/AuditorDashboardApi/program-audit-trail/1
```

**Get Audit Trail with Date Range**
```bash
GET /api/AuditorDashboardApi/program-audit-trail/1?from=2025-01-01T00:00:00Z&to=2025-03-31T23:59:59Z
```

**Get Activity Summary (Last 30 Days)**
```bash
GET /api/AuditorDashboardApi/activity-summary
```

**Get Custom Date Range**
```bash
GET /api/AuditorDashboardApi/activity-summary?from=2025-03-01T00:00:00Z&to=2025-03-26T23:59:59Z
```

---

## Test Case 10: Dashboard Summaries

### Test Steps

**Compliance Officer Dashboard Summary**
```bash
GET /api/ComplianceOfficerDashboardApi/dashboard-summary
```
Expected:
```json
{
  "success": true,
  "data": {
    "totalOpenIssues": 5,
    "criticalIssues": 1,
    "highPriorityIssues": 2,
    "maxBenefitViolations": 3,
    "disbursementDelays": 1,
    "pendingBenefitsCount": 8,
    "overdueBenefits": 2,
    "pendingDisbursementsCount": 5,
    "overdueDisbursements": 1,
    "flaggedOfficers": 1
  }
}
```

**Auditor Dashboard Summary**
```bash
GET /api/AuditorDashboardApi/dashboard-summary-enhanced
```

**Programs Overview**
```bash
GET /api/AuditorDashboardApi/programs-overview
```

---

## Test Case 11: Disbursement Delay Detection

### Test Steps

**Create a Benefit Dated 3+ Days Ago**
```bash
POST /api/BenefitApiController
{
  "applicationID": 1,
  "type": "Cash",
  "amount": 2000,
  "date": "2025-03-23",
  "status": "Pending"
}
```

**Run Compliance Check (Manually or Wait for Scheduled Job)**
```bash
POST /api/ComplianceOfficerDashboardApi/check-all
```

**Verify Issue Created**
```bash
GET /api/ComplianceOfficerDashboardApi/issues/filtered?violationType=DisbursementDelayed
```
Expected: Issue with `priority: "Critical"`

---

## Expected Test Results Summary

| Test Case | Expected Outcome | Status |
|-----------|------------------|--------|
| Max Benefit Auto-Detection | Violation created automatically | ✓ |
| Compliance Filtering | Issues filtered by criteria | ✓ |
| Pending Items | Pending benefits/disbursements shown | ✓ |
| Resolution | Issue marked as resolved with audit trail | ✓ |
| Officer Flag | Officer flagged and tracked | ✓ |
| Budget Tracking | Accurate budget allocation reports | ✓ |
| Resource Mgmt | Resources approved/flagged | ✓ |
| Audit Findings | Findings created and closed | ✓ |
| Audit Trail | All activities logged | ✓ |
| Dashboards | Summary statistics accurate | ✓ |
| Delay Detection | Auto-detect 2+ day delays | ✓ |

---

## 📝 Common Test Scenarios

### Scenario A: Complete Compliance Workflow
1. Create Program with MaxBenefitPerCitizen
2. Create Application
3. Allocate Benefit (exceeding max)
4. Observe auto-created compliance violation
5. Compliance Officer reviews
6. Compliance Officer resolves
7. Verify audit log

### Scenario B: Resource Auditing Workflow
1. Create Program
2. Add Resources (set status to "Pending")
3. Auditor reviews pending resources
4. Auditor approves or flags resources
5. Audit findings created if insufficient
6. Verify audit trail

### Scenario C: Money Flow Monitoring
1. Create Program with Budget
2. Create multiple Applications
3. Allocate multiple Benefits
4. Process Disbursements
5. Auditor views budget tracking
6. Check budget utilization percentage
7. Verify money flow analysis

---

## 🔍 Troubleshooting

**Issue: Compliance violation not created**
- Check: Did the allocation exceed `MaxBenefitPerCitizen`?
- Check: Is the benefit status not "Failed" or "Cancelled"?
- Solution: Manually call endpoint or check database

**Issue: Pending items not showing**
- Check: Is the benefit/disbursement status "Pending" or "InProgress"?
- Check: Is the item older than 2 days?
- Solution: Create test data with appropriate status

**Issue: Audit trail empty**
- Check: Were any actions logged (creates, updates, deletes)?
- Check: Is the date range correct?
- Solution: Perform an action and check immediately

---

## 📊 Performance Tips

- Use date filters to limit audit log results
- Page through large result sets
- Use filters before retrieving all data
- Monitor database for large compliance record counts

