using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using SmartInlet.Server.Responses;
using System.Security.Claims;

namespace SmartInlet.Server.Attributes
{
    public class CanAdministrateUsersAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.User?.FindFirstValue("can_administrate_users") != true.ToString())
            {
                BaseResponse.ErrorResponse response = new("The user is not able to administrate users");
                context.Result = new BadRequestObjectResult(response);
            }
        }
    }
}
