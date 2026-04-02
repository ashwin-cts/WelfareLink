# 🚀 HttpClientFactory Integration Guide for CitizenController

## ✅ What Was Done

Added `IHttpClientFactory` to your `CitizenController` constructor properly:

```csharp
public CitizenController(
    ICitizenService citizenService,
    ICitizenDocumentService documentService,
    IWelfareProgramService programService,
    IWelfareApplicationService applicationService,
    WelfareLinkDbContext context,
    IHttpClientFactory clientFactory)  // ← ADDED
{
    _citizenService = citizenService;
    _documentService = documentService;
    _programService = programService;
    _applicationService = applicationService;
    _context = context;
    _clientFactory = clientFactory;  // ← ASSIGNED
}
```

---

## 🎯 Your API-First Transition Plan

Since your whole project is moving to API, here's the pattern to follow:

### **Current Architecture (Service-Based)**
```
Controller → Service → Repository → Database
```

### **Future Architecture (API-Based)**
```
Controller → API Client (HttpClientFactory) → API → Service → Database
```

---

## 📝 Example Implementations

### **Example 1: Dashboard Method (Using API)**

#### Before (Direct Service):
```csharp
public async Task<IActionResult> Dashboard()
{
    var userId = HttpContext.Session.GetInt32("UserId");
    var citizenProfile = await _citizenService.GetCitizenByUserIdAsync(userId.Value);
    // ...
}
```

#### After (Using API via HttpClientFactory):
```csharp
public async Task<IActionResult> Dashboard()
{
    var userId = HttpContext.Session.GetInt32("UserId");
    var userRole = HttpContext.Session.GetString("UserRole");

    if (userId == null || userRole != "Citizen")
    {
        return RedirectToAction("Login", "Account");
    }

    try
    {
        var client = _clientFactory.CreateClient("WelfareLinkApi");
        var response = await client.GetAsync($"api/citizen/user/{userId}");
        
        if (response.IsSuccessStatusCode)
        {
            var citizenProfile = await response.Content.ReadFromJsonAsync<Citizen>();
            
            // Get documents from API as well
            var docsResponse = await client.GetAsync($"api/citizen/{citizenProfile.CitizenId}/documents");
            var documents = await docsResponse.Content.ReadFromJsonAsync<List<CitizenDocument>>();
            
            var viewModel = new CitizenDashboardViewModel
            {
                CitizenProfile = citizenProfile,
                Documents = documents,
                PendingDocuments = documents.Count(d => d.VerificationStatus == "Pending"),
                ApprovedDocuments = documents.Count(d => d.VerificationStatus == "Approved"),
                RejectedDocuments = documents.Count(d => d.VerificationStatus == "Rejected")
            };

            return View(viewModel);
        }

        return RedirectToAction("Login", "Account");
    }
    catch (Exception ex)
    {
        // Log error and fallback to service
        return View(new CitizenDashboardViewModel());
    }
}
```

---

### **Example 2: EditProfile Method (POST with API)**

#### Before (Direct Service):
```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> EditProfile(EditCitizenViewModel model)
{
    if (!ModelState.IsValid)
        return View(model);

    var citizen = await _citizenService.GetCitizenByIdAsync(model.CitizenId);
    // ... update and save
}
```

#### After (Using API via HttpClientFactory):
```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> EditProfile(EditCitizenViewModel model)
{
    if (!ModelState.IsValid)
        return View(model);

    try
    {
        var client = _clientFactory.CreateClient("WelfareLinkApi");
        
        // Create Citizen object to send to API
        var citizenData = new
        {
            citizenId = model.CitizenId,
            userId = 0, // Will be filled by API
            name = model.Name,
            dateOfBirth = model.DateOfBirth,
            address = model.Address,
            contactInfo = model.ContactInfo,
            status = model.Status,
            gender = model.Gender
        };
        
        var response = await client.PutAsJsonAsync($"api/citizen/{model.CitizenId}", citizenData);
        
        if (response.IsSuccessStatusCode)
        {
            HttpContext.Session.SetString("CitizenGender", model.Gender);
            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction(nameof(Dashboard));
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, "Failed to update profile through API");
            return View(model);
        }
    }
    catch (Exception ex)
    {
        ModelState.AddModelError(string.Empty, $"Error updating profile: {ex.Message}");
        return View(model);
    }
}
```

