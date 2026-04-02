# ✅ HttpClientFactory Setup - Quick Reference

## What's Done

Your `CitizenController` now has proper HttpClientFactory integration:

```csharp
public class CitizenController : Controller
{
    private readonly IHttpClientFactory _clientFactory;  // ← Ready to use
    
    public CitizenController(
        ICitizenService citizenService,
        ICitizenDocumentService documentService,
        IWelfareProgramService programService,
        IWelfareApplicationService applicationService,
        WelfareLinkDbContext context,
        IHttpClientFactory clientFactory)  // ← Injected
    {
        _clientFactory = clientFactory;  // ← Assigned
        // ... other assignments
    }
}
```

✅ Build: **Successful**  
✅ Configuration: **Already in Program.cs**  
✅ Ready to use: **Yes**

---

## Quick Pattern Examples

### Pattern 1: GET Single Item
```csharp
var client = _clientFactory.CreateClient("WelfareLinkApi");
var response = await client.GetAsync($"api/citizen/{citizenId}");

if (response.IsSuccessStatusCode)
{
    var citizen = await response.Content.ReadFromJsonAsync<Citizen>();
    // Use citizen data
}
```

### Pattern 2: POST Create Item
```csharp
var client = _clientFactory.CreateClient("WelfareLinkApi");
var response = await client.PostAsJsonAsync("api/citizen", citizenData);

if (response.IsSuccessStatusCode)
{
    var created = await response.Content.ReadFromJsonAsync<Citizen>();
    // Handle success
}
```

### Pattern 3: PUT Update Item
```csharp
var client = _clientFactory.CreateClient("WelfareLinkApi");
var response = await client.PutAsJsonAsync($"api/citizen/{id}", updatedData);

if (response.IsSuccessStatusCode)
{
    // Handle success
}
```

### Pattern 4: DELETE Item
```csharp
var client = _clientFactory.CreateClient("WelfareLinkApi");
var response = await client.DeleteAsync($"api/citizen/{id}");

if (response.IsSuccessStatusCode)
{
    // Item deleted
}
```

---

## Your API Base URL

```
https://localhost:7100/api/
```

### Available Endpoints (Created):
- ✅ GET /api/citizen
- ✅ GET /api/citizen/{id}
- ✅ GET /api/citizen/user/{userId}
- ✅ GET /api/citizen/{citizenId}/documents
- ✅ GET /api/citizen/{citizenId}/applications
- ✅ POST /api/citizen
- ✅ PUT /api/citizen/{id}
- ✅ DELETE /api/citizen/{id}

---

## Start Using It

Pick any method in CitizenController and update it to use the API instead of services.

Example methods to migrate:
1. Dashboard() - Easy GET
2. EditProfile() - POST with data
3. ProgramList() - Multiple API calls
4. SelectDocuments() - Complex logic

See **HTTPCLIENT_INTEGRATION_GUIDE.md** for full examples of each!

---

## 🚀 You're Ready!

Your project is now set up to gradually migrate to API-first architecture.

Start small, test thoroughly, and expand from there! ✨
