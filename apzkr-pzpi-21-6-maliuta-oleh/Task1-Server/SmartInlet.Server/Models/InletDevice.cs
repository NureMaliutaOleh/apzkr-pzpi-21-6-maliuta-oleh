using System.ComponentModel.DataAnnotations;

namespace SmartInlet.Server.Models
{
    public class InletDevice
    {
        [Key]
        public int Id { get; set; }
        public int? GroupId { get; set; }
        public Group? Group { get; set; }
        public int? AirSensorId { get; set; }
        public AirSensor? AirSensor { get; set; }
        public int? TempSensorId { get; set; }
        public TempSensor? TempSensor { get; set; }
        [Required]
        public string AccessCode { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        [Required]
        [MaxLength(100)]
        public string ControlType { get; set; }
        [Required]
        public bool IsOpened { get; set; }
        [Required]
        public bool IsBlocked { get; set; }
        [Required]
        public DateTime UpdatedAt { get; set; }
    }
}
