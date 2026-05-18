using WelfareLink.Models;

namespace WelfareLink.Interfaces;

public interface ICitizenDocumentService
{
    Task<IEnumerable<CitizenDocument>> GetDocumentsByCitizenIdAsync(int citizenId);
    Task<CitizenDocument> GetDocumentByIdAsync(int documentId);
    Task<bool> UploadDocumentAsync(CitizenDocument document, IFormFile file);
    Task<bool> UpdateVerificationStatusAsync(int documentId, string status);
    Task<bool> ReuploadDocumentAsync(int documentId, IFormFile file);
    Task<bool> DeleteDocumentAsync(int documentId);
    Task<string> SaveFileAsync(IFormFile file, string docType);

}
