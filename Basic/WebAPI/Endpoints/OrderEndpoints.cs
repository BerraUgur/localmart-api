using WebAPI.Models;
using WebAPI.ModelViews;
using WebAPI.Services.Abstract;

namespace WebAPI.Endpoints;

public static class OrderEndpoints
{
    public static void RegisterOrderEndpoints(this WebApplication app)
    {
        var orders = app.MapGroup("orders").RequireAuthorization()
            .WithTags("orders");

        orders.MapGet("", async (IOrderService orderService) =>
        {
            var orders = await orderService.GetAllOrdersAsync();
            return Results.Ok(orders);
        }).AllowAnonymous();
        
        orders.MapGet("/{id}", async (int id, IOrderService orderService) =>
        {
            var order = await orderService.GetOrderByIdAsync(id);
            return order != null ? Results.Ok(order) : Results.NotFound();
        });

        orders.MapGet("/user/{userId}", async (int userId, IOrderService orderService) =>
        {
            var orders = await orderService.GetOrdersByUserIdAsync(userId);
            return Results.Ok(orders);
        });

        orders.MapPost("", async (OrderRequest order, IOrderService orderService) =>
        {
            var createdOrder = await orderService.CreateOrderAsync(order);
            return createdOrder ? Results.Created($"/orders/{order.Id}", order) : Results.BadRequest();
        });

        orders.MapPut("/shipped/{id}", async (int id, IOrderService orderService) =>
        {
            var result = await orderService.UpdateOrderStatusToShippedAsync(id);
            return result ? Results.NoContent() : Results.BadRequest();
        });

        orders.MapPut("/shipped-product/{id}", async (int id, int pid, IOrderService orderService) =>
        {
            var result = await orderService.UpdateOrderStatusToShippedProductAsync(id, pid);
            return result ? Results.NoContent() : Results.BadRequest();
        });

        orders.MapPut("/delivered/{id}", async (int id, IOrderService orderService) =>
        {
            var result = await orderService.UpdateOrderStatusToDeliveredAsync(id);
            return result ? Results.NoContent() : Results.BadRequest();
        });
        orders.MapPut("/delivered-product/{id}", async (int id, int pid, IOrderService orderService) =>
        {
            var result = await orderService.UpdateOrderStatusToDeliveredProductAsync(id, pid);
            return result ? Results.NoContent() : Results.BadRequest();
        });
    }
}