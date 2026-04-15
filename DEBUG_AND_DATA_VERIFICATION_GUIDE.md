# 🔍 COMPLETE DEBUG & DATA VERIFICATION GUIDE

## **Quick Start**
1. **Open Browser Developer Tools** (Press F12)
2. **Go to Console tab**
3. **Navigate to Compliance Officer or Auditor Dashboard**
4. **Watch the console logs** to see data flow

---

## **STEP 1: VERIFY COMPLIANCE OFFICER DASHBOARD DATA**

### In Browser Console, You Should See:
```
Loading Compliance Dashboard Data...
Fetching /api/ComplianceOfficerDashboard/allocations
Allocations Response: [Array with benefitID, amount, citizen, program, etc.]
Allocations List (processed): [Array]
Total Allocations: X
Fetching /api/ComplianceOfficerDashboard/issues
Issues Response: [Array with RecordID, ViolationType, Priority, Status, etc.]
Issues List (processed): [Array]
Stats - Pending: X, Resolved: Y, Critical: Z
```

### If You See "No allocations found":
**This means either:**
1. ❌ No data in database (check step 2)
2. ❌ API returned empty array (check step 3)
3. ❌ Network error (check step 4)

---

## **STEP 2: CHECK DATABASE HAS DATA**

### Open Visual Studio SQL Server Object Explorer:
1. Right-click **WelfareLinkApi** project → **SQL Server Object Explorer**
2. Expand your database
3. Check these tables have data:

```sql
-- Benefits table (allocations)
SELECT TOP 5 BenefitID, Amount, Status, Date 
FROM Benefits 
WHERE Status NOT IN ('Failed', 'Cancelled');

-- Compliance Records table (issues)
SELECT TOP 5 RecordID, ViolationType, Priority, Status, CreatedDate 
FROM ComplianceRecords 
WHERE Status = 'Open';

-- Programs table (for program details)
SELECT TOP 5 ProgramID, Title, Budget, MaxBenefitPerCitizen 
FROM Programs;
```

### If tables are empty:
- Create test data:
  1. Login as WelfareOfficer
  2. Create a Program with MaxBenefitPerCitizen = 5000
  3. Create Welfare Application
  4. Create Benefit allocation
  5. This will populate the Benefits table

---

## **STEP 3: VERIFY API ENDPOINTS RETURN DATA**

### In Postman or Browser URL Bar:

#### Test 1: Allocations Endpoint
```
GET http://localhost:5000/api/ComplianceOfficerDashboard/allocations
```
**Should return array of objects with:**
```json
[
  {
    "benefitID": 1,
    "amount": 5000,
    "status": "Pending",
    "date": "2024-01-15",
    "citizen": {
      "citizenId": 1,
      "name": "John Doe",
      "contactInfo": "9876543210"
    },
    "program": {
      "programID": 1,
      "title": "Housing Support",
      "budget": 100000,
      "maxBenefitPerCitizen": 5000
    },
    "totalDisbursed": 2500
  }
]
```

#### Test 2: Issues Endpoint
```
GET http://localhost:5000/api/ComplianceOfficerDashboard/issues
```
**Should return:**
```json
[
  {
    "recordID": 1,
    "violationType": "MaxBenefitExceeded",
    "priority": "High",
    "description": "Citizen exceeded max benefit limit",
    "status": "Open",
    "createdDate": "2024-01-15",
    "benefitID": 1
  }
]
```

#### Test 3: Budget Monitoring Endpoint
```
GET http://localhost:5000/api/AuditorDashboard/budget-monitoring
```
**Should return:**
```json
[
  {
    "programID": 1,
    "title": "Housing Support",
    "budget": 100000,
    "totalAllocated": 15000,
    "maxBenefitPerCitizen": 5000,
    "applicationsCount": 5,
    "benefitsCount": 3
  }
]
```

#### Test 4: Resource Utilization Endpoint
```
GET http://localhost:5000/api/AuditorDashboard/resource-utilization
```
**Should return:**
```json
[
  {
    "resourceID": 1,
    "type": "Building Materials",
    "quantity": 100,
    "status": "Active",
    "program": {
      "programID": 1,
      "title": "Housing Support",
      "budget": 100000
    },
    "allocatedBenefits": 50,
    "totalBenefitAmount": 25000
  }
]
```

---

## **STEP 4: CHECK NETWORK REQUEST IN BROWSER**

### In Browser DevTools:
1. Press **F12**
2. Go to **Network tab**
3. Refresh dashboard page
4. You should see 4 requests:
   - `allocations` → Status 200 ✅
   - `issues` → Status 200 ✅
   - `budget-monitoring` → Status 200 ✅
   - `resource-utilization` → Status 200 ✅

### If any show 404 or 500:
- Check API is running on correct port (usually 5000 or 7123)
- Check route is exactly: `/api/ComplianceOfficerDashboard/allocations`
- Check API project is built & running

---

## **STEP 5: COMMON ISSUES & FIXES**

### ❌ **Issue: "No allocations found" message**

**Reason 1: Database is empty**
- Fix: Create test data (see Step 2)

**Reason 2: API is not running**
- Fix: Start WelfareLinkApi project (Run → Start Debugging / Ctrl+F5)

