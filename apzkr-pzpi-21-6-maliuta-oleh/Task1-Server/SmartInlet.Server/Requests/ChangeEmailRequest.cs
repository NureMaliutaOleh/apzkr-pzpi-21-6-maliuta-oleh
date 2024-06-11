using System.ComponentModel.DataAnnotations;

namespace SmartInlet.Server.Requests
{
    public class ChangeEmailRequest
    {
        [Required]
        public string Password { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
