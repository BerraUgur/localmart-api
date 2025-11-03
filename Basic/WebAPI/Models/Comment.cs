using WebAPI.Security;

namespace WebAPI.Models;
public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
    }