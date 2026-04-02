# 🚀 Step-by-Step: How to Use Your Citizen API

## Your Current Setup

✅ **API Controller:** `CitizenApiController.cs` in `/API_Controller/` folder  
✅ **MVC Controller:** `CitizenController.cs` in `/Controllers/` folder  
✅ **Shared Services:** Both use `ICitizenService`, `ICitizenDocumentService`, etc.  
✅ **Database:** Shared `WelfareLinkDbContext`  
✅ **Port:** `7100` (configured in Program.cs)  
✅ **HttpClientFactory:** Added with name "WelfareLinkApi"  

---

## 🎯 Three Ways Your API Works

### **1. Direct Browser Access (Read-Only)**

Open your browser and visit:
```
https://localhost:7100/api/citizen
```

You'll see JSON response like:
```json
[
  {
    "citizenId": 1,
    "userId": 1,
    "name": "John Doe",
    "status": "Active",
    ...
  }
]
```

**Other URLs you can try:**
```
https://localhost:7100/api/citizen/1
https://localhost:7100/api/citizen/user/5
https://localhost:7100/api/citizen/1/documents
https://localhost:7100/api/citizen/1/applications
```

---

### **2. From Postman (Test All Methods)**

**Install Postman** (if not already installed), then:

#### **Test GET All Citizens:**
```
Method:  GET
URL:     https://localhost:7100/api/citizen
Headers: Accept: application/json
Body:    (empty)

Click Send → You get JSON list
```

#### **Test GET Single Citizen:**
```
Method:  GET
URL:     https://localhost:7100/api/citizen/1
Headers: Accept: application/json
Body:    (empty)

Click Send → You get single citizen JSON
```

#### **Test POST (Create):**
```
Method:  POST
URL:     https://localhost:7100/api/citizen
Headers: Content-Type: application/json
Body:    
{
  "citizenId": 0,
  "userId": 123,
  "name": "Jane Smith",
  "dateOfBirth": "1995-05-20",
  "address": "789 Oak Ave",
  "contactInfo": "555-9999",
  "status": "Active",
  "gender": "Female"
}

Click Send → You get 201 Created with new citizen data
```

#### **Test PUT (Update):**
```
Method:  PUT
URL:     https://localhost:7100/api/citizen/1
Headers: Content-Type: application/json
Body:
{
  "citizenId": 1,
  "userId": 1,
  "name": "John Updated",
  "dateOfBirth": "1990-01-15",
  "address": "Updated Address",
  "contactInfo": "555-1111",
  "status": "Active",
  "gender": "Male"
}

Click Send → You get 200 OK
```

#### **Test DELETE:**
```
Method:  DELETE
URL:     https://localhost:7100/api/citizen/1
Headers: (none)
Body:    (empty)

Click Send → You get 200 OK
```

---

### **3. From Your MVC Controllers**

You can call the API from any MVC controller using the HttpClientFactory:

#### **Example 1: Simple GET in Your Controller**

```csharp
using Microsoft.AspNetCore.Mvc;

public class ReportController : Controller
{
    private readonly IHttpClientFactory _clientFactory;

    public ReportController(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task<IActionResult> ExportCitizensReport()
    {
        try
        {
            var client = _clientFactory.CreateClient("WelfareLinkApi");
            var response = await client.GetAsync("api/citizen");

            if (response.IsSuccessStatusCode)
            {
                var citizens = await response.Content.ReadFromJsonAsync<List<Citizen>>();
                
                // Now you have all citizens data
                // Export to CSV, PDF, or process further
                
                return File(ExportToCsv(citizens), "text/csv", "citizens.csv");
            }

            return BadRequest("Failed to fetch citizens from API");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    // Helper method to convert to CSV
    private byte[] ExportToCsv(List<Citizen> citizens)
    {
        // Your CSV conversion logic here
        return new byte[] { };
    }
}
```

#### **Example 2: GET by ID from Your Controller**

```csharp
public async Task<IActionResult> ViewCitizenDetails(int citizenId)
{
    var client = _clientFactory.CreateClient("WelfareLinkApi");
    var response = await client.GetAsync($"api/citizen/{citizenId}");

    if (response.IsSuccessStatusCode)
    {
        var citizen = await response.Content.ReadFromJsonAsync<Citizen>();
        return View(citizen);
    }

    return NotFound();
}
```

#### **Example 3: POST from Your Controller**

```csharp
[HttpPost]
public async Task<IActionResult> RegisterCitizenViaApi([FromBody] Citizen citizen)
{
    var client = _clientFactory.CreateClient("WelfareLinkApi");
    var response = await client.PostAsJsonAsync("api/citizen", citizen);

    if (response.IsSuccessStatusCode)
    {
        var newCitizen = await response.Content.ReadFromJsonAsync<Citizen>();
        return CreatedAtAction("ViewCitizenDetails", new { citizenId = newCitizen.CitizenId }, newCitizen);
    }

    return BadRequest("Failed to create citizen");
}
```

