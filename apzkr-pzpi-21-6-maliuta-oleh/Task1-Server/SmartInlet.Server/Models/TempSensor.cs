using System.ComponentModel.DataAnnotations;

namespace SmartInlet.Server.Models
{
    public class TempSensor
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
        public short? Kelvins { get; set; }
        [Required]
        public short KelvinLimitToOpen { get; set; }
        [Required]
        public short KelvinLimitToClose { get; set; }
        [Required]
        public bool IsBlocked { get; set; }
        [Required]
        public DateTime UpdatedAt { get; set; }
    }
}
