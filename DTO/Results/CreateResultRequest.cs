using System.ComponentModel.DataAnnotations;

namespace RaceDay_API.DTO.Results
{
    public class CreateResultRequest
    {
        [Required]
        public TimeSpan FinishTime { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int FinishingPosition { get; set; }
        public int? FinishPosition { get; internal set; }
    }
}
