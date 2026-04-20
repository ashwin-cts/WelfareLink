# Auditor Dashboard - Access URLs & Configuration

## 🌐 Live URLs

Once the application is running, access the Auditor Dashboard using these URLs:

### 1. Main Dashboard
```
http://localhost/Auditor/Dashboard
https://yourdomain.com/Auditor/Dashboard
```
**Description**: Main dashboard with summary metrics

---

### 2. Budget Monitoring (Program Breakdown)
```
http://localhost/Auditor/BudgetMonitoring
https://yourdomain.com/Auditor/BudgetMonitoring
```
**Description**: Detailed program-wise budget and resource breakdown

---

### 3. Resource Statement
```
http://localhost/Auditor/ResourceStatement
https://yourdomain.com/Auditor/ResourceStatement
```
**Description**: Resource allocation history from Program Officers

---

### 4. Disbursement Statement (Unfiltered)
```
http://localhost/Auditor/DisbursementStatement
https://yourdomain.com/Auditor/DisbursementStatement
```
**Description**: All disbursement records

---

### 5. Disbursement Statement (Filtered by Date)
```
http://localhost/Auditor/DisbursementStatement?filterDate=2025-03-26
https://yourdomain.com/Auditor/DisbursementStatement?filterDate=2025-03-26
```
**Description**: Disbursements for a specific date

**Date Format**: YYYY-MM-DD

---

### 6. Disbursement Statement (Filtered by Citizen ID)
```
http://localhost/Auditor/DisbursementStatement?filterCitizenId=123
https://yourdomain.com/Auditor/DisbursementStatement?filterCitizenId=123
```
**Description**: Disbursements for a specific citizen

**Citizen ID**: Integer value

---

### 7. Disbursement Statement (Filtered by Both)
```
http://localhost/Auditor/DisbursementStatement?filterDate=2025-03-26&filterCitizenId=123
https://yourdomain.com/Auditor/DisbursementStatement?filterDate=2025-03-26&filterCitizenId=123
```
**Description**: Disbursements for a specific citizen on a specific date

---

## 🔐 Prerequisites for Access

### User Requirements
1. **Account Status**: Active and verified
2. **Role**: One of the following:
   - `Auditor`
   - `GovernmentAuditor`
3. **Session**: Must be logged in
4. **Permissions**: Standard auditor permissions

### System Requirements
1. **Server**: ASP.NET Core application running
2. **API**: All required API endpoints accessible:
   - `/api/welfareapplicationapi`
   - `/api/welfareprogramapi`
   - `/api/resourceapi`
   - `/api/disbursementapi`
   - `/api/benefitapi`
3. **Database**: All required tables populated with data

---

## 🚀 How to Start Using

### Step 1: Login
1. Go to the login page (e.g., `http://localhost/Account/Login`)
2. Enter credentials for a user with "Auditor" role
3. Click Login

### Step 2: Navigate to Dashboard
**Option A**: Direct URL
- Type `http://localhost/Auditor/Dashboard` in browser

**Option B**: From menu
- Look for "Auditor" or "Dashboard" link in main navigation
- Click to access the dashboard

### Step 3: Explore the Dashboard
1. View the 5 summary metric cards
2. Click "View Budget Breakdown" for program details
3. Click "Resource Allocation History" for resource tracking
4. Click "Disbursement History" for payment tracking

---

## 🔄 Navigation Between Pages

All pages have a **navigation tab bar** at the top:

```
┌─────────────────────────────────────────────────────────┐
│ Dashboard │ Budget Monitoring │ Resource Statement │ Disbursement Statement │
└─────────────────────────────────────────────────────────┘
```

Simply click any tab to navigate to that page.

---

## 📝 Query Parameters

### For Disbursement Statement

#### filterDate Parameter
- **Key**: `filterDate`
- **Format**: `YYYY-MM-DD`
- **Example**: `2025-03-26`
- **Usage**: Filters disbursements by specific date

#### filterCitizenId Parameter
- **Key**: `filterCitizenId`
- **Format**: Integer
- **Example**: `123` or `1001`
- **Usage**: Filters disbursements by specific citizen

#### Combining Parameters
```
?filterDate=2025-03-26&filterCitizenId=123
```

---

## 🎨 Expected Dashboard Layout

### Dashboard Page
```
┌─────────────────────────────────────────────────────────┐
│ Government Auditor Dashboard                             │
├─────────────────────────────────────────────────────────┤
│ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐    │
│ │ Total    │ │ Total    │ │ Total    │ │ Total    │    │
│ │ Apps     │ │ Programs │ │ Budget   │ │ Resource │    │
│ │   125    │ │   15     │ │ ₹5,00,000│ │ ₹2,50,000│    │
│ └──────────┘ └──────────┘ └──────────┘ └──────────┘    │
│ ┌──────────────────┐                                     │
│ │ Total            │                                     │
│ │ Disbursement     │                                     │
│ │ ₹1,75,000        │                                     │
│ └──────────────────┘                                     │
├─────────────────────────────────────────────────────────┤
│ [View Budget Breakdown] [Resource History] [Disburse...] │
└─────────────────────────────────────────────────────────┘
```

