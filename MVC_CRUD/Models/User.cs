using System.ComponentModel.DataAnnotations;

namespace MVC_CRUD.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string? FullName { get; set; }

        [Required]
        public string? Email { get; set; }

        [Required]
        public string? Password { get; set; }

        [Required]
        public string? Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}