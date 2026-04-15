using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
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
        private static readonly JsonSerializerOptions _json = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public WelfareApiClient(HttpClient http)
        {
            _http = http;
        }

        // ──────────────────────────────────────────────
        // BENEFIT
        // ──────────────────────────────────────────────
        public async Task<IEnumerable<Benefit>> GetAllBenefitsAsync()
            => await _http.GetFromJsonAsync<IEnumerable<Benefit>>("api/benefitapi") ?? [];

        public async Task<Benefit?> GetBenefitByIdAsync(int id)
            => await _http.GetFromJsonAsync<Benefit>($"api/benefitapi/{id}");

        public async Task<bool> BenefitExistsAsync(int id)
            => (await GetBenefitByIdAsync(id)) != null;

        public async Task<(Benefit? benefit, string? error)> CreateBenefitAsync(Benefit benefit, int userId)
        {
            var response = await _http.PostAsJsonAsync($"api/benefitapi?officerId={userId}", benefit);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<BenefitResponse>();
                return (result?.Benefit, null);
            }
            var err = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return (null, err?.Error ?? "Failed to create benefit.");
        }

        public async Task<(Benefit? benefit, string? error)> UpdateBenefitAsync(Benefit benefit, int userId)
        {
            var response = await _http.PutAsJsonAsync($"api/benefitapi/{benefit.BenefitID}?officerId={userId}", benefit);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<BenefitResponse>();
                return (result?.Benefit, null);
            }
            var err = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return (null, err?.Error ?? "Failed to update benefit.");
        }

        public async Task DeleteBenefitAsync(int id)
            => await _http.DeleteAsync($"api/benefitapi/{id}");

        public async Task<DropdownData?> GetBenefitDropdownAsync(int? selectedId = null)
        {
            var url = selectedId.HasValue
                ? $"api/benefitapi/dropdown?selectedId={selectedId}"
                : "api/benefitapi/dropdown";
            return await _http.GetFromJsonAsync<DropdownData>(url);
        }

        public async Task<ProgramResourceInfo?> GetProgramResourceInfoAsync(int programId)
            => await _http.GetFromJsonAsync<ProgramResourceInfo>($"api/benefitapi/program-resource-info/{programId}");

        // ──────────────────────────────────────────────
        // DISBURSEMENT
        // ──────────────────────────────────────────────
        public async Task<IEnumerable<Disbursement>> GetAllDisbursementsAsync()
            => await _http.GetFromJsonAsync<IEnumerable<Disbursement>>("api/disbursementapi") ?? [];

        public async Task<DisbursementDetail?> GetDisbursementByIdAsync(int id)
            => await _http.GetFromJsonAsync<DisbursementDetail>($"api/disbursementapi/{id}");

        public async Task<IEnumerable<Disbursement>> GetDisbursementsByBenefitIdAsync(int benefitId)
            => await _http.GetFromJsonAsync<IEnumerable<Disbursement>>($"api/disbursementapi/benefit/{benefitId}") ?? [];

        public async Task<BenefitDetails?> GetDisbursementBenefitDetailsAsync(int benefitId)
            => await _http.GetFromJsonAsync<BenefitDetails>($"api/disbursementapi/benefit-details/{benefitId}");

        public async Task<(Disbursement? disbursement, string? error)> CreateDisbursementAsync(Disbursement disbursement)
        {
            var response = await _http.PostAsJsonAsync("api/disbursementapi", disbursement);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<DisbursementResponse>();
                return (result?.Disbursement, null);
            }
            var err = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return (null, err?.Error ?? "Failed to create disbursement.");
        }

        public async Task<(Disbursement? disbursement, string? error)> UpdateDisbursementAsync(Disbursement disbursement)
        {
            var response = await _http.PutAsJsonAsync($"api/disbursementapi/{disbursement.DisbursementID}", disbursement);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<DisbursementResponse>();
                return (result?.Disbursement, null);
            }
            var err = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return (null, err?.Error ?? "Failed to update disbursement.");
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
            var err = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return err?.Error ?? "Failed to delete disbursement.";
        }

        // ──────────────────────────────────────────────
        // ELIGIBILITY CHECK
        // ──────────────────────────────────────────────
        public async Task<IEnumerable<EligibilityCheck>> GetAllChecksAsync()
            => await _http.GetFromJsonAsync<IEnumerable<EligibilityCheck>>("api/eligibilitycheckapi") ?? [];

        public async Task<EligibilityCheck?> GetCheckByIdAsync(int id)
            => await _http.GetFromJsonAsync<EligibilityCheck>($"api/eligibilitycheckapi/{id}");

        public async Task<ApplicationInfo?> GetEligibilityApplicationInfoAsync(int applicationId)
            => await _http.GetFromJsonAsync<ApplicationInfo>($"api/eligibilitycheckapi/application-info/{applicationId}");

        public async Task<EligibilityCheck?> CreateCheckAsync(EligibilityCheck check, int? applicationId)
        {
            var url = applicationId.HasValue
                ? $"api/eligibilitycheckapi?applicationId={applicationId}"
                : "api/eligibilitycheckapi";
            var response = await _http.PostAsJsonAsync(url, check);
            if (!response.IsSuccessStatusCode) return null;
            var result = await response.Content.ReadFromJsonAsync<EligibilityCheckResponse>();
            return result?.Check;
        }

        public async Task UpdateCheckAsync(EligibilityCheck check)
            => await _http.PutAsJsonAsync($"api/eligibilitycheckapi/{check.CheckID}", check);

        public async Task DeleteCheckAsync(int id)
            => await _http.DeleteAsync($"api/eligibilitycheckapi/{id}");

        // ──────────────────────────────────────────────
        // RESOURCE
        // ──────────────────────────────────────────────
        public async Task<IEnumerable<Resource>> GetAllResourcesAsync()
            => await _http.GetFromJsonAsync<IEnumerable<Resource>>("api/resourceapi") ?? [];

        public async Task<ProgramResourceDetail?> GetResourcesByProgramIdAsync(int programId)
            => await _http.GetFromJsonAsync<ProgramResourceDetail>($"api/resourceapi/program/{programId}");

        public async Task<IEnumerable<ResourceUtilisationViewModel>> GetResourceUtilisationAsync()
            => await _http.GetFromJsonAsync<IEnumerable<ResourceUtilisationViewModel>>("api/resourceapi/utilisation") ?? [];

        public async Task<string?> AddResourceAsync(Resource resource)
        {
            var response = await _http.PostAsJsonAsync("api/resourceapi", resource);
            if (response.IsSuccessStatusCode) return null;
            var err = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return err?.Error ?? "Failed to allocate resource.";
        }

        public async Task<string?> UpdateResourceAsync(Resource resource)
        {
            var response = await _http.PutAsJsonAsync($"api/resourceapi/{resource.ResourceID}", resource);
            if (response.IsSuccessStatusCode) return null;
            var err = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return err?.Error ?? "Failed to update resource.";
        }

        // ──────────────────────────────────────────────
        // WELFARE APPLICATION
        // ──────────────────────────────────────────────
        public async Task<IEnumerable<WelfareApplication>> GetAllApplicationsAsync(string? status = null)
        {
            var url = string.IsNullOrEmpty(status)
                ? "api/welfareapplicationapi"
                : $"api/welfareapplicationapi?status={status}";
            return await _http.GetFromJsonAsync<IEnumerable<WelfareApplication>>(url) ?? [];
        }

        public async Task<IEnumerable<WelfareApplication>> GetPendingApplicationsAsync()
            => await _http.GetFromJsonAsync<IEnumerable<WelfareApplication>>("api/welfareapplicationapi/pending") ?? [];

        public async Task<WelfareApplication?> GetApplicationByIdAsync(int id)
            => await _http.GetFromJsonAsync<WelfareApplication>($"api/welfareapplicationapi/{id}");

        public async Task<bool> ApplicationExistsAsync(int id)
            => (await GetApplicationByIdAsync(id)) != null;

        public async Task<WelfareApplication?> CreateApplicationAsync(WelfareApplication application)
        {
            var response = await _http.PostAsJsonAsync("api/welfareapplicationapi", application);
            if (!response.IsSuccessStatusCode) return null;
            var result = await response.Content.ReadFromJsonAsync<ApplicationResponse>();
            return result?.Application;
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
            => await _http.GetFromJsonAsync<IEnumerable<WelfareApplication>>($"api/citizenapi/{citizenId}/applications") ?? [];

        public async Task<(bool success, string? error)> ApplyForProgramAsync(int citizenId, int programId, int[] selectedDocumentIds)
        {
            var payload = new { CitizenID = citizenId, ProgramID = programId, SelectedDocumentIds = selectedDocumentIds };
            var response = await _http.PostAsJsonAsync("api/citizenapi/apply", payload);
            if (response.IsSuccessStatusCode) return (true, null);
            var err = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return (false, err?.Error ?? "Failed to submit application.");
        }

        // ──────────────────────────────────────────────
        // WELFARE PROGRAM
        // ──────────────────────────────────────────────
        public async Task<IEnumerable<WelfareProgram>> GetAllProgramsAsync()
            => await _http.GetFromJsonAsync<IEnumerable<WelfareProgram>>("api/welfareprogramapi") ?? [];

        public async Task<ProgramDetailViewModel?> GetProgramByIdAsync(int id)
            => await _http.GetFromJsonAsync<ProgramDetailViewModel>($"api/welfareprogramapi/{id}");

        public async Task<BudgetDashboardViewModel?> GetBudgetMonitoringAsync()
            => await _http.GetFromJsonAsync<BudgetDashboardViewModel>("api/welfareprogramapi/budget-monitoring");

        public async Task<IEnumerable<ProgramPerformanceViewModel>> GetProgramPerformanceAsync()
            => await _http.GetFromJsonAsync<IEnumerable<ProgramPerformanceViewModel>>("api/welfareprogramapi/performance") ?? [];

        public async Task<string?> AddProgramAsync(WelfareProgram program)
        {
            var response = await _http.PostAsJsonAsync("api/welfareprogramapi", program);
            if (response.IsSuccessStatusCode) return null;
            var err = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return err?.Error ?? "Failed to create program.";
        }

        public async Task<string?> UpdateProgramAsync(WelfareProgram program)
        {
            var response = await _http.PutAsJsonAsync($"api/welfareprogramapi/{program.ProgramID}", program);
            if (response.IsSuccessStatusCode) return null;
            var err = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return err?.Error ?? "Failed to update program.";
        }

        public async Task<string?> SuspendProgramAsync(int id)
        {
            var response = await _http.PatchAsync($"api/welfareprogramapi/{id}/suspend", null);
            if (response.IsSuccessStatusCode) return null;
            var err = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return err?.Error ?? "Failed to suspend programme.";
        }

        // ──────────────────────────────────────────────
        // CITIZEN
        // ──────────────────────────────────────────────
        public async Task<Citizen?> GetCitizenByIdAsync(int id)
            => await _http.GetFromJsonAsync<Citizen>($"api/citizenapi/{id}");

        public async Task<Citizen?> GetCitizenByUserIdAsync(int userId)
            => await _http.GetFromJsonAsync<Citizen>($"api/citizenapi/by-user/{userId}");

        public async Task<CitizenDashboardData?> GetCitizenDashboardAsync(int citizenId)
            => await _http.GetFromJsonAsync<CitizenDashboardData>($"api/citizenapi/{citizenId}/dashboard");

        public async Task<(bool success, string? error)> CreateCitizenProfileAsync(CreateCitizenViewModelWithCredentials model)
        {
            var response = await _http.PostAsJsonAsync("api/citizenapi", new
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
            if (response.IsSuccessStatusCode) return (true, null);
            var err = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return (false, err?.Error ?? "Failed to create profile.");
        }

        public async Task<string?> UpdateCitizenProfileAsync(Citizen citizen)
        {
            var response = await _http.PutAsJsonAsync($"api/citizenapi/{citizen.CitizenId}", citizen);
            if (response.IsSuccessStatusCode) return null;
            var err = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return err?.Error ?? "Failed to update profile.";
        }

        public async Task<string?> UpdateCitizenApplicationAsync(WelfareApplication application)
        {
            var response = await _http.PutAsJsonAsync($"api/citizenapi/application/{application.ApplicationID}", application);
            if (response.IsSuccessStatusCode) return null;
            var err = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return err?.Error ?? "Failed to update application.";
        }

        // ──────────────────────────────────────────────
        // CITIZEN DOCUMENT
        // ──────────────────────────────────────────────
        public async Task<IEnumerable<CitizenDocument>> GetDocumentsByCitizenIdAsync(int citizenId, string? status = null)
        {
            var url = string.IsNullOrEmpty(status)
                ? $"api/citizendocumentapi/citizen/{citizenId}"
                : $"api/citizendocumentapi/citizen/{citizenId}?status={status}";
            return await _http.GetFromJsonAsync<IEnumerable<CitizenDocument>>(url) ?? [];
        }

        public async Task<CitizenDocument?> GetDocumentByIdAsync(int id)
            => await _http.GetFromJsonAsync<CitizenDocument>($"api/citizendocumentapi/{id}");

        public async Task<(bool success, string? error)> UploadDocumentAsync(int citizenId, string docType, string documentName, IFormFile file)
        {
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(citizenId.ToString()), "citizenId");
            content.Add(new StringContent(docType), "docType");
            content.Add(new StringContent(documentName), "documentName");
            using var stream = file.OpenReadStream();
            content.Add(new StreamContent(stream), "file", file.FileName);
            var response = await _http.PostAsync("api/citizendocumentapi/upload", content);
            if (response.IsSuccessStatusCode) return (true, null);
            var err = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return (false, err?.Error ?? "Failed to upload document.");
        }

        public async Task<(bool success, string? error)> ReuploadDocumentAsync(int documentId, IFormFile file)
        {
            using var content = new MultipartFormDataContent();
            using var stream = file.OpenReadStream();
            content.Add(new StreamContent(stream), "file", file.FileName);
            var response = await _http.PutAsync($"api/citizendocumentapi/{documentId}/reupload", content);
            if (response.IsSuccessStatusCode) return (true, null);
            var err = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return (false, err?.Error ?? "Failed to reupload document.");
        }

        public async Task<bool> DeleteDocumentAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/citizendocumentapi/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<(byte[]? bytes, string? contentType, string? fileName)> GetDocumentFileAsync(int id)
        {
            var response = await _http.GetAsync($"api/citizendocumentapi/{id}/file");
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
            var content = new StringContent(JsonSerializer.Serialize(status), Encoding.UTF8, "application/json");
            var response = await _http.PatchAsync($"api/citizendocumentapi/{id}/verify", content);
            return response.IsSuccessStatusCode;
        }

        // ──────────────────────────────────────────────
        // BENEFIT ANALYTICS
        // ──────────────────────────────────────────────
        public async Task<AnalyticsDashboardViewModel?> GetBenefitAnalyticsDashboardAsync()
            => await _http.GetFromJsonAsync<AnalyticsDashboardViewModel>("api/benefitanalyticsapi/dashboard");

        // ──────────────────────────────────────────────
        // WELFARE APPLICATION ANALYTICS
        // ──────────────────────────────────────────────
        public async Task<Dictionary<string, object>?> GetApplicationAnalyticsDashboardAsync()
            => await _http.GetFromJsonAsync<Dictionary<string, object>>("api/welfareapplicationanalyticsapi/dashboard");

        public async Task<IEnumerable<StatusBreakdownItem>> GetApplicationStatusBreakdownAsync()
            => await _http.GetFromJsonAsync<IEnumerable<StatusBreakdownItem>>("api/welfareapplicationanalyticsapi/status-breakdown") ?? [];

        public async Task<Dictionary<string, object>?> GetApplicationMonthlyTrendsAsync(int year)
            => await _http.GetFromJsonAsync<Dictionary<string, object>>($"api/welfareapplicationanalyticsapi/monthly-trends?year={year}");

        public async Task<Dictionary<string, object>?> GetEligibilityReportAsync()
            => await _http.GetFromJsonAsync<Dictionary<string, object>>("api/welfareapplicationanalyticsapi/eligibility-report");

        // ──────────────────────────────────────────────
        // AUDIT LOG
        // ──────────────────────────────────────────────
        public async Task<IEnumerable<AuditLog>> GetAllAuditLogsAsync()
            => await _http.GetFromJsonAsync<IEnumerable<AuditLog>>("api/auditlogapi") ?? [];

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
            => await _http.GetFromJsonAsync<IEnumerable<ProgramAuditSummary>>("api/auditapi/dashboard") ?? [];

        public async Task<IEnumerable<Audit>> GetAllAuditsAsync()
            => await _http.GetFromJsonAsync<IEnumerable<Audit>>("api/auditapi") ?? [];

        public async Task<Audit?> GetAuditByIdAsync(int id)
            => await _http.GetFromJsonAsync<Audit>($"api/auditapi/{id}");

        public async Task<(Audit? audit, string? error)> CreateAuditAsync(Audit audit)
        {
            var response = await _http.PostAsJsonAsync("api/auditapi", audit);
            if (response.IsSuccessStatusCode)
                return (await response.Content.ReadFromJsonAsync<Audit>(_json), null);
            var err = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return (null, err?.Error ?? "Failed to create audit.");
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
            => await _http.GetFromJsonAsync<IEnumerable<ComplianceRecord>>("api/complaincerecordapi") ?? [];

        public async Task<IEnumerable<ComplianceRecord>> GetOpenComplianceRecordsAsync()
            => await _http.GetFromJsonAsync<IEnumerable<ComplianceRecord>>("api/complaincerecordapi/open") ?? [];

        public async Task<ComplianceRecord?> GetComplianceRecordByIdAsync(int id)
            => await _http.GetFromJsonAsync<ComplianceRecord>($"api/complaincerecordapi/{id}");

        public async Task<(ComplianceRecord? record, string? error)> CreateComplianceRecordAsync(ComplianceRecord record)
        {
            var response = await _http.PostAsJsonAsync("api/complaincerecordapi", record);
            if (response.IsSuccessStatusCode)
                return (await response.Content.ReadFromJsonAsync<ComplianceRecord>(_json), null);
            var err = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return (null, err?.Error ?? "Failed to create compliance record.");
        }

        public async Task<bool> UpdateComplianceStatusAsync(int id, string status, int? resolvedByUserId, string? notes)
        {
            var payload = new { Status = status, ResolvedByUserId = resolvedByUserId, Notes = notes };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _http.PatchAsync($"api/complaincerecordapi/{id}/status", content);
            return response.IsSuccessStatusCode;
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
        public IEnumerable<DropdownItem> Dropdown { get; set; } = [];
        public IEnumerable<ApplicationDropdownDetail> Applications { get; set; } = [];
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
        public bool HasResource { get; set; }
    }

    public class DisbursementDetail
    {
        public Disbursement? Disbursement { get; set; }
        public double BenefitTotalAmount { get; set; }
        public double TotalDisbursed { get; set; }
        public double PendingBalance { get; set; }
        public IEnumerable<Disbursement> SiblingDisbursements { get; set; } = [];
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
        public IEnumerable<Resource> Resources { get; set; } = [];
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
        public IEnumerable<CitizenDocument> Documents { get; set; } = [];
    }

    public class CitizenDashboardData
    {
        public Citizen? CitizenProfile { get; set; }
        public IEnumerable<CitizenDocument> Documents { get; set; } = [];
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
}
