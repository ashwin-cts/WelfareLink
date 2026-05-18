namespace WelfareLink.CitizenManagement.API.DTOs
{
    public class UpdateCitizenDto
    {
        public int CitizenId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string ContactInfo { get; set; }
        public string Address { get; set; }
    }
}