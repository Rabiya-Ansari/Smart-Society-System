using System.ComponentModel.DataAnnotations;
using SmartSociety.Models.Enums;

namespace SmartSociety.Models
{
    public class Flat
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Block name is required.")]
        [StringLength(50, MinimumLength = 1,
            ErrorMessage = "Block name must be between 1 and 50 characters.")]
        [RegularExpression(
            @"^[A-Za-z0-9]+(?:[- ]?[A-Za-z0-9]+)*$",
            ErrorMessage = "Block name contains invalid characters."
        )]
        public string BlockName { get; set; } = string.Empty;


        [Required(ErrorMessage = "Flat number is required.")]
        [StringLength(20, MinimumLength = 1,
            ErrorMessage = "Flat number must be between 1 and 20 characters.")]
        [RegularExpression(
            @"^[A-Za-z0-9]+(?:[-/]?[A-Za-z0-9]+)*$",
            ErrorMessage = "Flat number contains invalid characters."
        )]
        public string FlatNumber { get; set; } = string.Empty;


        [Required(ErrorMessage = "Occupancy type is required.")]
        [EnumDataType(typeof(OccupancyType))]
        public OccupancyType OccupancyType { get; set; }


        public bool IsOccupied { get; set; } = false;


        // Navigation Properties

        public ICollection<ResidentProfile> Residents { get; set; }
            = new List<ResidentProfile>();

        public ICollection<MaintenanceBill> MaintenanceBills { get; set; }
            = new List<MaintenanceBill>();

        public ICollection<Visitor> Visitors { get; set; }
            = new List<Visitor>();
    }
}