using System.ComponentModel.DataAnnotations;
using SmartSociety.Data;

namespace SmartSociety.Models
{
    public class ResidentProfile
    {
        public int Id { get; set; }


        [Required(ErrorMessage = "Application user is required.")]
        public string ApplicationUserId { get; set; } = string.Empty;


        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(
            100,
            MinimumLength = 2,
            ErrorMessage = "Full name must be between 2 and 100 characters."
        )]
        [RegularExpression(
            @"^[A-Za-z]+(?:[ '-][A-Za-z]+)*$",
            ErrorMessage = "Full name contains invalid characters."
        )]
        public string FullName { get; set; } = string.Empty;


        [Required(ErrorMessage = "CNIC is required.")]
        [RegularExpression(
            @"^\d{5}-\d{7}-\d$",
            ErrorMessage = "CNIC must be in format 12345-1234567-1."
        )]
        public string CNIC { get; set; } = string.Empty;


        [Required(ErrorMessage = "Flat is required.")]
        public int FlatId { get; set; }


        // Navigation Properties

        public ApplicationUser ApplicationUser { get; set; } = null!;

        public Flat Flat { get; set; } = null!;

        public ICollection<Vehicle> Vehicles { get; set; }
            = new List<Vehicle>();

        public ICollection<EmergencyContact> EmergencyContacts { get; set; }
            = new List<EmergencyContact>();

        public ICollection<FamilyMember> FamilyMembers { get; set; }
            = new List<FamilyMember>();

        public ICollection<Complaint> Complaints { get; set; }
            = new List<Complaint>();

        public ICollection<AmenityBooking> AmenityBookings { get; set; }
            = new List<AmenityBooking>();
    }
}