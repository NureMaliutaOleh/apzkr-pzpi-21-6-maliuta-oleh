using System.ComponentModel.DataAnnotations;

namespace SmartInlet.Server.Models
{
    public class GroupMember
    {
        public int Id { get; set; }
        [Required]
        public int UserId { get; set; }
        public User User { get; set; }
        [Required]
        public int GroupId { get; set; }
        public Group Group { get; set; }
        [Required]
        public bool CanEditMembers { get; set; }
        [Required]
        public bool CanEditDevices { get; set; }
    }
}
