using System.ComponentModel.DataAnnotations;

namespace SmartInlet.Server.Requests
{
    public class UpdateGroupMemberRequest
    {
        [Required]
        public bool CanEditMembers { get; set; }
        [Required]
        public bool CanEditDevices { get; set; }
    }
}
