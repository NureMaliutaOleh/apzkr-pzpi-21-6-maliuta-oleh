using System.ComponentModel.DataAnnotations;

namespace SmartInlet.Server.Requests
{
    public class SignUpRequest
    {
        [Required]
        [StringLength(480)]
        public string Username { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        [StringLength(480)]
        public string Password { get; set; }
        [Required]
        public string ConfirmPassword { get; set; }
    }
}
