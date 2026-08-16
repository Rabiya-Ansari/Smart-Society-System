using SmartSociety.Data;
using SmartSociety.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace SmartSociety.Models
{
    public class Complaint
    {
        public int Id { get; set; }

        // Resident Foreign Key
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Please select a resident."
        )]
        public int ResidentProfileId { get; set; }

        // Complaint Category
        [EnumDataType(
            typeof(ComplaintCategory),
            ErrorMessage = "Please select a valid complaint category."
        )]
        public ComplaintCategory Category { get; set; }

        // Description
        [Required(ErrorMessage = "Complaint description is required.")]
        [StringLength(
            1000,
            MinimumLength = 10,
            ErrorMessage = "Description must be between 10 and 1000 characters."
        )]
        public string Description { get; set; } = string.Empty;

        // Status
        [EnumDataType(typeof(ComplaintStatus))]
        public ComplaintStatus Status { get; set; } = ComplaintStatus.Pending;

        // Created date
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Resolved date
        public DateTime? ResolvedAt { get; set; }

        // Work notes
        [StringLength(
            1000,
            ErrorMessage = "Work notes cannot exceed 1000 characters."
        )]
        public string? WorkNotes { get; set; }

        // SLA
        public DateTime? SlaTargetDate { get; set; }

        // Maintenance Staff
        public string? AssignedStaffId { get; set; }

        // Navigation Properties
        public ResidentProfile? ResidentProfile { get; set; }

        public ApplicationUser? AssignedStaff { get; set; }
    }
}