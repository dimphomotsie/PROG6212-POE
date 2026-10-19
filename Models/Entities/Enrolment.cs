namespace RaceDay_API.Models.Entities
{
    public class Enrolment
    {
        public int EnrolmentId { get; set; }
        public int ParticipantId { get; set; }
        public int EventId { get; set; }
        public int CategoryId { get; set; }
        public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending";


        // Navigation 
        public Participant Participant { get; set; } = null;
        public Event Event { get; set; } = null;
        public Category Category { get; set; } = null;
        public Result? Result { get; set; }  
    }
}
