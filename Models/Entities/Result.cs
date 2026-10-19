namespace RaceDay_API.Models.Entities
{
    public class Result
    {
        public int ResultId { get; set; }
        public int EnrolmentId { get; set; }
        public TimeSpan? FinishTime { get; set; }
        public int? FinishPostion { get; set; }
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;


        // Navigation 
        public Enrolment Enrolment { get; set; } = null;
    }
}
