# API Response Examples

## 1. GET /api/AuditorDashboard/statistics

### Request
```
GET https://localhost:7100/api/AuditorDashboard/statistics
```

### Response (200 OK)
```json
{
  "totalApplications": 150,
  "totalPrograms": 12,
  "totalBudget": 50000000,
  "totalResource": 75000000,
  "totalDisbursement": 25000000
}
```

---

## 2. GET /api/AuditorDashboard/program-breakdown

### Request
```
GET https://localhost:7100/api/AuditorDashboard/program-breakdown
```

### Response (200 OK)
```json
[
  {
    "programID": 1,
    "programName": "Senior Citizens Support Scheme",
    "programStatus": "Active",
    "programBudget": 5000000,
    "allocatedResourceForProgram": 3500000,
    "citizensApplied": 120,
    "totalDisbursedForProgram": 2500000,
    "remainingResource": 1000000,
    "utilizationPercentage": 71.43
  },
  {
    "programID": 2,
    "programName": "Disability Assistance Program",
    "programStatus": "Active",
    "programBudget": 3000000,
    "allocatedResourceForProgram": 2000000,
    "citizensApplied": 85,
    "totalDisbursedForProgram": 1500000,
    "remainingResource": 500000,
    "utilizationPercentage": 75.00
  }
]
```

---

## 3. GET /api/AuditorDashboard/resource-statement

### Request
```
GET https://localhost:7100/api/AuditorDashboard/resource-statement
```

### Response (200 OK)
```json
[
  {
    "resourceId": 101,
    "programName": "Senior Citizens Support Scheme",
    "allocatedResource": 500000,
    "allocationDate": "2024-01-15",
    "remainingAllocationPending": 1500000
  },
  {
    "resourceId": 102,
    "programName": "Senior Citizens Support Scheme",
    "allocatedResource": 750000,
    "allocationDate": "2024-01-20",
    "remainingAllocationPending": 750000
  },
  {
    "resourceId": 103,
    "programName": "Disability Assistance Program",
    "allocatedResource": 400000,
    "allocationDate": "2024-01-10",
    "remainingAllocationPending": 1200000
  }
]
```

**Note**: Resource 101 and 102 are both for the same program but allocated on different dates - shown as separate rows.

---

## 4. GET /api/AuditorDashboard/disbursement-statement

### Request (No Filters)
```
GET https://localhost:7100/api/AuditorDashboard/disbursement-statement
```

### Response (200 OK)
```json
[
  {
    "citizenId": 501,
    "citizenName": "Ramesh Kumar",
    "maxBenefitOfProgram": 5000,
    "benefitAllocatedByOfficer": 4000,
    "disbursed": 2000,
    "remainDisburse": 2000,
    "disbursementDate": "2024-03-10",
    "disbursementStatus": "Completed"
  },
  {
    "citizenId": 502,
    "citizenName": "Priya Sharma",
    "maxBenefitOfProgram": 5000,
    "benefitAllocatedByOfficer": 3500,
    "disbursed": 3500,
    "remainDisburse": 0,
    "disbursementDate": "2024-03-09",
    "disbursementStatus": "Completed"
  },
  {
    "citizenId": 501,
    "citizenName": "Ramesh Kumar",
    "maxBenefitOfProgram": 5000,
    "benefitAllocatedByOfficer": 4000,
    "disbursed": 2000,
    "remainDisburse": 2000,
    "disbursementDate": "2024-02-15",
    "disbursementStatus": "Completed"
  }
]
```

---

### Request (Filter by Citizen ID)
```
GET https://localhost:7100/api/AuditorDashboard/disbursement-statement?citizenId=501
```

### Response (200 OK)
```json
[
  {
    "citizenId": 501,
    "citizenName": "Ramesh Kumar",
    "maxBenefitOfProgram": 5000,
    "benefitAllocatedByOfficer": 4000,
    "disbursed": 2000,
    "remainDisburse": 2000,
    "disbursementDate": "2024-03-10",
    "disbursementStatus": "Completed"
  },
  {
    "citizenId": 501,
    "citizenName": "Ramesh Kumar",
    "maxBenefitOfProgram": 5000,
    "benefitAllocatedByOfficer": 4000,
    "disbursed": 2000,
    "remainDisburse": 2000,
    "disbursementDate": "2024-02-15",
    "disbursementStatus": "Completed"
  }
]
```

