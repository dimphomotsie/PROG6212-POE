using Microsoft.AspNetCore.Mvc;
using RaceDay_API.Data;
using RaceDay_API.DTO.Events;
using RaceDay_API.Extensions;
using RaceDay_API.Filters;
using RaceDay_API.Models.Entities;
using Microsoft.EntityFrameworkCore;


namespace RaceDay_API.Controllers
{

    [ApiController]
    [Route("api/events")]
    [AuthorizeRole("Organiser", "Participant")]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context) => _context = context;

        // Returns all events 
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var events = await _context.Events
                .Include(e => e.Organiser).ThenInclude(o => o.User)
                .Include(e => e.Categories)
                .Include(e => e.Enrolments)
                .OrderBy(e => e.EventDate)
                .Select(e => new EventResponse
                {
                    EventId = e.EventId,
                    OrganiserId = e.OrganiserId,
                    OrganiserName = e.Organiser.User.FullName,
                    Name = e.Name,
                    Description = e.Description,
                    EventDate = e.EventDate,
                    Location = e.Location,
                    Distance = e.Distance,
                    EventType = e.EventType,
                    BannerImageUrl = e.BannerImageUrl,
                    CategoryCount = e.Categories.Count,
                    EnrolmentCount = e.Enrolments.Count
                })
                .ToListAsync();

            return Ok(events);
        }

        // Returns a single event with its categories.
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var ev = await _context.Events
                .Include(e => e.Organiser).ThenInclude(o => o.User)
                .Include(e => e.Categories)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (ev == null) return NotFound(new { message = "Event not found" });

            return Ok(new
            {
                ev.EventId,
                ev.Name,
                ev.Description,
                ev.EventDate,
                ev.Location,
                ev.Distance,
                ev.EventType,
                ev.BannerImageUrl,
                organiserName = ev.Organiser.User.FullName,
                categories = ev.Categories.Select(c => new { c.CategoryId, c.Name, c.Description })
            });
        }

        // Creates a new event. Organiser only.
        [HttpPost]
        [AuthorizeRole("Organiser")]
        public async Task<IActionResult> Create([FromBody] CreateEventRequest request)
        {
            var organiser = await HttpContext.Session.GetLoggedInOrganiserAsync(_context);
            if (organiser == null)
                return Unauthorized(new { message = "Organiser profile not found" });

            var ev = new Event
            {
                OrganiserId = organiser.OrganiserId,
                Name = request.Name,
                Description = request.Description,
                EventDate = request.EventDate,
                Location = request.Location,
                Distance = request.Distance,
                EventType = request.EventType,
                BannerImageUrl = request.BannerImageUrl
            };

            _context.Events.Add(ev);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = ev.EventId },
                new { ev.EventId, ev.Name, ev.EventDate });
        }

        // Updates an event. Only the owning Organiser.
        [HttpPut("{id:int}")]
        [AuthorizeRole("Organiser")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateEventRequest request)
        {
            var organiser = await HttpContext.Session.GetLoggedInOrganiserAsync(_context);
            if (organiser == null) return Unauthorized();

            var ev = await _context.Events.FirstOrDefaultAsync(e => e.EventId == id);
            if (ev == null) return NotFound(new { message = "Event not found" });

            if (ev.OrganiserId != organiser.OrganiserId)
                return StatusCode(403, new { message = "You do not own this event" });

            ev.Name = request.Name;
            ev.Description = request.Description;
            ev.EventDate = request.EventDate;
            ev.Location = request.Location;
            ev.Distance = request.Distance;
            ev.EventType = request.EventType;
            ev.BannerImageUrl = request.BannerImageUrl;

            await _context.SaveChangesAsync();
            return Ok(new { ev.EventId, ev.Name, ev.EventDate });
        }

        // Deletes an event. Only the owning Organiser.
        [HttpDelete("{id:int}")]
        [AuthorizeRole("Organiser")]
        public async Task<IActionResult> Delete(int id)
        {
            var organiser = await HttpContext.Session.GetLoggedInOrganiserAsync(_context);
            if (organiser == null) return Unauthorized();

            var ev = await _context.Events.FirstOrDefaultAsync(e => e.EventId == id);
            if (ev == null) return NotFound(new { message = "Event not found" });

            if (ev.OrganiserId != organiser.OrganiserId)
                return StatusCode(403, new { message = "You do not own this event" });

            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
