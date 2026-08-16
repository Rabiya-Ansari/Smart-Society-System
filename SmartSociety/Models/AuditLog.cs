using System.ComponentModel.DataAnnotations;
using SmartSociety.Data;

namespace SmartSociety.Models
{
    public class AuditLog
    {
        public int Id { get; set; }


        // Identity User Foreign Key
        [Required(ErrorMessage = "User is required.")]
        public string ApplicationUserId { get; set; } = string.Empty;


        [Required(ErrorMessage = "Action is required.")]
        [StringLength(
            100,
            MinimumLength = 2,
            ErrorMessage = "Action must be between 2 and 100 characters."
        )]
        [RegularExpression(
            @"^[A-Za-z]+(?:[A-Za-z0-9 _-]*[A-Za-z0-9])?$",
            ErrorMessage = "Action contains invalid characters."
        )]
        public string Action { get; set; } = string.Empty;


        [StringLength(
            100,
            ErrorMessage = "Entity name cannot exceed 100 characters."
        )]
        [RegularExpression(
            @"^[A-Za-z][A-Za-z0-9_]*$",
            ErrorMessage = "Entity name contains invalid characters."
        )]
        public string? EntityName { get; set; }


        [StringLength(
            100,
            ErrorMessage = "Entity ID cannot exceed 100 characters."
        )]
        public string? EntityId { get; set; }


        [StringLength(
            1000,
            ErrorMessage = "Details cannot exceed 1000 characters."
        )]
        public string? Details { get; set; }


        [Required(ErrorMessage = "Timestamp is required.")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;


        [StringLength(
            45,
            ErrorMessage = "IP address cannot exceed 45 characters."
        )]
        public string? IpAddress { get; set; }


        // Navigation Property

        public ApplicationUser ApplicationUser { get; set; } = null!;
    }
}