using System;
using System.ComponentModel.DataAnnotations;

namespace WelfareLink.ViewModels
{
    public class ComplainceRecordViewModel
    {
        [Display(Name = "Complaince ID")]
        public string ComplainceID { get; set; } = string.Empty;

        [Display(Name = "Entity ID")]
        public string EntityID { get; set; } = string.Empty;

        [Display(Name = "Type")]
        public string Type { get; set; } = string.Empty;

        [Display(Name = "Result")]
        public string Result { get; set; } = string.Empty;

        [Display(Name = "Date")]
        public DateTime Date { get; set; }

        [Display(Name = "Notes")]
        public string Notes { get; set; } = string.Empty;
    }

    public class ComplainceRecordCreateModel
    {
        [Required]
        [MaxLength(36)]
        [Display(Name = "Entity ID")]
        public string EntityID { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string Type { get; set; } = string.Empty;

        [MaxLength(30)]
        public string Result { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string Notes { get; set; } = string.Empty;
    }
}
