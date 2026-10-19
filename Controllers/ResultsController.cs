using Microsoft.AspNetCore.Mvc;
using RaceDay_API.Data;
using RaceDay_API.DTO.Results;
using RaceDay_API.Extensions;
using RaceDay_API.Filters;
using RaceDay_API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace RaceDay_API.Controllers
{
    [ApiController]
    [Route("api")]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context) => _context = context;

        
        [HttpPost("enrolments/{enrolmentId:int}/results")]
        [AuthorizeRole("Organiser")]
        public async Task<IActionResult> Capture(int enrolmentId, [FromBody] CreateResultRequest request)
        {
            var organiser = await HttpContext.Session.GetLoggedInOrganiserAsync(_context);
            if (organiser == null) return Unauthorized();

            var enrolment = await _context.Enrolments
                .Include(e => e.Event)
                .FirstOrDefaultAsync(e => e.EnrolmentId == enrolmentId);
            if (enrolment == null) return NotFound(new { message = "Enrolment not found" });

            if (enrolment.Event.OrganiserId != organiser.OrganiserId)
                return StatusCode(403, new { message = "You do not own this event" });

            if (await _context.Results.AnyAsync(r => r.EnrolmentId == enrolmentId))
                return BadRequest(new { message = "Result already captured for this enrolment" });

            var result = new Result
            {
                EnrolmentId = enrolmentId,
                FinishTime = request.FinishTime,
                FinishPosition = request.FinishPosition
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                result.ResultId,
                result.EnrolmentId,
                finishTime = result.FinishTime?.ToString(@"hh\:mm\:ss"),
                result.FinishPosition
            });
        }

        
        [HttpGet("participants/results")]
        [AuthorizeRole("Participant")]
        public async Task<IActionResult> GetMyResults()
        {
            var userId = HttpContext.Session.GetUserId();
            var participant = await _context.Participants
                .FirstOrDefaultAsync(p => p.UserId == userId);
            if (participant == null) return Unauthorized();

            var results = await _context.Enrolments
                .Where(e => e.ParticipantId == participant.ParticipantId && e.Result != null)
                .Include(e => e.Event)
                .Include(e => e.Category)
                .Include(e => e.Result)
                .Select(e => new
                {
                    eventName = e.Event.Name,
                    eventDate = e.Event.EventDate,
                    categoryName = e.Category.Name,
                    finishTime = e.Result!.FinishTime,
                    finishingPosition = e.Result.FinishPosition
                })
                .ToListAsync();

            return Ok(results);
        }
    }
}
