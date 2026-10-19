using RaceDay_API.Data;
using RaceDay_API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace RaceDay_API.Extensions
{
    public static class SessionExtensions
    {
        public static int? GetUserId(this ISession session)
        => session.GetInt32("UserId");

        public static string? GetRole(this ISession session)
            => session.GetString("Role");

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
