using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;

namespace WelfareLink.Models
{
    public class Citizen
    {
       
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CitizenId { get; set; }

        public int UserId { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }
        [StringLength(300)]
        public string Address { get; set; }
        [StringLength(50)]
        public string ContactInfo { get; set; }
        [StringLength(50)]
        public string Status { get; set; }
        [StringLength(20)]
        public string? Gender { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //Navigation Property one to many for CitizenDocument
        public virtual ICollection<CitizenDocument> CitizenDocuments { get; set; }



    }
}
