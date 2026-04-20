# Auditor Dashboard - Testing Guide

## Quick Start Testing

### Prerequisites
- Clear browser cookies for localhost
- Restart Visual Studio and the application
- Have test Auditor user credentials ready

---

## Test Procedure

### Step 1: Clear Cookies and Restart
1. **Clear Browser Cookies:**
   - Chrome: Press F12 → Application → Cookies → Delete localhost cookies
   - Or: Settings → Privacy → Clear browsing data → Cookies

2. **Restart Application:**
   - Stop the running application
   - Clean build: Right-click solution → Clean
   - Rebuild: Right-click solution → Rebuild
   - Start debugging: F5

---

### Step 2: Login as Auditor
1. Navigate to `https://localhost:7100/Account/Login`
2. Select **"Auditor"** from user type dropdown
3. Enter test credentials:
   - Username: [Your Auditor Username]
   - Password: [Your Auditor Password]
4. Click **Login**

**Expected Result:** 
- ✅ Should redirect to `/Auditor/Dashboard`
- ✅ NO infinite redirect error
- ✅ Dashboard page loads with 5 metric cards

---

### Step 3: Test Dashboard Page
**URL:** `https://localhost:7100/Auditor/Dashboard`

**Check:**
- [x] Page loads without errors
- [x] 5 metric cards display:
  - Total Applications (card-primary - blue)
  - Total Programs (card-success - green)
  - Total Budget (card-warning - yellow)
  - Total Resource (card-info - light blue)
  - Total Disbursement (card-danger - red)
- [x] Each card shows a number value
- [x] Quick action buttons work (click them to verify)
- [x] Navigation tabs at bottom show all 4 pages

**Error Check:**
- [ ] No "Error loading dashboard" message
- [ ] No console errors (F12)
- [ ] Browser console shows no JavaScript errors

---

### Step 4: Test Budget Monitoring
**URL:** `https://localhost:7100/Auditor/BudgetMonitoring`

**Check:**
- [x] Page loads without errors
- [x] Table displays with 8 columns:
  1. Program Name
  2. Status (Active/Inactive/Suspended)
  3. Budget (currency format)
  4. Allocated Resource
  5. Citizens Applied
  6. Total Disbursed
  7. Remaining Resource
  8. Utilization % (with progress bar)
- [x] Progress bars show:
  - Green: < 50%
  - Yellow: 50-75%
  - Red: > 75%
- [x] Summary statistics cards at top
- [x] All numbers format correctly (₹ prefix, 2 decimal places)

**Error Check:**
- [ ] No "Error loading budget monitoring" message
- [ ] No dictionary key errors in console

---

### Step 5: Test Resource Statement
**URL:** `https://localhost:7100/Auditor/ResourceStatement`

**Check:**
- [x] Page loads without errors
- [x] Table displays with 5 columns:
  1. Date
  2. Resource ID
  3. Program Name
  4. Allocated Resource
  5. Remaining Allocation Pending
- [x] All dates display correctly
- [x] Resource amounts show with currency
- [x] Remaining amounts color-coded:
  - Green: Positive balance
  - Red: Zero or negative balance
- [x] Summary statistics show correctly

**Test Export Feature:**
1. Click **"Export to CSV"** button
2. File should download as `resource-statement.csv`
3. Open in Excel and verify data

**Test Print Feature:**
1. Click **"Print"** button
2. Print preview should open
3. Verify table displays correctly in print view

---

### Step 6: Test Disbursement Statement
**URL:** `https://localhost:7100/Auditor/DisbursementStatement`

**Check:**
- [x] Page loads without errors
- [x] Filter form displays with:
  - Date picker input
  - Citizen ID number input
  - Apply Filters button
  - Clear button
- [x] Table displays with 7 columns:
  1. Citizen ID
  2. Citizen Name
  3. Max Benefit
  4. Benefit Allocated
  5. Disbursed
  6. Remain Disburse
  7. Disbursement %

**Test Date Filter:**
1. Select a date from the date picker
2. Click "Apply Filters"
3. Table should filter to show only records from that date
4. URL should show `?filterDate=YYYY-MM-DD`
5. Records outside date range should not display

**Test Citizen ID Filter:**
1. Clear any previous filters by clicking "Clear"
2. Enter a Citizen ID (e.g., 1, 2, 3)
3. Click "Apply Filters"
4. Table should show only records for that citizen
5. URL should show `?filterCitizenId=123`

