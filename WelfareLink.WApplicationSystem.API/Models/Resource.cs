using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WelfareLink.WApplicationSystem.API.Models
{
    public class Resource
    {
        [Key]
        public int ResourceID { get; set; }

        [Required]
        [ForeignKey("Program")]
        public int ProgramID { get; set; }

        [Required]
        [StringLength(50)]
        public string Type { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Quantity { get; set; }

        [StringLength(50)]
        public string? Status { get; set; }

        [JsonIgnore]
        public WelfareProgram? Program { get; set; }    //navigation Property
    }
}
