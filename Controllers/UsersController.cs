using Microsoft.AspNetCore.Mvc;
using RaceDay_API.Data;
using RaceDay_API.Filters;
using Microsoft.EntityFrameworkCore;

namespace RaceDay_API.Controllers
{

    [ApiController]
    [Route("api/users")]
    [AuthorizeRole("Organiser", "Participant")]
    public class UsersController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public UsersController(RaceDayDbContext context)
        {
            _context = context;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            var user = await _context.Users
                .Include(u => u.Organiser)
                .Include(u => u.Participant)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) return NotFound();

            return Ok(new
            {
                userId = user.UserId,
                email = user.Email,
                fullName = user.FullName,
                role = user.Role,
                organisationName = user.Organiser?.OrganisationName,
                dateOfBirth = user.Participant?.DateOfBirth,
                contactNumber = user.Participant?.ContactNumber
            });
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            var user = await _context.Users
                .Include(u => u.Organiser)
                .Include(u => u.Participant)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) return NotFound();

            user.FullName = request.FullName;

            if (user.Role == "Organiser" && user.Organiser != null)
            {
                user.Organiser.OrganisationName = request.OrganisationName;
            }
            else if (user.Role == "Participant" && user.Participant != null)
            {
                user.Participant.DateOfBirth = request.DateOfBirth;
                user.Participant.ContactNumber = request.ContactNumber;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Profile updated" });
        }
    }

    public class UpdateProfileRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string? OrganisationName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? ContactNumber { get; set; }
    }
}
