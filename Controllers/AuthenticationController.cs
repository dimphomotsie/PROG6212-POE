using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using RaceDay_API.Data;
using RaceDay_API.Models.Entities;
using RaceDay_API.DTO.Authentication;

namespace RaceDay_API.Controllers
{

    [ApiController]
    [Route("api/auth")]
    public class AuthenticationController : Controller
    {
        private readonly RaceDayDbContext _context;

        public AuthenticationController(RaceDayDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RaceDay_API.DTO.Authentication.RegisterRequest request)
        {
            // Validates role
            if (request.Role != "Organiser" && request.Role != "Participant")
                return BadRequest(new { message = "Role must be 'Organiser' or 'Participant'" });

            // Checks if email exists
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest(new { message = "Email already registered" });

            // Creates a user with hashed password
            var user = new User
            {
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FullName = request.FullName,
                Role = request.Role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Creates a role-specific record
            if (request.Role == "Organiser")
            {
                _context.Organisers.Add(new Organiser
                {
                    UserId = user.UserId,
                    OrganisationName = request.OrganisationName
                });
            }
            else
            {
                _context.Participants.Add(new Participant
                {
                    UserId = user.UserId,
                    DateOfBirth = request.DateOfBirth,
                    ContactNumber = request.ContactNumber
                });
            }

            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                userId = user.UserId,
                email = user.Email,
                fullName = user.FullName,
                role = user.Role
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] RaceDay_API.DTO.Authentication.LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized(new { message = "Invalid email or password" });

            // Stores in session
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("Role", user.Role);
            HttpContext.Session.SetString("Email", user.Email);

            return Ok(new
            {
                userId = user.UserId,
                email = user.Email,
                fullName = user.FullName,
                role = user.Role
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return NoContent();
        }
    }
}
