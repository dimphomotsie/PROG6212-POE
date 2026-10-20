using Microsoft.AspNetCore.Mvc;
using RaceDay_API.Data;
using RaceDay_API.DTO.Enrolments;
using RaceDay_API.Extensions;
using RaceDay_API.Filters;
using RaceDay_API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace RaceDay_API.Controllers
{
    
    // Manages event enrolments. Participants can enrol in events by
    // selecting a category, and can view their own enrolments.
    // Organisers can view all enrolments for their own events and
    // update their status (Pending, Confirmed, Cancelled).
    
    [ApiController]
    [Route("api")]
    [AuthorizeRole("Organiser", "Participant")]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context) => _context = context;


        /// <summary>
        /// Enrols the logged-in Participant in an event by selecting a
        /// category. The category must belong to the specified event,
        /// and a participant cannot enrol twice in the same event.
        /// </summary>
        /// <param name="eventId">The event ID.</param>
        /// <param name="request">The selected category.</param>
        /// <response code="201">Enrolment created.</response>
        /// <response code="400">Already enrolled, or category mismatch.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not a Participant.</response>
        /// <response code="404">Event or category not found.</response>
        [HttpPost("events/{eventId:int}/enrolments")]
        [AuthorizeRole("Participant")]
        public async Task<IActionResult> Enrol(int eventId, [FromBody] CreateEnrolmentRequest request)
        {
            var userId = HttpContext.Session.GetUserId();
            var participant = await _context.Participants
                .FirstOrDefaultAsync(p => p.UserId == userId);
            if (participant == null) return Unauthorized();

            
            if (!await _context.Events.AnyAsync(e => e.EventId == eventId))
                return NotFound(new { message = "Event not found" });

            
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == request.CategoryId);
            if (category == null)
                return NotFound(new { message = "Category not found" });
            if (category.EventId != eventId)
                return BadRequest(new { message = "Category does not belong to this event" });

            
            var already = await _context.Enrolments
                .AnyAsync(e => e.ParticipantId == participant.ParticipantId
                            && e.EventId == eventId);
            if (already)
                return BadRequest(new { message = "You are already enrolled in this event" });

            var enrolment = new Enrolment
            {
                ParticipantId = participant.ParticipantId,
                EventId = eventId,
                CategoryId = request.CategoryId,
                Status = "Pending"
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                enrolment.EnrolmentId,
                enrolment.EventId,
                enrolment.CategoryId,
                enrolment.Status,
                enrolment.EnrolmentDate
            });
        }




        /// <summary>
        /// Returns enrolments for the logged-in user. Participants see
        /// their own enrolments; Organisers see all enrolments for the
        /// events they own.
        /// </summary>
        /// <response code="200">List of enrolments returned.</response>
        /// <response code="401">Not authenticated.</response>
        [HttpGet("enrolments")]
        public async Task<IActionResult> GetMine()
        {
            var userId = HttpContext.Session.GetUserId();
            var role = HttpContext.Session.GetRole();

            if (role == "Participant")
            {
                var participant = await _context.Participants
                    .FirstOrDefaultAsync(p => p.UserId == userId);
                if (participant == null) return Unauthorized();

                var list = await _context.Enrolments
                    .Where(e => e.ParticipantId == participant.ParticipantId)
                    .Include(e => e.Event)
                    .Include(e => e.Category)
                    .Select(e => new
                    {
                        e.EnrolmentId,
                        eventName = e.Event.Name,
                        eventDate = e.Event.EventDate,
                        categoryName = e.Category.Name,
                        e.Status,
                        e.EnrolmentDate
                    })
                    .ToListAsync();

                return Ok(list);
            }
            else 
            {
                var organiser = await HttpContext.Session.GetLoggedInOrganiserAsync(_context);
                if (organiser == null) return Unauthorized();

                var list = await _context.Enrolments
                    .Where(e => e.Event.OrganiserId == organiser.OrganiserId)
                    .Include(e => e.Participant).ThenInclude(p => p.User)
                    .Include(e => e.Event)
                    .Include(e => e.Category)
                    .Select(e => new
                    {
                        e.EnrolmentId,
                        participantName = e.Participant.User.FullName,
                        eventName = e.Event.Name,
                        categoryName = e.Category.Name,
                        e.Status,
                        e.EnrolmentDate
                    })
                    .ToListAsync();

                return Ok(list);
            }
        }



        /// <summary>
        /// Updates the status of an enrolment. Only the owning Organiser
        /// may change it. Valid statuses: Pending, Confirmed, Cancelled.
        /// </summary>
        /// <param name="id">The enrolment ID.</param>
        /// <param name="request">The new status.</param>
        /// <response code="200">Status updated.</response>
        /// <response code="400">Invalid status.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not the owning Organiser.</response>
        /// <response code="404">Enrolment not found.</response>
        [HttpPut("enrolments/{id:int}/status")]
        [AuthorizeRole("Organiser")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateEnrolmentStatusRequest request)
        {
            var organiser = await HttpContext.Session.GetLoggedInOrganiserAsync(_context);
            if (organiser == null) return Unauthorized();

            var enrolment = await _context.Enrolments
                .Include(e => e.Event)
                .FirstOrDefaultAsync(e => e.EnrolmentId == id);
            if (enrolment == null) return NotFound(new { message = "Enrolment not found" });

            if (enrolment.Event.OrganiserId != organiser.OrganiserId)
                return StatusCode(403, new { message = "You do not own this event" });

            enrolment.Status = request.Status;
            await _context.SaveChangesAsync();
            return Ok(new { enrolment.EnrolmentId, enrolment.Status });
        }
    }
}

/*
1. ASP.NET Core Web API Controllers (ControllerBase, [ApiController], [Route("api")]).
Author: Microsoft
Link: <https://learn.microsoft.com/en-us/aspnet/core/web-api/>
Used in: Defining /api/events/{eventId}/enrolments and /api/enrolments endpoints.
Date Accessed: 18 October 2026

2. EF Core Include with ThenInclude (Participant -> User).
Author: Microsoft
Link: <https://learn.microsoft.com/en-us/ef/core/querying/related-data/eager>
Used in: Loading the participant's User record so their FullName can appear in the Organiser's enrolment list.
Date Accessed: 18 October 2026

3. LINQ Query Operators (AnyAsync, FirstOrDefaultAsync, Where, Select, ToListAsync).
Author: Microsoft
Link: <https://learn.microsoft.com/en-us/dotnet/csharp/linq/>
Used in: Preventing duplicate enrolments, validating category-event matching, and shaping role-specific response DTOs.
Date Accessed: 18 October 2026

5. Session Role Branching.
Author: Microsoft
Link: <https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state>
Used in: Returning different data to Participants (own enrolments) vs. Organisers (all enrolments for their events).
Date Accessed: 18 October 2026
*/