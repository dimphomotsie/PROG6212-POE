using Microsoft.AspNetCore.Mvc;
using RaceDay_API.Data;
using RaceDay_API.DTO.Categories;
using RaceDay_API.Extensions;
using RaceDay_API.Filters;
using RaceDay_API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace RaceDay_API.Controllers
{
    // Manages categories belonging to events (e.g., "Under 20",
    // "Senior (20-39)", "10km"). All authenticated users can view
    // categories. Only the owning Organiser can add or delete them.
    
    [ApiController]
    [AuthorizeRole("Organiser", "Participant")]
    public class CategoriesController : Controller
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context) => _context = context;


        /// <summary>
        /// Returns all categories for a specific event.
        /// </summary>
        /// <param name="eventId">The event ID.</param>
        /// <response code="200">Categories returned.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="404">Event not found.</response>
        [HttpGet("api/events/{eventId:int}/categories")]
        public async Task<IActionResult> GetForEvent(int eventId)
        {
            if (!await _context.Events.AnyAsync(e => e.EventId == eventId))
                return NotFound(new { message = "Event not found" });

            var categories = await _context.Categories
                .Where(c => c.EventId == eventId)
                .Select(c => new { c.CategoryId, c.Name, c.Description })
                .ToListAsync();

            return Ok(categories);
        }


        /// <summary>
        /// Adds a category to a specific event. Only the owning Organiser
        /// may add categories.
        /// </summary>
        /// <param name="eventId">The event ID.</param>
        /// <param name="request">Category details: name and optional description.</param>
        /// <response code="201">Category created.</response>
        /// <response code="400">Validation failed.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not the owning Organiser.</response>
        /// <response code="404">Event not found.</response>
        [HttpPost("api/events/{eventId:int}/categories")]
        [AuthorizeRole("Organiser")]
        public async Task<IActionResult> Create(int eventId, [FromBody] CreateCategoryRequest request)
        {
            var organiser = await HttpContext.Session.GetLoggedInOrganiserAsync(_context);
            if (organiser == null) return Unauthorized();

            var ev = await _context.Events.FirstOrDefaultAsync(e => e.EventId == eventId);
            if (ev == null) return NotFound(new { message = "Event not found" });

            if (ev.OrganiserId != organiser.OrganiserId)
                return StatusCode(403, new { message = "You do not own this event" });

            var category = new Category
            {
                EventId = eventId,
                Name = request.Name,
                Description = request.Description
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return StatusCode(201, new { category.CategoryId, category.Name });
        }


        /// <summary>
        /// Deletes a category. Only the owning Organiser may delete it,
        /// and categories with active enrolments cannot be deleted.
        /// </summary>
        /// <param name="id">The category ID.</param>
        /// <response code="204">Category deleted.</response>
        /// <response code="400">Category has enrolments.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not the owning Organiser.</response>
        /// <response code="404">Category not found.</response>
        [HttpDelete("api/categories/{id:int}")]
        [AuthorizeRole("Organiser")]
        public async Task<IActionResult> Delete(int id)
        {
            var organiser = await HttpContext.Session.GetLoggedInOrganiserAsync(_context);
            if (organiser == null) return Unauthorized();

            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryId == id);
            if (category == null) return NotFound(new { message = "Category not found" });

            if (category.Event.OrganiserId != organiser.OrganiserId)
                return StatusCode(403, new { message = "You do not own this event" });

            // Prevent deleting a category that has enrolments
            if (await _context.Enrolments.AnyAsync(e => e.CategoryId == id))
                return BadRequest(new { message = "Cannot delete a category with enrolments" });

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}

/*
1. ASP.NET Core Web API Controllers with Nested Routes (ControllerBase, [ApiController], [HttpGet], [HttpPost], [HttpDelete]).
Author: Microsoft
Link: <https://learn.microsoft.com/en-us/aspnet/core/web-api/>
Used in: Defining /api/events/{eventId}/categories and /api/categories/{id} endpoints.
Date Accessed: 18 October 2026


2. EF Core Query Operators (AnyAsync, FirstOrDefaultAsync, Include, Where, Select).
Author: Microsoft
Link: <https://learn.microsoft.com/en-us/ef/core/querying/>
Used in: Validating event existence, loading the owning event for ownership checks, and preventing deletion of categories with enrolments.
Date Accessed: 18 October 2026

*/