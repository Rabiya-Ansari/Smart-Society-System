using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SmartSociety.Models
{
    public class EmergencyContact
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Resident is required.")]
        public int ResidentProfileId { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, MinimumLength = 2)]
        [RegularExpression(
            @"^[A-Za-z]+(?:[ '-][A-Za-z]+)*$",
            ErrorMessage = "Name can contain only letters, spaces, apostrophes and hyphens."
        )]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Relationship is required.")]
        [StringLength(50, MinimumLength = 2)]
        [RegularExpression(
            @"^[A-Za-z]+(?:[ -][A-Za-z]+)*$",
            ErrorMessage = "Relationship contains invalid characters."
        )]
        public string Relationship { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(
            @"^(03\d{9})$",
            ErrorMessage = "Enter a valid Pakistani mobile number, e.g. 03001234567."
        )]
        public string PhoneNumber { get; set; } = string.Empty;

        // Navigation property ko form validation se exclude karo
        [ValidateNever]
        public ResidentProfile ResidentProfile { get; set; } = null!;
    }
}