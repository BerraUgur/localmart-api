using WebAPI.Security;

namespace WebAPI.Models;

public class Address
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string OpenAddress { get; set; }
    public string City { get; set; }
    public string District { get; set; }
    public string PostalCode { get; set; }
    public virtual User User { get; set; } = null!;
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}