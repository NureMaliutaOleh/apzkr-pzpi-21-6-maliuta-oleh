using System.ComponentModel.DataAnnotations;

namespace SmartInlet.Server.Models
{
    public class Group
    {
        public int Id { get; set; }
        [Required]
        public int OwnerId { get; set; }
        public User Owner { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        [Required]
        public bool JoinOffersFromUsersAllowed { get; set; }

        public ICollection<GroupMember> GroupMembers { get; set; } = [];
        public ICollection<InletDevice> InletDevices { get; set; } = [];
        public ICollection<AirSensor> AirSensors { get; set; } = [];
        public ICollection<TempSensor> TempSensors { get; set; } = [];
        public ICollection<JoinOffer> JoinOffers { get; set; } = [];
    }
}
