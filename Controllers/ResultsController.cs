using Microsoft.AspNetCore.Mvc;
using RaceDay_API.Data;
using RaceDay_API.DTO.Results;
using RaceDay_API.Extensions;
using RaceDay_API.Filters;
using RaceDay_API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace RaceDay_API.Controllers
{
    
    // Manages race results. Organisers capture finish times and
    // positions for enrolments in their events. Participants view
    // their own race history across all completed events.
    
    [ApiController]
    [Route("api")]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context) => _context = context;



        /// <summary>
        /// Captures a finish time and finishing position for a specific
        /// enrolment. Only the owning Organiser may do this, and only
        /// once per enrolment.
        /// </summary>
        /// <param name="enrolmentId">The enrolment ID.</param>
        /// <param name="request">Finish time (HH:mm:ss) and finishing position.</param>
        /// <response code="201">Result recorded.</response>
        /// <response code="400">Result already exists, or validation failed.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not the owning Organiser.</response>
        /// <response code="404">Enrolment not found.</response>
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


        /// <summary>
        /// Returns the logged-in Participant's personal race history,
        /// including event name, date, category, finish time, and
        /// finishing position.
        /// </summary>
        /// <response code="200">List of personal results.</response>
        /// <response code="401">Not authenticated.</response>
        /// <response code="403">Not a Participant.</response>
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

/*
1. ASP.NET Core Web API Controllers (ControllerBase, [ApiController], [Route("api")]).
Author: Microsoft
Link: <https://learn.microsoft.com/en-us/aspnet/core/web-api/>
Used in: Defining /api/enrolments/{id}/results and /api/participants/results endpoints.
Date Accessed: 18 October 2026



3. EF Core Include and Where Filters.
Author: Microsoft
Link: <https://learn.microsoft.com/en-us/ef/core/querying/>
Used in: Returning only results for the logged-in participant and only for their own events' enrolments.
Date Accessed: 18 October 2026

4. TimeSpan JSON Binding (.NET System.Text.Json).
Author: Microsoft
Link: <https://learn.microsoft.com/en-us/dotnet/api/system.timespan>
Used in: Accepting finish times in HH:mm:ss format and storing them on the Result entity.
Date Accessed: 18 October 2026
*/