### Budget Monitoring Page
```
┌─────────────────────────────────────────────────────────┐
│ Budget Monitoring - Program Breakdown                    │
├─────────────────────────────────────────────────────────┤
│ Program│Status │Budget │Allocated│Citizens│Disbursed... │
├─────────────────────────────────────────────────────────┤
│ Health │Active │₹50,000│₹25,000  │   50   │  ₹15,000... │
│ Edu    │Active │₹75,000│₹60,000  │   75   │  ₹50,000... │
│ Food   │Active │₹40,000│₹20,000  │   30   │  ₹12,000... │
└─────────────────────────────────────────────────────────┘
```

---

## ⚠️ Common Issues & Solutions

### Issue: "Unauthorized - Access Denied"
**Solution**: 
- Ensure user is logged in
- Verify user has "Auditor" or "GovernmentAuditor" role
- Clear browser cache and refresh

### Issue: "No data found" on all pages
**Solution**:
- Verify API endpoints are accessible
- Check database has data
- Ensure database connections are configured
- Check server logs for API errors

### Issue: Filters not working on Disbursement page
**Solution**:
- Clear filters with "Clear" button
- Ensure date format is YYYY-MM-DD
- Verify citizen ID is correct integer
- Refresh page and try again

### Issue: Export/Print buttons not working
**Solution**:
- Enable JavaScript in browser
- Try different browser
- Check browser console for errors
- Ensure pop-ups are allowed

### Issue: Performance is slow
**Solution**:
- Check network connection
- Verify API response times
- Try during off-peak hours
- Contact system administrator

---

## 📱 Mobile Access

The dashboard is **fully responsive** and works on mobile devices:

### Mobile Browsers Tested
- ✅ Chrome Mobile
- ✅ Safari iOS
- ✅ Firefox Mobile
- ✅ Edge Mobile

### Mobile Features
- Responsive tables with horizontal scroll
- Optimized card layout
- Touch-friendly buttons
- Readable on small screens

### Recommended Screen Sizes
- Desktop: 1024px or larger
- Tablet: 768px - 1024px
- Mobile: 320px - 768px

---

## 🔄 Data Refresh

### Automatic Refresh
- Page data is fetched on each page load
- No automatic real-time refresh
- Manual refresh available via browser F5

### Manual Refresh Methods
1. **Browser Refresh**: Press F5 or Ctrl+R
2. **Navigation**: Click to a different page and back
3. **Link**: Click on the page link again

### Data Latency
- API call to data display: ~1-2 seconds
- Depends on network and API performance
- Typically refreshes within a minute

---

## 🌍 Timezone Considerations

### Date Display
- All dates use **UTC timezone** from server
- Displayed in format: **YYYY-MM-DD HH:MM**
- Filters accept **YYYY-MM-DD** format

### Example
```
Date Displayed: 2025-03-26 10:30
Filter Format: 2025-03-26
```

---

## 📊 Sample Data Scenarios

### Scenario 1: View Program Performance
1. Go to `/Auditor/BudgetMonitoring`
2. Review utilization percentages
3. Identify programs with >75% utilization (Red warning)
4. Click program name for more details (future feature)

### Scenario 2: Track Citizen Disbursements
1. Go to `/Auditor/DisbursementStatement`
2. Enter Citizen ID: `123`
3. Click "Apply Filters"
4. View all disbursements for this citizen
5. Export to CSV if needed

### Scenario 3: Daily Audit Report
1. Go to `/Auditor/DisbursementStatement`
2. Select today's date
3. Click "Apply Filters"
4. View all disbursements for today
5. Print or export report

### Scenario 4: Resource Allocation Analysis
1. Go to `/Auditor/ResourceStatement`
2. Review allocation history
3. Export to CSV for spreadsheet analysis
4. Verify all allocations match budget

---

## 🔧 Configuration

### Application Settings Required
Ensure these are configured in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "your_connection_string"
  },
  "HttpClients": {
    "DashboardClient": {
      "BaseAddress": "http://localhost:5000"
    }
  }
}
```

### Environment Variables
- `ASPNETCORE_ENVIRONMENT`: Production/Development
- `ConnectionString`: Database connection
- `API_BASE_URL`: API endpoint base URL

---

## 📞 Support

### For Technical Issues
- Check logs in server console
- Verify all API endpoints are running
- Ensure database is accessible
- Check user session is valid

### For Feature Requests
- Document the feature need
- Provide use case examples
- Contact development team

### For Data Discrepancies
- Verify data in source database
- Check API response formats
- Review calculation logic
- Contact system administrator

---

## 🎯 Dashboard Features Quick Reference

| Feature | Page | Access Method |
|---------|------|---|
| View Metrics | Dashboard | Direct link |
| Program Budget | Budget Monitoring | Tab or direct link |
| Resource History | Resource Statement | Tab or direct link |
| Disbursement Track | Disbursement Statement | Tab or direct link |
| Date Filter | Disbursement Statement | Query parameter |
| Citizen Filter | Disbursement Statement | Query parameter |
| Export CSV | Resource/Disbursement | Button on page |
| Print Report | Resource/Disbursement | Button on page |
| Summary Stats | All pages | Bottom of page |

---

## ✅ Verification Checklist Before Production

- ✅ All API endpoints configured and running
- ✅ Database has test data
- ✅ User has Auditor role assigned
- ✅ HTTPS configured for production
- ✅ Timezone settings correct
- ✅ API response times acceptable (<2s)
- ✅ Mobile access verified
- ✅ Export functionality tested
- ✅ Error pages display correctly
- ✅ Performance is acceptable

---

**Document Version**: 1.0
**Last Updated**: 2025
**Status**: ✅ Ready for Production
