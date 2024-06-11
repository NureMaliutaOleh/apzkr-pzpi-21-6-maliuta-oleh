using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartInlet.Server.Attributes;
using SmartInlet.Server.Models;
using SmartInlet.Server.Requests;
using SmartInlet.Server.Responses;
using SmartInlet.Server.Services.DB;
using System.Net;

namespace SmartInlet.Server.Controllers
{
    [ApiController]
    [Route("api/groups")]
    public class GroupController : BaseController
    {
        public GroupController(DbApp db) : base(db) { }

        [Authorized]
        [HttpPost]
        public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequest request)
        {
            if (await DB.Groups.AnyAsync(p => p.Name == request.Name))
            {
                return BadRequest(new BaseResponse.ErrorResponse("Group with the same name already exists!"));
            }

            if (request.Name.Length < 3 || request.Name.Length > 50)
            {
                return BadRequest(new BaseResponse.ErrorResponse("Allowed name range: [3, 50]."));
            }

            User user = AuthorizedUser;

            Group group = (await DB.Groups.AddAsync(new Group()
            {
                Name = request.Name,
                JoinOffersFromUsersAllowed = request.JoinOffersFromUsersAllowed,
                OwnerId = user.Id,
                Owner = user
            })).Entity;

            await DB.GroupMembers.AddAsync(new GroupMember()
            {
                UserId = user.Id,
                User = user,
                GroupId = group.Id,
                Group = group,
                CanEditMembers = true,
                CanEditDevices = true
            });

            await DB.SaveChangesAsync();
            return Ok(new GroupResponse(group));
        }

        [HttpGet]
        public async Task<IActionResult> GetGroups(
            [FromQuery] PageRequest pageRequest,
            SearchGroupsRequest request)
        {
            request.Query = (request.Query ?? "").ToLower();

            IQueryable<Group> query = DB.Groups
                .Include(p => p.Owner)
                .Where(p => p.Name.ToLower().Contains(request.Query))
                .OrderBy(p => p.Name);

            int totalItemsCount = await query.CountAsync();
            int totalPagesCount = (int)Math.Ceiling((double)totalItemsCount / pageRequest.PageSize);
            query = query.Skip((pageRequest.Page - 1) * pageRequest.PageSize).Take(pageRequest.PageSize);

            List<GroupResponse.View> result = await query
                .Select(p => new GroupResponse.View(p))
                .ToListAsync();

            PageResponse<GroupResponse.View> response = new(
                result,
                pageRequest.Page,
                pageRequest.PageSize,
                totalPagesCount);

            return Ok(response);
        }

        [HttpGet("{groupName}/info")]
        public async Task<IActionResult> GetGroupInfo([FromRoute] string groupName)
        {
            Group? group = await DB.Groups
                .Include(p => p.Owner)
                .SingleOrDefaultAsync(p => p.Name == WebUtility.UrlDecode(groupName));

            if (group == null)
            {
                return NotFound(new BaseResponse.ErrorResponse("Group not found!"));
            }

            return Ok(new GroupResponse(group));
        }

        [HttpPut("{groupName}/info")]
        public async Task<IActionResult> UpdateGroupInfo(
            [FromRoute] string groupName,
            [FromBody] UpdateGroupInfoRequest request)
        {
            Group? group = await DB.Groups
                .Include(p => p.Owner)
                .SingleOrDefaultAsync(p => p.Name == WebUtility.UrlDecode(groupName));

            if (group == null)
            {
                return NotFound(new BaseResponse.ErrorResponse("Group not found!"));
            }

            if (group.OwnerId != AuthorizedUserId)
            {
                return BadRequest(new BaseResponse.ErrorResponse("You are not the admin of the group!"));
            }

            bool different = false;

            if (group.Name != (request.Name ?? group.Name))
            {
                if (await DB.Groups.AnyAsync(p => p.Name == request.Name))
                {
                    return BadRequest(new BaseResponse.ErrorResponse("There is an group with the same name!"));
                }

                group.Name = request.Name!;
                different = true;
            }

            if (group.JoinOffersFromUsersAllowed != request.JoinOffersFromUsersAllowed)
            {
                group.JoinOffersFromUsersAllowed = request.JoinOffersFromUsersAllowed;
                different = true;
            }

            if (!different)
            {
                return Ok(new BaseResponse.SuccessResponse("Data are identical"));
            }

            await DB.SaveChangesAsync();

            return Ok(new GroupResponse(group));
        }

