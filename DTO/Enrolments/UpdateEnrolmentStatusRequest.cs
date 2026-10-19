using System.ComponentModel.DataAnnotations;

namespace RaceDay_API.DTO.Enrolments
{
    public class UpdateEnrolmentStatusRequest
    {
        [Required]
        [RegularExpression("^(Pending|Confirmed|Cancelled)$",
        ErrorMessage = "Status must be Pending, Confirmed, or Cancelled")]
        public string Status { get; set; } = string.Empty;
    }
}
