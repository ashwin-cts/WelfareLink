using System.ComponentModel.DataAnnotations;
using WelfareLink.WApplicationSystem.API.Models;

namespace WelfareLink.WApplicationSystem.API.ViewModels
{
    public class DocumentUploadViewModel
    {
        public int CitizenId { get; set; }

        [Required]
        [Display(Name = "Document Type")]
        public string DocType { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Document Name")]
        public string DocumentName { get; set; }

        [Required]
        [Display(Name = "Select File")]
        public IFormFile FileUpload { get; set; }
    }

    public class DocumentStatusViewModel
    {
        public IEnumerable<CitizenDocument> Documents { get; set; }
        public string FilterStatus { get; set; }
    }

    public class ReuploadDocumentViewModel
    {
        public int DocumentID { get; set; }

        public string DocumentName { get; set; } = string.Empty;

        public string DocType { get; set; } = string.Empty;

        public string? CurrentFileURI { get; set; }

        public string? VerificationStatus { get; set; }

        [Required]
        [Display(Name = "Select New File")]
        public IFormFile FileUpload { get; set; }
    }
}
