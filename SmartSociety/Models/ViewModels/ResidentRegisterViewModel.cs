using System.ComponentModel.DataAnnotations;

namespace SmartSociety.Models.ViewModels
{
    public class ResidentRegisterViewModel
    {
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


        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;


        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(
            @"^(?:\+92|0)3[0-9]{9}$",
            ErrorMessage = "Enter a valid Pakistani phone number."
        )]
        public string PhoneNumber { get; set; } = string.Empty;


        [Required(ErrorMessage = "CNIC is required.")]
        [RegularExpression(
            @"^\d{5}-\d{7}-\d$",
            ErrorMessage = "CNIC must be in format 12345-1234567-1."
        )]
        public string CNIC { get; set; } = string.Empty;


        [Required(ErrorMessage = "Flat is required.")]
        public int FlatId { get; set; }


        [Required(ErrorMessage = "Password is required.")]
        [StringLength(
            100,
            MinimumLength = 8,
            ErrorMessage = "Password must be at least 8 characters."
        )]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;


        [Required(ErrorMessage = "Confirm password is required.")]
        [Compare(
            nameof(Password),
            ErrorMessage = "Password and confirm password do not match."
        )]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}