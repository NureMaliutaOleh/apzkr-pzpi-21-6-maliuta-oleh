using System.ComponentModel.DataAnnotations;

namespace SmartInlet.Server.Requests
{
    public class EmailRequest
    {
        [Required]
        public string Title { get; set; }
        [Required]
        public string Content { get; set; }
    }
}
