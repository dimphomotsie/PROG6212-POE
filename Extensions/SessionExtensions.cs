namespace RaceDay_API.Extensions
{
    public static class SessionExtensions
    {
        public static int? GetUserId(this ISession session)
        => session.GetInt32("UserId");

        public static string? GetRole(this ISession session)
            => session.GetString("Role");
    }
}
