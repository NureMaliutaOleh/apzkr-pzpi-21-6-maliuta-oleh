using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SmartInlet.Server.Responses;

namespace SmartInlet.Server.Attributes
{
    public class AuthorizedAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.User?.Identity?.IsAuthenticated != true)
            {
                BaseResponse.ErrorResponse response = new("Unauthorized.");
                context.Result = new UnauthorizedObjectResult(response);
            }
        }
    }
}
