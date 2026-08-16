using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SmartSociety.Models
{
    public class FamilyMember
    {
        public int Id { get; set; }

        // Foreign Key
        [Required(ErrorMessage = "Resident is required.")]
        public int ResidentProfileId { get; set; }

        // Navigation Property
        [ValidateNever]
        public ResidentProfile ResidentProfile { get; set; } = null!;

        // Name
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(
            100,
            MinimumLength = 2,
            ErrorMessage = "Name must be between 2 and 100 characters."
        )]
        [RegularExpression(
            @"^[A-Za-z]+(?:[ '-][A-Za-z]+)*$",
            ErrorMessage = "Name can contain only letters, spaces, apostrophes and hyphens."
        )]
        public string Name { get; set; } = string.Empty;

        // Relationship
        [Required(ErrorMessage = "Relationship is required.")]
        [StringLength(
            50,
            MinimumLength = 2,
            ErrorMessage = "Relationship must be between 2 and 50 characters."
        )]
        [RegularExpression(
            @"^[A-Za-z]+(?:[ -][A-Za-z]+)*$",
            ErrorMessage = "Relationship contains invalid characters."
        )]
        public string Relationship { get; set; } = string.Empty;

        // Date of Birth
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }
    }
}