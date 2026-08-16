using System.ComponentModel.DataAnnotations;

namespace SmartSociety.Models
{
    public class Poll
    {
        public int Id { get; set; }


        [Required(ErrorMessage = "Question is required.")]
        [StringLength(
            500,
            MinimumLength = 5,
            ErrorMessage = "Question must be between 5 and 500 characters."
        )]
        public string Question { get; set; } = string.Empty;


        [Required(ErrorMessage = "Start date is required.")]
        public DateTime StartDate { get; set; }


        [Required(ErrorMessage = "End date is required.")]
        public DateTime EndDate { get; set; }


        public bool IsActive { get; set; } = true;


        // Navigation Properties

        public ICollection<PollOption> PollOptions { get; set; }
            = new List<PollOption>();

        public ICollection<PollVote> PollVotes { get; set; }
            = new List<PollVote>();
    }
}