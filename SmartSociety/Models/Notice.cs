using System.ComponentModel.DataAnnotations;
using SmartSociety.Models.Enums;

namespace SmartSociety.Models
{
    public class Notice
    {
        public int Id { get; set; }


        [Required(ErrorMessage = "Title is required.")]
        [StringLength(
            200,
            MinimumLength = 3,
            ErrorMessage = "Title must be between 3 and 200 characters."
        )]
        [RegularExpression(
            @"^[A-Za-z0-9]+(?:[A-Za-z0-9 ,.'!?()&:/-]*[A-Za-z0-9.!?)]$)?$",
            ErrorMessage = "Title contains invalid characters."
        )]
        public string Title { get; set; } = string.Empty;


        [Required(ErrorMessage = "Content is required.")]
        [StringLength(
            5000,
            MinimumLength = 10,
            ErrorMessage = "Content must be between 10 and 5000 characters."
        )]
        public string Content { get; set; } = string.Empty;


        [Required(ErrorMessage = "Notice type is required.")]
        [EnumDataType(typeof(NoticeType))]
        public NoticeType NoticeType { get; set; }


        [Required(ErrorMessage = "Publish date is required.")]
        public DateTime PublishDate { get; set; } = DateTime.UtcNow;


        [Required(ErrorMessage = "Expiry date is required.")]
        public DateTime ExpiryDate { get; set; }


        public bool IsPublished { get; set; } = false;
    }
}