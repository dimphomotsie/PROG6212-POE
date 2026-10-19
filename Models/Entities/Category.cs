namespace RaceDay_API.Models.Entities
{
    public class Category
    {
        public int CategoryId { get; set; }
        public int EventId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Navigation 
        public Event Event { get; set; } = null;
        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>(); 
    }
}
