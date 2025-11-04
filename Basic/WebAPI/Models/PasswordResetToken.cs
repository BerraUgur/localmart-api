using System;
using WebAPI.Security;

namespace WebAPI.Models
{
    public class PasswordResetToken
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public string Email { get; set; }
        public int? UserId { get; set; } // Optional UserId for cascade delete
        public DateTime ExpirationDate { get; set; }
        public bool IsUsed { get; set; }
        public virtual User? User { get; set; }
    }
}