---

### Request (Filter by Date Range)
```
GET https://localhost:7100/api/AuditorDashboard/disbursement-statement?fromDate=2024-03-01&toDate=2024-03-15
```

### Response (200 OK)
```json
[
  {
    "citizenId": 501,
    "citizenName": "Ramesh Kumar",
    "maxBenefitOfProgram": 5000,
    "benefitAllocatedByOfficer": 4000,
    "disbursed": 2000,
    "remainDisburse": 2000,
    "disbursementDate": "2024-03-10",
    "disbursementStatus": "Completed"
  },
  {
    "citizenId": 502,
    "citizenName": "Priya Sharma",
    "maxBenefitOfProgram": 5000,
    "benefitAllocatedByOfficer": 3500,
    "disbursed": 3500,
    "remainDisburse": 0,
    "disbursementDate": "2024-03-09",
    "disbursementStatus": "Completed"
  }
]
```

---

### Request (Filter by Both)
```
GET https://localhost:7100/api/AuditorDashboard/disbursement-statement?citizenId=501&fromDate=2024-03-01&toDate=2024-03-15
```

### Response (200 OK)
```json
[
  {
    "citizenId": 501,
    "citizenName": "Ramesh Kumar",
    "maxBenefitOfProgram": 5000,
    "benefitAllocatedByOfficer": 4000,
    "disbursed": 2000,
    "remainDisburse": 2000,
    "disbursementDate": "2024-03-10",
    "disbursementStatus": "Completed"
  }
]
```

---

## Response Codes

| Code | Meaning |
|------|---------|
| 200 | Success - Data returned |
| 204 | No Content - No data available |
| 400 | Bad Request - Invalid parameters |
| 404 | Not Found - Resource doesn't exist |
| 500 | Server Error - Database error |

---

## Data Types

| Field | Type | Format | Example |
|-------|------|--------|---------|
| totalApplications | int | Positive integer | 150 |
| totalBudget | decimal | Currency | 5000000 |
| remainingResource | decimal | Currency | 1000000 |
| utilizationPercentage | decimal | Percentage (0-100) | 71.43 |
| citizenId | int | Positive integer | 501 |
| disbursementDate | string | ISO 8601 (YYYY-MM-DD) | 2024-03-10 |
| disbursementStatus | string | Enum | "Completed", "Pending" |

---

## Error Responses

### 500 Server Error
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
  "title": "An error occurred while processing your request.",
  "status": 500,
  "traceId": "0HN1GMVT5LG8U:00000001"
}
```

---

## Notes

- All dates are in ISO 8601 format (YYYY-MM-DD)
- All currency values are in Indian Rupees (₹)
- Percentages are decimal (0-100), not fractions
- Empty arrays `[]` indicate no matching records
- All responses are JSON (application/json)
- Query parameters are case-sensitive: `citizenId` not `CitizenId`

---

## Using cURL

```bash
# Get statistics
curl -X GET "https://localhost:7100/api/AuditorDashboard/statistics" \
  -H "Content-Type: application/json" \
  -k

# Get program breakdown
curl -X GET "https://localhost:7100/api/AuditorDashboard/program-breakdown" \
  -H "Content-Type: application/json" \
  -k

# Get disbursements for specific citizen
curl -X GET "https://localhost:7100/api/AuditorDashboard/disbursement-statement?citizenId=501" \
  -H "Content-Type: application/json" \
  -k
```

## Using Postman

1. Create new Collection: "Auditor API"
2. Create requests for each endpoint:
   - GET {{api_url}}/api/AuditorDashboard/statistics
   - GET {{api_url}}/api/AuditorDashboard/program-breakdown
   - GET {{api_url}}/api/AuditorDashboard/resource-statement
   - GET {{api_url}}/api/AuditorDashboard/disbursement-statement
3. Set variable: `api_url = https://localhost:7100`
4. Disable SSL verification in Postman settings for development
