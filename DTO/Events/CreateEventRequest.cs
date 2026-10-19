using System.ComponentModel.DataAnnotations;

namespace RaceDay_API.DTO.Events
{
    public class CreateEventRequest
    {
        [Required, MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime EventDate { get; set; }

        [Required, MaxLength(255)]
        public string Location { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Distance { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^(Run|Walk|Cycle)$",
            ErrorMessage = "EventType must be Run, Walk, or Cycle")]
        public string EventType { get; set; } = string.Empty;

        public string? BannerImageUrl { get; set; }
    }
}
