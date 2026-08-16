using System.ComponentModel.DataAnnotations;
using SmartSociety.Data;

namespace SmartSociety.Models
{
    public class PollVote
    {
        public int Id { get; set; }


        // Poll Foreign Key
        [Required(ErrorMessage = "Poll is required.")]
        public int PollId { get; set; }


        // Selected Option Foreign Key
        [Required(ErrorMessage = "Poll option is required.")]
        public int PollOptionId { get; set; }


        // Resident Foreign Key
        [Required(ErrorMessage = "Resident is required.")]
        public int ResidentProfileId { get; set; }


        [Required(ErrorMessage = "Vote date is required.")]
        public DateTime VotedAt { get; set; } = DateTime.UtcNow;


        // Navigation Properties

        public Poll Poll { get; set; } = null!;

        public PollOption PollOption { get; set; } = null!;

        public ResidentProfile ResidentProfile { get; set; } = null!;
    }
}