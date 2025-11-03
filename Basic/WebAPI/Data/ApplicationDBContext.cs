using Microsoft.EntityFrameworkCore;
using WebAPI.Models;
using WebAPI.Security;

namespace WebAPI.Data;

public class ApplicationDBContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<OperationClaim> OperationClaims { get; set; }
    public DbSet<UserOperationClaim> UserOperationClaims { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<Log> Logs { get; set; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
    public ApplicationDBContext(DbContextOptions options) : base(options) { }
}