using System.ComponentModel.DataAnnotations;

namespace SmartInlet.Server.Requests
{
    public class ChangeSensorLimitsRequest
    {
        [Required]
        public string GroupName { get; set; }
        [Required]
        public short ToOpen { get; set; }
        [Required]
        public short ToClose { get; set; }
    }
}
