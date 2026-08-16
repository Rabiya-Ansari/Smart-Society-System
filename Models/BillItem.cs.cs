using System.ComponentModel.DataAnnotations;

namespace SmartSociety.Models
{
    public class BillItem
    {
        public int Id { get; set; }


        [Required(ErrorMessage = "Maintenance bill is required.")]
        public int MaintenanceBillId { get; set; }


        [Required(ErrorMessage = "Description is required.")]
        [StringLength(
            200,
            MinimumLength = 2,
            ErrorMessage = "Description must be between 2 and 200 characters."
        )]
        [RegularExpression(
            @"^[A-Za-z0-9]+(?:[A-Za-z0-9 ,.'()&:/-]*[A-Za-z0-9])?$",
            ErrorMessage = "Description contains invalid characters."
        )]
        public string Description { get; set; } = string.Empty;


        [Range(
            0.01,
            999999999.99,
            ErrorMessage = "Amount must be greater than 0."
        )]
        public double Amount { get; set; }


        // Navigation Property

        public MaintenanceBill MaintenanceBill { get; set; } = null!;
    }
}