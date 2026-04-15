# Compliance Officer Dashboard - Testing Guide

## How to Test the Feature

### Prerequisites
1. Start the application
2. Log in as a Compliance Officer
3. Navigate to the Compliance Officer Dashboard

### Test Case 1: View All Applications
**Expected Behavior**:
- Dashboard displays all welfare applications in a table
- Shows Application ID, Citizen Name, Program, Status, Max Benefit, Allocated, Disbursed, and Remaining columns
- Statistics cards show total counts and amounts

**Steps**:
1. Login as Compliance Officer
2. Navigate to Dashboard
3. Observe the applications table populates with data

---

### Test Case 2: Identify Pending Allocations
**Expected Behavior**:
- Applications that are approved but have no benefits allocated for 2+ days show in "Pending Allocation" stat
- These applications have a red flag button

**Steps**:
1. Look at the "Pending Allocation" card - should show a count
2. Find an application with IsPendingAllocation = true (check API response)
3. The flag button should be enabled for this application

---

### Test Case 3: Identify No Disbursement Cases
**Expected Behavior**:
- Applications with allocated benefits but no disbursements for 2+ days show in "No Disbursement" stat
- Flag button is highlighted for these

**Steps**:
1. Look at the "No Disbursement" card - should show a count
2. Find an application with HasNoDisbursement = true
3. The flag button should be highlighted in red

---

### Test Case 4: Flag an Application - Wrong Disbursement
**Expected Behavior**:
- Opens a modal with flag options
- Selection of "Wrong Disbursement" option opens compliance form
- Form shows pre-populated issue type

**Steps**:
1. Click the Flag button on any application
2. Click "Wrong Disbursement" button
3. Verify the compliance form modal opens
4. Verify "ViolationType" field shows "Wrong Disbursement"
5. Verify the pre-filled description mentions disbursement issues
6. Enter any additional details in Description field
7. Select a Priority level (High, Medium, Low)
8. Click "Submit Issue"
9. Verify success message and modal closes
10. Verify dashboard refreshes

---

### Test Case 5: Flag an Application - Still Pending
**Expected Behavior**:
- Flags for pending allocations without disbursement within 2 days
- Form pre-populates with "Still Pending" as issue type

**Steps**:
1. Click the Flag button on a pending application
2. Click "Still Pending (No Allocation)" button
3. Verify the compliance form modal opens
4. Verify "ViolationType" field shows "Still Pending"
5. Verify the pre-filled description mentions 2-day timeframe
6. Add any additional context to Description
7. Select Priority
8. Click "Submit Issue"
9. Verify success message and dashboard updates

---

### Test Case 6: Validate Required Fields
**Expected Behavior**:
- Cannot submit without a description
- Error message appears if description is empty

**Steps**:
1. Click Flag button and select an issue type
2. Leave Description empty
3. Click "Submit Issue"
4. Verify alert message: "Please provide a description"
5. Modal remains open

---

### Test Case 7: View Application Details
**Expected Behavior**:
- Clicking the eye icon navigates to application details page

**Steps**:
1. Click the eye icon (view details button) on any application
2. Verify navigation to application details page
3. Verify correct application data is displayed

---

### Test Case 8: Refresh Dashboard
**Expected Behavior**:
- Refresh button reloads data without page refresh
- Statistics update with latest data

**Steps**:
1. Click the "Refresh" button on the dashboard
2. Verify loading spinner or text appears
3. Verify table data reloads
4. Verify statistics cards update

---

### Test Case 9: Verify API Endpoint
**Expected Behavior**:
- API returns all applications with correct data structure

**Steps**:
1. Open browser DevTools (F12)
2. Go to Network tab
3. Navigate to dashboard
4. Find the request to `/api/ComplianceOfficerDashboard/dashboard/applications-list`
5. Verify response contains:
   - `success: true`
   - `count`: number of applications
   - `data`: array of applications with all required fields
6. Verify each application object has:
   - ApplicationID, CitizenName, ProgramTitle
   - ApplicationStatus, MaxBenefit, TotalBenefitAllocated, TotalDisbursed
   - IsPendingAllocation, HasNoDisbursement flags
   - Benefits array with disbursement details

