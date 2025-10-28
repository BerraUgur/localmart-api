using WebAPI.ModelViews;
using WebAPI.Services.Abstract;
using WebAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Endpoints;

public static class OrderEndpoints
{
    public static void RegisterOrderEndpoints(this WebApplication app)
    {
        var orders = app.MapGroup("orders").RequireAuthorization().WithTags("orders");

        orders.MapGet("", async (IOrderService orderService, [FromServices] ILogger<object> logger) =>
        {
            try
            {
                var ordersData = await orderService.GetAllOrdersAsync();
                var response = new ApiResponse<object>(200, "Orders retrieved successfully.", ordersData);
                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving orders");
                var response = new ApiResponse<object>(500, "An error occurred while retrieving orders.", new object());
                return Results.Problem(response.Message);
            }
        }).AllowAnonymous();

        orders.MapGet("/{id}", async (int id, IOrderService orderService, [FromServices] ILogger<object> logger) =>
        {
            try
            {
                var orderData = await orderService.GetOrderByIdAsync(id);
                if (orderData != null)
                {
                    var response = new ApiResponse<object>(200, "Order retrieved successfully.", orderData);
                    return Results.Ok(response);
                }
                else
                {
                    logger.LogWarning("Order not found. Id: {OrderId}", id);
                    var response = new ApiResponse<object>(404, "Order not found.", new object());
                    return Results.NotFound(response);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving order. Id: {OrderId}", id);
                var response = new ApiResponse<object>(500, "An error occurred while retrieving the order.", new object());
                return Results.Problem(response.Message);
            }
        });

        orders.MapGet("/user/{userId}", async (int userId, IOrderService orderService, [FromServices] ILogger<object> logger) =>
        {
            try
            {
                var ordersData = await orderService.GetOrdersByUserIdAsync(userId);
                var response = new ApiResponse<object>(200, "User orders retrieved successfully.", ordersData);
                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving user orders. UserId: {UserId}", userId);
                var response = new ApiResponse<object>(500, "An error occurred while retrieving user orders.", new object());
                return Results.Problem(response.Message);
            }
        });

        orders.MapPost("", async (OrderRequest order, IOrderService orderService, [FromServices] ILogger<object> logger) =>
        {
            try
            {
                var createdOrder = await orderService.CreateOrderAsync(order);
                if (createdOrder)
                {
                    var response = new ApiResponse<object>(201, "Order created successfully.", order);
                    return Results.Created($"/orders/{order.Id}", response);
                }
                else
                {
                    logger.LogWarning("Order could not be created. Id: {OrderId}", order.Id);
                    var response = new ApiResponse<object>(400, "Order could not be created.", new object());
                    return Results.BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating order. Id: {OrderId}", order.Id);
                var response = new ApiResponse<object>(500, "An error occurred while creating the order.", new object());
                return Results.Problem(response.Message);
            }
        });

        orders.MapPut("/shipped/{id}", async (int id, IOrderService orderService, [FromServices] ILogger<object> logger) =>
        {
            try
            {
                var result = await orderService.UpdateOrderStatusToShippedAsync(id);
                if (result)
                {
                    var response = new ApiResponse<object>(204, "Order shipped successfully.", new object());
                    return Results.NoContent();
                }
                else
                {
                    logger.LogWarning("Order could not be shipped. Id: {OrderId}", id);
                    var response = new ApiResponse<object>(400, "Order could not be shipped.", new object());
                    return Results.BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error shipping order. Id: {OrderId}", id);
                var response = new ApiResponse<object>(500, "An error occurred while shipping the order.", new object());
                return Results.Problem(response.Message);
            }
        });

        orders.MapPut("/shipped-product/{id}", async (int id, int pid, IOrderService orderService, [FromServices] ILogger<object> logger) =>
        {
            try
            {
                var result = await orderService.UpdateOrderStatusToShippedProductAsync(id, pid);
                if (result)
                {
                    var response = new ApiResponse<object>(204, "Product shipped successfully.", new object());
                    return Results.NoContent();
                }
                else
                {
                    logger.LogWarning("Product could not be shipped. OrderId: {OrderId}, ProductId: {ProductId}", id, pid);
                    var response = new ApiResponse<object>(400, "Product could not be shipped.", new object());
                    return Results.BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error shipping product. OrderId: {OrderId}, ProductId: {ProductId}", id, pid);
                var response = new ApiResponse<object>(500, "An error occurred while shipping the product.", new object());
                return Results.Problem(response.Message);
            }
        });

        orders.MapPut("/delivered/{id}", async (int id, IOrderService orderService, [FromServices] ILogger<object> logger) =>
        {
            try
            {
                var result = await orderService.UpdateOrderStatusToDeliveredAsync(id);
                if (result)
                {
                    var response = new ApiResponse<object>(204, "Order delivered successfully.", new object());
                    return Results.NoContent();
                }
                else
                {
                    logger.LogWarning("Order could not be delivered. Id: {OrderId}", id);
                    var response = new ApiResponse<object>(400, "Order could not be delivered.", new object());
                    return Results.BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error delivering order. Id: {OrderId}", id);
                var response = new ApiResponse<object>(500, "An error occurred while delivering the order.", new object());
                return Results.Problem(response.Message);
            }
        });

        orders.MapPut("/delivered-product/{id}", async (int id, int pid, IOrderService orderService, [FromServices] ILogger<object> logger) =>
        {
            try
            {
                var result = await orderService.UpdateOrderStatusToDeliveredProductAsync(id, pid);
                if (result)
                {
                    var response = new ApiResponse<object>(204, "Product delivered successfully.", new object());
                    return Results.NoContent();
                }
                else
                {
                    logger.LogWarning("Product could not be delivered. OrderId: {OrderId}, ProductId: {ProductId}", id, pid);
                    var response = new ApiResponse<object>(400, "Product could not be delivered.", new object());
                    return Results.BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error delivering product. OrderId: {OrderId}, ProductId: {ProductId}", id, pid);
                var response = new ApiResponse<object>(500, "An error occurred while delivering the product.", new object());
                return Results.Problem(response.Message);
            }
        });

        orders.MapDelete("/{id}", async (int id, IOrderService orderService, [FromServices] ILogger<object> logger) =>
        {
            try
            {
                var result = await orderService.DeleteOrderAsync(id);
                if (result)
                {
                    var response = new ApiResponse<object>(204, "Order deleted successfully.", new object());
                    return Results.NoContent();
                }
                else
                {
                    logger.LogWarning("Order could not be deleted. Id: {OrderId}", id);
                    var response = new ApiResponse<object>(404, "Order not found.", new object());
                    return Results.NotFound(response);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting order. Id: {OrderId}", id);
                var response = new ApiResponse<object>(500, "An error occurred while deleting the order.", new object());
                return Results.Problem(response.Message);
            }
        });
    }
}