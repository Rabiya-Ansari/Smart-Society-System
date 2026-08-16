using System.ComponentModel.DataAnnotations;
using SmartSociety.Models.Enums;

namespace SmartSociety.Models
{
    public class MaintenanceBill
    {
        public int Id { get; set; }


        [Required(ErrorMessage = "Flat is required.")]
        public int FlatId { get; set; }


        [Range(
            0.01,
            999999999.99,
            ErrorMessage = "Amount due must be greater than 0."
        )]
        public double AmountDue { get; set; }


        [Required(ErrorMessage = "Due date is required.")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }


        [Required(ErrorMessage = "Payment status is required.")]
        [EnumDataType(typeof(PaymentStatus))]
        public PaymentStatus PaymentStatus { get; set; }


        [Required(ErrorMessage = "Billing month is required.")]
        [DataType(DataType.Date)]
        public DateTime BillingMonth { get; set; }


        [Range(
            0,
            999999999.99,
            ErrorMessage = "Penalty amount cannot be negative."
        )]
        public double PenaltyAmount { get; set; }


        // Navigation Properties

        public Flat Flat { get; set; } = null!;

        public ICollection<BillItem> BillItems { get; set; }
            = new List<BillItem>();

        public ICollection<Payment> Payments { get; set; }
            = new List<Payment>();
    }
}