#### **Example 4: GET Documents from Your Controller**

```csharp
public async Task<IActionResult> GetCitizenDocuments(int citizenId)
{
    var client = _clientFactory.CreateClient("WelfareLinkApi");
    var response = await client.GetAsync($"api/citizen/{citizenId}/documents");

    if (response.IsSuccessStatusCode)
    {
        var documents = await response.Content.ReadFromJsonAsync<List<CitizenDocument>>();
        return View(documents);
    }

    return View(new List<CitizenDocument>());
}
```

---

## 📊 Comparison: MVC vs API

| Feature | MVC Controller | API Controller |
|---------|----------------|----------------|
| **Returns** | HTML Views | JSON Data |
| **Session Check** | ✅ Yes | ❌ No (Stateless) |
| **ViewModels** | ✅ Uses validation | ❌ Uses Models directly |
| **Access From** | Web Browsers | Any Client (Mobile, External Apps, etc.) |
| **CSRF Token** | ✅ Required | ❌ Not needed |
| **Example Route** | `/Citizen/Dashboard` | `/api/citizen` |

### **Key Point: Both Share the Same Services**
```
CitizenController (MVC)     ┐
                            ├─→ ICitizenService ─→ Database
CitizenApiController (API) ┘
```

So when you update a citizen via MVC, the API sees the same updated data!

---

## 🧪 Complete Testing Workflow

### **Test 1: Verify MVC Works**
1. Run app (`F5`)
2. Go to: `https://localhost:7100/Citizen/Dashboard`
3. You should see your dashboard
4. ✅ MVC is working

### **Test 2: Verify API Works Directly**
1. Open browser
2. Go to: `https://localhost:7100/api/citizen`
3. You should see JSON
4. ✅ API is working

### **Test 3: Verify Both Share Data**
1. Create a citizen via MVC dashboard
2. Go to API: `https://localhost:7100/api/citizen`
3. You should see the citizen you just created
4. ✅ Both share the same database

### **Test 4: Test API Full CRUD with Postman**
1. **CREATE**: POST to `/api/citizen` with new data
2. **READ**: GET from `/api/citizen/{id}`
3. **UPDATE**: PUT to `/api/citizen/{id}` with updated data
4. **DELETE**: DELETE to `/api/citizen/{id}`
5. ✅ All operations working

### **Test 5: Call API from MVC Controller**
1. Add code to any MVC controller using HttpClientFactory
2. Run and verify it calls the API successfully
3. ✅ MVC can call API

---

## 🔍 How Flow Works in Detail

### **Scenario: Mobile App Creates a Citizen**

```
Step 1: Mobile App
        POST to https://localhost:7100/api/citizen
        With JSON: { "name": "Bob", "userId": 10, ... }
                                    ↓
Step 2: CitizenApiController.CreateCitizen() runs
        - Checks if data is null
        - Validates model state
        - Calls ICitizenService.CreateCitizenProfileAsync()
                                    ↓
Step 3: ICitizenService (Shared)
        - Runs business logic validation
        - Calls ICitizenRepository.AddAsync()
                                    ↓
Step 4: ICitizenRepository
        - Calls _context.Citizens.AddAsync()
        - Saves to SQL Server Database
                                    ↓
Step 5: Database
        - INSERT new citizen record
        - Returns generated CitizenId
                                    ↓
Step 6: Back to API Controller
        - Returns 201 Created
        - With new citizen JSON: { "citizenId": 42, "name": "Bob", ... }
                                    ↓
Step 7: Mobile App Receives
        Status: 201
        Body: { "citizenId": 42, ... }
        ✅ Success!

---

Meanwhile: If MVC calls Dashboard:

GET: https://localhost:7100/Citizen/Dashboard
                                    ↓
CitizenController.Dashboard() runs
- Calls ICitizenService.GetCitizenByUserIdAsync()
                                    ↓
[SAME SERVICE LAYER - See the new "Bob" citizen in database!]
                                    ↓
Returns HTML view with updated data
✅ MVC also sees the same data!
```

---

## 📝 Key Takeaways

1. **Your API is at:** `https://localhost:7100/api/citizen`
2. **Your MVC is at:** `https://localhost:7100/Citizen/*`
3. **Both share:** Services, Repositories, and Database
4. **No conflicts:** They work independently but access same data
5. **HttpClientFactory:** Named "WelfareLinkApi" for easy access in MVC

---

## 🎓 Next Steps

1. **Test the API using Postman** (easiest way to start)
2. **Use CitizenApiClientController.cs as reference** (in /Controllers/)
3. **Add API calls to your existing MVC controllers** when needed
4. **Document your API** (you can add Swagger/OpenAPI later)
5. **Secure your API** (add authentication if needed)

---

**Happy coding! 🚀**
