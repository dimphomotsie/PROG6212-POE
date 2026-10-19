using System.ComponentModel.DataAnnotations;

namespace RaceDay_API.DTO.Enrolments
{
    public class CreateEnrolmentRequest
    {
        [Required]
        public int CategoryId { get; set; }
    }
}
