namespace WelfareLink.UserManagement.API.Exceptions
{
    public class BusinessValidationException : Exception
    {
        public BusinessValidationException(string message) : base(message)
        {
        }
    }
}