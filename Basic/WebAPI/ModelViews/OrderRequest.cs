using WebAPI.Models;

namespace WebAPI.ModelViews;

public class OrderRequest
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int AddressId { get; set; }
    public DateTime OrderDate { get; set; }
    public string Note { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Preparing;
    public ICollection<OrderItemRequest> OrderItems { get; set; } = new List<OrderItemRequest>();
}