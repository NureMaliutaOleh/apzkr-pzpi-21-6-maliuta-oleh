using SmartInlet.Server.Models;
using System.Text.Json.Serialization;

namespace SmartInlet.Server.Responses
{
    public class JoinOfferResponse : BaseResponse
    {
        public JoinOfferResponse(JoinOffer data) : base(true, null, null)
        {
            Data = new View(data);
        }

        public JoinOfferResponse(ICollection<JoinOffer> data) : base(true, null, null)
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
            [JsonPropertyName("text")]
            public string? Text { get; set; }
            [JsonPropertyName("sentByGroup")]
            public bool SentByGroup { get; set; }
            [JsonPropertyName("sentAt")]
            public DateTime SentAt { get; set; }

            public View(JoinOffer obj)
            {
                Id = obj.Id;
                Group = obj.Group.Name;
                User = obj.User.Username;
                Text = obj.Text;
                SentByGroup = obj.SentByGroup;
                SentAt = obj.SentAt;
            }
        }
    }
}
