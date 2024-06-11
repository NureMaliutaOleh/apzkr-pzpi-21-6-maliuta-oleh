using System.ComponentModel.DataAnnotations;

namespace SmartInlet.Server.Requests
{
    public class SendJoinOfferToGroupRequest
    {
        [Required]
        public string GroupName { get; set; }
        public string? Text { get; set; }
    }
}
