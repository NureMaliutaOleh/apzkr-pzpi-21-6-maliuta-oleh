using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartInlet.Server.Models;
using SmartInlet.Server.Requests;
using SmartInlet.Server.Responses;
using SmartInlet.Server.Services.Email;
using SmartInlet.Server.Tools;
using System.Net;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using SmartInlet.Server.Attributes;
using SmartInlet.Server.Services.DB;

namespace SmartInlet.Server.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : BaseController
    {
        private readonly IEmailService _emailService;

        public UserController(DbApp db, IEmailService emailService) : base(db)
        {
            _emailService = emailService;
        }

        [HttpPost("sign-up")]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
        {
            if (request.Password.Length < 3 || request.Password.Length > 50)
            {
                return BadRequest(new BaseResponse.ErrorResponse("Allowed username range: [3, 50]."));
            }

            if (request.Password.Length < 6 || request.Password.Length > 100)
            {
                return BadRequest(new BaseResponse.ErrorResponse("Allowed password range: [6, 100]."));
            }

            if (await DB.Users.AnyAsync(p => p.Username == request.Username))
            {
                return BadRequest(new BaseResponse.ErrorResponse("There is a user with the same login!"));
            }

            if (await DB.Users.AnyAsync(p => p.Email == request.Email))
            {
                return BadRequest(new BaseResponse.ErrorResponse("There is a user with the same email!"));
            }

            if (request.Password != request.ConfirmPassword)
            {
                return BadRequest(new BaseResponse.ErrorResponse("The password is not confirmed!"));
            }

            string token = StringTool.RandomString(256);

            try
            {
                await _emailService.SendEmailUseTemplateAsync(
                    email: request.Email,
                    templateName: "registration_confirm.html",
                    parameters: new Dictionary<string, string>
                    {
                        { "login", request.Username },
                        { "link", $"https://{HttpContext.Request.Host}/api/users/confirm-email/{WebUtility.UrlEncode(request.Email)}?token={token}" }
                    });
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse.ErrorResponse(ex.Message));
            }

            User user = (await DB.Users.AddAsync(new User
            {
                Username = request.Username,
                FirstName = request.FirstName.IsNullOrEmpty() ? null : request.FirstName,
                LastName = request.LastName.IsNullOrEmpty() ? null : request.LastName,
                PasswordHash = PasswordTool.Hash(request.Password),
                Email = request.Email,
                IsActivated = false,
                RegisteredAt = DateTime.UtcNow
            })).Entity;

            await DB.ActivationCodes.AddAsync(new ActivationCode
            {
                UserId = user.Id,
                User = user,
                Code = token,
                Action = "confirm-registration",
                ExpiresAt = DateTime.UtcNow.AddHours(12)
            });

            await DB.SaveChangesAsync();
            return Ok(new BaseResponse.SuccessResponse(null));
        }

        [HttpPost("sign-in")]
        public async Task<IActionResult> SignIn([FromBody] SignInRequest request)
        {
            User? user = await DB.Users
                .SingleOrDefaultAsync(p => p.Username == request.Username);

            if (user == null)
            {
                return NotFound(new BaseResponse.ErrorResponse("Invalid login!"));
            }

            if (!PasswordTool.Validate(request.Password, user.PasswordHash))
            {
                return BadRequest(new BaseResponse.ErrorResponse("Invalid password!"));
            }

            if (!user.IsActivated)
            {
                return Unauthorized(new BaseResponse.ErrorResponse("The user is not activated!"));
            }

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(new ClaimsIdentity(
            [
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Username),
                new("can_administrate_devices", user.CanAdministrateDevices.ToString()),
                new("can_administrate_users", user.CanAdministrateUsers.ToString())
            ], CookieAuthenticationDefaults.AuthenticationScheme)));

            return Ok(new UserResponse(user));
        }

        [HttpGet("sign-out")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Ok(new BaseResponse.SuccessResponse(null));
        }

        [HttpGet("check")]
        public IActionResult Check()
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                return Unauthorized(new BaseResponse.ErrorResponse(null));
            }
            return Ok(new BaseResponse.SuccessResponse(null));
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] PageRequest pageRequest, SearchUsersRequest request)
        {
            request.Query = (request.Query ?? "").ToLower();

            IQueryable<User> query = DB.Users
                .Where(p => p.IsActivated && p.Username.ToLower().Contains(request.Query))
                .OrderBy(p => p.Username);

            int totalItemsCount = await query.CountAsync();
            int totalPagesCount = (int)Math.Ceiling((double)totalItemsCount / pageRequest.PageSize);
            query = query.Skip((pageRequest.Page - 1) * pageRequest.PageSize).Take(pageRequest.PageSize);

            List<UserResponse.ShortView> result = await query
                .Select(p => new UserResponse.ShortView(p))
                .ToListAsync();

            PageResponse<UserResponse.ShortView> response = new(
                result,
                pageRequest.Page,
                pageRequest.PageSize,
                totalPagesCount);

            return Ok(response);
        }

        [Authorized]
        [HttpGet("info")]
        public IActionResult GetSelfInfo()
        {
            return Ok(new UserResponse(AuthorizedUser));
        }

        [HttpGet("{username}/info")]
        public async Task<IActionResult> GetUserInfo([FromRoute] string username)
        {
            User? user = await DB.Users
                .Include(p => p.GroupMembers)
                .SingleOrDefaultAsync(p => p.Username == WebUtility.UrlDecode(username));

            if (user == null)
            {
                return NotFound(new BaseResponse.ErrorResponse("User not found!"));
            }

            return Ok(new BaseResponse.SuccessResponse(
                    new UserResponse.ShortView(user)));
        }

        [Authorized]
        [HttpPut("info")]
        public async Task<IActionResult> UpdateSelfInfo([FromBody] UpdateUserInfoRequest request)
        {
            User user = AuthorizedUser;

            bool different = false;

            if (user.Username != (request.Username ?? user.Username))
            {
                if (await DB.Users.AnyAsync(p => p.Username == request.Username))
                {
                    return BadRequest(new BaseResponse.ErrorResponse("There is a user with the same login!"));
                }

                user.Username = request.Username!;
                different = true;
            }

            if (user.FirstName != (request.FirstName ?? user.FirstName))
            {
                user.FirstName = request.FirstName == "" ? null : request.FirstName;
                different = true;
            }

            if (user.LastName != (request.LastName ?? user.LastName))
            {
                user.LastName = request.LastName == "" ? null : request.LastName;
                different = true;
            }

            if (!different)
            {
                return Ok(new BaseResponse.SuccessResponse("Data are identical"));
            }

            await DB.SaveChangesAsync();
            return Ok(new UserResponse(user));
        }

        [Authorized]
        [HttpPut("password")]
        public async Task<IActionResult> UpdateUserPassword([FromBody] UpdateUserPasswordRequest request)
        {
            User? user = AuthorizedUser;

            if (!PasswordTool.Validate(request.OldPassword, user.PasswordHash))
            {
                return BadRequest(new BaseResponse.ErrorResponse("Invalid old password!"));
            }

            if (request.NewPassword.Length < 6)
            {
                return BadRequest(new BaseResponse.ErrorResponse("New password is too short!"));
            }

            if (request.NewPassword != request.ConfirmPassword)
            {
                return BadRequest(new BaseResponse.ErrorResponse("The password is not confirmed!"));
            }

            user.PasswordHash = PasswordTool.Hash(request.NewPassword);

            await DB.SaveChangesAsync();
            return Ok(new UserResponse(user));
        }

        [Authorized]
        [HttpPut("email")]
        public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailRequest request)
        {
            User user = AuthorizedUser;

            if (user.Email == request.Email)
            {
                return BadRequest(new BaseResponse.ErrorResponse("This is your current email!"));
            }

            if (await DB.Users.AnyAsync(p => p.Email == request.Email))
            {
                return BadRequest(new BaseResponse.ErrorResponse("There is a user with the same email!"));
            }

            if (!PasswordTool.Validate(request.Password, user.PasswordHash))
            {
                return BadRequest(new BaseResponse.ErrorResponse("Invalid password!"));
            }

            string token = StringTool.RandomString(256);

            try
            {
                await _emailService.SendEmailUseTemplateAsync(
                    email: request.Email,
                    templateName: "change_email_confirm.html",
                    parameters: new Dictionary<string, string>
                    {
                        { "login", user.Username },
                        { "link", $"https://{HttpContext.Request.Host}/api/users/confirm-email/{WebUtility.UrlEncode(user.Email)}?token={token}" }
                    });
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse.ErrorResponse(ex.Message));
            }

            DateTime time = DateTime.UtcNow;

            await DB.ActivationCodes.AddAsync(new ActivationCode
            {
                UserId = user.Id,
                User = user,
                Code = token,
                Action = $"change-email,{request.Email}",
                ExpiresAt = time.AddHours(12)
            });

            await DB.SaveChangesAsync();
            return Ok(new BaseResponse.SuccessResponse("Email is sent!"));
        }

        [Authorized]
        [HttpDelete]
        public async Task<IActionResult> DeleteUserAccount()
        {
            User user = AuthorizedUser;

            string token = StringTool.RandomString(256);

            try
            {
                await _emailService.SendEmailUseTemplateAsync(
                    email: user.Email,
                    templateName: "delete_account_confirm.html",
                    parameters: new Dictionary<string, string>
                    {
                        { "login", user.Username },
                        { "link", $"https://{HttpContext.Request.Host}/api/users/confirm-email/{WebUtility.UrlEncode(user.Email)}?token={token}" }
                    });
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse.ErrorResponse(ex.Message));
            }

            DateTime time = DateTime.UtcNow;

            await DB.ActivationCodes.AddAsync(new ActivationCode
            {
                UserId = user.Id,
                User = user,
                Code = token,
                Action = "delete-user",
                ExpiresAt = time.AddHours(12)
            });

            await DB.SaveChangesAsync();
            return Ok(new BaseResponse.SuccessResponse("Email is sent!"));
        }

        [HttpPost("reset-password-permission")]
        public async Task<IActionResult> SendResetPasswordPermission([FromBody][Required] string username)
        {
            User? user = await DB.Users.SingleOrDefaultAsync(x => x.Username == username);

            if (user == null)
            {
                return NotFound(new BaseResponse.ErrorResponse("User not found!"));
            }

            string token = StringTool.RandomString(256);

            try
            {
                await _emailService.SendEmailUseTemplateAsync(
                    email: user.Email,
                    templateName: "reset_password_permission.html",
                    parameters: new Dictionary<string, string>
                    {
                        { "login", user.Username },
                        { "link", $"https://localhost:5173/reset-password?username={WebUtility.UrlEncode(user.Username)}&token={token}" }
                    });
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse.ErrorResponse(ex.Message));
            }

            await DB.ActivationCodes.AddAsync(new ActivationCode
            {
                UserId = user.Id,
                Code = token,
                Action = "reset-password-permission",
                ExpiresAt = DateTime.UtcNow.AddHours(12)
            });

            await DB.SaveChangesAsync();
            return Ok(new BaseResponse.SuccessResponse(null));
        }

        [HttpPut("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (request.NewPassword.Length < 6)
            {
                return BadRequest(new BaseResponse.ErrorResponse("Too short password!"));
            }

            if (request.NewPassword != request.ConfirmNewPassword)
            {
                return BadRequest(new BaseResponse.ErrorResponse("The password is not confirmed!"));
            }

            User? user = await DB.Users.SingleOrDefaultAsync(x => x.Username == request.Username);

            if (user == null)
            {
                return NotFound(new BaseResponse.ErrorResponse("User not found!"));
            }

            ActivationCode? activationCode = await DB.ActivationCodes
                    .SingleOrDefaultAsync(x => user.Id == x.UserId && x.Code == request.Token);

            if (activationCode == null)
            {
                return NotFound(new BaseResponse.ErrorResponse("The link is expired or unavailable!"));
            }

            if (DateTime.UtcNow > activationCode.ExpiresAt)
            {
                return BadRequest(new BaseResponse.ErrorResponse("The link is expired!"));
            }

            user.PasswordHash = PasswordTool.Hash(request.NewPassword);
            DB.ActivationCodes.Remove(activationCode);

            await DB.SaveChangesAsync();
            return Ok(new BaseResponse.SuccessResponse("The password is changed!"));
        }

        [HttpGet("confirm-email/{email}")]
        public async Task<IActionResult> ConfirmEmail([FromRoute] string email, [FromQuery] string? token)
        {
            ActivationCode? activationCode = await DB.ActivationCodes
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.User.Email == WebUtility.UrlDecode(email) && p.Code == token);

            if (activationCode == null)
            {
                return NotFound(new BaseResponse.ErrorResponse("The link is expired or unavailable!"));
            }

            if (DateTime.UtcNow > activationCode.ExpiresAt)
            {
                return BadRequest(new BaseResponse.ErrorResponse("The link is expired!"));
            }

            User user = activationCode.User;
            string[] splited = activationCode.Action.Split(',');

            switch (splited[0])
            {
                case "confirm-registration":
                    user.IsActivated = true;
                    DB.ActivationCodes.Remove(activationCode);
                    break;
                case "change-email":
                    user.Email = splited[1];
                    DB.ActivationCodes.Remove(activationCode);
                    break;
                case "delete-user":
                    ActivationCode[] acList = await DB.ActivationCodes
                        .Where(p => p.UserId == user.Id)
                        .ToArrayAsync();

                    JoinOffer[] joinOfferList = await DB.JoinOffers
                        .Where(p => p.UserId == user.Id)
                        .ToArrayAsync();

                    GroupMember[] memberList = await DB.GroupMembers
                        .Where(p => p.UserId == user.Id)
                        .ToArrayAsync();

                    await HttpContext.SignOutAsync();

                    DB.GroupMembers.RemoveRange(memberList);
                    DB.ActivationCodes.RemoveRange(acList);
                    DB.JoinOffers.RemoveRange(joinOfferList);
                    DB.Users.Remove(user);
                    break;
                default:
                    return BadRequest(new BaseResponse.ErrorResponse("Unknown action."));
            }

            await DB.SaveChangesAsync();
            return Ok(new BaseResponse.SuccessResponse(null));
        }

        [Authorized]
        [HttpPost("send-join-offer")]
        public async Task<IActionResult> SendJoinOfferToGroup([FromBody] SendJoinOfferToGroupRequest request)
        {
            Group? group = await DB.Groups
                .SingleOrDefaultAsync(p => p.Name == request.GroupName);

            if (group == null)
            {
                return NotFound(new BaseResponse.ErrorResponse("Group not found!"));
            }

            if (!group.JoinOffersFromUsersAllowed)
            {
                return BadRequest(new BaseResponse.ErrorResponse("Group does not allow join offers from users!"));
            }

            User user = AuthorizedUser;

            if (await DB.JoinOffers.AnyAsync(p => p.GroupId == group.Id && p.UserId == user.Id && !p.SentByGroup))
            {
                return BadRequest(new BaseResponse.ErrorResponse("Join offer is already sent to the group!"));
            }

            List<GroupMember> members = await DB.GroupMembers
                .Where(p => p.GroupId == group.Id)
                .ToListAsync();

            if (members.Any(p => p.UserId == user.Id))
            {
                return BadRequest(new BaseResponse.ErrorResponse("You already are a member of the group!"));
            }

            JoinOffer offer = (await DB.JoinOffers.AddAsync(new JoinOffer
            {
                GroupId = group.Id,
                Group = group,
                UserId = user.Id,
                User = user,
                Text = request.Text,
                SentByGroup = false,
                SentAt = DateTime.UtcNow
            })).Entity;

            await DB.SaveChangesAsync();
            return Ok(new JoinOfferResponse(offer));
        }

        [Authorized]
        [HttpPost("accept-join-offer/{offerId}")]
        public async Task<IActionResult> AcceptJoinOffer([FromRoute] int offerId, [FromQuery] string? accepted)
        {
            JoinOffer? offer = await DB.JoinOffers
                .Include(p => p.Group)
                .SingleOrDefaultAsync(p => p.Id == offerId && p.SentByGroup);

            if (offer == null)
            {
                return NotFound(new BaseResponse.ErrorResponse("Offer not found!"));
            }

            User user = AuthorizedUser;

            if (offer.UserId != user.Id)
            {
                return BadRequest(new BaseResponse.ErrorResponse("The offer does not belong to you!"));
            }

            if (accepted == "true")
            {
                GroupMember member = (await DB.GroupMembers.AddAsync(new GroupMember
                {
                    GroupId = offer.Group.Id,
                    Group = offer.Group,
                    UserId = user.Id,
                    User = user
                })).Entity;

                JoinOffer? offerFromUser = await DB.JoinOffers
                    .SingleOrDefaultAsync(p => p.UserId == offer.UserId && p.GroupId == offer.GroupId && !p.SentByGroup);

                if (offerFromUser != null)
                {
                    DB.JoinOffers.Remove(offerFromUser);
                }

                DB.JoinOffers.Remove(offer);
                await DB.SaveChangesAsync();
                return Ok(new GroupMemberResponse(member));
            }
            else
            {
                DB.JoinOffers.Remove(offer);
                await DB.SaveChangesAsync();
                return Ok(new BaseResponse.SuccessResponse(null));
            }
        }

        [Authorized]
        [HttpGet("sent-join-offers")]
        public async Task<IActionResult> GetSentJoinOffers(
            [FromQuery] PageRequest pageRequest,
            [FromQuery] JoinOfferListRequest request)
        {
            IQueryable<JoinOffer> query = DB.JoinOffers
                .Include(p => p.Group)
                .Include(p => p.User)
                .Where(p => p.UserId == AuthorizedUserId && !p.SentByGroup);

            switch (request.SortParameter)
            {
                case "group":
                    query = request.SortDirection == "asc"
                        ? query.OrderBy(p => p.Group.Name)
                        : query.OrderByDescending(p => p.Group.Name);
                    break;
                case "date":
                    query = request.SortDirection == "asc"
                        ? query.OrderBy(p => p.SentAt)
                        : query.OrderByDescending(p => p.SentAt);
                    break;
            }

            int totalItemsCount = await query.CountAsync();
            int totalPagesCount = (int)Math.Ceiling((double)totalItemsCount / pageRequest.PageSize);
            query = query.Skip((pageRequest.Page - 1) * pageRequest.PageSize).Take(pageRequest.PageSize);

            List<JoinOfferResponse.View> result = await query.Select(p => new JoinOfferResponse.View(p)).ToListAsync();

            PageResponse<JoinOfferResponse.View> response = new(
                result,
                pageRequest.Page,
                pageRequest.PageSize,
                totalPagesCount);

            return Ok(response);
        }

        [Authorized]
        [HttpGet("received-join-offers")]
        public async Task<IActionResult> GetReceivedJoinOffers(
            [FromQuery] PageRequest pageRequest,
            [FromQuery] JoinOfferListRequest request)
        {
            IQueryable<JoinOffer> query = DB.JoinOffers
                .Include(p => p.Group)
                .Include(p => p.User)
                .Where(p => p.UserId == AuthorizedUserId && p.SentByGroup);

            switch (request.SortParameter)
            {
                case "group":
                    query = request.SortDirection == "asc"
                        ? query.OrderBy(p => p.Group.Name)
                        : query.OrderByDescending(p => p.Group.Name);
                    break;
                case "date":
                    query = request.SortDirection == "asc"
                        ? query.OrderBy(p => p.SentAt)
                        : query.OrderByDescending(p => p.SentAt);
                    break;
            }

            int totalItemsCount = await query.CountAsync();
            int totalPagesCount = (int)Math.Ceiling((double)totalItemsCount / pageRequest.PageSize);
            query = query.Skip((pageRequest.Page - 1) * pageRequest.PageSize).Take(pageRequest.PageSize);

            List<JoinOfferResponse.View> result = await query.Select(p => new JoinOfferResponse.View(p)).ToListAsync();

            PageResponse<JoinOfferResponse.View> response = new(
                result,
                pageRequest.Page,
                pageRequest.PageSize,
                totalPagesCount);

            return Ok(response);
        }

        [Authorized]
        [HttpDelete("cancel-join-offer/{offerId}")]
        public async Task<IActionResult> CancelJoinOffer([FromRoute] int offerId)
        {
            JoinOffer? offer = await DB.JoinOffers
                .SingleOrDefaultAsync(p => p.Id == offerId && !p.SentByGroup);

            if (offer == null)
            {
                return NotFound(new BaseResponse.ErrorResponse("Offer not found!"));
            }

            User user = AuthorizedUser;

            if (offer.UserId != user.Id)
            {
                return BadRequest(new BaseResponse.ErrorResponse("The offer does not belong to you!"));
            }

            DB.JoinOffers.Remove(offer);
            await DB.SaveChangesAsync();
            return Ok(new BaseResponse.SuccessResponse(null));
        }
    }
}
