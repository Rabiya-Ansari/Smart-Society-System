using System.ComponentModel.DataAnnotations;
using SmartSociety.Data;
using SmartSociety.Models.Enums;

namespace SmartSociety.Models
{
    public class GateLog
    {
        public int Id { get; set; }


        // Visitor Foreign Key
        [Required(ErrorMessage = "Visitor is required.")]
        public int VisitorId { get; set; }


        // Security Guard Identity User Foreign Key
        [Required(ErrorMessage = "Security guard is required.")]
        public string SecurityGuardId { get; set; } = string.Empty;


        [Required(ErrorMessage = "Entry time is required.")]
        public DateTime EntryTime { get; set; }


        public DateTime? ExitTime { get; set; }


        [Required(ErrorMessage = "Gate log status is required.")]
        [EnumDataType(typeof(GateLogStatus))]
        public GateLogStatus Status { get; set; }


        [StringLength(
            500,
            ErrorMessage = "Remarks cannot exceed 500 characters."
        )]
        public string? Remarks { get; set; }


        // Navigation Properties

        public Visitor Visitor { get; set; } = null!;

        public ApplicationUser SecurityGuard { get; set; } = null!;
    }
}