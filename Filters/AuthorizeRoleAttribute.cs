using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay_API.Filters
{
    // Custom action filter that enforces role-based access control.
    // Apply to a controller or action method with one or more role names.
    // If the caller is not authenticated, returns 401 Unauthorized.
    // If the caller is authenticated but not in one of the allowed roles,
    // returns 403 Forbidden.
    
    public class AuthorizeRoleAttribute : ActionFilterAttribute
    {
        private readonly string[] _roles;

        // Creates the filter with the set of roles permitted to call the decorated action.
        public AuthorizeRoleAttribute(params string[] roles)
        {
            _roles = roles;
        }

        // Executes before the action method. Reads the caller's role
        // from the session and rejects the request if it does not match.
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = context.HttpContext.Session.GetInt32("UserId");
            var role = context.HttpContext.Session.GetString("Role");

            if (userId == null)
            {
                context.Result = new UnauthorizedObjectResult(new { message = "Not authenticated" });
                return;
            }

            if (!_roles.Contains(role))
            {
                context.Result = new ObjectResult(new { message = "Forbidden" })
                {
                    StatusCode = 403
                };
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}

/*
1. ActionExecutingContext / UnauthorizedObjectResult / ObjectResult.
Author: Microsoft
Link: <https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.filters.actionexecutingcontext>
Used in: Short-circuiting the request with 401 Unauthorized or 403 Forbidden when the session role does not match the required role.
Date Accessed: 18 October 2026

2. ActionFilterAttribute (Microsoft.AspNetCore.Mvc.Filters).
Author: Microsoft
Link: <https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/filters>
Used in: Implementing a custom action filter to enforce role-based access control (Organiser vs. Participant) at the API level.
Date Accessed: 18 October 2026

3. ISession.GetInt32 / ISession.GetString.
Author: Microsoft
Link: <https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.isession>
Used in: Reading UserId and Role from the server-side session to determine the caller's identity.
Date Accessed: 18 October 2026
*/
