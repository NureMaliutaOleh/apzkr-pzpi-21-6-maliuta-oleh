using System.ComponentModel.DataAnnotations;

namespace SmartInlet.Server.Requests
{
    public class AccessDeviceRequest
    {
        [Required]
        public string GroupName { get; set; }
        [Required]
        public int DeviceId { get; set; }
        [Required]
        public string DeviceType { get; set; }
        [Required]
        public string AccessCode { get; set; }
    }
}
