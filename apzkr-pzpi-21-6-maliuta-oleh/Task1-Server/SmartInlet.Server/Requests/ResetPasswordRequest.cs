using System.ComponentModel.DataAnnotations;

namespace SmartInlet.Server.Requests
{
    public class ResetPasswordRequest
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string NewPassword { get; set; }
        [Required]
        public string ConfirmNewPassword { get; set; }
        [Required]
        public string Token { get; set; }
    }
}
