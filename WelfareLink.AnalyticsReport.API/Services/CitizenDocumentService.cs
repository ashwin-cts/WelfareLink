using WelfareLink.AnalyticsReport.API.Interfaces;
using WelfareLink.AnalyticsReport.API.Models;

namespace WelfareLink.AnalyticsReport.API.Services;

public class CitizenDocumentService : ICitizenDocumentService
{
    private readonly ICitizenDocumentRepository _documentRepository;
    private readonly IWebHostEnvironment _environment;
    private readonly IAuditLogService _auditLogService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CitizenDocumentService(ICitizenDocumentRepository documentRepository, IWebHostEnvironment environment, IAuditLogService auditLogService, IHttpContextAccessor httpContextAccessor)
    {
        _documentRepository = documentRepository;
        _environment = environment;
        _auditLogService = auditLogService;
        _httpContextAccessor = httpContextAccessor;
    }

    private int? GetCurrentUserId()
    {
        return _httpContextAccessor?.HttpContext?.Session.GetInt32("UserId");
    }

    public async Task<IEnumerable<CitizenDocument>> GetDocumentsByCitizenIdAsync(int citizenId)
    {
        return await _documentRepository.GetByCitizenIdAsync(citizenId);
    }

    public async Task<CitizenDocument> GetDocumentByIdAsync(int documentId)
    {
        return await _documentRepository.GetByIdAsync(documentId);
    }

    public async Task<bool> UploadDocumentAsync(CitizenDocument document, IFormFile file)
    {
        try
        {
            if (file != null && file.Length > 0)
            {
                document.FileURI = await SaveFileAsync(file, document.DocType);
            }

            document.UploadedDate = DateTime.UtcNow;
            document.VerificationStatus = "Pending";

            await _documentRepository.AddAsync(document);

            var userId = GetCurrentUserId();
            await _auditLogService.LogActionAsync(
                userId: userId,
                action: "Upload",
                entityType: "CitizenDocument",
                entityId: document.DocumentID,
                description: $"Doc Id {document.DocumentID} Uploaded document '{document.DocType}' for user ID of citizen:{userId}",
                status: "Success"
            );

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteDocumentAsync(int documentId)
    {
        try
        {
            var document = await _documentRepository.GetByIdAsync(documentId);
            if (document != null)
            {
                if (!string.IsNullOrEmpty(document.FileURI))
                {
                    var webRoot = _environment.WebRootPath
                                  ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
                    var filePath = Path.Combine(webRoot, document.FileURI.TrimStart('/'));
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }

                await _documentRepository.DeleteAsync(documentId);

                var userId = GetCurrentUserId();
                await _auditLogService.LogActionAsync(
                    userId: userId,
                    action: "Delete",
                    entityType: "CitizenDocument",
                    entityId: documentId,
                    description: $"Doc Id {documentId} Deleted document '{document.DocType}' for user ID of citizen:{userId}",
                    status: "Success"
                );
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateVerificationStatusAsync(int documentId, string status)
    {
        try
        {
            var document = await _documentRepository.GetByIdAsync(documentId);
            if (document == null) return false;

            document.VerificationStatus = status;
            await _documentRepository.UpdateAsync(document);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ReuploadDocumentAsync(int documentId, IFormFile file)
    {
        try
        {
            var document = await _documentRepository.GetByIdAsync(documentId);
            if (document == null) return false;

            // Delete old file
            if (!string.IsNullOrEmpty(document.FileURI))
            {
                var webRoot = _environment.WebRootPath
                              ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
                var oldPath = Path.Combine(webRoot, document.FileURI.TrimStart('/'));
                if (File.Exists(oldPath))
                {
                    File.Delete(oldPath);
                }
            }

            // Save new file
            document.FileURI = await SaveFileAsync(file, document.DocType);
            document.UploadedDate = DateTime.UtcNow;
            document.VerificationStatus = "Pending";

            await _documentRepository.UpdateAsync(document);

            var userId = GetCurrentUserId();
            await _auditLogService.LogActionAsync(
                userId: userId,
                action: "Upload",
                entityType: "CitizenDocument",
                entityId: documentId,
                description: $"Re-uploaded document '{document.DocType}' for citizen",
                status: "Success"
            );

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> SaveFileAsync(IFormFile file, string docType)
    {
        // WebRootPath is null when the API has no wwwroot folder yet — fall back to ContentRootPath/wwwroot
        var webRoot = _environment.WebRootPath
                      ?? Path.Combine(_environment.ContentRootPath, "wwwroot");

        var uploadsFolder = Path.Combine(webRoot, "uploads", "documents");

        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        var istTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istZone);

        var uniqueFileName = $"{docType}_{istTime:yyyyMMddHHmmss}_{Guid.NewGuid()}_{file.FileName}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        return $"/uploads/documents/{uniqueFileName}";
    }
}


