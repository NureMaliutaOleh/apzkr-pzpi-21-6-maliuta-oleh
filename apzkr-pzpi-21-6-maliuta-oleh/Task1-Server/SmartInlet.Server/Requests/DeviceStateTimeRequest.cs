using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace SmartInlet.Server.Requests
{
    public class DeviceStateTimeRequest
    {
        [Required]
        public string GroupName { get; set; }
        [Required]
        public bool Opens { get; set; }
        [Required]
        public TimeOnly Time { get; set; }
    }
}
