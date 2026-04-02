# WelfareLink API Flow Guide

## 📊 Architecture Overview

```
┌──────────────────────────────────────────────────────────────────┐
│                         Your Application                          │
│                  https://localhost:7100                           │
└──────────────────────────────────────────────────────────────────┘
                              │
                ┌─────────────┴─────────────┐
                │                           │
                ▼                           ▼
    ┌──────────────────────┐     ┌──────────────────────┐
    │   MVC Controller     │     │   API Controller     │
    │   (Web Browser)      │     │   (JSON Response)    │
    │                      │     │                      │
    │ CitizenController    │     │ CitizenApiController │
    │                      │     │                      │
    │ - Dashboard()        │     │ - GET /api/citizen   │
    │ - CreateProfile()    │     │ - POST /api/citizen  │
    │ - EditProfile()      │     │ - PUT /api/citizen   │
    │ - MyApplications()   │     │ - DELETE /api/citizen│
    │                      │     │                      │
    │ Returns: HTML Views  │     │ Returns: JSON        │
    └──────┬───────────────┘     └────────┬─────────────┘
           │                              │
           └──────────────┬───────────────┘
                          │
           ┌──────────────▼───────────────┐
           │   Shared Services Layer      │
           │   (Business Logic & Data)    │
           │                              │
           │ - ICitizenService            │
           │ - ICitizenRepository         │
           │ - WelfareLinkDbContext       │
           │                              │
           └──────────────┬───────────────┘
                          │
           ┌──────────────▼───────────────┐
           │      SQL Server Database     │
           └──────────────────────────────┘
```

---

## 🔄 Three Ways to Access Your API

### **1️⃣ From Web Browser (Direct Access)**
```
URL: https://localhost:7100/api/citizen
Result: JSON response showing all citizens

GET https://localhost:7100/api/citizen
GET https://localhost:7100/api/citizen/5
GET https://localhost:7100/api/citizen/user/123
```

### **2️⃣ From MVC Controller (Using HttpClientFactory)**
This is what your sample code shows! Your MVC controller can call the API:

```csharp
public class YourController : Controller
{
    private readonly IHttpClientFactory _clientFactory;

    public YourController(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task<IActionResult> GetCitizenData()
    {
        var client = _clientFactory.CreateClient("WelfareLinkApi");
        var response = await client.GetAsync("api/citizen/5");
        
        if (response.IsSuccessStatusCode)
        {
            var citizen = await response.Content.ReadFromJsonAsync<Citizen>();
            return View(citizen);
        }
        return View();
    }
}
```

### **3️⃣ From External Client (Postman, Mobile App, etc.)**
```
Tools: Postman, Insomnia, cURL, JavaScript fetch()
Base URL: https://localhost:7100/api/citizen

GET    https://localhost:7100/api/citizen
POST   https://localhost:7100/api/citizen
PUT    https://localhost:7100/api/citizen/5
DELETE https://localhost:7100/api/citizen/5
```

---

## 📝 API Endpoints Reference

### **Get All Citizens**
```
GET /api/citizen

Response (200 OK):
[
  {
    "citizenId": 1,
    "userId": 1,
    "name": "John Doe",
    "dateOfBirth": "1990-01-15",
    "address": "123 Main St",
    "contactInfo": "555-1234",
    "status": "Active",
    "gender": "Male",
    "createdAt": "2024-01-01T00:00:00"
  }
]
```

### **Get Citizen by ID**
```
GET /api/citizen/5

Response (200 OK):
{
  "citizenId": 5,
  "userId": 5,
  "name": "Jane Smith",
  ...
}

Response (404 Not Found):
{
  "message": "Citizen with ID 5 not found"
}
```

### **Get Citizen by User ID**
```
GET /api/citizen/user/123

Response (200 OK):
{
  "citizenId": 5,
  "userId": 123,
  "name": "Jane Smith",
  ...
}
```

### **Get Citizen Documents**
```
GET /api/citizen/5/documents

Response (200 OK):
[
  {
    "documentId": 1,
    "citizenId": 5,
    "docType": "NationalID",
    "fileName": "id.pdf",
    "verificationStatus": "Approved"
  }
]
```

### **Get Citizen Applications**
```
GET /api/citizen/5/applications

Response (200 OK):
[
  {
    "applicationId": 1,
    "citizenId": 5,
    "programId": 2,
    "status": "Pending",
    "submittedDate": "2024-01-15"
  }
]
```

### **Create Citizen**
```
POST /api/citizen
Content-Type: application/json

Body:
{
  "citizenId": 0,
  "userId": 123,
  "name": "New Citizen",
  "dateOfBirth": "1990-01-15",
  "address": "456 Oak Ave",
  "contactInfo": "555-5678",
  "status": "Active",
  "gender": "Female"
}

Response (201 Created):
{
  "citizenId": 6,
  "userId": 123,
  ...
}
```

