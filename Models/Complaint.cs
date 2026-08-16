using System.ComponentModel.DataAnnotations;
using SmartSociety.Data;
using SmartSociety.Models.Enums;

namespace SmartSociety.Models
{
    public class Complaint
    {
        public int Id { get; set; }


        // Resident Foreign Key
        [Required(ErrorMessage = "Resident is required.")]
        public int ResidentProfileId { get; set; }


        [Required(ErrorMessage = "Complaint category is required.")]
        [EnumDataType(typeof(ComplaintCategory))]
        public ComplaintCategory Category { get; set; }


        [Required(ErrorMessage = "Complaint description is required.")]
        [StringLength(
            1000,
            MinimumLength = 10,
            ErrorMessage = "Description must be between 10 and 1000 characters."
        )]
        public string Description { get; set; } = string.Empty;


        [Required(ErrorMessage = "Complaint status is required.")]
        [EnumDataType(typeof(ComplaintStatus))]
        public ComplaintStatus Status { get; set; } = ComplaintStatus.Pending;


        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public DateTime? ResolvedAt { get; set; }


        [StringLength(1000, ErrorMessage = "Work notes cannot exceed 1000 characters.")]
        public string? WorkNotes { get; set; }


        public DateTime? SlaTargetDate { get; set; }


        // Maintenance Staff Identity User Foreign Key
        public string? AssignedStaffId { get; set; }


        // Navigation Properties

        public ResidentProfile ResidentProfile { get; set; } = null!;

        public ApplicationUser? AssignedStaff { get; set; }
    }
}