---

### **Example 3: GetAllPrograms (Simple GET)**

```csharp
private async Task<List<WelfareProgram>> GetProgramsFromApi()
{
    try
    {
        var client = _clientFactory.CreateClient("WelfareLinkApi");
        var response = await client.GetAsync("api/program");  // Assuming you'll have this endpoint
        
        if (response.IsSuccessStatusCode)
        {
            var programs = await response.Content.ReadFromJsonAsync<List<WelfareProgram>>();
            return programs ?? new List<WelfareProgram>();
        }
    }
    catch (Exception ex)
    {
        // Log error
        Console.WriteLine($"Error fetching programs from API: {ex.Message}");
    }
    
    return new List<WelfareProgram>();
}
```

---

## 🔄 Migration Strategy (Step-by-Step)

### **Phase 1: Add API Clients (Current)**
✅ Add `IHttpClientFactory` to controllers

### **Phase 2: Create Wrapper Methods**
Create private helper methods that call your API:
```csharp
private async Task<Citizen> GetCitizenFromApiAsync(int citizenId)
{
    var client = _clientFactory.CreateClient("WelfareLinkApi");
    var response = await client.GetAsync($"api/citizen/{citizenId}");
    
    if (response.IsSuccessStatusCode)
        return await response.Content.ReadFromJsonAsync<Citizen>();
    
    return null;
}

private async Task<bool> UpdateCitizenViaApiAsync(Citizen citizen)
{
    var client = _clientFactory.CreateClient("WelfareLinkApi");
    var response = await client.PutAsJsonAsync($"api/citizen/{citizen.CitizenId}", citizen);
    return response.IsSuccessStatusCode;
}
```

### **Phase 3: Replace Service Calls Gradually**
Replace one method at a time:
```csharp
// Old way:
var citizen = await _citizenService.GetCitizenByIdAsync(id);

// New way:
var citizen = await GetCitizenFromApiAsync(id);
```

### **Phase 4: Remove Direct Service Dependencies**
Once all methods use API, you can remove direct service injection.

---

## 💡 Best Practices for Your Migration

### **1. Use Try-Catch for API Calls**
```csharp
try
{
    var client = _clientFactory.CreateClient("WelfareLinkApi");
    var response = await client.GetAsync("api/citizen");
    // ...
}
catch (HttpRequestException ex)
{
    // Network error
    ModelState.AddModelError(string.Empty, "Unable to connect to API");
}
catch (Exception ex)
{
    // Other errors
    ModelState.AddModelError(string.Empty, "An error occurred");
}
```

### **2. Implement Timeout**
```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
var response = await client.GetAsync("api/citizen", cancellationToken: cts.Token);
```

### **3. Create a Helper Service for Common API Calls**
```csharp
public interface IWelfareLinkApiClient
{
    Task<Citizen> GetCitizenAsync(int id);
    Task<bool> UpdateCitizenAsync(Citizen citizen);
    Task<List<WelfareApplication>> GetApplicationsAsync(int citizenId);
}

public class WelfareLinkApiClient : IWelfareLinkApiClient
{
    private readonly IHttpClientFactory _clientFactory;
    
    public WelfareLinkApiClient(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }
    
    public async Task<Citizen> GetCitizenAsync(int id)
    {
        var client = _clientFactory.CreateClient("WelfareLinkApi");
        var response = await client.GetAsync($"api/citizen/{id}");
        
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<Citizen>();
        
        return null;
    }
    
    // ... other methods
}
```

