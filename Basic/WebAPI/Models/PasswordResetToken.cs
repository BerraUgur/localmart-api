using System;

namespace WebAPI.Models
{
    public class PasswordResetToken
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public string Email { get; set; } // veya UserId
        public DateTime ExpirationDate { get; set; }
        public bool IsUsed { get; set; }
    }
}