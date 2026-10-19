namespace RaceDay_API.Models.Entities
{
    public class Participant
    {
        public int ParticipantId { get; set; }
        public int UserId { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? ContactNumber { get; set; }

        // Navigation 
        public User User { get; set; }
        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}
