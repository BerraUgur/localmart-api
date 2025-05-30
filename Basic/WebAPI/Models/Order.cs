using WebAPI.Security;

namespace WebAPI.Models;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int AddressId { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
    public string Note { get; set; }
    // public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Preparing;
    public virtual User User { get; set; } = null!;
    public virtual Address Address { get; set; } = null!;
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public virtual Order Order { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
    public OrderStatus Status { get; set; } = OrderStatus.Preparing;
}  
