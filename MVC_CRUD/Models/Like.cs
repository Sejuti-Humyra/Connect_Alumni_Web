using System.ComponentModel.DataAnnotations.Schema;

namespace MVC_CRUD.Models
{
    public class Like
    {
        public int Id { get; set; }

        public int PostId { get; set; }
        [ForeignKey("PostId")]
        public Post? Post { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}