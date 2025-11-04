using WebAPI.Models;
using WebAPI.Security.Enums;

namespace WebAPI.Security;

public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public Role Role { get; set; } = Role.Seller;
    public byte[] PasswordSalt { get; set; }
    public byte[] PasswordHash { get; set; }
    public bool Status { get; set; }

    public virtual ICollection<UserOperationClaim> UserOperationClaims { get; set; } = null!;
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = null!;
    public virtual ICollection<Comment> Comments { get; set; } = null!;
    public virtual ICollection<Product> Products { get; set; } = null!;
    public virtual ICollection<Address> Addresses { get; set; } = null!;
    public virtual ICollection<Order> Orders { get; set; } = null!;
    public virtual ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = null!;
    public User()
    {
        FirstName = string.Empty;
        LastName = string.Empty;
        Username = string.Empty;
        Email = string.Empty;
        PasswordHash = Array.Empty<byte>();
        PasswordSalt = Array.Empty<byte>();
    }

    public User(
        string firstName,
        string lastName,
        string username,
        string email,
        byte[] passwordSalt,
        byte[] passwordHash,
        bool status
    )
    {
        FirstName = firstName;
        LastName = lastName;
        Username = username;
        Email = email;
        PasswordSalt = passwordSalt;
        PasswordHash = passwordHash;
        Status = status;
    }
}