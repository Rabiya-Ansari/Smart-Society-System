using System.ComponentModel.DataAnnotations;
using SmartSociety.Models.Enums;

namespace SmartSociety.Models
{
    public class Payment
    {
        public int Id { get; set; }


        [Required(ErrorMessage = "Maintenance bill is required.")]
        public int MaintenanceBillId { get; set; }


        [Required(ErrorMessage = "Application user is required.")]
        public string ApplicationUserId { get; set; } = string.Empty;


        [Range(
            0.01,
            999999999.99,
            ErrorMessage = "Payment amount must be greater than 0."
        )]
        public double Amount { get; set; }


        [Required(ErrorMessage = "Payment status is required.")]
        [EnumDataType(typeof(PaymentStatus))]
        public PaymentStatus PaymentStatus { get; set; }


        [Required(ErrorMessage = "Payment method is required.")]
        [EnumDataType(typeof(PaymentMethod))]
        public PaymentMethod PaymentMethod { get; set; }


        [Required(ErrorMessage = "Payment date is required.")]
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;


        [StringLength(
            100,
            ErrorMessage = "Transaction reference cannot exceed 100 characters."
        )]
        [RegularExpression(
            @"^[A-Za-z0-9_-]+$",
            ErrorMessage = "Transaction reference contains invalid characters."
        )]
        public string? TransactionReference { get; set; }


        // Navigation Properties

        public MaintenanceBill MaintenanceBill { get; set; } = null!;

        public Data.ApplicationUser ApplicationUser { get; set; } = null!;
    }
}