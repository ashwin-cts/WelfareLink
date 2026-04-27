using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WelfareLink.AnalyticsReport.API.Models
{
    public class WelfareApplicationDocument
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("WelfareApplication")]
        public int ApplicationID { get; set; }

        [Required]
        [ForeignKey("CitizenDocument")]
        public int DocumentID { get; set; }

        [JsonIgnore]
        public virtual WelfareApplication? WelfareApplication { get; set; }

        public virtual CitizenDocument? CitizenDocument { get; set; }
    }
}
