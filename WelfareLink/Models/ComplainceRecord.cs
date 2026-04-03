using System;
using System.ComponentModel.DataAnnotations;

namespace WelfareLink.Models
{
    public class ComplainceRecord
    {
        // Primary key as varchar(36) in the ER diagram
        [Key]
        [MaxLength(36)]
        public string ComplainceID { get; set; } = string.Empty;

        // Entity that the record references (varchar(36))
        [MaxLength(36)]
        public string EntityID { get; set; } = string.Empty;

        // Type (varchar(30))
        [MaxLength(30)]
        public string Type { get; set; } = string.Empty;

        // Result (varchar(30))
        [MaxLength(30)]
        public string Result { get; set; } = string.Empty;

        // Timestamp
        public DateTime Date { get; set; }

        // Notes (varchar(2000))
        [MaxLength(2000)]
        public string Notes { get; set; } = string.Empty;
    }
}