**Reason 3: Database connection is wrong**
- Fix: Check `appsettings.json` connection string matches your SQL Server

**Reason 4: Port mismatch**
- Fix: Check MVC is calling correct API port
  - Open WelfareLink/Services/WelfareApiClient.cs
  - Verify BaseAddress matches API port (e.g., http://localhost:5000)

---

### ❌ **Issue: Console shows "Error: Failed to fetch"**

**Reason 1: CORS issue**
- API not allowing requests from MVC origin
- Fix: Check WelfareLinkApi/Program.cs has CORS enabled:
```csharp
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", builder => {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});
```

**Reason 2: API endpoint doesn't exist**
- Check controller name: `ComplianceOfficerDashboardApiController`
- Check route: `[Route("api/[controller]")]`
- Check method: `[HttpGet("allocations")]`

---

### ❌ **Issue: Data loads but shows wrong values**

**Reason: Property name mismatch**
- API returns `BenefitID`, view expects `benefitID`
- Fix: Already done! Views now handle both cases with:
```javascript
const benefitID = a.BenefitID || a.benefitID;
```

---

## **STEP 6: MANUAL DATA CREATION FOR TESTING**

### If database is completely empty:

#### 1. Create a Program
- Login as **ProgramManager**
- Go to Program → Add Program
- Fill in:
  - Title: "Test Housing Program"
  - Budget: 100000
  - **MaxBenefitPerCitizen: 5000** ← Important!
  - Start Date: Today
  - End Date: +30 days
- Click Save

#### 2. Create a Citizen & Welfare Application
- Login as **Citizen**
- Go to My Applications
- Apply for program "Test Housing Program"

#### 3. Create Benefit Allocation
- Login as **WelfareOfficer**
- Go to Applications
- Approve the application
- Create Benefit:
  - Amount: 4000
  - Type: Monthly
  - Status: Pending

#### 4. Now Check Compliance Officer Dashboard
- Login as **ComplianceOfficer**
- Go to My Dashboard
- Should see:
  - ✅ 1 Allocation
  - ✅ Citizen name
  - ✅ Program name
  - ✅ Benefit amount
  - ✅ Disbursed amount

---

## **STEP 7: CREATE TEST COMPLIANCE RECORD**

### To test compliance dashboard with issues:

#### Option 1: Automatic (if max benefit exceeded)
```
Create multiple benefits for same citizen in same program totaling > MaxBenefit
```

#### Option 2: Manual via Compliance Officer Dashboard
1. Login as **ComplianceOfficer**
2. Go to **My Allocations**
3. Find a benefit
4. Click **"Raise Issue"** button
5. Enter:
   - Violation Type: "ManualCheck"
   - Description: "Test issue"
   - Priority: "High"
6. Now go to **My Issues** → Should see it listed

---

## **STEP 8: VERIFY AUDITOR DASHBOARD DATA**

### Login as **GovernmentAuditor**
### Go to Dashboard → Should See:

```
📊 KPI Cards:
- Total Programs: X
- Active Applications: Y
- Flagged Benefits: Z
- Budget Utilization: X%

📈 Budget Status (5 programs):
- Program name
- Budget bar with percentage
- Allocated vs Remaining

📦 Resource Utilization (5 resources):
- Resource type
- Resource quantity used
- Allocation count
```

### If empty:
1. Check database has resources (see Step 2 SQL query)
2. Check API endpoints return data (see Step 3)
3. Check browser console for errors (see Step 4)

---

## **COMPLETE CHECKLIST**

- [ ] Browser console shows data logs (no errors)
- [ ] All 4 API endpoints return 200 status
- [ ] Database tables have test data
- [ ] Compliance Dashboard shows allocations
- [ ] Compliance Dashboard shows issues
- [ ] Auditor Dashboard shows budget status
- [ ] Auditor Dashboard shows resource utilization
- [ ] KPI numbers are correct
- [ ] Progress bars show correct percentages
- [ ] Clicking "Raise Issue" works on allocations
- [ ] Resolved issues count updates correctly

---

## **IF STILL NOT WORKING**

### Check these files in exact order:

1. **WelfareLink/Services/WelfareApiClient.cs**
   - BaseAddress should match API port

2. **WelfareLinkApi/Program.cs**
   - CORS should be enabled
   - Controllers should be registered
   - Database context should be configured

3. **WelfareLinkApi/appsettings.json**
   - Connection string should point to correct database

4. **Both appsettings.json files**
   - Check they reference correct ports/servers

5. **Browser DevTools → Network tab**
   - Check actual API response (click request → Response tab)
   - Copy response and compare with expected format

---

## **ERROR MESSAGES & MEANINGS**

| Error Message | Meaning | Fix |
|---|---|---|
| "ERR_TIMED_OUT" | API not responding | Start API project |
| "CORS error" | Different domain/port | Enable CORS in API |
| "404 Not Found" | Endpoint doesn't exist | Check route/controller |
| "400 Bad Request" | Wrong parameter format | Check request format |
| "Empty array" | No database data | Create test data |
| "Property undefined" | Wrong property name | Use both naming patterns |

