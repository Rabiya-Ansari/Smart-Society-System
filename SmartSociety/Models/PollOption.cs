using System.ComponentModel.DataAnnotations;

namespace SmartSociety.Models
{
    public class PollOption
    {
        public int Id { get; set; }


        // Poll Foreign Key
        [Required(ErrorMessage = "Poll is required.")]
        public int PollId { get; set; }


        [Required(ErrorMessage = "Option text is required.")]
        [StringLength(
            200,
            MinimumLength = 1,
            ErrorMessage = "Option must be between 1 and 200 characters."
        )]
        [RegularExpression(
            @"^[A-Za-z0-9]+(?:[A-Za-z0-9 ,.'!?()&:/-]*[A-Za-z0-9.!?)]$)?$",
            ErrorMessage = "Option contains invalid characters."
        )]
        public string OptionText { get; set; } = string.Empty;


        // Navigation Properties

        public Poll Poll { get; set; } = null!;

        public ICollection<PollVote> PollVotes { get; set; }
            = new List<PollVote>();
    }
}