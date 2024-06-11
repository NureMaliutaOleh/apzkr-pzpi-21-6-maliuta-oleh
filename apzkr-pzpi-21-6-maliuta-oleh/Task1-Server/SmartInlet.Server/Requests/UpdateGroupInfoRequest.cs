using System.ComponentModel.DataAnnotations;

namespace SmartInlet.Server.Requests
{
    public class UpdateGroupInfoRequest
    {
        [Required]
        [StringLength(50, MinimumLength = 4)]
        public string Name { get; set; }
        [Required]
        public bool JoinOffersFromUsersAllowed { get; set; }
    }
}
