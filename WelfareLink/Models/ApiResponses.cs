namespace WelfareLink.Models
{
    /// <summary>
    /// Generic API response wrapper for operations that return just a message
    /// </summary>
    public class MessageResponse
    {
        public string? Message { get; set; }
    }

    /// <summary>
    /// Response from citizen profile creation
    /// </summary>
    public class CreateProfileResponse
    {
        public string? Message { get; set; }
        public int CitizenId { get; set; }
    }

    /// <summary>
    /// Response from application submission
    /// </summary>
    public class ApplicationSubmissionResponse
    {
        public string? Message { get; set; }
        public int ApplicationID { get; set; }
    }

    /// <summary>
    /// Generic error response
    /// </summary>
    public class ErrorResponse
    {
        public string? Error { get; set; }
        public string? Message { get; set; }
    }
}
