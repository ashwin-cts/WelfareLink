using WelfareLink.ProgramManagement.API.Models;

namespace WelfareLink.ProgramManagement.API.Interfaces;

public interface IWelfareApplicationDocumentService
{
    Task<IEnumerable<WelfareApplicationDocument>> GetApplicationDocumentsAsync(int applicationId);
    Task<IEnumerable<int>> GetApplicationDocumentIdsAsync(int applicationId);
    Task AddApplicationDocumentsAsync(int applicationId, int[] documentIds);
    Task UpdateApplicationDocumentsAsync(int applicationId, int[] newDocumentIds);
    Task RemoveApplicationDocumentsAsync(int applicationId);
}
