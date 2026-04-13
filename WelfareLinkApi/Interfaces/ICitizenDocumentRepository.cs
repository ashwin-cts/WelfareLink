using System.Collections.Generic;
using System.Threading.Tasks;
using WelfareLinkApi.Models;

namespace WelfareLinkApi.Interfaces;

public interface ICitizenDocumentRepository
{
    Task<IEnumerable<CitizenDocument>> GetByCitizenIdAsync(int citizenId);
    Task<CitizenDocument> GetByIdAsync(int documentId); // Changed to int
    Task AddAsync(CitizenDocument document);
    Task UpdateAsync(CitizenDocument document);
    Task DeleteAsync(int documentId);
}
