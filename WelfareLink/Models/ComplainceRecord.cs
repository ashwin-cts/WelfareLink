using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WelfareLink.Models
{
    public class ComplianceRecord
    {
        [Key]
        [StringLength(36)]
        public string ComplianceID { get; set; }
       // [cite: 100]

        [Required(ErrorMessage = "Entity ID is required")]
        [StringLength(36)]
        public string EntityID { get; set; }
      //  [cite: 100]

        [Required(ErrorMessage = "Type is required")]
        [StringLength(30)]
        public string Type { get; set; } // Application, Benefit, or Program [cite: 73, 100]

        [Required(ErrorMessage = "Result is required")]
        [StringLength(30)]
        public string Result { get; set; }
      //  [cite: 100]

        [Required(ErrorMessage = "Date is required")]
        [DataType(DataType.DateTime)]
        public DateTime Date { get; set; }
      //  [cite: 100]

        [StringLength(2000)]
        public string? Notes { get; set; }
       // [cite: 100]
    }
}
