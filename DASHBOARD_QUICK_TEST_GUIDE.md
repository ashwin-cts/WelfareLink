# Quick Test Guide - Compliance Officer Dashboard

## Step 1: Restart Applications
1. Stop both `WelfareLink` and `WelfareLinkApi`
2. Rebuild solution: `dotnet build`
3. Start `WelfareLinkApi` first: `dotnet run --project WelfareLinkApi`
4. Start `WelfareLink` second: `dotnet run --project WelfareLink`

## Step 2: Login as Compliance Officer
1. Go to `https://localhost:7100`
2. Login with Compliance Officer credentials
   - Username: (compliance officer username)
   - Password: (password)
3. You should be redirected to: `https://localhost:7100/ComplianceOfficer/Dashboard`

## Step 3: Verify Dashboard Loads
- Check that the page displays without errors
- You should see 4 statistics cards at the top:
  - Total Applications (number)
  - Pending Allocation (number)
  - No Disbursement (number)
  - Total Disbursed (amount in ₹)

## Step 4: Verify Applications Table
- In "All Applications" section, verify:
  - [ ] Table has 9 columns
  - [ ] Applications data is displayed
  - [ ] Each row shows: ID, Citizen Name, Program, Status, Max Benefit, Allocated, Disbursed, Remaining

## Step 5: Test Detail Expansion
- Click the chevron (▼) button on any application
- Expected result:
  - [ ] Row expands to show "Allocated Benefits & Disbursements"
  - [ ] Benefits table displays with: Benefit ID, Type, Amount, Status, Days, Disbursements button
  
## Step 6: Test Disbursement Details
- In the expanded benefits section, click on a disbursement count button
- Expected result:
  - [ ] Disbursement details table expands
  - [ ] Shows: Date, Amount, Status for each disbursement

## Step 7: Test Flag Functionality
- Click the flag button (🚩) on an application
- Expected result:
  - [ ] Modal appears with flag options
  - [ ] Can select issue type
  - [ ] Can enter description
  - [ ] Can select priority

## Step 8: Open Browser DevTools
Press **F12** and check:

### Console Tab
Look for messages:
```
✓ API Response Status: 200
✓ API Response OK: true
✓ API Response Data: {success: true, count: X, data: [...]}
✓ Parsed Applications: [{...}, ...]
```

### Network Tab
Look for request: `dashboard/applications-list`
- [ ] Status: **200 OK**
- [ ] Headers include: `Access-Control-Allow-Origin: https://localhost:7100`
- [ ] Response contains JSON with `success: true` and `data` array

### Application Tab (or Cookies)
- [ ] Session cookies are present
- [ ] No CORS errors in console

## Step 9: Verify Statistics Accuracy
- Count applications in table manually
- Verify "Total Applications" card shows same count
- Verify other statistics match data

## Expected Results Summary

### ✅ Dashboard Should Load Successfully
- No errors displayed
- Page responsive
- Statistics cards populated
- Applications table visible

### ✅ All Data Visible
- Application IDs and details display
- Benefit amounts show correctly
- Disbursement totals calculate correctly
- Remaining amounts are accurate

### ✅ Interactive Features Work
- Rows expand/collapse
- Disbursements show/hide
- Flag buttons trigger modals
- View details button navigates

### ✅ Browser Console Clean
- No CORS errors
- No JavaScript errors
- API logs show successful 200 responses
- Data parsed correctly

## If Issues Occur

### Error: "Failed to fetch applications"
1. Check browser console for CORS errors
2. Verify both applications are running
3. Check WelfareLinkApi on `https://localhost:7141/swagger`
4. Ensure appsettings.json has correct BaseUrl

### Error: "No applications found"
1. Verify database has test data
2. Check connection string
3. Review SQL queries in dashboard controller

### Data Not Displaying in Table
1. Open DevTools Network tab
2. Look at API response JSON
3. Ensure all required fields are present
4. Check JavaScript console for parse errors

### Styling Issues
1. Verify Bootstrap 5 is loaded
2. Check for CSS conflicts
3. Inspect element in DevTools

## Command Reference

### Full Rebuild & Run
```powershell
# Terminal 1 - API
cd WelfareLinkApi
dotnet run

# Terminal 2 - MVC
cd WelfareLink
dotnet run
```

### Clean Build
```powershell
dotnet clean
dotnet build
```

### Check Specific Port
```powershell
# Check if port 7141 is listening (API)
netstat -ano | findstr :7141

# Check if port 7100 is listening (MVC)
netstat -ano | findstr :7100
```

## Test Data Requirements

The dashboard will work best with test data containing:
- [ ] At least 1 welfare application
- [ ] Application status: "Approved"
- [ ] At least 1 benefit allocated
- [ ] At least 1 disbursement record
- [ ] Dates within last 30 days (for accurate "Days Allocated" display)

## Performance Check

Dashboard should load:
- [ ] Statistics: < 500ms
- [ ] Applications table: < 1 second
- [ ] Detail expansion: Instant (< 100ms)

Monitor Network tab in DevTools to verify response times.

## Success Criteria

✅ All items below must be true for complete success:

- [ ] Dashboard page loads without errors
- [ ] Four statistics cards display with numbers
- [ ] Applications table displays all records
- [ ] All columns show correct data
- [ ] Benefit details expand/collapse
- [ ] Disbursement details visible
- [ ] Flag functionality works
- [ ] Browser console has no errors
- [ ] Network responses show 200 OK
- [ ] CORS headers present in response

## Rollback Instructions

If issues occur, rollback changes:

```bash
# Revert Program.cs changes
git checkout WelfareLinkApi/Program.cs

# Revert API Controller changes
git checkout WelfareLinkApi/Controllers/ComplianceOfficerDashboardApiController.cs

# Revert Dashboard view changes
git checkout WelfareLink/Views/ComplianceOfficer/Dashboard.cshtml

# Rebuild
dotnet clean
dotnet build
```