Then register in Program.cs:
```csharp
builder.Services.AddScoped<IWelfareLinkApiClient, WelfareLinkApiClient>();
```

And use in controller:
```csharp
public class CitizenController : Controller
{
    private readonly IWelfareLinkApiClient _apiClient;
    
    public CitizenController(IWelfareLinkApiClient apiClient)
    {
        _apiClient = apiClient;
    }
    
    public async Task<IActionResult> Dashboard()
    {
        var citizen = await _apiClient.GetCitizenAsync(userId);
        // ...
    }
}
```

---

## 📋 Checklist for Full Migration

- [ ] All API controllers created
- [ ] HttpClientFactory added to all controllers
- [ ] Helper methods implemented in controllers
- [ ] Try-catch error handling added
- [ ] IWelfareLinkApiClient service created (recommended)
- [ ] All tests passing
- [ ] Direct service calls removed from controllers
- [ ] Database context removed from controllers
- [ ] Controllers are now thin, API calls do the work

---

## ⚙️ Configuration (Already Done)

In your `Program.cs`, you have:

```csharp
builder.Services.AddHttpClient("WelfareLinkApi", client =>
{
    client.BaseAddress = new Uri("https://localhost:7100/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});
```

This means you can use:
```csharp
var client = _clientFactory.CreateClient("WelfareLinkApi");
var response = await client.GetAsync("api/citizen");  // Full URL = https://localhost:7100/api/citizen
```

---

## 🔗 Common API Patterns

### **GET - Fetch Data**
```csharp
var client = _clientFactory.CreateClient("WelfareLinkApi");
var response = await client.GetAsync("api/citizen/1");
var citizen = await response.Content.ReadFromJsonAsync<Citizen>();
```

### **POST - Create Data**
```csharp
var client = _clientFactory.CreateClient("WelfareLinkApi");
var response = await client.PostAsJsonAsync("api/citizen", citizen);
var createdCitizen = await response.Content.ReadFromJsonAsync<Citizen>();
```

### **PUT - Update Data**
```csharp
var client = _clientFactory.CreateClient("WelfareLinkApi");
var response = await client.PutAsJsonAsync($"api/citizen/{id}", citizen);
```

### **DELETE - Remove Data**
```csharp
var client = _clientFactory.CreateClient("WelfareLinkApi");
var response = await client.DeleteAsync($"api/citizen/{id}");
```

---

## 🎯 Next Steps

1. **✅ HttpClientFactory is now injected** - You can start using it!

2. **Create helper methods** - Wrap common API calls

3. **Update methods one by one** - Don't change everything at once

4. **Test each change** - Make sure the API responds correctly

5. **Create IWelfareLinkApiClient service** - Centralize all API calls (recommended)

6. **Remove service dependencies** - Once API is your source of truth

---

## 📚 Example Project Structure (Future State)

```
WelfareLink/
├── API_Controller/           ← Your APIs
│   ├── CitizenApiController.cs
│   ├── ProgramApiController.cs
│   ├── ApplicationApiController.cs
│   └── ...
│
├── Controllers/              ← Thin controllers
│   ├── CitizenController.cs (uses API)
│   ├── ProgramController.cs (uses API)
│   └── ...
│
├── ApiClients/               ← Centralized API calls
│   ├── IWelfareLinkApiClient.cs
│   ├── WelfareLinkApiClient.cs
│   └── ...
│
└── Views/                    ← Presentation only
    ├── Citizen/
    ├── Program/
    └── ...
```

---

## ✨ Benefits of This Approach

✅ **Separation of Concerns** - Controllers focus on HTTP, API handles logic  
✅ **Easy to Scale** - API can be deployed separately  
✅ **Easy Testing** - Mock the API client  
✅ **Future Proof** - Mobile/web clients can use same API  
✅ **Gradual Migration** - Update one method at a time  

---

**You're all set to start migrating to API-first architecture!** 🚀

Start with Example 1 and update one method at a time. Let me know if you need help with any specific method!
