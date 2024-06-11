using System.ComponentModel.DataAnnotations;

namespace SmartInlet.Server.Models
{
    public class AirSensor
    {
        [Key]
        public int Id { get; set; }
        public int? GroupId { get; set; }
        public Group? Group { get; set; }
        public int? InletDeviceId { get; set; }
        public InletDevice? InletDevice { get; set; }
        [Required]
        public string AccessCode { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        [Required]
        public short? Aqi { get; set; }
        [Required]
        public short AqiLimitToOpen { get; set; }
        [Required]
        public short AqiLimitToClose { get; set; }
        [Required]
        public bool IsBlocked { get; set; }
        [Required]
        public DateTime UpdatedAt { get; set; }
    }
}
