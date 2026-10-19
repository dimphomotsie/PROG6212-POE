namespace RaceDay_API.Models.Entities
{
    public class Event
    {
        public int EventId { get; set; }
        public int OrganiserId {  get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Distance {  get; set; } = string.Empty;
        public string EventType { get; set;  } = string.Empty;
        public string? BannerImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation 

        public Organiser Organiser { get; set; } = null;
        public ICollection<Category> Category { get; set; } = new List<Category>();
        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();

    }
}