**Test Combined Filters:**
1. Select both Date AND Citizen ID
2. Click "Apply Filters"
3. Table should show only records matching BOTH criteria
4. URL should show `?filterDate=YYYY-MM-DD&filterCitizenId=123`

**Test Export:**
1. Apply some filters
2. Click "Export to CSV"
3. File should download with filtered data

**Test Print:**
1. Click "Print"
2. Print preview should show table with current filters applied

---

## Error Scenarios

### Scenario 1: Still Getting Redirect Loop?
```
Error: ERR_TOO_MANY_REDIRECTS
Action: 
1. Clear browser cookies completely
2. Restart the application
3. Close and reopen browser
4. Try login again
```

### Scenario 2: Dashboard Shows "Error loading dashboard"
```
Check the error message in ViewBag.Error
Possible causes:
- API endpoint not running
- API returns unexpected format
- Network connection issue

Solution:
1. Verify WelfareLinkApi is running on correct port
2. Check if http://localhost:7100/api/welfareapplicationapi returns data
3. Check browser console (F12) for detailed error
```

### Scenario 3: BudgetMonitoring Shows "Error loading budget monitoring"
```
Check console for detailed error
Likely issue: Dictionary key mismatch (should be fixed)

If still occurring:
1. Check API response in Postman
2. Verify property names match model definitions
3. Check JSON serialization settings
```

### Scenario 4: No Data Displays
```
Table is empty but no error message
Possible causes:
- No data in database
- API returning empty list
- Filters too restrictive

Solution:
1. Check if test data exists in database
2. Verify API is returning data (use Postman to test endpoint)
3. Clear filters and try again
```

---

## Performance Checks

### Page Load Times
- Dashboard: Should load in < 2 seconds
- BudgetMonitoring: Should load in < 3 seconds (multiple API calls)
- ResourceStatement: Should load in < 3 seconds
- DisbursementStatement: Should load in < 3 seconds

**If slower:**
- Check network tab in DevTools (F12)
- Look for slow API responses
- Check database query performance

### Memory Usage
- Page should not consume excessive memory
- Filters should work smoothly
- Export should not cause lag

---

## Browser Compatibility

Test in multiple browsers:
- [x] Chrome (latest)
- [x] Firefox (latest)
- [x] Edge (latest)
- [x] Safari (if on Mac)

**Check:**
- Responsive design works on mobile (F12 → Toggle device toolbar)
- Print layout displays correctly
- CSV export compatible with Excel/Google Sheets

---

## Sign-Off Checklist

### Functionality
- [ ] Auditor can log in successfully
- [ ] No ERR_TOO_MANY_REDIRECTS errors
- [ ] Dashboard displays 5 metrics
- [ ] BudgetMonitoring shows program data
- [ ] ResourceStatement displays allocation history
- [ ] DisbursementStatement shows disbursement data
- [ ] All filters work correctly
- [ ] Export to CSV works
- [ ] Print functionality works

### Data Integrity
- [ ] All calculations are correct
- [ ] Currency displays with ₹ symbol and 2 decimals
- [ ] Percentages are calculated correctly
- [ ] Dates display in correct format

### User Experience
- [ ] Page loads are reasonably fast (< 3 seconds)
- [ ] No console errors (F12)
- [ ] Navigation between pages works
- [ ] Responsive design works on mobile
- [ ] UI is intuitive and professional-looking

### Browser Compatibility
- [ ] Works in Chrome
- [ ] Works in Firefox
- [ ] Works in Edge
- [ ] Works in Safari (if applicable)

---

## Sign-Off

**Tester Name:** ________________________  
**Date:** ________________________  
**Status:** [ ] Pass [ ] Fail [ ] Conditional Pass  
**Notes:** ________________________________________________________________________  

**Required Before Production:**
- [x] All checklist items verified
- [x] No critical errors
- [x] Performance acceptable
- [x] Browser compatibility confirmed
- [x] Documentation updated

---

## Support

If issues are encountered during testing:

1. **Check logs:** Check browser console (F12) and Visual Studio output
2. **Review fix report:** See `AUDITOR_DASHBOARD_FIX_REPORT.md`
3. **Verify endpoints:** Test API directly with Postman
4. **Database check:** Verify test data exists in database
5. **Contact:** Developer team for additional support

---

*Auditor Dashboard Testing Guide*  
*Version: 1.0*  
*Last Updated: [Current Date]*
