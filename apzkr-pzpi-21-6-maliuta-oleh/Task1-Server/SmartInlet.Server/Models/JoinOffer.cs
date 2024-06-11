using System.ComponentModel.DataAnnotations;

namespace SmartInlet.Server.Models
{
    public class JoinOffer
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int GroupId { get; set; }
        public Group Group { get; set; }
        [Required]
        public int UserId { get; set; }
        public User User { get; set; }
        public string? Text { get; set; }
        [Required]
        public bool SentByGroup { get; set; }
        [Required]
        public DateTime SentAt { get; set; }
    }
}