        [Authorized]
        [HttpPost("{orgName}/send-join-offer")]
        public async Task<IActionResult> SendJoinOfferToUser(
            [FromRoute] string orgName,
            [FromBody] SendJoinOfferToUserRequest request)
        {
            Group? group = await DB.Groups
                .Include(p => p.Owner)
                .SingleOrDefaultAsync(p => p.Name == WebUtility.UrlDecode(orgName));

            if (group == null)
            {
                return NotFound(new BaseResponse.ErrorResponse("Group not found!"));
            }

            GroupMember? member = await DB.GroupMembers
                .SingleOrDefaultAsync(p => p.GroupId == group.Id && p.UserId == AuthorizedUserId);

            if (member == null)
            {
                return NotFound(new BaseResponse.ErrorResponse("You are not a member of the group!"));
            }

            if (!member.CanEditMembers)
            {
                return NotFound(new BaseResponse.ErrorResponse("You are not allowed to send join offers!"));
            }

            User? user = await DB.Users
                .SingleOrDefaultAsync(p => p.Username == request.Username);

            if (user == null)
            {
                return NotFound(new BaseResponse.ErrorResponse("User not found!"));
            }

            if (await DB.JoinOffers.AnyAsync(p => p.GroupId == group.Id && p.UserId == user.Id && p.SentByGroup))
            {
                return BadRequest(new BaseResponse.ErrorResponse("Join offer is already sent to the user!"));
            }

            List<GroupMember> members = await DB.GroupMembers
                .Where(p => p.GroupId == group.Id)
                .ToListAsync();

            if (members.Any(p => p.UserId == user.Id))
            {
                return BadRequest(new BaseResponse.ErrorResponse("The user already is a member of the group!"));
            }

            JoinOffer offer = (await DB.JoinOffers.AddAsync(new JoinOffer
            {
                GroupId = group.Id,
                Group = group,
                UserId = user.Id,
                User = user,
                Text = request.Text,
                SentByGroup = true,
                SentAt = DateTime.UtcNow
            })).Entity;

            await DB.SaveChangesAsync();
            return Ok(new JoinOfferResponse(offer));
        }

        [Authorized]
        [HttpPost("{groupName}/accept-join-offer/{offerId}")]
        public async Task<IActionResult> AcceptJoinOffer(
            [FromRoute] string groupName,
            [FromRoute] int offerId,
            [FromQuery] string? accepted)
        {
            Group? group = await DB.Groups
                .Include(p => p.Owner)
                .SingleOrDefaultAsync(p => p.Name == WebUtility.UrlDecode(groupName));

            if (group == null)
            {
                return NotFound(new BaseResponse.ErrorResponse("Group not found!"));
            }

            GroupMember? member = await DB.GroupMembers
                .SingleOrDefaultAsync(p => p.GroupId == group.Id && p.UserId == AuthorizedUserId);

            if (member == null)
            {
                return NotFound(new BaseResponse.ErrorResponse("You are not a member of the group!"));
            }

            if (!member.CanEditMembers)
            {
                return NotFound(new BaseResponse.ErrorResponse("You are not allowed to accept join offers!"));
            }

            JoinOffer? offer = await DB.JoinOffers
                .Include(p => p.User)
                .SingleOrDefaultAsync(p => p.Id == offerId && !p.SentByGroup);

            if (offer == null)
            {
                return NotFound(new BaseResponse.ErrorResponse("Offer not found!"));
            }

            if (offer.GroupId != group.Id)
            {
                return BadRequest(new BaseResponse.ErrorResponse("The offer does not belong to the group!"));
            }

            User? user = await DB.Users
                .SingleOrDefaultAsync(p => p.Username == offer.User.Username);

            if (user == null)
            {
                return NotFound(new BaseResponse.ErrorResponse("User not found!"));
            }

            if (accepted == "true")
            {
                GroupMember newMember = (await DB.GroupMembers.AddAsync(new GroupMember
                {
                    GroupId = group.Id,
                    Group = group,
                    UserId = user.Id,
                    User = user
                })).Entity;

                JoinOffer? offerFromOrg = await DB.JoinOffers
                    .SingleOrDefaultAsync(p => p.UserId == offer.UserId && p.GroupId == offer.GroupId && p.SentByGroup);

                if (offerFromOrg != null)
                {
                    DB.JoinOffers.Remove(offerFromOrg);
                }

                DB.JoinOffers.Remove(offer);
                await DB.SaveChangesAsync();
                return Ok(new GroupMemberResponse(newMember));
            }
            else
            {
                DB.JoinOffers.Remove(offer);
                await DB.SaveChangesAsync();
                return Ok(new BaseResponse.SuccessResponse(null));
            }
        }

