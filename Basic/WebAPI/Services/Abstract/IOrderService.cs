using WebAPI.Models;
using WebAPI.ModelViews;

namespace WebAPI.Services.Abstract;

public interface IOrderService
{
    Task<IEnumerable<Order>> GetAllOrdersAsync();
    Task<bool> CreateOrderAsync(OrderRequest orderRequest);
    Task<Order> GetOrderByIdAsync(int orderId);
    Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId);
    Task<bool> DeleteOrderAsync(int orderId);
    Task<bool> UpdateOrderStatusToShippedAsync(int orderId);
    Task<bool> UpdateOrderStatusToShippedProductAsync(int orderId, int pid);
    Task<bool> UpdateOrderStatusToDeliveredAsync(int orderId);
    Task<bool> UpdateOrderStatusToDeliveredProductAsync(int orderId, int pid);
}