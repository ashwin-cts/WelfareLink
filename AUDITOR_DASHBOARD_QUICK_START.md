# Auditor Dashboard - Quick Start Guide

## Access the Auditor Dashboard

### Prerequisites
- User account with "Auditor" or "GovernmentAuditor" role
- Logged into the system

### Main Dashboard
**URL**: `localhost/Auditor/Dashboard`

**What you'll see:**
- 5 summary cards showing key metrics:
  1. **Total Applications** - Count of all welfare applications
  2. **Total Programs** - Count of all welfare programs
  3. **Total Budget** - Sum of all program budgets (₹)
  4. **Total Resource** - Total resource allocation in INR (₹)
  5. **Total Disbursement** - Total amount disbursed to citizens (₹)

**Quick Actions:**
- Click "View Budget Breakdown" to see program-wise details
- Click "Resource Allocation History" to see resource allocation timeline
- Click "Disbursement History" to see all disbursements with filters

---

## 1. Program Budget Breakdown

**URL**: `localhost/Auditor/BudgetMonitoring`

**Features:**
- See all programs with their budget details
- Monitor resource allocation and disbursement for each program
- Track program status and utilization percentage

**Table Columns:**
| Column | Description |
|--------|-------------|
| Program Name | Name of the welfare program |
| Program Status | Active/Inactive/Suspended status |
| Program Budget | Total budget allocated to the program (₹) |
| Allocated Resource | Total resources allocated to this program (₹) |
| Citizens Applied | Number of citizens who applied |
| Total Disbursed | Total amount disbursed so far (₹) |
| Remaining Resource | Budget remaining (₹) |
| Utilization % | Percentage of budget used (visual bar) |

**How to use:**
1. Review each program's performance
2. Identify programs with high utilization (>75% - Red warning)
3. Monitor medium utilization (50-75% - Yellow caution)
4. Track well-managed programs (<50% - Green)

---

## 2. Resource Allocation Statement

**URL**: `localhost/Auditor/ResourceStatement`

**Features:**
- View complete history of resource allocations
- Track resource allocation from Program Officers
- Monitor remaining allocation pending for each program
- Export data to CSV for reporting

**Table Columns:**
| Column | Description |
|--------|-------------|
| Date | Date and time of resource allocation |
| Resource ID | Unique identifier for the resource |
| Program Name | Program for which resource was allocated |
| Allocated Resource | Amount allocated in this transaction (₹) |
| Remaining Allocation Pending | Budget still available for this program (₹) |

**Important Note:**
- Multiple allocations for the same program appear as **separate rows**
- Each row represents a specific allocation event
- Dates help track allocation history over time

**Export Options:**
- Click "Print Report" to print or save as PDF
- Click "Export to CSV" to download as spreadsheet

---

## 3. Disbursement Statement

**URL**: `localhost/Auditor/DisbursementStatement`

**Features:**
- View all disbursements made to citizens
- Track how much benefit is allocated vs. actually disbursed
- Filter by date and/or citizen ID
- Monitor remaining disbursements pending

### How to Filter

**By Date:**
1. Select a date from the "Filter by Date" field
2. Click "Apply Filters"
3. See only disbursements for that date

**By Citizen ID:**
1. Enter the citizen ID in the "Filter by Citizen ID" field
2. Click "Apply Filters"
3. See only disbursements for that citizen

**By Both:**
1. Select a date AND enter citizen ID
2. Click "Apply Filters"
3. See disbursements for that specific citizen on that date

**Clear Filters:**
- Click the "Clear" button to remove all filters and see all records

### Disbursement Table

**Table Columns:**
| Column | Description |
|--------|-------------|
| Citizen ID | Unique identifier for the citizen |
| Citizen Name | Name of the citizen |
| Max Benefit of Program | Maximum benefit allowed for the program (₹) |
| Benefit Allocated | Amount allocated by welfare officer (₹) |
| Disbursed | Amount actually disbursed to citizen (₹) |
| Remain Disburse | Amount still pending disbursement (₹) |
| Disbursement % | Percentage of allocated benefit disbursed (visual bar) |

**Progress Bar Colors:**
- 🟢 **Green (75%+)**: Good disbursement progress
- 🟡 **Yellow (50-75%)**: Medium progress, monitor closely
- 🔵 **Blue (<50%)**: Low disbursement rate, may need review

**Export Options:**
- Click "Print Report" to print or save as PDF
- Click "Export to CSV" to download as spreadsheet

---

## Understanding the Metrics

### Utilization Percentage
Calculated as: (Total Disbursed / Program Budget) × 100

**Example:**
- Program Budget: ₹1,00,000
- Total Disbursed: ₹75,000
- Utilization: 75%

### Remaining Resource
- **Positive** (✅): Budget still available for allocation
- **Negative** (⚠️): Over-allocation (spending exceeds budget)

### Remaining Disburse
- Amount that was allocated but not yet given to citizens
- Should be 0 when all allocated benefits are disbursed

---

## Key Insights to Monitor

### Red Flags 🚩
1. **Utilization > 100%** - Spending exceeds budget
2. **No Disbursement for 2+ weeks** - Delayed payments
3. **Large "Remaining Disburse"** - Pending payments accumulating
4. **Citizens Applied ≠ Actual Disbursements** - Incomplete allocations

### Good Indicators ✅
1. **Utilization 50-75%** - Healthy spending pace
2. **Consistent Disbursement dates** - Regular payments
3. **Low "Remaining Disburse"** - Efficient payment processing
4. **All Citizens Accounted For** - Complete allocation tracking

---

## Navigation Tips

**Quick Navigation:**
- Use the **navigation tabs** at the top of each page to switch between:
  - Dashboard
  - Budget Monitoring
  - Resource Statement
  - Disbursement Statement

**Consistent Layout:**
- All pages follow the same design
- Same navigation tabs on every page
- Easy to switch between views

---

## Troubleshooting

**Issue**: Page shows "No data found"
- **Solution**: 
  - Verify programs/applications exist in the system
  - Check if API endpoints are available
  - Try refreshing the page

**Issue**: Filters not working
- **Solution**:
  - Click "Clear" to reset filters
  - Re-enter filter criteria
  - Ensure date format is correct (YYYY-MM-DD)

**Issue**: Numbers seem incorrect
- **Solution**:
  - Note that sums exclude pending/rejected items
  - Multiple allocations sum across all records
  - Check individual program details

---

## Contact & Support

For issues or feature requests, please contact:
- **Development Team**: [Email/Contact info]
- **System Admin**: [Email/Contact info]

---

## Revision History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025 | Initial implementation |

---

**Last Updated**: 2025
**Status**: ✅ Active & Ready to Use
