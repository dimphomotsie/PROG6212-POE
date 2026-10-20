using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using RaceDay_API.Data;
using RaceDay_API.Models.Entities;
using RaceDay_API.DTO.Authentication;

namespace RaceDay_API.Controllers
{
    // Handles user registration, login, and logout. On successful login,
    // stores UserId and Role in the session so subsequent requests are
    // authenticated and role-aware. Passwords are hashed with BCrypt.

    [ApiController]
    [Route("api/auth")]
    public class AuthenticationController : Controller
    {
        private readonly RaceDayDbContext _context;

        public AuthenticationController(RaceDayDbContext context)
        {
            _context = context;
        }


        /// <summary>
        /// Registers a new user as either an Organiser or a Participant.
        /// The password is hashed before being stored and a role-specific
        /// record (Organiser or Participant) is created.
        /// </summary>
        /// <param name="request">Registration details: email, password,
        /// full name, role, and optional organisation name or date of
        /// birth and contact number.</param>
        /// <response code="201">User registered successfully.</response>
        /// <response code="400">Email already registered, or invalid role.</response>
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


        /// <summary>
        /// Authenticates a user with email and password. On success,
        /// stores UserId, Role, and Email in the session.
        /// </summary>
        /// <param name="request">Login credentials.</param>
        /// <response code="200">Credentials valid; session created.</response>
        /// <response code="401">Invalid email or password.</response>

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


        /// <summary>
        /// Ends the current user's session. Clears all stored session data
        /// so subsequent requests are treated as unauthenticated.
        /// </summary>
        /// <response code="204">Session cleared.</response>

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return NoContent();
        }
    }
}

/*
1. ASP.NET Core Web API Controllers (ControllerBase, [ApiController], [Route]).
Author: Microsoft
Link: <https://learn.microsoft.com/en-us/aspnet/core/web-api/>
Used in: Defining the /api/auth register, login, and logout endpoints.
Date Accessed: 18 October 2026

2. BCrypt.Net-Next (BCrypt.HashPassword, BCrypt.Verify).
Author: BcryptNet
Link: <https://github.com/BcryptNet/bcrypt.net>
Used in: Hashing passwords before storage and verifying credentials during login.
Date Accessed: 18 October 2026

3. ControllerBase Helper Methods (BadRequest, Unauthorized, NoContent, StatusCode).
Author: Microsoft
Link: <https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.controllerbase>
Used in: Returning 201 Created, 400 Bad Request, 401 Unauthorized, and 204 No Content from auth actions.
Date Accessed: 18 October 2026

4. ISession.SetInt32 / SetString.
Author: Microsoft
Link: <https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.isession>
Used in: Storing UserId, Role, and Email in the session after successful login.
Date Accessed: 18 October 2026

5. LINQ Query Operators (AnyAsync, FirstOrDefaultAsync) — Microsoft.EntityFrameworkCore 10.0.13.
Author: Microsoft
Link: <https://learn.microsoft.com/en-us/dotnet/csharp/linq/>
Used in: Checking for existing emails and looking up users during login.
Date Accessed: 18 October 2026
*/