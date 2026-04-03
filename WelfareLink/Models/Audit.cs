using System;
using System.ComponentModel.DataAnnotations;

namespace WelfareLink.Models
{
    public class Audit
    {
        // Primary key as varchar(36) in the ER diagram
        [Key]
        [MaxLength(36)]
        public string AuditID { get; set; } = string.Empty;

        // Reference to the user/officer who performed the audit
        [MaxLength(36)]
        public string OfficerID { get; set; } = string.Empty;

        // Description of scope (varchar(500))
        [MaxLength(500)]
        public string Scope { get; set; } = string.Empty;

        // Findings (varchar(4000))
        [MaxLength(4000)]
        public string Findings { get; set; } = string.Empty;

        // Timestamp
        public DateTime Date { get; set; }

        // Status (varchar(30))
        [MaxLength(30)]
        public string Status { get; set; } = string.Empty;
    }
}
