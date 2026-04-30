using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVC_CRUD.Models
{
    public class Job
    {
        public int Id { get; set; }
        [Required] public string? Title { get; set; }
        [Required] public string? Company { get; set; }
        public string? Location { get; set; }
        public string? Type { get; set; }
        public string? Description { get; set; }
        public string? ApplyUrl { get; set; }
        public DateTime PostedAt { get; set; } = DateTime.Now;
        public int PostedById { get; set; }
        [ForeignKey("PostedById")]
        public User? PostedBy { get; set; }
    }
}
