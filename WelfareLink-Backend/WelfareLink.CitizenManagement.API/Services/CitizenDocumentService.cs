using System.Security.Claims;
using WelfareLink.CitizenManagement.API.Interfaces;
using WelfareLink.CitizenManagement.API.Models;
using WelfareLink.CitizenManagement.API.Exceptions; // Added for custom exceptions

namespace WelfareLink.CitizenManagement.API.Services;

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
        // Securely extracts the UserId from the JWT Token sent by Postman/Client
        var userIdClaim = _httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? _httpContextAccessor?.HttpContext?.User?.FindFirst("UserId")?.Value;

        if (int.TryParse(userIdClaim, out int userId))
        {
            return userId;
        }

        return null;
    }

    public async Task<IEnumerable<CitizenDocument>> GetDocumentsByCitizenIdAsync(int citizenId)
    {
        return await _documentRepository.GetByCitizenIdAsync(citizenId);
    }

    public async Task<CitizenDocument> GetDocumentByIdAsync(int documentId)
    {
        var document = await _documentRepository.GetByIdAsync(documentId);
        if (document == null)
        {
            throw new NotFoundException($"Document with ID {documentId} was not found.");
        }
        return document;
    }

    public async Task<bool> UploadDocumentAsync(CitizenDocument document, IFormFile file)
    {
        try
        {
            if (file != null && file.Length > 0)
            {
                document.FileURI = await SaveFileAsync(file, document.DocType);
            }
            else
            {
                throw new BusinessValidationException("A valid file must be provided for upload.");
            }

            document.UploadedDate = DateTime.Now;
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
        catch (Exception)
        {
            // FIX: Removed the 'ex' parameter to match your custom exception constructor
            throw new BadRequestException("An error occurred while attempting to upload the document.");
        }
    }

    public async Task<bool> DeleteDocumentAsync(int documentId)
    {
        try
        {
            var document = await _documentRepository.GetByIdAsync(documentId);
            if (document == null)
            {
                throw new NotFoundException($"Cannot delete: Document with ID {documentId} was not found.");
            }

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

            return true;
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception)
        {
            // FIX: Removed the 'ex' parameter
            throw new BadRequestException("An error occurred while attempting to delete the document.");
        }
    }

    public async Task<bool> UpdateVerificationStatusAsync(int documentId, string status)
    {
        try
        {
            var document = await _documentRepository.GetByIdAsync(documentId);
            if (document == null)
            {
                throw new NotFoundException($"Cannot update status: Document with ID {documentId} was not found.");
            }

            document.VerificationStatus = status;
            await _documentRepository.UpdateAsync(document);
            return true;
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception)
        {
            // FIX: Removed the 'ex' parameter
            throw new BadRequestException("An error occurred while attempting to update the document's verification status.");
        }
    }

    public async Task<bool> ReuploadDocumentAsync(int documentId, IFormFile file)
    {
        try
        {
            var document = await _documentRepository.GetByIdAsync(documentId);
            if (document == null)
            {
                throw new NotFoundException($"Cannot re-upload: Document with ID {documentId} was not found.");
            }

            if (file == null || file.Length == 0)
            {
                throw new BusinessValidationException("A valid file must be provided for re-upload.");
            }

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
            document.UploadedDate = DateTime.Now;
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
        catch (NotFoundException)
        {
            throw;
        }
        catch (BusinessValidationException)
        {
            throw;
        }
        catch (Exception)
        {
            // FIX: Removed the 'ex' parameter
            throw new BadRequestException("An error occurred while attempting to re-upload the document.");
        }
    }

    public async Task<string> SaveFileAsync(IFormFile file, string docType)
    {
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