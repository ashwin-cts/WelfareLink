namespace WelfareLink.UserManagement.API.Models
{
    public class CitizenApplyRequest
    {
        public int CitizenID { get; set; }
        public int ProgramID { get; set; }
        public int[]? SelectedDocumentIds { get; set; }
    }

    public class CreateCitizenRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; } = string.Empty;
        public string ContactInfo { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
    }
}
