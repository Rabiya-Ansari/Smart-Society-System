using System.ComponentModel.DataAnnotations;

namespace SmartSociety.Models
{
    public class Amenity
    {
        public int Id { get; set; }


        [Required(ErrorMessage = "Amenity name is required.")]
        [StringLength(
            100,
            MinimumLength = 2,
            ErrorMessage = "Amenity name must be between 2 and 100 characters."
        )]
        [RegularExpression(
            @"^[A-Za-z]+(?:[ '-][A-Za-z]+)*$",
            ErrorMessage = "Amenity name contains invalid characters."
        )]
        public string Name { get; set; } = string.Empty;


        [Required(ErrorMessage = "Description is required.")]
        [StringLength(
            500,
            MinimumLength = 5,
            ErrorMessage = "Description must be between 5 and 500 characters."
        )]
        public string Description { get; set; } = string.Empty;


        [Required(ErrorMessage = "Location is required.")]
        [StringLength(
            200,
            MinimumLength = 2,
            ErrorMessage = "Location must be between 2 and 200 characters."
        )]
        [RegularExpression(
            @"^[A-Za-z0-9]+(?:[ ,.'/#-][A-Za-z0-9]+)*$",
            ErrorMessage = "Location contains invalid characters."
        )]
        public string Location { get; set; } = string.Empty;


        [Range(
            1,
            10000,
            ErrorMessage = "Capacity must be between 1 and 10000."
        )]
        public int Capacity { get; set; }


        public bool IsActive { get; set; } = true;


        // Navigation Property

        public ICollection<AmenityBooking> AmenityBookings { get; set; }
            = new List<AmenityBooking>();
    }
}