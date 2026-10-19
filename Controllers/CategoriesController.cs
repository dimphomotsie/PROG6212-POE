using Microsoft.AspNetCore.Mvc;
using RaceDay_API.Data;
using RaceDay_API.DTO.Categories;
using RaceDay_API.Extensions;
using RaceDay_API.Filters;
using RaceDay_API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace RaceDay_API.Controllers
{

    [ApiController]
    [AuthorizeRole("Organiser", "Participant")]
    public class CategoriesController : Controller
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context) => _context = context;

        // Lists all categories for an event.
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

        // Adds a category to an event. Owning Organiser only.
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

        // Deletes a category. Owning Organiser only. 
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