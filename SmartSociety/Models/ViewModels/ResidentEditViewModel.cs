using System.ComponentModel.DataAnnotations;

namespace SmartSociety.Models.ViewModels
{
    public class ResidentEditViewModel
    {
        public int Id { get; set; }


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


        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(
            100,
            ErrorMessage = "Email cannot exceed 100 characters."
        )]
        public string Email { get; set; } = string.Empty;


        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(
            @"^(?:\+92|0)3[0-9]{9}$",
            ErrorMessage = "Enter a valid Pakistani phone number."
        )]
        public string PhoneNumber { get; set; } = string.Empty;


        [Required(ErrorMessage = "Flat is required.")]
        public int FlatId { get; set; }
    }
}