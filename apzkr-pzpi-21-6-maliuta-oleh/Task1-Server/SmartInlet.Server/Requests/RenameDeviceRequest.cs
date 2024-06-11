using System.ComponentModel.DataAnnotations;

namespace SmartInlet.Server.Requests
{
    public class RenameDeviceRequest
    {
        [Required]
        public string GroupName { get; set; }
        [Required]
        public string DeviceType { get; set; }
        [Required]
        public string DeviceName { get; set; }
    }
}
