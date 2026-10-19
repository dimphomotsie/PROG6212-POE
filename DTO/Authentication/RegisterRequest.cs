namespace RaceDay_API.DTO.Authentication
{
    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // "Organiser" or "Participant"
        public string? OrganisationName { get; set; } // For Organiser
        public DateTime? DateOfBirth { get; set; } // For Participant
        public string? ContactNumber { get; set; } // For Participant 
    }
}
