using Microsoft.AspNetCore.Mvc;
using SmartInlet.Server.Models;
using SmartInlet.Server.Services.DB;
using System.Security.Claims;

namespace SmartInlet.Server.Controllers
{
    public class BaseController : ControllerBase
    {
        protected DbApp DB { get; }
        protected int AuthorizedUserId
        {
            get
            {
                var strId = HttpContext.User.Claims
                    .FirstOrDefault(p => p.Type == ClaimTypes.NameIdentifier)?.Value;

                if (!int.TryParse(strId, out int id))
                {
                    throw new InvalidOperationException("This property accessible only for authorized users.");
                }

                return id;
            }
        }
        protected User AuthorizedUser
        {
            get
            {
                User? user = DB.Users.SingleOrDefault(p => p.Id == AuthorizedUserId);

                return user is null
                    ? throw new InvalidOperationException("This property accessible only for authorized users.")
                    : user;
            }
        }

        public BaseController(DbApp db)
        {
            DB = db;
        }
    }
}
