using System.ComponentModel.DataAnnotations;
using SmartSociety.Models.Enums;

namespace SmartSociety.Models
{
    public class AmenityBooking
    {
        public int Id { get; set; }


        // Resident Foreign Key
        [Required(ErrorMessage = "Resident is required.")]
        public int ResidentProfileId { get; set; }


        // Amenity Foreign Key
        [Required(ErrorMessage = "Amenity is required.")]
        public int AmenityId { get; set; }


        [Required(ErrorMessage = "Booking date is required.")]
        [DataType(DataType.Date)]
        public DateTime BookingDate { get; set; }


        [Required(ErrorMessage = "Start time is required.")]
        public TimeSpan StartTime { get; set; }


        [Required(ErrorMessage = "End time is required.")]
        public TimeSpan EndTime { get; set; }


        [Required(ErrorMessage = "Booking status is required.")]
        [EnumDataType(typeof(BookingStatus))]
        public BookingStatus Status { get; set; }
            = BookingStatus.Pending;


        [StringLength(
            500,
            ErrorMessage = "Remarks cannot exceed 500 characters."
        )]
        public string? Remarks { get; set; }


        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        // Navigation Properties

        public ResidentProfile ResidentProfile { get; set; } = null!;

        public Amenity Amenity { get; set; } = null!;
    }
}