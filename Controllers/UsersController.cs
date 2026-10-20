using Microsoft.AspNetCore.Mvc;
using RaceDay_API.Data;
using RaceDay_API.Filters;
using Microsoft.EntityFrameworkCore;

namespace RaceDay_API.Controllers
{
   
    // Handles viewing and updating the currently authenticated user's
    // profile. Accessible to both Organisers and Participants. Returns
    // role-specific fields (OrganisationName for Organisers;
    // DateOfBirth and ContactNumber for Participants).
    

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


        /// <summary>
        /// Returns the logged-in user's profile details.
        /// </summary>
        /// <response code="200">Profile returned.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="404">User not found.</response>
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


        /// <summary>
        /// Returns the logged-in user's profile details.
        /// </summary>
        /// <response code="200">Profile returned.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="404">User not found.</response>
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

/*
1. ASP.NET Core Web API Controllers (ControllerBase, [ApiController], [Route]).
Author: Microsoft
Link: <https://learn.microsoft.com/en-us/aspnet/core/web-api/>
Used in: Defining the /api/users/profile GET and PUT endpoints.
Date Accessed: 18 October 2026

2. AuthorizeRole Filter (Custom).
Author: Self (based on Microsoft ActionFilterAttribute documentation)
Link: <https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/filters>
Used in: Restricting profile endpoints to logged-in Organisers and Participants.
Date Accessed: 18 October 2026

3. EF Core Include (Eager Loading) — Microsoft.EntityFrameworkCore 10.0.13.
Author: Microsoft
Link: <https://learn.microsoft.com/en-us/ef/core/querying/related-data/eager>
Used in: Loading the Organiser or Participant navigation property alongside the User record.
Date Accessed: 18 October 2026

4. LINQ Query Operators (FirstOrDefaultAsync, Include).
Author: Microsoft
Link: <https://learn.microsoft.com/en-us/dotnet/csharp/linq/>
Used in: Fetching the profile record and applying updates.
Date Accessed: 18 October 2026
*/
