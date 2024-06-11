using System.ComponentModel.DataAnnotations;

namespace SmartInlet.Server.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Username { get; set; }
        [MaxLength(50)]
        public string? FirstName { get; set; }
        [MaxLength(50)]
        public string? LastName { get; set; }
        [Required]
        [MaxLength(500)]
        public string Email { get; set; }
        [Required]
        public string PasswordHash { get; set; }
        [Required]
        public bool CanAdministrateDevices { get; set; }
        [Required]
        public bool CanAdministrateUsers { get; set; }
        [Required]
        public bool IsActivated { get; set; }
        [Required]
        public DateTime RegisteredAt { get; set; }

        public ICollection<Group> Groups { get; set; } = [];
        public ICollection<GroupMember> GroupMembers { get; set; } = [];
        public ICollection<ActivationCode> ActivationCodes { get; set; } = [];
        public ICollection<JoinOffer> JoinOffers { get; set; } = [];
    }
}