---

### Test Case 10: Compliance Record Creation
**Expected Behavior**:
- After submitting a flag, a compliance record is created in the database
- Record appears in ComplianceRecord table with correct data

**Steps**:
1. Flag an application with "Wrong Disbursement" issue
2. Check the database ComplianceRecords table
3. Verify a new record exists with:
   - ViolationType = "Wrong Disbursement" or "Still Pending"
   - ApplicationID matching the flagged application
   - Status = "Open"
   - CreatedDate = current date/time
   - Description matching what was entered

---

## API Response Verification

### Check the API Response Structure
```json
GET /api/ComplianceOfficerDashboard/dashboard/applications-list

Response:
{
  "success": true,
  "count": 5,
  "data": [
    {
      "ApplicationID": 1,
      "CitizenName": "Test Citizen",
      "CitizenID": 1,
      "ProgramTitle": "Test Program",
      "ProgramID": 1,
      "ApplicationStatus": "Approved",
      "SubmittedDate": "2024-01-15",
      "MaxBenefit": 5000,
      "TotalBenefitAllocated": 5000,
      "TotalDisbursed": 2500,
      "RemainingToDisborse": 2500,
      "BenefitCount": 1,
      "DisbursementCount": 2,
      "IsPendingAllocation": false,
      "HasNoDisbursement": false,
      "Benefits": [
        {
          "BenefitID": 1,
          "BenefitType": "Monthly Stipend",
          "BenefitAmount": 5000,
          "BenefitStatus": "Active",
          "BenefitDate": "2024-01-20",
          "DaysAllocated": 20,
          "DisbursementCount": 2,
          "TotalBenefitDisbursed": 2500,
          "RemainingBenefit": 2500,
          "Disbursements": [...]
        }
      ]
    }
  ]
}
```

---

## Database Verification

### Check ComplianceRecords Table
After submitting compliance issues, verify in database:

```sql
SELECT * FROM ComplianceRecords 
WHERE EntityType = 'Benefit' AND ViolationType IN ('Wrong Disbursement', 'Still Pending')
ORDER BY CreatedDate DESC;
```

Expected columns populated:
- RecordID (auto-generated)
- RaisedByUserId (current user ID)
- EntityType = 'Benefit'
- ApplicationID (from flagged application)
- ViolationType (selected issue type)
- Description (user-entered text)
- Status = 'Open'
- Priority (selected level)
- CreatedDate (current timestamp)

---

## Troubleshooting

### Issue: API returns 404
- **Solution**: Ensure the WelfareLinkApi is running on the correct port
- Check appsettings.json for API URL configuration

### Issue: Table doesn't populate
- **Solution**: Check browser console for JavaScript errors
- Verify the API endpoint is accessible
- Check if user has ComplianceOfficer role

### Issue: Flag button doesn't respond
- **Solution**: Check browser console for JavaScript errors
- Verify bootstrap modal library is loaded
- Check if jQuery is available (if required by your setup)

### Issue: Compliance record not created
- **Solution**: Check if the API request succeeded (check Network tab)
- Verify user session has valid UserId
- Check database for constraint violations

---

## Performance Testing

### Load Testing Scenario
- Dashboard with 100+ applications
- Each application with multiple benefits and disbursements
- Expected load time: < 3 seconds for initial data load

### Data Volume Testing
- Test with 50, 100, 500, 1000 applications
- Monitor API response times
- Verify pagination/scroll performance if implemented

---

## Accessibility Testing

### Screen Reader Testing
- Verify table headers are properly marked
- Verify button labels are descriptive
- Verify modal headers are announced

### Keyboard Navigation
- Tab through all interactive elements
- Verify Enter key activates buttons
- Verify Escape closes modals

### Color Contrast
- Verify badge colors meet WCAG standards
- Verify warning/error colors are distinguishable

---

## Notes
- The dashboard automatically flags applications based on business rules
- Compliance officers can manually flag applications with additional context
- All flagging actions are logged for audit purposes
- Dashboard data refreshes without full page reload
