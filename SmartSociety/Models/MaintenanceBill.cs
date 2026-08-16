using System.ComponentModel.DataAnnotations;
using SmartSociety.Models.Enums;

namespace SmartSociety.Models
{
    public class MaintenanceBill
    {
        public int Id { get; set; }

        // ==============================
        // Flat
        // ==============================

        [Display(Name = "Flat")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a flat.")]
        public int FlatId { get; set; }


        // ==============================
        // Billing Month
        // ==============================

        [Display(Name = "Billing Month")]
        [Required(ErrorMessage = "Billing month is required.")]
        [DataType(DataType.Date)]
        public DateTime BillingMonth { get; set; }


        // ==============================
        // Amount Due
        // ==============================

        [Display(Name = "Amount Due")]
        [Range(
            0.01,
            999999999.99,
            ErrorMessage = "Amount due must be greater than 0."
        )]
        public double AmountDue { get; set; }


        // ==============================
        // Due Date
        // ==============================

        [Display(Name = "Due Date")]
        [Required(ErrorMessage = "Due date is required.")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }


        // ==============================
        // Penalty
        // ==============================

        [Display(Name = "Penalty Amount")]
        [Range(
            0,
            999999999.99,
            ErrorMessage = "Penalty amount cannot be negative."
        )]
        public double PenaltyAmount { get; set; }


        // ==============================
        // Payment Status
        // ==============================

        [Display(Name = "Payment Status")]
        public PaymentStatus PaymentStatus { get; set; }
            = PaymentStatus.Pending;


        // ==============================
        // Navigation Properties
        // ==============================

        public Flat? Flat { get; set; }

        public ICollection<BillItem> BillItems { get; set; }
            = new List<BillItem>();

        public ICollection<Payment> Payments { get; set; }
            = new List<Payment>();
    }
}