using System.ComponentModel.DataAnnotations;

namespace SmartInlet.Server.Requests
{
    public class AdminDeviceRequest
    {
        [Required]
        public string DeviceType { get; set; }
        [Required]
        public int DeviceId { get; set; }
    }
}
