namespace RaceDay_API.DTO.Events
{
    public class EventResponse
    {
        public int EventId { get; set; }
        public int OrganiserId { get; set; }
        public string OrganiserName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Distance { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string? BannerImageUrl { get; set; }
        public int CategoryCount { get; set; }
        public int EnrolmentCount { get; set; }
    }
}
