using System.ComponentModel.DataAnnotations;

namespace SmartSociety.Models
{
    public class Vehicle
    {
        public int Id { get; set; }

        // Foreign Key
        [Required(ErrorMessage = "Resident is required.")]
        public int ResidentProfileId { get; set; }

        [Required(ErrorMessage = "Vehicle registration number is required.")]
        [StringLength(
            20,
            MinimumLength = 2,
            ErrorMessage = "Registration number must be between 2 and 20 characters."
        )]
        [RegularExpression(
            @"^[A-Za-z0-9 -]+$",
            ErrorMessage = "Vehicle registration number contains invalid characters."
        )]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vehicle type is required.")]
        [StringLength(
            50,
            MinimumLength = 2,
            ErrorMessage = "Vehicle type must be between 2 and 50 characters."
        )]
        [RegularExpression(
            @"^[A-Za-z]+(?:[ -][A-Za-z]+)*$",
            ErrorMessage = "Vehicle type contains invalid characters."
        )]
        public string VehicleType { get; set; } = string.Empty;

        [StringLength(50)]
        [RegularExpression(
            @"^[A-Za-z0-9 ]+$",
            ErrorMessage = "Make contains invalid characters."
        )]
        public string? Make { get; set; }

        [StringLength(50)]
        [RegularExpression(
            @"^[A-Za-z0-9 ]+$",
            ErrorMessage = "Model contains invalid characters."
        )]
        public string? Model { get; set; }

        [StringLength(30)]
        [RegularExpression(
            @"^[A-Za-z]+(?:[ -][A-Za-z]+)*$",
            ErrorMessage = "Color contains invalid characters."
        )]
        public string? Color { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Property
        public ResidentProfile? ResidentProfile { get; set; }
    }
}