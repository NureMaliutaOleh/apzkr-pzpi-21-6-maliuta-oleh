using System.ComponentModel.DataAnnotations;

namespace SmartInlet.Server.Requests
{
    public class SendJoinOfferToUserRequest
    {
        [Required]
        public string Username { get; set; }
        public string? Text { get; set; }
    }
}
