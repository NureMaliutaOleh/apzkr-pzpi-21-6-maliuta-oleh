using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInlet.Server.Attributes;
using SmartInlet.Server.Models;
using SmartInlet.Server.Requests;
using SmartInlet.Server.Responses;
using SmartInlet.Server.Services.DB;
using SmartInlet.Server.Services.Email;
using System.Net;

namespace SmartInlet.Server.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/users")]
    public class AdminUsersController : BaseController
    {
        private readonly IEmailService _emailService;

        public AdminUsersController(DbApp db, IEmailService emailService) : base(db)
        {
            _emailService = emailService;
        }

        [Authorized, CanAdministrateUsers]
        [HttpPost("{username}/send-email")]
        public async Task<IActionResult> SendEmailToUser(
            [FromRoute] string username,
            [FromBody] EmailRequest request)
        {
            User? user = await DB.Users
                .SingleOrDefaultAsync(p => p.Username == WebUtility.UrlDecode(username));

            if (user == null)
            {
                return NotFound(new BaseResponse.ErrorResponse("User not found!"));
            }

            try
            {
                await _emailService.SendEmailUseTemplateAsync(
                    email: user.Email,
                    templateName: "simple_email.html",
                    parameters: new Dictionary<string, string>
                    {
                        { "title", request.Title },
                        { "username", WebUtility.UrlDecode(username) },
                        { "content", request.Content }
                    });
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse.ErrorResponse(ex.Message));
            }

            return Ok(new UserResponse(user));
        }

        [Authorized, CanAdministrateUsers]
        [HttpPut("{username}/rights")]
        public async Task<IActionResult> UpdateUserRights(
            [FromRoute] string username,
            [FromBody] UpdateUserRightsRequest request)
        {
            User? user = await DB.Users
                .SingleOrDefaultAsync(p => p.Username == WebUtility.UrlDecode(username));

            if (user == null)
            {
                return NotFound(new BaseResponse.ErrorResponse("User not found!"));
            }

            user.CanAdministrateDevices = request.CanAdministrateDevices;
            user.CanAdministrateUsers = request.CanAdministrateUsers;

            await DB.SaveChangesAsync();
            return Ok(new UserResponse(user));
        }

        [Authorized, CanAdministrateUsers]
        [HttpDelete("{username}")]
        public async Task<IActionResult> DeleteUser([FromRoute] string username)
        {
            User? user = await DB.Users
                .SingleOrDefaultAsync(p => p.Username == WebUtility.UrlDecode(username));

            if (user == null)
            {
                return NotFound(new BaseResponse.ErrorResponse("User not found!"));
            }

            ActivationCode[] acList = await DB.ActivationCodes
                .Where(p => p.UserId == user.Id)
                .ToArrayAsync();

            JoinOffer[] joinOfferList = await DB.JoinOffers
                .Where(p => p.UserId == user.Id)
                .ToArrayAsync();

            GroupMember[] memberList = await DB.GroupMembers
                .Where(p => p.UserId == user.Id)
                .ToArrayAsync();

            DB.GroupMembers.RemoveRange(memberList);
            DB.ActivationCodes.RemoveRange(acList);
            DB.JoinOffers.RemoveRange(joinOfferList);
            DB.Users.Remove(user);

            try
            {
                await _emailService.SendEmailUseTemplateAsync(
                    email: user.Email,
                    templateName: "simple_email.html",
                    parameters: new Dictionary<string, string>
                    {
                        { "title", "Your account is deleted!" },
                        { "username", WebUtility.UrlDecode(username) },
                        { "content", "Your account is deleted by the administration!" }
                    });
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse.ErrorResponse(ex.Message));
            }

            await DB.SaveChangesAsync();
            return Ok(new BaseResponse.SuccessResponse(null));
        }
    }
}