### **Update Citizen**
```
PUT /api/citizen/5
Content-Type: application/json

Body:
{
  "citizenId": 5,
  "userId": 5,
  "name": "Updated Name",
  "address": "New Address",
  ...
}

Response (200 OK):
{
  "message": "Citizen profile updated successfully"
}
```

### **Delete Citizen**
```
DELETE /api/citizen/5

Response (200 OK):
{
  "message": "Citizen profile deleted successfully"
}
```

---

## ✅ Data Flow Between MVC and API

### **Example: User Creates a Citizen Profile**

```
Step 1: User submits form in Browser
        ↓
Step 2: CitizenController.CreateProfile() [MVC] receives request
        ↓
Step 3: Validates data with ModelState
        ↓
Step 4: Calls ICitizenService.CreateCitizenProfileAsync()
        ↓
Step 5: Service calls ICitizenRepository
        ↓
Step 6: Repository saves to Database
        ↓
Step 7: MVC returns HTML View response

---

OPTIONAL: External Client calls same API:

Step 1: Mobile App sends JSON POST to /api/citizen
        ↓
Step 2: CitizenApiController.CreateCitizen() receives request
        ↓
Step 3: Validates data with ModelState
        ↓
Step 4: Calls ICitizenService.CreateCitizenProfileAsync() [SAME SERVICE]
        ↓
Step 5: Service calls ICitizenRepository [SAME REPOSITORY]
        ↓
Step 6: Repository saves to Database [SAME DATABASE]
        ↓
Step 7: API returns JSON response
```

---

## 🔐 Understanding Validations

### **MVC Controller Validations** (CitizenController.cs)
```csharp
[ValidateAntiForgeryToken]  // Prevents CSRF attacks
if (!ModelState.IsValid)     // Validates ViewModels
    return View(model);

// Plus Custom Validations:
- Check if username already exists
- Check user role from session
```

### **API Controller Validations** (CitizenApiController.cs)
```csharp
// NO [ValidateAntiForgeryToken] (stateless)
// NO ViewModels (uses Models directly)

if (citizen == null)                    // Basic null check
    return BadRequest(...);

if (id != citizen.CitizenId)           // ID mismatch check
    return BadRequest(...);
```

### **Shared Service Validations** (Both Use)
```csharp
// ICitizenService methods validate:
- Database constraints
- Required fields
- Data integrity
- Business logic rules
```

---

## 🧪 How to Test Your API

### **Test 1: Using Postman**
1. Open Postman
2. Create a new request
3. Method: **GET**
4. URL: `https://localhost:7100/api/citizen`
5. Headers: Add `Accept: application/json`
6. Click **Send**

### **Test 2: Using Insomnia**
1. Open Insomnia
2. New Request
3. Method: **GET**
4. URL: `https://localhost:7100/api/citizen`
5. Send

### **Test 3: Using Browser**
1. Navigate to: `https://localhost:7100/api/citizen`
2. You should see JSON response

### **Test 4: Using Command Line (PowerShell)**
```powershell
# GET all citizens
Invoke-WebRequest -Uri "https://localhost:7100/api/citizen" -Method Get

# GET citizen by ID
Invoke-WebRequest -Uri "https://localhost:7100/api/citizen/5" -Method Get

# POST create citizen
$body = @{
    citizenId = 0
    userId = 123
    name = "Test Citizen"
    dateOfBirth = "1990-01-15"
    address = "Test Address"
    contactInfo = "555-1234"
    status = "Active"
    gender = "Male"
} | ConvertTo-Json

Invoke-WebRequest -Uri "https://localhost:7100/api/citizen" `
    -Method Post `
    -Headers @{"Content-Type"="application/json"} `
    -Body $body
```

### **Test 5: Using cURL**
```bash
# GET
curl -X GET "https://localhost:7100/api/citizen"

# POST
curl -X POST "https://localhost:7100/api/citizen" \
  -H "Content-Type: application/json" \
  -d '{"citizenId":0,"userId":123,"name":"Test","dateOfBirth":"1990-01-15","address":"123 St","contactInfo":"555-1234","status":"Active","gender":"Male"}'
```

---

## 🚀 Quick Start Checklist

- ✅ HttpClientFactory added to Program.cs with port 7100
- ✅ CitizenApiController created with full CRUD endpoints
- ✅ All services and repositories registered
- ✅ Database configured
- ✅ Session configured for MVC

### **Next Steps:**
1. Run your application: `F5` or `dotnet run`
2. Test the API using one of the methods above
3. Use CitizenApiClientController as a reference to integrate API calls in your MVC controllers
4. Both MVC and API share the same business logic (services)

---

## 📌 Important Notes

1. **Both MVC and API share the same services** - Changes to business logic affect both
2. **Port 7100** - Make sure this matches your launch settings
3. **HTTPS** - Your app uses HTTPS by default
4. **No conflicts** - Both can run simultaneously without affecting each other
5. **Database** - Both MVC and API use the same database

Good luck! 🎉
