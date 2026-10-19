using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay_API.Filters
{
    public class AuthorizeRoleAttribute : ActionFilterAttribute
    {
        private readonly string[] _roles;

        public AuthorizeRoleAttribute(params string[] roles)
        {
            _roles = roles;
        }

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
