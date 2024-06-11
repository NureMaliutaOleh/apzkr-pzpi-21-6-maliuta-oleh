using System.Text.Json.Serialization;
using SmartInlet.Server.Models;

namespace SmartInlet.Server.Responses
{
    public class GroupResponse : BaseResponse
    {
        public GroupResponse(Group data) : base(true, null, null)
        {
            Data = new View(data);
        }

        public GroupResponse(ICollection<Group> data) : base(true, null, null)
        {
            Data = data.Select(p => new View(p));
        }

        public class View
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }
            [JsonPropertyName("owner")]
            public string Owner { get; set;}
            [JsonPropertyName("joinOffersFromUsersAllowed")]
            public bool JoinOffersFromUsersAllowed { get; set; }

            public View(Group obj)
            {
                Name = obj.Name;
                Owner = obj.Owner.Username;
                JoinOffersFromUsersAllowed = obj.JoinOffersFromUsersAllowed;
            }
        }
    }
}
