using System.ComponentModel.DataAnnotations;

namespace SmartInlet.Server.Requests
{
    public class ChangeSensorRequest
    {
        [Required]
        public string GroupName { get; set; }
        [Required]
        public int SensorId { get; set; }
    }
}
