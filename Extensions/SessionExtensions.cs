using RaceDay_API.Data;
using RaceDay_API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace RaceDay_API.Extensions
{
    // Extension methods that simplify reading user information from
    // the ASP.NET Core session and looking up the logged-in Organiser.
    // Reduces repeated session access code across controllers.
    public static class SessionExtensions
    {
        // Returns the UserId stored in the session, or null if not logged in.
        public static int? GetUserId(this ISession session)
        => session.GetInt32("UserId");

        // Returns the Role ("Organiser" or "Participant") stored in the
        // session, or null if not logged in.
        public static string? GetRole(this ISession session)
            => session.GetString("Role");

        // Looks up the Organiser record for the currently logged-in user.
        // Returns null if the user is not logged in or is not an Organiser.
        public static async Task<Organiser?> GetLoggedInOrganiserAsync(
       this ISession session, RaceDayDbContext context)
        {
            var userId = session.GetUserId();
            if (userId == null) return null;

            return await context.Organisers
                .FirstOrDefaultAsync(o => o.UserId == userId);
        }
    }
}

/*
1. Entity Framework Core FirstOrDefaultAsync (Microsoft.EntityFrameworkCore 10.0.13).
Author: Microsoft
Link: <https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.entityframeworkqueryableextensions.firstordefaultasync>
Used in: Looking up the Organiser record for the logged-in user in GetLoggedInOrganiserAsync.
Date Accessed: 18 October 2026

2. ISession Extension Methods.
Author: Microsoft
Link: <https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state>
Used in: Adding GetUserId, GetRole, and GetLoggedInOrganiserAsync helper methods to simplify session access in controllers.
Date Accessed: 18 October 2026
*/