namespace RaceDay_API.Models.Entities
{
    public class Organiser
    {
        public int OrganiserId { get; set; }
        public int UserId { get; set; }
        public string? OrganisationName { get; set; }

        // Navigation
        public User User { get; set; } = null;
        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}
