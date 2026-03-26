using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WelfareLink.Models
{
    public class WelfareProgram
    {
        [Key]
        public int ProgramID { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Budget is required")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Budget { get; set; }

        // Status is set by service layer, NOT required from user input
        [StringLength(50)]
        public string? Status { get; set; }

        public ICollection<Resource>? Resources { get; set; }
    }
}
