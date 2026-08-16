using System.ComponentModel.DataAnnotations;

namespace SmartSociety.Models
{
    public class Visitor
    {
        public int Id { get; set; }

        // Foreign Key
        [Required(ErrorMessage = "Flat is required.")]
        public int FlatId { get; set; }

        [Required(ErrorMessage = "Visitor name is required.")]
        [StringLength(
            100,
            MinimumLength = 2,
            ErrorMessage = "Visitor name must be between 2 and 100 characters."
        )]
        [RegularExpression(
            @"^[A-Za-z]+(?:[ '-][A-Za-z]+)*$",
            ErrorMessage = "Visitor name can contain only letters, spaces, apostrophes and hyphens."
        )]
        public string VisitorName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(
            @"^(03\d{9})$",
            ErrorMessage = "Enter a valid Pakistani mobile number, e.g. 03001234567."
        )]
        public string Phone { get; set; } = string.Empty;

        [StringLength(
            20,
            MinimumLength = 2,
            ErrorMessage = "Vehicle number must be between 2 and 20 characters."
        )]
        [RegularExpression(
            @"^[A-Za-z0-9 -]+$",
            ErrorMessage = "Vehicle number contains invalid characters."
        )]
        public string? VehicleNumber { get; set; }

        [Required(ErrorMessage = "Gate pass code is required.")]
        [StringLength(
            50,
            MinimumLength = 6,
            ErrorMessage = "Gate pass code must be between 6 and 50 characters."
        )]
        [RegularExpression(
            @"^[A-Za-z0-9-]+$",
            ErrorMessage = "Gate pass code can contain only letters, numbers and hyphens."
        )]
        public string GatePassCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Valid from date and time is required.")]
        public DateTime ValidFrom { get; set; }

        [Required(ErrorMessage = "Valid until date and time is required.")]
        public DateTime ValidUntil { get; set; }

        public bool IsApproved { get; set; } = false;

        // Navigation Property
        public Flat? Flat { get; set; }

        public ICollection<GateLog> GateLogs { get; set; }
            = new List<GateLog>();
    }
}