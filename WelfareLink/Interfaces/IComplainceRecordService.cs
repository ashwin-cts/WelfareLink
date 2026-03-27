namespace WelfareLink.Interfaces;

public interface IComplainceRecordService
{ 
   Task<IEnumerable<ComplainceRecord>> GetAllRecordsAsync();
  Task<ComplainceRecord> GetRecordByIdAsync(string complianceId);
  Task<bool> CreateRecordAsync(ComplainceRecord record);
  Task<IEnumerable<ComplainceRecord>> GetRecordsByEntityAsync(string entityId);
}
