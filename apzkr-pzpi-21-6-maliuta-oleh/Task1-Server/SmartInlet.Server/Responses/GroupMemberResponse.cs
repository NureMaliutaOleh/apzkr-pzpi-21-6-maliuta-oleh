using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SmartInlet.Server.Models;

namespace SmartInlet.Server.Responses
{
    public class GroupMemberResponse : BaseResponse
    {
        public GroupMemberResponse(GroupMember data) : base(true, null, null)
        {
            Data = new View(data);
        }

        public GroupMemberResponse(ICollection<GroupMember> data) : base(true, null, null)
        {
            Data = data.Select(p => new View(p));
        }

        public class View
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }
            [JsonPropertyName("group")]
            public string Group { get; set; }
            [JsonPropertyName("user")]
            public string User { get; set; }
            [JsonPropertyName("canEditMembers")]
            public bool CanEditMembers { get; set; }
            [JsonPropertyName("canEditDevices")]
            public bool CanEditDevices { get; set; }

            public View(GroupMember obj)
            {
                Id = obj.Id;
                Group = obj.Group.Name;
                User = obj.User.Username;
                CanEditMembers = obj.CanEditMembers;
                CanEditDevices = obj.CanEditDevices;
            }
        }
    }
}