        [Authorized]
        [HttpGet("{groupName}/sent-join-offers")]
        public async Task<IActionResult> GetSentJoinOffers(
            [FromRoute] string groupName,
            [FromQuery] PageRequest pageRequest,
            [FromQuery] JoinOfferListRequest request)
        {
            Group? group = await DB.Groups
                .Include(p => p.Owner)
                .SingleOrDefaultAsync(p => p.Name == WebUtility.UrlDecode(groupName));

            if (group == null)
            {
                return NotFound(new BaseResponse.ErrorResponse("Group not found!"));
            }

            IQueryable<JoinOffer> query = DB.JoinOffers
                .Include(p => p.Group)
                .Include(p => p.User)
                .Where(p => p.GroupId == group.Id && p.SentByGroup);

            switch (request.SortParameter)
            {
                case "user":
                    query = request.SortDirection == "asc"
                        ? query.OrderBy(p => p.User.Username)
                        : query.OrderByDescending(p => p.User.Username);
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
        [HttpGet("{groupName}/received-join-offers")]
        public async Task<IActionResult> GetReceivedJoinOffers(
            [FromRoute] string groupName,
            [FromQuery] PageRequest pageRequest,
            [FromQuery] JoinOfferListRequest request)
        {
            Group? group = await DB.Groups
                .Include(p => p.Owner)
                .SingleOrDefaultAsync(p => p.Name == WebUtility.UrlDecode(groupName));

            if (group == null)
            {
                return NotFound(new BaseResponse.ErrorResponse("Group not found!"));
            }

            IQueryable<JoinOffer> query = DB.JoinOffers
                .Include(p => p.Group)
                .Include(p => p.User)
                .Where(p => p.GroupId == group.Id && !p.SentByGroup);

            switch (request.SortParameter)
            {
                case "user":
                    query = request.SortDirection == "asc"
                        ? query.OrderBy(p => p.User.Username)
                        : query.OrderByDescending(p => p.User.Username);
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
        [HttpDelete("{groupName}/cancel-join-offer/{offerId}")]
        public async Task<IActionResult> CancelJoinOffer(
            [FromRoute] string groupName,
            [FromRoute] int offerId)
        {
            Group? group = await DB.Groups
                .Include(p => p.Owner)
                .SingleOrDefaultAsync(p => p.Name == WebUtility.UrlDecode(groupName));

            if (group == null)
            {
                return NotFound(new BaseResponse.ErrorResponse("Group not found!"));
            }

            GroupMember? member = await DB.GroupMembers
                .SingleOrDefaultAsync(p => p.GroupId == group.Id && p.UserId == AuthorizedUserId);

            if (member == null)
            {
                return NotFound(new BaseResponse.ErrorResponse("You are not a member of the group!"));
            }

            if (!member.CanEditMembers)
            {
                return NotFound(new BaseResponse.ErrorResponse("You are not allowed to cancel join offers!"));
            }

            JoinOffer? offer = await DB.JoinOffers
                .Include(p => p.User)
                .SingleOrDefaultAsync(p => p.Id == offerId && p.SentByGroup);

            if (offer == null)
            {
                return NotFound(new BaseResponse.ErrorResponse("Offer not found!"));
            }

            if (offer.GroupId != group.Id)
            {
                return BadRequest(new BaseResponse.ErrorResponse("The offer does not belong to the group!"));
            }

            User? user = await DB.Users
                .SingleOrDefaultAsync(p => p.Username == offer.User.Username);

            if (user == null)
            {
                return NotFound(new BaseResponse.ErrorResponse("User not found!"));
            }

            DB.JoinOffers.Remove(offer);
            await DB.SaveChangesAsync();
            return Ok(new BaseResponse.SuccessResponse(null));
        }

        [Authorized]
        [HttpGet("{groupName}/members")]
        public async Task<IActionResult> GetGroupMembers(
            [FromRoute] string groupName,
            [FromQuery] PageRequest pageRequest)
        {
            Group? group = await DB.Groups
                .SingleOrDefaultAsync(p => p.Name == WebUtility.UrlDecode(groupName));

            if (group == null)
            {
                return NotFound(new BaseResponse.ErrorResponse("Group not found!"));
            }

            IQueryable<GroupMember> query = DB.GroupMembers
                .Include(p => p.Group)
                .Include(p => p.User)
                .Where(p => p.GroupId == group.Id)
                .OrderBy(p => p.User.Username);

            int totalItemsCount = await query.CountAsync();
            int totalPagesCount = (int)Math.Ceiling((double)totalItemsCount / pageRequest.PageSize);
            query = query.Skip((pageRequest.Page - 1) * pageRequest.PageSize).Take(pageRequest.PageSize);

            List<GroupMemberResponse.View> result = await query
                .Select(p => new GroupMemberResponse.View(p)).ToListAsync();

            PageResponse<GroupMemberResponse.View> response = new(
                result,
                pageRequest.Page,
                pageRequest.PageSize,
                totalPagesCount);

            return Ok(response);
        }

        [Authorized]
        [HttpPut("{groupName}/members/{memberId}")]
        public async Task<IActionResult> UpdateMemberInfo(
            [FromRoute] string groupName,
            [FromRoute] int memberId,
            [FromBody] UpdateGroupMemberRequest request)
        {
            Group? group = await DB.Groups
                .Include(p => p.Owner)
                .SingleOrDefaultAsync(p => p.Name == WebUtility.UrlDecode(groupName));

            if (group == null)
            {
                return NotFound(new BaseResponse.ErrorResponse("Group not found!"));
            }

            GroupMember? ownMember = await DB.GroupMembers
                .SingleOrDefaultAsync(p => p.GroupId == group.Id && p.UserId == AuthorizedUserId);

            if (ownMember == null)
            {
                return NotFound(new BaseResponse.ErrorResponse("You are not a member of the group!"));
            }

            if (!ownMember.CanEditMembers)
            {
                return NotFound(new BaseResponse.ErrorResponse("You are not allowed to edit members!"));
            }

            GroupMember? member = await DB.GroupMembers
                .Include(p => p.User)
                .Include(p => p.Group)
                .SingleOrDefaultAsync(p => p.Id == memberId);

            if (member == null)
            {
                return NotFound(new BaseResponse.ErrorResponse("Member not found!"));
            }

            if (member.GroupId != group.Id)
            {
                return NotFound(new BaseResponse.ErrorResponse("The user is not a member of the group!"));
            }

            if (member.UserId == AuthorizedUserId)
            {
                return NotFound(new BaseResponse.ErrorResponse("You can not edit own info!"));
            }

            if (member.UserId == group.OwnerId)
            {
                return NotFound(new BaseResponse.ErrorResponse("You can not edit an admin!"));
            }

            bool different = false;

            if (member.CanEditMembers != request.CanEditMembers)
            {
                member.CanEditMembers = request.CanEditMembers;
                different = true;
            }

            if (member.CanEditDevices != request.CanEditDevices)
            {
                member.CanEditDevices = request.CanEditDevices;
                different = true;
            }

            if (!different)
            {
                return Ok(new BaseResponse.SuccessResponse("Data are identical"));
            }

            await DB.SaveChangesAsync();
            return Ok(new GroupMemberResponse(member));
        }

        [Authorized]
        [HttpDelete("{groupName}/members/{memberId}")]
        public async Task<IActionResult> KickMember(
            [FromRoute] string groupName,
            [FromRoute] int memberId)
        {
            Group? group = await DB.Groups
                .SingleOrDefaultAsync(p => p.Name == WebUtility.UrlDecode(groupName));

            if (group == null)
            {
                return NotFound(new BaseResponse.ErrorResponse("Group not found!"));
            }

            GroupMember? ownMember = await DB.GroupMembers
                .SingleOrDefaultAsync(p => p.GroupId == group.Id && p.UserId == AuthorizedUserId);

            if (ownMember == null)
            {
                return NotFound(new BaseResponse.ErrorResponse("You are not a member of the group!"));
            }

            if (!ownMember.CanEditMembers)
            {
                return NotFound(new BaseResponse.ErrorResponse("You are not allowed to delete members!"));
            }

            if (memberId == ownMember.Id)
            {
                return NotFound(new BaseResponse.ErrorResponse("You can not kick yourself!"));
            }

            GroupMember? member = await DB.GroupMembers
                .SingleOrDefaultAsync(p => p.Id == memberId);

            if (member == null)
            {
                return NotFound(new BaseResponse.ErrorResponse("Member not found!"));
            }

            if (member.GroupId != group.Id)
            {
                return NotFound(new BaseResponse.ErrorResponse("The user is not a member of the group!"));
            }

            if (member.UserId == group.OwnerId)
            {
                return NotFound(new BaseResponse.ErrorResponse("You can not kick an admin!"));
            }

            DB.GroupMembers.Remove(member);
            await DB.SaveChangesAsync();
            return Ok(new BaseResponse.SuccessResponse(null));
        }

        [Authorized]
        [HttpDelete("{groupName}")]
        public async Task<IActionResult> QuitOrDeleteGroup(
            [FromRoute] string groupName,
            [FromQuery(Name = "heir_id")] string? heirId)
        {
            Group? group = await DB.Groups
                .SingleOrDefaultAsync(p => p.Name == WebUtility.UrlDecode(groupName));

            if (group == null)
            {
                return NotFound(new BaseResponse.ErrorResponse("Group not found!"));
            }

            if (group.OwnerId != AuthorizedUserId)
            {
                return BadRequest(new BaseResponse.ErrorResponse("You are not the admin of the group!"));
            }

            GroupMember ownMember = await DB.GroupMembers
               .SingleAsync(p => p.GroupId == group.Id && p.UserId == AuthorizedUserId);

            if (!heirId.IsNullOrEmpty())
            {
                GroupMember? heir = await DB.GroupMembers
                    .Include(p => p.User)
                    .SingleOrDefaultAsync(p => p.GroupId == group.Id && p.UserId.ToString() == heirId);

                if (heir == null)
                {
                    return BadRequest(new BaseResponse.ErrorResponse("The user is not a member of the group!"));
                }

                heir.CanEditMembers = true;
                heir.CanEditDevices = true;

                group.OwnerId = heir.UserId;
                group.Owner = heir.User;

                DB.Remove(ownMember);
            }
            else
            {
                DB.Remove(group);
            }

            await DB.SaveChangesAsync();
            return Ok(new BaseResponse.SuccessResponse(null));
        }
    }
}
