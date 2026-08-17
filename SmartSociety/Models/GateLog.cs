using SmartSociety.Data;
using SmartSociety.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace SmartSociety.Models
{
    public class GateLog
    {
        public int Id { get; set; }

        // Visitor
     

        [Display(Name = "Visitor")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a visitor.")]
        public int VisitorId { get; set; }

        public Visitor? Visitor { get; set; }


        // Security Guard


        [Display(Name = "Security Guard")]
        [Required(ErrorMessage = "Please select a security guard.")]
        public string SecurityGuardId { get; set; } = string.Empty;

        public ApplicationUser? SecurityGuard { get; set; }


        // Entry Time


        [Display(Name = "Entry Time")]
        [Required(ErrorMessage = "Entry time is required.")]
        public DateTime EntryTime { get; set; }

        // Exit Time


        [Display(Name = "Exit Time")]
        public DateTime? ExitTime { get; set; }

        // Status


        [Display(Name = "Status")]
        public GateLogStatus Status { get; set; }

        // Remarks


        [Display(Name = "Remarks")]
        [StringLength(
            500,
            ErrorMessage = "Remarks cannot exceed 500 characters."
        )]
        public string? Remarks { get; set; }
    }
}