using System.ComponentModel.DataAnnotations;
using SmartSociety.Models.Enums;

namespace SmartSociety.Models
{
    public class AmenityBooking
    {
        public int Id { get; set; }


        // ==============================
        // Resident Foreign Key
        // ==============================

        [Display(Name = "Resident")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a resident.")]
        public int ResidentProfileId { get; set; }


        // ==============================
        // Amenity Foreign Key
        // ==============================

        [Display(Name = "Amenity")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select an amenity.")]
        public int AmenityId { get; set; }


        // ==============================
        // Booking Date
        // ==============================

        [Display(Name = "Booking Date")]
        [Required(ErrorMessage = "Booking date is required.")]
        [DataType(DataType.Date)]
        public DateTime BookingDate { get; set; }


        // ==============================
        // Start Time
        // ==============================

        [Display(Name = "Start Time")]
        [Required(ErrorMessage = "Start time is required.")]
        public TimeSpan StartTime { get; set; }


        // ==============================
        // End Time
        // ==============================

        [Display(Name = "End Time")]
        [Required(ErrorMessage = "End time is required.")]
        public TimeSpan EndTime { get; set; }


        // ==============================
        // Booking Status
        // ==============================

        [Display(Name = "Status")]
        public BookingStatus Status { get; set; }
            = BookingStatus.Pending;


        // ==============================
        // Remarks
        // ==============================

        [Display(Name = "Remarks")]
        [StringLength(
            500,
            ErrorMessage = "Remarks cannot exceed 500 characters."
        )]
        public string? Remarks { get; set; }


        // ==============================
        // Created At
        // ==============================

        [Required]
        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;


        // ==============================
        // Navigation Properties
        // ==============================

        public ResidentProfile? ResidentProfile { get; set; }

        public Amenity? Amenity { get; set; }
    }
}