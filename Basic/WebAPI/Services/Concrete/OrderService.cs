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
        public OrderService(ApplicationDBContext context, IHttpContextAccessor contextAccessor)
        {
            _context = context;
            _contextAccessor = contextAccessor;
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)  // OrderItem ile ilgili Product bilgisini dahil et
            .ToListAsync();

            //return await _context.Orders.ToListAsync();
        }
        public async Task<bool> CreateOrderAsync(OrderRequest orderRequest)
        {
            var orderItems = new List<OrderItem>();
            foreach (var item in orderRequest.OrderItems)
            {
                orderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                });
            }
            
            var order = new Order
            {
                UserId = orderRequest.UserId,
                Note = orderRequest.Note,
                AddressId = orderRequest.AddressId
            };
            order.OrderItems = orderItems;
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems).ThenInclude(x=>x.Product)
                .ToListAsync();
        }

        // public async Task<bool> DeleteOrderAsync(int orderId)
        // {
        //
        //     _context.Orders.Remove(await _context.Orders.FindAsync(orderId));
        //     await _context.SaveChangesAsync();
        //     return true;
        // }
        
        private bool CheckAccess(int userId)
        {
            if (_contextAccessor.HttpContext.User.IsInRole("Admin") || _contextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value == userId.ToString())
            {
                return false;
            }

            return true;
    }

        public async Task<bool> UpdateOrderStatusToShippedAsync(int orderId)
        {
            var updateOrderStatus = await UpdateOrderStatus(orderId, OrderStatus.Shipped);
            if (updateOrderStatus)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> UpdateOrderStatusToShippedProductAsync(int orderId, int pid)
        {
            var updateOrderStatus = await UpdateOrderProductStatus(orderId, pid, OrderStatus.Shipped);
            if (updateOrderStatus)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> UpdateOrderStatusToDeliveredAsync(int orderId)
        {
            var updateOrderStatus = await UpdateOrderStatus(orderId, OrderStatus.Delivered);
            if (updateOrderStatus)
            {
                return true;
            }

            return false;
        }
        public async Task<bool> UpdateOrderStatusToDeliveredProductAsync(int orderId, int pid)
        {
            var updateOrderStatus = await UpdateOrderProductStatus(orderId, pid, OrderStatus.Delivered);
            if (updateOrderStatus)
            {
                return true;
            }

            return false;
        }

        private async Task<bool> UpdateOrderStatus(int orderId,OrderStatus orderStatus)
        {
            var order = await _context.Orders.Include(x=>x.OrderItems).ThenInclude(x=>x.Product).FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
            {
                return false;
            }

            //if (CheckAccess(order.UserId) || order.OrderItems.Any(x=> !CheckAccess(x.Product.SellerUserId)))
            //{
            //    throw new UnauthorizedAccessException("Access denied");
            //}
            
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
                return false;
            }

            // Ýlgili OrderItem'ý bul
            var orderItem = order.OrderItems.FirstOrDefault(oi => oi.ProductId == pid);

            if (orderItem == null)
            {
                return false;
            }

            // OrderItem'ýn status'unu güncelle
            orderItem.Status = orderStatus;

            await _context.SaveChangesAsync();
            return true;
        }

    }