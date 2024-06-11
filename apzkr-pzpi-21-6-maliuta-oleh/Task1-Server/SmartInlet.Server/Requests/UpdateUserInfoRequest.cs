using System.ComponentModel.DataAnnotations;

namespace SmartInlet.Server.Requests
{
    public class UpdateUserInfoRequest
    {
        [StringLength(32, MinimumLength = 4, ErrorMessage = "Username is too short or long")]
        public string? Username { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
