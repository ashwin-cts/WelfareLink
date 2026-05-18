using Microsoft.EntityFrameworkCore;
using WelfareLink.UserManagement.API.Data;
using WelfareLink.UserManagement.API.Interfaces;
using WelfareLink.UserManagement.API.Models;

namespace WelfareLink.UserManagement.API.Services;

public class WelfareApplicationDocumentService : IWelfareApplicationDocumentService
{
    private readonly WelfareLinkDbContext _context;

    public WelfareApplicationDocumentService(WelfareLinkDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<WelfareApplicationDocument>> GetApplicationDocumentsAsync(int applicationId)
    {
        return await _context.WelfareApplicationDocuments
            .Where(d => d.ApplicationID == applicationId)
            .ToListAsync();
    }

    public async Task<IEnumerable<int>> GetApplicationDocumentIdsAsync(int applicationId)
    {
        return await _context.WelfareApplicationDocuments
            .Where(d => d.ApplicationID == applicationId)
            .Select(d => d.DocumentID)
            .ToListAsync();
    }

    public async Task AddApplicationDocumentsAsync(int applicationId, int[] documentIds)
    {
        if (documentIds == null || documentIds.Length == 0)
            return;

        var appDocs = documentIds.Select(docId => new WelfareApplicationDocument
        {
            ApplicationID = applicationId,
            DocumentID = docId
        });
        
        _context.WelfareApplicationDocuments.AddRange(appDocs);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateApplicationDocumentsAsync(int applicationId, int[] newDocumentIds)
    {
        // Remove old document links
        var oldDocs = await _context.WelfareApplicationDocuments
            .Where(d => d.ApplicationID == applicationId)
            .ToListAsync();
        _context.WelfareApplicationDocuments.RemoveRange(oldDocs);

        // Add new document links
        if (newDocumentIds != null && newDocumentIds.Length > 0)
        {
            var newDocs = newDocumentIds.Select(docId => new WelfareApplicationDocument
            {
                ApplicationID = applicationId,
                DocumentID = docId
            });
            _context.WelfareApplicationDocuments.AddRange(newDocs);
        }

        await _context.SaveChangesAsync();
    }

    public async Task RemoveApplicationDocumentsAsync(int applicationId)
    {
        var docs = await _context.WelfareApplicationDocuments
            .Where(d => d.ApplicationID == applicationId)
            .ToListAsync();
        _context.WelfareApplicationDocuments.RemoveRange(docs);
        await _context.SaveChangesAsync();
    }
}
