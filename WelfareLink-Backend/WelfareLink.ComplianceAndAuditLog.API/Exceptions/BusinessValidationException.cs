namespace WelfareLink.ComplianceAndAuditLog.API.Exceptions
{
    public class BusinessValidationException : Exception
    {
        public BusinessValidationException(string message) : base(message)
        {
        }
    }
}