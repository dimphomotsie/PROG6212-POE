using Microsoft.AspNetCore.Mvc;
using RaceDay_API.Data;
using RaceDay_API.DTO.Events;
using RaceDay_API.Extensions;
using RaceDay_API.Filters;
using RaceDay_API.Models.Entities;
using Microsoft.EntityFrameworkCore;


namespace RaceDay_API.Controllers
{

    
    // Manages events. All authenticated users can view events.
    // Only Organisers can create, update, or delete events, and only
    // the organiser who created the event can modify it.
    
    [ApiController]
    [Route("api/events")]
    [AuthorizeRole("Organiser", "Participant")]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context) => _context = context;


        /// <summary>
        /// Returns all events, ordered by event date, with organiser name,
        /// category count, and enrolment count.
        /// </summary>
        /// <response code="200">List of events returned.</response>
        /// <response code="401">Not authenticated.</response>
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


        /// <summary>
        /// Returns a single event by ID, including its categories.
        /// </summary>
        /// <param name="id">The event ID.</param>
        /// <response code="200">Event returned.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="404">Event not found.</response>
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


        /// <summary>
        /// Creates a new event owned by the logged-in Organiser.
        /// </summary>
        /// <param name="request">Event details: name, description, date,
        /// location, distance, and type (Run, Walk, or Cycle).</param>
        /// <response code="201">Event created.</response>
        /// <response code="400">Validation failed.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not an Organiser.</response>
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


        /// <summary>
        /// Updates an existing event. Only the owning Organiser may do this.
        /// </summary>
        /// <param name="id">The event ID.</param>
        /// <param name="request">Updated event details.</param>
        /// <response code="200">Event updated.</response>
        /// <response code="400">Validation failed.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not the owning Organiser.</response>
        /// <response code="404">Event not found.</response>
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


        /// <summary>
        /// Deletes an event. Only the owning Organiser may do this.
        /// Cascade delete also removes its categories and enrolments.
        /// </summary>
        /// <param name="id">The event ID.</param>
        /// <response code="204">Event deleted.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not the owning Organiser.</response>
        /// <response code="404">Event not found.</response>
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

/*
1. ASP.NET Core Web API Controllers (ControllerBase, [ApiController], [Route]).
Author: Microsoft
Link: <https://learn.microsoft.com/en-us/aspnet/core/web-api/>
Used in: Defining the /api/events CRUD endpoints.
Date Accessed: 18 October 2026


2. CreatedAtAction Helper.
Author: Microsoft
Link: <https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.controllerbase.createdataction>
Used in: Returning 201 Created with a Location header pointing to GET /api/events/{id}.
Date Accessed: 18 October 2026

3. EF Core Query Operators (Include, ThenInclude, OrderBy, Select, ToListAsync).
Author: Microsoft
Link: <https://learn.microsoft.com/en-us/ef/core/querying/>
Used in: Loading Organiser, Categories, and Enrolments together, sorting by EventDate, and projecting into EventResponse DTOs.
Date Accessed: 18 October 2026


4. [ProducesResponseType] Attribute.
Author: Microsoft
Link: <https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/include-metadata>
Used in: Documenting 200, 201, 400, 401, 403, 404 responses in Swagger.
Date Accessed: 18 October 2026
*/
