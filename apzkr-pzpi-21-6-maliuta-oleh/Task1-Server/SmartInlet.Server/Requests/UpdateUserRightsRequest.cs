using System.ComponentModel.DataAnnotations;

namespace SmartInlet.Server.Requests
{
    public class UpdateUserRightsRequest
    {
        [Required]
        public bool CanAdministrateDevices { get; set; }
        [Required]
        public bool CanAdministrateUsers { get; set; }
    }
}
