using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;
using Microsoft.AspNetCore.Http;
using WelfareLink.Models;
using WelfareLink.ViewModels;

namespace WelfareLink.Services
{
    /// <summary>
    /// Typed HTTP client that calls all WelfareLinkApi endpoints.
    /// MVC controllers should inject this instead of individual services.
    /// </summary>
    public class WelfareApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpClientFactory _httpClientFactory;
        private static readonly JsonSerializerOptions _json = new()
        {
            PropertyNameCaseInsensitive = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public WelfareApiClient(HttpClient http, IHttpClientFactory httpClientFactory)
        {
            _http = http;
            _httpClientFactory = httpClientFactory;
        }

        // ──────────────────────────────────────────────
        // BENEFIT
        // ──────────────────────────────────────────────
        public async Task<IEnumerable<Benefit>> GetAllBenefitsAsync()
            => await _http.GetFromJsonAsync<IEnumerable<Benefit>>("api/benefitapi", _json) ?? Enumerable.Empty<Benefit>();

        public async Task<Benefit?> GetBenefitByIdAsync(int id)
            => await _http.GetFromJsonAsync<Benefit>($"api/benefitapi/{id}", _json);

        public async Task<bool> BenefitExistsAsync(int id)
            => (await GetBenefitByIdAsync(id)) != null;

        public async Task<(Benefit? benefit, string? error)> CreateBenefitAsync(Benefit benefit, int userId)
        {
            var response = await _http.PostAsJsonAsync($"api/benefitapi?officerId={userId}", benefit);
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var content = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrWhiteSpace(content))
                        return (null, null);
                    var result = JsonSerializer.Deserialize<BenefitResponse>(content, _json);
                    return (result?.Benefit, null);
                }
                catch
                {
                    return (null, null);
                }
            }
            try
            {
                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                    return (null, "Failed to create benefit.");
                var err = JsonSerializer.Deserialize<ErrorResponse>(content, _json);
                return (null, err?.Error ?? "Failed to create benefit.");
            }
            catch
            {
                return (null, "Failed to create benefit.");
            }
        }

        public async Task<(Benefit? benefit, string? error)> UpdateBenefitAsync(Benefit benefit, int userId)
        {
            var response = await _http.PutAsJsonAsync($"api/benefitapi/{benefit.BenefitID}?officerId={userId}", benefit);
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var content = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrWhiteSpace(content))
                        return (null, null);
                    var result = JsonSerializer.Deserialize<BenefitResponse>(content, _json);
                    return (result?.Benefit, null);
                }
                catch
                {
                    return (null, null);
                }
            }
            try
            {
                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                    return (null, "Failed to update benefit.");
                var err = JsonSerializer.Deserialize<ErrorResponse>(content, _json);
                return (null, err?.Error ?? "Failed to update benefit.");
            }
            catch
            {
                return (null, "Failed to update benefit.");
            }
        }

        public async Task DeleteBenefitAsync(int id)
            => await _http.DeleteAsync($"api/benefitapi/{id}");

        public async Task<DropdownData?> GetBenefitDropdownAsync(int? selectedId = null)
        {
            var url = selectedId.HasValue
                ? $"api/benefitapi/dropdown?selectedId={selectedId}"
                : "api/benefitapi/dropdown";
            return await _http.GetFromJsonAsync<DropdownData>(url, _json);
        }

        public async Task<ProgramResourceInfo?> GetProgramResourceInfoAsync(int programId)
            => await _http.GetFromJsonAsync<ProgramResourceInfo>($"api/benefitapi/program-resource-info/{programId}", _json);

        // ──────────────────────────────────────────────
        // DISBURSEMENT
        // ──────────────────────────────────────────────
        public async Task<IEnumerable<Disbursement>> GetAllDisbursementsAsync()
            => await _http.GetFromJsonAsync<IEnumerable<Disbursement>>("api/disbursementapi", _json) ?? Enumerable.Empty<Disbursement>();

        public async Task<DisbursementDetail?> GetDisbursementByIdAsync(int id)
            => await _http.GetFromJsonAsync<DisbursementDetail>($"api/disbursementapi/{id}", _json);

        public async Task<IEnumerable<Disbursement>> GetDisbursementsByBenefitIdAsync(int benefitId)
            => await _http.GetFromJsonAsync<IEnumerable<Disbursement>>($"api/disbursementapi/benefit/{benefitId}", _json) ?? [];

        public async Task<BenefitDetails?> GetDisbursementBenefitDetailsAsync(int benefitId)
            => await _http.GetFromJsonAsync<BenefitDetails>($"api/disbursementapi/benefit-details/{benefitId}", _json);

        public async Task<(Disbursement? disbursement, string? error)> CreateDisbursementAsync(Disbursement disbursement)
        {
            var response = await _http.PostAsJsonAsync("api/disbursementapi", disbursement);
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var content = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrWhiteSpace(content))
                        return (null, null);
                    var result = JsonSerializer.Deserialize<DisbursementResponse>(content, _json);
                    return (result?.Disbursement, null);
                }
                catch
                {
                    return (null, null);
                }
            }
            try
            {
                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                    return (null, "Failed to create disbursement.");
                var err = JsonSerializer.Deserialize<ErrorResponse>(content, _json);
                return (null, err?.Error ?? "Failed to create disbursement.");
            }
            catch
            {
                return (null, "Failed to create disbursement.");
            }
        }

        public async Task<(Disbursement? disbursement, string? error)> UpdateDisbursementAsync(Disbursement disbursement)
        {
            var response = await _http.PutAsJsonAsync($"api/disbursementapi/{disbursement.DisbursementID}", disbursement);
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var content = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrWhiteSpace(content))
                        return (null, null);
                    var result = JsonSerializer.Deserialize<DisbursementResponse>(content, _json);
                    return (result?.Disbursement, null);
                }
                catch
                {
                    return (null, null);
                }
            }
            try
            {
                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                    return (null, "Failed to update disbursement.");
                var err = JsonSerializer.Deserialize<ErrorResponse>(content, _json);
                return (null, err?.Error ?? "Failed to update disbursement.");
            }
            catch
            {
                return (null, "Failed to update disbursement.");
            }
        }

        public async Task<bool> DisbursementExistsAsync(int id)
        {
            var detail = await GetDisbursementByIdAsync(id);
            return detail?.Disbursement != null;
        }

        public async Task<string?> DeleteDisbursementAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/disbursementapi/{id}");
            if (response.IsSuccessStatusCode) return null;
            try
            {
                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                    return "Failed to delete disbursement.";
                var err = JsonSerializer.Deserialize<ErrorResponse>(content, _json);
                return err?.Error ?? "Failed to delete disbursement.";
            }
            catch
            {
                return "Failed to delete disbursement.";
            }
        }

        // ──────────────────────────────────────────────
        // ELIGIBILITY CHECK
        // ──────────────────────────────────────────────
        public async Task<IEnumerable<EligibilityCheck>> GetAllChecksAsync()
            => await _http.GetFromJsonAsync<IEnumerable<EligibilityCheck>>("api/eligibilitycheckapi", _json) ?? Enumerable.Empty<EligibilityCheck>();

        public async Task<EligibilityCheck?> GetCheckByIdAsync(int id)
            => await _http.GetFromJsonAsync<EligibilityCheck>($"api/eligibilitycheckapi/{id}", _json);

        public async Task<ApplicationInfo?> GetEligibilityApplicationInfoAsync(int applicationId)
            => await _http.GetFromJsonAsync<ApplicationInfo>($"api/eligibilitycheckapi/application-info/{applicationId}", _json);

        public async Task<EligibilityCheck?> CreateCheckAsync(EligibilityCheck check, int? applicationId)
        {
            var url = applicationId.HasValue
                ? $"api/eligibilitycheckapi?applicationId={applicationId}"
                : "api/eligibilitycheckapi";
            var response = await _http.PostAsJsonAsync(url, check);
            if (!response.IsSuccessStatusCode) return null;
            try
            {
                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                    return null;
                var result = JsonSerializer.Deserialize<EligibilityCheckResponse>(content, _json);
                return result?.Check;
            }
            catch
            {
                return null;
            }
        }

        public async Task UpdateCheckAsync(EligibilityCheck check)
            => await _http.PutAsJsonAsync($"api/eligibilitycheckapi/{check.CheckID}", check);

        public async Task DeleteCheckAsync(int id)
            => await _http.DeleteAsync($"api/eligibilitycheckapi/{id}");

        // ──────────────────────────────────────────────
        // RESOURCE
        // ──────────────────────────────────────────────
        public async Task<IEnumerable<Resource>> GetAllResourcesAsync()
            => await _http.GetFromJsonAsync<IEnumerable<Resource>>("api/resourceapi", _json) ?? Enumerable.Empty<Resource>();

        public async Task<ProgramResourceDetail?> GetResourcesByProgramIdAsync(int programId)
            => await _http.GetFromJsonAsync<ProgramResourceDetail>($"api/resourceapi/program/{programId}", _json);

        public async Task<IEnumerable<ResourceUtilisationViewModel>> GetResourceUtilisationAsync()
            => await _http.GetFromJsonAsync<IEnumerable<ResourceUtilisationViewModel>>("api/resourceapi/utilisation", _json) ?? Enumerable.Empty<ResourceUtilisationViewModel>();

        public async Task<string?> AddResourceAsync(Resource resource)
        {
            var response = await _http.PostAsJsonAsync("api/resourceapi", resource);

            try
            {
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    if (string.IsNullOrWhiteSpace(content))
                        return null;

                    try
                    {
                        var result = JsonSerializer.Deserialize<MessageResponse>(content, _json);
                        return null;
                    }
                    catch
                    {
                        return null;
                    }
                }

                if (string.IsNullOrWhiteSpace(content))
                    return "Failed to allocate resource.";

                try
                {
                    var err = JsonSerializer.Deserialize<ErrorResponse>(content, _json);
                    return err?.Error ?? "Failed to allocate resource.";
                }
                catch
                {
                    return "Failed to allocate resource.";
                }
            }
            catch (HttpRequestException ex)
            {
                return $"Connection error: {ex.Message}";
            }
        }

        public async Task<string?> UpdateResourceAsync(Resource resource)
        {
            var response = await _http.PutAsJsonAsync($"api/resourceapi/{resource.ResourceID}", resource);

            try
            {
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    if (string.IsNullOrWhiteSpace(content))
                        return null;

                    try
                    {
                        var result = JsonSerializer.Deserialize<MessageResponse>(content, _json);
                        return null;
                    }
                    catch
                    {
                        return null;
                    }
                }

                if (string.IsNullOrWhiteSpace(content))
                    return "Failed to update resource.";

                try
                {
                    var err = JsonSerializer.Deserialize<ErrorResponse>(content, _json);
                    return err?.Error ?? "Failed to update resource.";
                }
                catch
                {
                    return "Failed to update resource.";
                }
            }
            catch (HttpRequestException ex)
            {
                return $"Connection error: {ex.Message}";
            }
        }

        // ──────────────────────────────────────────────
        // WELFARE APPLICATION
        // ──────────────────────────────────────────────
        public async Task<IEnumerable<WelfareApplication>> GetAllApplicationsAsync(string? status = null)
        {
            var url = string.IsNullOrEmpty(status)
                ? "api/welfareapplicationapi"
                : $"api/welfareapplicationapi?status={status}";
            return await _http.GetFromJsonAsync<IEnumerable<WelfareApplication>>(url, _json) ?? Enumerable.Empty<WelfareApplication>();
        }

        public async Task<IEnumerable<WelfareApplication>> GetPendingApplicationsAsync()
            => await _http.GetFromJsonAsync<IEnumerable<WelfareApplication>>("api/welfareapplicationapi/pending", _json) ?? [];

        public async Task<WelfareApplication?> GetApplicationByIdAsync(int id)
            => await _http.GetFromJsonAsync<WelfareApplication>($"api/welfareapplicationapi/{id}", _json);

        public async Task<bool> ApplicationExistsAsync(int id)
            => (await GetApplicationByIdAsync(id)) != null;

        public async Task<WelfareApplication?> CreateApplicationAsync(WelfareApplication application)
        {
            var response = await _http.PostAsJsonAsync("api/welfareapplicationapi", application);
            if (!response.IsSuccessStatusCode) return null;
            try
            {
                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                    return null;
                var result = JsonSerializer.Deserialize<ApplicationResponse>(content, _json);
                return result?.Application;
            }
            catch
            {
                return null;
            }
        }

        public async Task UpdateApplicationAsync(WelfareApplication application)
            => await _http.PutAsJsonAsync($"api/welfareapplicationapi/{application.ApplicationID}", application);

        public async Task<bool> UpdateApplicationStatusAsync(int id, string status)
        {
            var response = await _http.PatchAsJsonAsync($"api/welfareapplicationapi/{id}/status", status);
            return response.IsSuccessStatusCode;
        }

        public async Task DeleteApplicationAsync(int id)
            => await _http.DeleteAsync($"api/welfareapplicationapi/{id}");

        public async Task<IEnumerable<WelfareApplication>> GetApplicationsByCitizenIdAsync(int citizenId)
        {
            var userMgmtClient = _httpClientFactory.CreateClient("UserManagement");
            return await userMgmtClient.GetFromJsonAsync<IEnumerable<WelfareApplication>>($"api/citizenapi/{citizenId}/applications", _json) ?? [];
        }

        public async Task<(bool success, string? error)> ApplyForProgramAsync(int citizenId, int programId, int[] selectedDocumentIds)
        {
            var userMgmtClient = _httpClientFactory.CreateClient("UserManagement");
            var payload = new { CitizenID = citizenId, ProgramID = programId, SelectedDocumentIds = selectedDocumentIds };
            var response = await userMgmtClient.PostAsJsonAsync("api/citizenapi/apply", payload);

            try
            {
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Success - API returns { Message: "...", ApplicationID: 123 }
                    if (string.IsNullOrWhiteSpace(content))
                        return (true, null);

                    try
                    {
                        var result = JsonSerializer.Deserialize<ApplicationSubmissionResponse>(content, _json);
                        return (true, null);
                    }
                    catch
                    {
                        return (true, null);
                    }
                }

                // Handle failure response
                if (string.IsNullOrWhiteSpace(content))
                    return (false, "Failed to submit application.");

                try
                {
                    var err = JsonSerializer.Deserialize<ErrorResponse>(content, _json);
                    return (false, err?.Error ?? "Failed to submit application.");
                }
                catch
                {
                    return (false, "Failed to submit application.");
                }
            }
            catch (HttpRequestException ex)
            {
                return (false, $"Connection error: {ex.Message}");
            }
        }

        // ──────────────────────────────────────────────
        // WELFARE PROGRAM
        // ──────────────────────────────────────────────
        public async Task<IEnumerable<WelfareProgram>> GetAllProgramsAsync()
            => await _http.GetFromJsonAsync<IEnumerable<WelfareProgram>>("api/welfareprogramapi", _json) ?? Enumerable.Empty<WelfareProgram>();

        public async Task<ProgramDetailViewModel?> GetProgramByIdAsync(int id)
            => await _http.GetFromJsonAsync<ProgramDetailViewModel>($"api/welfareprogramapi/{id}", _json);

        public async Task<BudgetDashboardViewModel?> GetBudgetMonitoringAsync()
            => await _http.GetFromJsonAsync<BudgetDashboardViewModel>("api/welfareprogramapi/budget-monitoring", _json);

        public async Task<IEnumerable<ProgramPerformanceViewModel>> GetProgramPerformanceAsync()
            => await _http.GetFromJsonAsync<IEnumerable<ProgramPerformanceViewModel>>("api/welfareprogramapi/performance", _json) ?? Enumerable.Empty<ProgramPerformanceViewModel>();

        public async Task<string?> AddProgramAsync(WelfareProgram program)
        {
            var response = await _http.PostAsJsonAsync("api/welfareprogramapi", program);

            try
            {
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    if (string.IsNullOrWhiteSpace(content))
                        return null;

                    try
                    {
                        var result = JsonSerializer.Deserialize<MessageResponse>(content, _json);
                        return null;
                    }
                    catch
                    {
                        return null;
                    }
                }

                if (string.IsNullOrWhiteSpace(content))
                    return "Failed to create program.";

                try
                {
                    var err = JsonSerializer.Deserialize<ErrorResponse>(content, _json);
                    return err?.Error ?? "Failed to create program.";
                }
                catch
                {
                    return "Failed to create program.";
                }
            }
            catch (HttpRequestException ex)
            {
                return $"Connection error: {ex.Message}";
            }
        }

        public async Task<string?> UpdateProgramAsync(WelfareProgram program)
        {
            var response = await _http.PutAsJsonAsync($"api/welfareprogramapi/{program.ProgramID}", program);

            try
            {
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    if (string.IsNullOrWhiteSpace(content))
                        return null;

                    try
                    {
                        var result = JsonSerializer.Deserialize<MessageResponse>(content, _json);
                        return null;
                    }
                    catch
                    {
                        return null;
                    }
                }

                if (string.IsNullOrWhiteSpace(content))
                    return "Failed to update program.";

                try
                {
                    var err = JsonSerializer.Deserialize<ErrorResponse>(content, _json);
                    return err?.Error ?? "Failed to update program.";
                }
                catch
                {
                    return "Failed to update program.";
                }
            }
            catch (HttpRequestException ex)
            {
                return $"Connection error: {ex.Message}";
            }
        }

        public async Task<string?> SuspendProgramAsync(int id)
        {
            var response = await _http.PatchAsync($"api/welfareprogramapi/{id}/suspend", null);

            try
            {
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    if (string.IsNullOrWhiteSpace(content))
                        return null;

                    try
                    {
                        var result = JsonSerializer.Deserialize<MessageResponse>(content, _json);
                        return null;
                    }
                    catch
                    {
                        return null;
                    }
                }

                if (string.IsNullOrWhiteSpace(content))
                    return "Failed to suspend programme.";

                try
                {
                    var err = JsonSerializer.Deserialize<ErrorResponse>(content, _json);
                    return err?.Error ?? "Failed to suspend programme.";
                }
                catch
                {
                    return "Failed to suspend programme.";
                }
            }
            catch (HttpRequestException ex)
            {
                return $"Connection error: {ex.Message}";
            }
        }

        // ──────────────────────────────────────────────
        // CITIZEN
        // ──────────────────────────────────────────────
        public async Task<Citizen?> GetCitizenByIdAsync(int id)
        {
            var userMgmtClient = _httpClientFactory.CreateClient("UserManagement");
            return await userMgmtClient.GetFromJsonAsync<Citizen>($"api/citizenapi/{id}", _json);
        }

        public async Task<Citizen?> GetCitizenByUserIdAsync(int userId)
        {
            var userMgmtClient = _httpClientFactory.CreateClient("UserManagement");
            return await userMgmtClient.GetFromJsonAsync<Citizen>($"api/citizenapi/by-user/{userId}", _json);
        }

        public async Task<CitizenDashboardData?> GetCitizenDashboardAsync(int citizenId)
        {
            var userMgmtClient = _httpClientFactory.CreateClient("UserManagement");
            return await userMgmtClient.GetFromJsonAsync<CitizenDashboardData>($"api/citizenapi/{citizenId}/dashboard", _json);
        }

        public async Task<(bool success, string? error)> CreateCitizenProfileAsync(CreateCitizenViewModelWithCredentials model)
        {
            var userMgmtClient = _httpClientFactory.CreateClient("UserManagement");
            var response = await userMgmtClient.PostAsJsonAsync("api/citizenapi", new
            {
                model.Username,
                model.Password,
                model.Name,
                model.Email,
                DateOfBirth = model.DateOfBirth,
                model.Address,
                model.ContactInfo,
                model.Gender
            });

            try
            {
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    if (string.IsNullOrWhiteSpace(content))
                        return (true, null);

                    // Try to deserialize the success response to extract CitizenId if needed
                    try
                    {
                        var result = JsonSerializer.Deserialize<CreateProfileResponse>(content, _json);
                        return (true, null);
                    }
                    catch
                    {
                        // If deserialization fails but status is success, still consider it success
                        return (true, null);
                    }
                }

                // Handle failure response
                if (string.IsNullOrWhiteSpace(content))
                    return (false, "Failed to create profile.");

                try
                {
                    var err = JsonSerializer.Deserialize<ErrorResponse>(content, _json);
                    return (false, err?.Error ?? "Failed to create profile.");
                }
                catch
                {
                    return (false, "Failed to create profile.");
                }
            }
            catch (HttpRequestException ex)
            {
                return (false, $"Connection error: {ex.Message}");
            }
        }

        public async Task<string?> UpdateCitizenProfileAsync(Citizen citizen)
        {
            var userMgmtClient = _httpClientFactory.CreateClient("UserManagement");
            var response = await userMgmtClient.PutAsJsonAsync($"api/citizenapi/{citizen.CitizenId}", citizen);

            try
            {
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Success - API returns { Message: "..." }
                    if (string.IsNullOrWhiteSpace(content))
                        return null;

                    try
                    {
                        var result = JsonSerializer.Deserialize<MessageResponse>(content, _json);
                        return null; // Success
                    }
                    catch
                    {
                        return null; // Treat as success even if deserialization fails
                    }
                }

                // Handle failure response
                if (string.IsNullOrWhiteSpace(content))
                    return "Failed to update profile.";

                try
                {
                    var err = JsonSerializer.Deserialize<ErrorResponse>(content, _json);
                    return err?.Error ?? "Failed to update profile.";
                }
                catch
                {
                    return "Failed to update profile.";
                }
            }
            catch (HttpRequestException ex)
            {
                return $"Connection error: {ex.Message}";
            }
        }

        public async Task<string?> UpdateCitizenApplicationAsync(WelfareApplication application)
        {
            var userMgmtClient = _httpClientFactory.CreateClient("UserManagement");
            var response = await userMgmtClient.PutAsJsonAsync($"api/citizenapi/application/{application.ApplicationID}", application);

            try
            {
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Success - API returns { Message: "..." }
                    if (string.IsNullOrWhiteSpace(content))
                        return null;

                    try
                    {
                        var result = JsonSerializer.Deserialize<MessageResponse>(content, _json);
                        return null; // Success
                    }
                    catch
                    {
                        return null; // Treat as success even if deserialization fails
                    }
                }

                // Handle failure response
                if (string.IsNullOrWhiteSpace(content))
                    return "Failed to update application.";

                try
                {
                    var err = JsonSerializer.Deserialize<ErrorResponse>(content, _json);
                    return err?.Error ?? "Failed to update application.";
                }
                catch
                {
                    return "Failed to update application.";
                }
            }
            catch (HttpRequestException ex)
            {
                return $"Connection error: {ex.Message}";
            }
        }

        // ──────────────────────────────────────────────
        // CITIZEN DOCUMENT
        // ──────────────────────────────────────────────
        public async Task<IEnumerable<CitizenDocument>> GetDocumentsByCitizenIdAsync(int citizenId, string? status = null)
        {
            var userMgmtClient = _httpClientFactory.CreateClient("UserManagement");
            var url = string.IsNullOrEmpty(status)
                ? $"api/citizendocumentapi/citizen/{citizenId}"
                : $"api/citizendocumentapi/citizen/{citizenId}?status={status}";
            return await userMgmtClient.GetFromJsonAsync<IEnumerable<CitizenDocument>>(url, _json) ?? Enumerable.Empty<CitizenDocument>();
        }

        public async Task<CitizenDocument?> GetDocumentByIdAsync(int id)
        {
            var userMgmtClient = _httpClientFactory.CreateClient("UserManagement");
            return await userMgmtClient.GetFromJsonAsync<CitizenDocument>($"api/citizendocumentapi/{id}", _json);
        }

        public async Task<(bool success, string? error)> UploadDocumentAsync(int citizenId, string docType, string documentName, IFormFile file)
        {
            var userMgmtClient = _httpClientFactory.CreateClient("UserManagement");
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(citizenId.ToString()), "citizenId");
            content.Add(new StringContent(docType), "docType");
            content.Add(new StringContent(documentName), "documentName");
            using var stream = file.OpenReadStream();
            content.Add(new StreamContent(stream), "file", file.FileName);
            var response = await userMgmtClient.PostAsync("api/citizendocumentapi/upload", content);

            try
            {
                var contentStr = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    if (string.IsNullOrWhiteSpace(contentStr))
                        return (true, null);

                    try
                    {
                        var result = JsonSerializer.Deserialize<MessageResponse>(contentStr, _json);
                        return (true, null);
                    }
                    catch
                    {
                        return (true, null);
                    }
                }

                if (string.IsNullOrWhiteSpace(contentStr))
                    return (false, "Failed to upload document.");

                try
                {
                    var err = JsonSerializer.Deserialize<ErrorResponse>(contentStr, _json);
                    return (false, err?.Error ?? "Failed to upload document.");
                }
                catch
                {
                    return (false, "Failed to upload document.");
                }
            }
            catch (HttpRequestException ex)
            {
                return (false, $"Connection error: {ex.Message}");
            }
        }

        public async Task<(bool success, string? error)> ReuploadDocumentAsync(int documentId, IFormFile file)
        {
            var userMgmtClient = _httpClientFactory.CreateClient("UserManagement");
            using var content = new MultipartFormDataContent();
            using var stream = file.OpenReadStream();
            content.Add(new StreamContent(stream), "file", file.FileName);
            var response = await userMgmtClient.PutAsync($"api/citizendocumentapi/{documentId}/reupload", content);

            try
            {
                var contentStr = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    if (string.IsNullOrWhiteSpace(contentStr))
                        return (true, null);

                    try
                    {
                        var result = JsonSerializer.Deserialize<MessageResponse>(contentStr, _json);
                        return (true, null);
                    }
                    catch
                    {
                        return (true, null);
                    }
                }

                if (string.IsNullOrWhiteSpace(contentStr))
                    return (false, "Failed to reupload document.");

                try
                {
                    var err = JsonSerializer.Deserialize<ErrorResponse>(contentStr, _json);
                    return (false, err?.Error ?? "Failed to reupload document.");
                }
                catch
                {
                    return (false, "Failed to reupload document.");
                }
            }
            catch (HttpRequestException ex)
            {
                return (false, $"Connection error: {ex.Message}");
            }
        }

        public async Task<bool> DeleteDocumentAsync(int id)
        {
            var userMgmtClient = _httpClientFactory.CreateClient("UserManagement");
            var response = await userMgmtClient.DeleteAsync($"api/citizendocumentapi/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<(byte[]? bytes, string? contentType, string? fileName)> GetDocumentFileAsync(int id)
        {
            var userMgmtClient = _httpClientFactory.CreateClient("UserManagement");
            var response = await userMgmtClient.GetAsync($"api/citizendocumentapi/{id}/file");
            if (!response.IsSuccessStatusCode) return (null, null, null);

            var bytes = await response.Content.ReadAsByteArrayAsync();
            var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
            var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
                           ?? response.Content.Headers.ContentDisposition?.FileName
                           ?? $"document-{id}";
            return (bytes, contentType, fileName);
        }

        public async Task<bool> UpdateDocumentVerificationStatusAsync(int id, string status)
        {
            var userMgmtClient = _httpClientFactory.CreateClient("UserManagement");
            var content = new StringContent(JsonSerializer.Serialize(status), Encoding.UTF8, "application/json");
            var response = await userMgmtClient.PatchAsync($"api/citizendocumentapi/{id}/verify", content);
            return response.IsSuccessStatusCode;
        }

        // ──────────────────────────────────────────────
        // BENEFIT ANALYTICS
        // ──────────────────────────────────────────────
        public async Task<AnalyticsDashboardViewModel?> GetBenefitAnalyticsDashboardAsync()
            => await _http.GetFromJsonAsync<AnalyticsDashboardViewModel>("api/benefitanalyticsapi/dashboard", _json);

        // ──────────────────────────────────────────────
        // WELFARE APPLICATION ANALYTICS
        // ──────────────────────────────────────────────
        public async Task<Dictionary<string, object>?> GetApplicationAnalyticsDashboardAsync()
            => await _http.GetFromJsonAsync<Dictionary<string, object>>("api/welfareapplicationanalyticsapi/dashboard", _json);

        public async Task<IEnumerable<StatusBreakdownItem>> GetApplicationStatusBreakdownAsync()
            => await _http.GetFromJsonAsync<IEnumerable<StatusBreakdownItem>>("api/welfareapplicationanalyticsapi/status-breakdown", _json) ?? Enumerable.Empty<StatusBreakdownItem>();

        public async Task<Dictionary<string, object>?> GetApplicationMonthlyTrendsAsync(int year)
            => await _http.GetFromJsonAsync<Dictionary<string, object>>($"api/welfareapplicationanalyticsapi/monthly-trends?year={year}", _json);

        public async Task<Dictionary<string, object>?> GetEligibilityReportAsync()
            => await _http.GetFromJsonAsync<Dictionary<string, object>>("api/welfareapplicationanalyticsapi/eligibility-report", _json);

        // ──────────────────────────────────────────────
        // AUDIT LOG
        // ──────────────────────────────────────────────
        public async Task<IEnumerable<AuditLog>> GetAllAuditLogsAsync()
            => await _http.GetFromJsonAsync<IEnumerable<AuditLog>>("api/auditlogapi", _json) ?? Enumerable.Empty<AuditLog>();

        public async Task<AuditLogPagedResponse?> GetPagedAuditLogsAsync(int pageNumber = 1, int pageSize = 10)
            => await _http.GetFromJsonAsync<AuditLogPagedResponse>($"api/auditlogapi/paged?pageNumber={pageNumber}&pageSize={pageSize}", _json);

        public async Task<AuditLogPagedResponse?> GetPagedAuditLogsByEntityTypeAsync(string entityType, int pageNumber = 1, int pageSize = 10)
            => await _http.GetFromJsonAsync<AuditLogPagedResponse>($"api/auditlogapi/paged/entity/{entityType}?pageNumber={pageNumber}&pageSize={pageSize}", _json);

        public async Task<AuditLogPagedResponse?> GetPagedAuditLogsByActionAsync(string action, int pageNumber = 1, int pageSize = 10)
            => await _http.GetFromJsonAsync<AuditLogPagedResponse>($"api/auditlogapi/paged/action/{action}?pageNumber={pageNumber}&pageSize={pageSize}", _json);

        public async Task<AuditLogPagedResponse?> GetPagedAuditLogsByDateRangeAsync(DateTime startDate, DateTime endDate, int pageNumber = 1, int pageSize = 10)
        {
            var start = startDate.ToString("yyyy-MM-dd");
            var end = endDate.ToString("yyyy-MM-dd");
            return await _http.GetFromJsonAsync<AuditLogPagedResponse>($"api/auditlogapi/paged/date-range?startDate={start}&endDate={end}&pageNumber={pageNumber}&pageSize={pageSize}", _json);
        }

        public async Task<bool> CreateAuditLogAsync(int? userId, string action, string entityType, int entityId, string description)
        {
            var payload = new { UserId = userId, Action = action, EntityType = entityType, EntityId = entityId, Description = description };
            var response = await _http.PostAsJsonAsync("api/auditlogapi", payload);
            return response.IsSuccessStatusCode;
        }

        // ──────────────────────────────────────────────
        // AUDIT (Government Auditor)
        // ──────────────────────────────────────────────
        public async Task<IEnumerable<ProgramAuditSummary>> GetGovernmentAuditorDashboardAsync()
            => await _http.GetFromJsonAsync<IEnumerable<ProgramAuditSummary>>("api/auditapi/dashboard", _json) ?? Enumerable.Empty<ProgramAuditSummary>();

        public async Task<IEnumerable<Audit>> GetAllAuditsAsync()
            => await _http.GetFromJsonAsync<IEnumerable<Audit>>("api/auditapi", _json) ?? Enumerable.Empty<Audit>();

        public async Task<Audit?> GetAuditByIdAsync(int id)
            => await _http.GetFromJsonAsync<Audit>($"api/auditapi/{id}", _json);

        public async Task<(Audit? audit, string? error)> CreateAuditAsync(Audit audit)
        {
            var response = await _http.PostAsJsonAsync("api/auditapi", audit);
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var content = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrWhiteSpace(content))
                        return (null, null);
                    var result = JsonSerializer.Deserialize<Audit>(content, _json);
                    return (result, null);
                }
                catch
                {
                    return (null, null);
                }
            }
            try
            {
                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                    return (null, "Failed to create audit.");
                var err = JsonSerializer.Deserialize<ErrorResponse>(content, _json);
                return (null, err?.Error ?? "Failed to create audit.");
            }
            catch
            {
                return (null, "Failed to create audit.");
            }
        }

        public async Task<bool> UpdateAuditStatusAsync(int id, string status)
        {
            var content = new StringContent(JsonSerializer.Serialize(status), Encoding.UTF8, "application/json");
            var response = await _http.PatchAsync($"api/auditapi/{id}/status", content);
            return response.IsSuccessStatusCode;
        }

        // ──────────────────────────────────────────────
        // COMPLIANCE RECORD
        // ──────────────────────────────────────────────
        public async Task<IEnumerable<ComplianceRecord>> GetAllComplianceRecordsAsync()
            => await _http.GetFromJsonAsync<IEnumerable<ComplianceRecord>>("api/complaincerecordapi", _json) ?? Enumerable.Empty<ComplianceRecord>();

        public async Task<IEnumerable<ComplianceRecord>> GetOpenComplianceRecordsAsync()
            => await _http.GetFromJsonAsync<IEnumerable<ComplianceRecord>>("api/complaincerecordapi/open", _json) ?? Enumerable.Empty<ComplianceRecord>();

        public async Task<ComplianceRecord?> GetComplianceRecordByIdAsync(int id)
            => await _http.GetFromJsonAsync<ComplianceRecord>($"api/complaincerecordapi/{id}", _json);

        public async Task<(ComplianceRecord? record, string? error)> CreateComplianceRecordAsync(ComplianceRecord record)
        {
            var response = await _http.PostAsJsonAsync("api/complaincerecordapi", record);
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var content = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrWhiteSpace(content))
                        return (null, null);
                    var result = JsonSerializer.Deserialize<ComplianceRecord>(content, _json);
                    return (result, null);
                }
                catch
                {
                    return (null, null);
                }
            }
            try
            {
                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                    return (null, "Failed to create compliance record.");
                var err = JsonSerializer.Deserialize<ErrorResponse>(content, _json);
                return (null, err?.Error ?? "Failed to create compliance record.");
            }
            catch
            {
                return (null, "Failed to create compliance record.");
            }
        }

        public async Task<bool> UpdateComplianceStatusAsync(int id, string status, int? resolvedByUserId, string? notes)
        {
            var payload = new { Status = status, ResolvedByUserId = resolvedByUserId, Notes = notes };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _http.PatchAsync($"api/complaincerecordapi/{id}/status", content);
            return response.IsSuccessStatusCode;
        }

        // ──────────────────────────────────────────────
        // USER
        // ──────────────────────────────────────────────
        public async Task<(User? user, string? error)> CreateUserAsync(User user)
        {
            var userMgmtClient = _httpClientFactory.CreateClient("UserManagement");
            var response = await userMgmtClient.PostAsJsonAsync("api/userapi", user);
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var content = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrWhiteSpace(content))
                        return (null, "Server returned empty response.");
                    var result = JsonSerializer.Deserialize<User>(content, _json);
                    return (result, null);
                }
                catch (JsonException ex)
                {
                    return (null, $"Invalid JSON response: {ex.Message}");
                }
            }
            try
            {
                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                    return (null, "Failed to create user.");
                var err = JsonSerializer.Deserialize<ErrorResponse>(content, _json);
                return (null, err?.Error ?? "Failed to create user.");
            }
            catch
            {
                return (null, "Failed to create user.");
            }
        }

        public async Task<(User? user, string? error)> LoginAsync(string username, string password, string userType)
        {
            var loginRequest = new { Username = username, Password = password, UserType = userType };
            try
            {
                var userMgmtClient = _httpClientFactory.CreateClient("UserManagement");
                var response = await userMgmtClient.PostAsJsonAsync("api/userapi/login", loginRequest);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    if (string.IsNullOrWhiteSpace(content))
                        return (null, "Server returned empty response. Please check if the API is running correctly.");

                    try
                    {
                        var result = JsonSerializer.Deserialize<User>(content, _json);
                        return (result, null);
                    }
                    catch (JsonException ex)
                    {
                        return (null, $"Invalid response from server: {ex.Message}");
                    }
                }

                // Failure response
                if (string.IsNullOrWhiteSpace(content))
                    return (null, "Invalid username or password");

                try
                {
                    var err = JsonSerializer.Deserialize<ErrorResponse>(content, _json);
                    return (null, err?.Error ?? "Invalid username or password");
                }
                catch
                {
                    return (null, "Invalid username or password");
                }
            }
            catch (HttpRequestException ex)
            {
                return (null, $"Connection error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (null, $"Error: {ex.Message}");
            }
        }

        public async Task<(User? user, string? error)> GetUserAsync(int userId)
        {
            var userMgmtClient = _httpClientFactory.CreateClient("UserManagement");
            var response = await userMgmtClient.GetAsync($"api/userapi/{userId}");
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var content = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrWhiteSpace(content))
                        return (null, null);
                    var result = JsonSerializer.Deserialize<User>(content, _json);
                    return (result, null);
                }
                catch
                {
                    return (null, null);
                }
            }
            try
            {
                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                    return (null, "User not found");
                var err = JsonSerializer.Deserialize<ErrorResponse>(content, _json);
                return (null, err?.Error ?? "User not found");
            }
            catch
            {
                return (null, "User not found");
            }
        }

        public async Task<(User? user, string? error)> UpdateProfileAsync(int userId, string? fullName, string? email)
        {
            var userMgmtClient = _httpClientFactory.CreateClient("UserManagement");
            var updateRequest = new { FullName = fullName, Email = email };
            var response = await userMgmtClient.PutAsJsonAsync($"api/userapi/{userId}/profile", updateRequest);
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var content = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrWhiteSpace(content))
                        return (null, null);
                    var result = JsonSerializer.Deserialize<User>(content, _json);
                    return (result, null);
                }
                catch
                {
                    return (null, null);
                }
            }
            try
            {
                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                    return (null, "Failed to update profile");
                var err = JsonSerializer.Deserialize<ErrorResponse>(content, _json);
                return (null, err?.Error ?? "Failed to update profile");
            }
            catch
            {
                return (null, "Failed to update profile");
            }
        }

        public async Task<(bool success, string? error)> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var userMgmtClient = _httpClientFactory.CreateClient("UserManagement");
            var changePasswordRequest = new { CurrentPassword = currentPassword, NewPassword = newPassword };
            var response = await userMgmtClient.PutAsJsonAsync($"api/userapi/{userId}/password", changePasswordRequest);
            if (response.IsSuccessStatusCode)
                return (true, null);
            try
            {
                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                    return (false, "Failed to change password");
                var err = JsonSerializer.Deserialize<ErrorResponse>(content, _json);
                return (false, err?.Error ?? "Failed to change password");
            }
            catch
            {
                return (false, "Failed to change password");
            }
        }

        public async Task<(bool success, string? error)> BlockUserAsync(int userId)
        {
            var userMgmtClient = _httpClientFactory.CreateClient("UserManagement");
            var response = await userMgmtClient.PutAsJsonAsync($"api/userapi/{userId}/block", new { });
            if (response.IsSuccessStatusCode)
                return (true, null);
            try
            {
                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                    return (false, "Failed to block user");
                var err = JsonSerializer.Deserialize<ErrorResponse>(content, _json);
                return (false, err?.Error ?? "Failed to block user");
            }
            catch
            {
                return (false, "Failed to block user");
            }
        }

        public async Task<(bool success, string? error)> UnblockUserAsync(int userId)
        {
            var userMgmtClient = _httpClientFactory.CreateClient("UserManagement");
            var response = await userMgmtClient.PutAsJsonAsync($"api/userapi/{userId}/unblock", new { });
            if (response.IsSuccessStatusCode)
                return (true, null);
            try
            {
                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                    return (false, "Failed to unblock user");
                var err = JsonSerializer.Deserialize<ErrorResponse>(content, _json);
                return (false, err?.Error ?? "Failed to unblock user");
            }
            catch
            {
                return (false, "Failed to unblock user");
            }
        }
    }

    // ──────────────────────────────────────────────
    // Response DTOs used only for deserialization
    // ──────────────────────────────────────────────
    public record ErrorResponse(string Error);
    public record BenefitResponse(string Message, Benefit Benefit);
    public record DisbursementResponse(string Message, Disbursement Disbursement);
    public record ApplicationResponse(string Message, WelfareApplication Application);
    public record EligibilityCheckResponse(string Message, EligibilityCheck Check);

    public class DropdownData
    {
        public IEnumerable<DropdownItem> Dropdown { get; set; } = Enumerable.Empty<DropdownItem>();
        public IEnumerable<ApplicationDropdownDetail> Applications { get; set; } = Enumerable.Empty<ApplicationDropdownDetail>();
    }

    public class DropdownItem
    {
        public int ApplicationID { get; set; }
        public string Display { get; set; } = string.Empty;
        public bool Selected { get; set; }
    }

    public class ApplicationDropdownDetail
    {
        public int ApplicationID { get; set; }
        public int CitizenID { get; set; }
        public string CitizenName { get; set; } = string.Empty;
        public int ProgramID { get; set; }
        public string ProgramTitle { get; set; } = string.Empty;
        public string ProgramDesc { get; set; } = string.Empty;
        public decimal? ProgramMaxBenefit { get; set; }
        public decimal? ProgramBudget { get; set; }
        public string ProgramStatus { get; set; } = string.Empty;
        public string SubmittedDate { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class ProgramResourceInfo
    {
        public double TotalResource { get; set; }
        public double AlreadyAllocated { get; set; }
        public double RemainingResource { get; set; }
        public double MaxBenefitPerCitizen { get; set; }
        public bool HasResource { get; set; }
    }

    public class DisbursementDetail
    {
        public Disbursement? Disbursement { get; set; }
        public double BenefitTotalAmount { get; set; }
        public double TotalDisbursed { get; set; }
        public double PendingBalance { get; set; }
        public IEnumerable<Disbursement> SiblingDisbursements { get; set; } = Enumerable.Empty<Disbursement>();
    }

    public class BenefitDetails
    {
        public string? BenefitType { get; set; }
        public double BenefitAmount { get; set; }
        public string? BenefitStatus { get; set; }
        public string? ProgramTitle { get; set; }
        public decimal? ProgramBudget { get; set; }
        public int? CitizenId { get; set; }
        public string? CitizenName { get; set; }
        public double TotalResource { get; set; }
        public double TotalDisbursedForProgram { get; set; }
        public double AvailableResource { get; set; }
        public bool IsResourceExhausted { get; set; }
    }

    public class ProgramResourceDetail
    {
        public IEnumerable<Resource> Resources { get; set; } = Enumerable.Empty<Resource>();
        public string ProgramTitle { get; set; } = string.Empty;
        public decimal ProgramBudget { get; set; }
        public decimal TotalAllocated { get; set; }
        public decimal RemainingBudget { get; set; }
        public decimal UtilisationPercentage { get; set; }
    }

    public class ApplicationInfo
    {
        public WelfareApplication? Application { get; set; }
        public Citizen? Citizen { get; set; }
        public IEnumerable<CitizenDocument> Documents { get; set; } = Enumerable.Empty<CitizenDocument>();
    }

    public class CitizenDashboardData
    {
        public Citizen? CitizenProfile { get; set; }
        public IEnumerable<CitizenDocument> Documents { get; set; } = Enumerable.Empty<CitizenDocument>();
        public int PendingDocuments { get; set; }
        public int ApprovedDocuments { get; set; }
        public int RejectedDocuments { get; set; }
    }

    public class StatusBreakdownItem
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class AuditLog
    {
        public int LogID { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class Audit
    {
        public int AuditID { get; set; }
        public int? ProgramID { get; set; }
        public string? ProgramTitle { get; set; }
        public int AuditedByUserId { get; set; }
        public string? AuditedByUserName { get; set; }
        public DateTime AuditDate { get; set; }
        public string FindingType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Open";
        public DateTime? ResolvedDate { get; set; }
    }

    public class ComplianceRecord
    {
        public int RecordID { get; set; }
        public int? RaisedByUserId { get; set; }
        public string? RaisedByUserName { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public string ViolationType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Open";
        public DateTime CreatedDate { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public int? ResolvedByUserId { get; set; }
        public string? ResolvedByUserName { get; set; }
        public string? Notes { get; set; }
        public string? CitizenName { get; set; }
        public string? ProgramTitle { get; set; }
    }

    // ──────────────────────────────────────────────
    // Government Auditor dashboard summary
    // ──────────────────────────────────────────────
    public class ProgramAuditSummary
    {
        public int ProgramID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Budget { get; set; }
        public string Status { get; set; } = string.Empty;
        public int TotalBeneficiaries { get; set; }
        public double TotalBenefitAmount { get; set; }
        public double TotalDisbursed { get; set; }
        public double RemainingBudget { get; set; }
        public int TotalResources { get; set; }
        public int OpenAudits { get; set; }
    }

    // ──────────────────────────────────────────────
    // Paged Audit Log Response
    // ──────────────────────────────────────────────
    public class AuditLogPagedResponse
    {
        public IEnumerable<AuditLog> Data { get; set; } = Enumerable.Empty<AuditLog>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
