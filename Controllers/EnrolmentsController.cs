using Microsoft.AspNetCore.Mvc;
using RaceDay_API.Data;
using RaceDay_API.DTO.Enrolments;
using RaceDay_API.Extensions;
using RaceDay_API.Filters;
using RaceDay_API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace RaceDay_API.Controllers
{
    [ApiController]
    [Route("api")]
    [AuthorizeRole("Organiser", "Participant")]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context) => _context = context;

        
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