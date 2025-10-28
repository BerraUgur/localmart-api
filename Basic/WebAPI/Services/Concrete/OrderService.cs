using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.Models;
using WebAPI.ModelViews;
using WebAPI.Services.Abstract;

namespace WebAPI.Services.Concrete;

public class OrderService : IOrderService
{
    private readonly ApplicationDBContext _context;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly ILogger<OrderService> _logger;

    public OrderService(ApplicationDBContext context, IHttpContextAccessor contextAccessor, ILogger<OrderService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<Order>> GetAllOrdersAsync()
    {
        try
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all orders.");
            throw new ApplicationException("An error occurred while fetching orders.", ex);
        }
    }

    public async Task<bool> CreateOrderAsync(OrderRequest orderRequest)
    {
        if (orderRequest == null) throw new ArgumentNullException(nameof(orderRequest));
        try
        {
            var orderItems = orderRequest.OrderItems?.Select(item => new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
            }).ToList() ?? new List<OrderItem>();

            var order = new Order
            {
                UserId = orderRequest.UserId,
                Note = orderRequest.Note,
                AddressId = orderRequest.AddressId,
                OrderItems = orderItems
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order.");
            throw new ApplicationException("An error occurred while creating the order.", ex);
        }
    }

    public async Task<Order> GetOrderByIdAsync(int orderId)
    {
        try
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
            {
                _logger.LogWarning($"Order with id {orderId} not found.");
                throw new KeyNotFoundException("Order not found.");
            }
            return order;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching order by id {orderId}.");
            throw new ApplicationException("An error occurred while fetching the order.", ex);
        }
    }

    public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId)
    {
        try
        {
            return await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                .ThenInclude(x => x.Product)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching orders for userId {userId}.");
            throw new ApplicationException("An error occurred while fetching user orders.", ex);
        }
    }

    public async Task<bool> DeleteOrderAsync(int orderId)
    {
        try
        {
            var order = await _context.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
            {
                _logger.LogWarning($"Order with id {orderId} not found for deletion.");
                throw new KeyNotFoundException("Order not found.");
            }
            // Access control: Only admin or owner can delete order
            var httpContext = _contextAccessor.HttpContext;
            string userId = httpContext?.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            if (!httpContext?.User.IsInRole("Admin") == true && userId != order.UserId.ToString())
            {
                _logger.LogWarning($"Access denied for order deletion. UserId: {order.UserId}");
                throw new UnauthorizedAccessException("Access denied");
            }
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting order with id {orderId}.");
            throw new ApplicationException("An error occurred while deleting the order.", ex);
        }
    }

    private bool CheckAccess(int userId)
    {
        var httpContext = _contextAccessor.HttpContext;
        if (httpContext == null)
            return false;
        if (httpContext.User.IsInRole("Admin") || httpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value == userId.ToString())
        {
            return true;
        }
        return false;
    }

    public async Task<bool> UpdateOrderStatusToShippedAsync(int orderId)
    {
        try
        {
            return await UpdateOrderStatus(orderId, OrderStatus.Shipped);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating order status to shipped for orderId {orderId}.");
            throw new ApplicationException("An error occurred while updating order status.", ex);
        }
    }

    public async Task<bool> UpdateOrderStatusToShippedProductAsync(int orderId, int pid)
    {
        try
        {
            return await UpdateOrderProductStatus(orderId, pid, OrderStatus.Shipped);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating product status to shipped for orderId {orderId}, productId {pid}.");
            throw new ApplicationException("An error occurred while updating product status.", ex);
        }
    }

    public async Task<bool> UpdateOrderStatusToDeliveredAsync(int orderId)
    {
        try
        {
            return await UpdateOrderStatus(orderId, OrderStatus.Delivered);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating order status to delivered for orderId {orderId}.");
            throw new ApplicationException("An error occurred while updating order status.", ex);
        }
    }
    public async Task<bool> UpdateOrderStatusToDeliveredProductAsync(int orderId, int pid)
    {
        try
        {
            return await UpdateOrderProductStatus(orderId, pid, OrderStatus.Delivered);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating product status to delivered for orderId {orderId}, productId {pid}.");
            throw new ApplicationException("An error occurred while updating product status.", ex);
        }
    }

    private async Task<bool> UpdateOrderStatus(int orderId, OrderStatus orderStatus)
    {
        var order = await _context.Orders.Include(x => x.OrderItems).ThenInclude(x => x.Product).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null)
        {
            _logger.LogWarning($"Order with id {orderId} not found for status update.");
            return false;
        }
        // Access control: Only admin or owner/seller can update order status
        var httpContext = _contextAccessor.HttpContext;
        string userId = httpContext?.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        if (!httpContext?.User.IsInRole("Admin") == true && userId != order.UserId.ToString() && order.OrderItems.Any(x => userId != x.Product.SellerUserId.ToString()))
        {
            _logger.LogWarning($"Access denied for order status update. UserId: {order?.UserId}");
            throw new UnauthorizedAccessException("Access denied");
        }
        order.Status = orderStatus;
        await _context.SaveChangesAsync();
        return true;
    }

    private async Task<bool> UpdateOrderProductStatus(int orderId, int pid, OrderStatus orderStatus)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            _logger.LogWarning($"Order with id {orderId} not found for product status update.");
            return false;
        }
        var orderItem = order.OrderItems.FirstOrDefault(oi => oi.ProductId == pid);
        if (orderItem == null)
        {
            _logger.LogWarning($"OrderItem with productId {pid} not found in order {orderId}.");
            return false;
        }
        // Access control: Only admin or owner/seller can update product status
        var httpContext = _contextAccessor.HttpContext;
        string userId = httpContext?.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        if (!httpContext?.User.IsInRole("Admin") == true && userId != order.UserId.ToString() && userId != orderItem.Product.SellerUserId.ToString())
        {
            _logger.LogWarning($"Access denied for product status update. UserId: {userId}");
            throw new UnauthorizedAccessException("Access denied");
        }
        orderItem.Status = orderStatus;
        await _context.SaveChangesAsync();
        return true;
    }
}