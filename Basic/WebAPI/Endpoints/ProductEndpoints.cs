using WebAPI.Filters;
using WebAPI.ModelViews;
using WebAPI.Models;
using WebAPI.Services.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Endpoints;
public static class ProductEndpoints
{
    public static void RegisterProductEndpoints(this WebApplication app)
    {
        var product = app.MapGroup("products").RequireAuthorization("multi").WithTags("products");

        product.MapGet("", async (IProductService productService, [FromServices] ILogger<object> logger) =>
        {
            try
            {
                var productsData = await productService.GetAllProductsAsync();
                var response = new ApiResponse<object>(200, "Products retrieved successfully.", productsData);
                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving products.");
                var response = new ApiResponse<object>(500, "An error occurred while retrieving products.", new object());
                return Results.Problem(response.Message);
            }
        }).AllowAnonymous();

        product.MapGet("/{id}", async (int id, IProductService productService, [FromServices] ILogger<object> logger) =>
        {
            try
            {
                var productData = await productService.GetProductByIdAsync(id);
                if (productData != null)
                {
                    var response = new ApiResponse<object>(200, "Product retrieved successfully.", productData);
                    return Results.Ok(response);
                }
                else
                {
                    logger.LogWarning("Product not found. Id: {ProductId}", id);
                    var response = new ApiResponse<object>(404, "Product not found.", new object());
                    return Results.NotFound(response);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving product. Id: {ProductId}", id);
                var response = new ApiResponse<object>(500, "An error occurred while retrieving the product.", new object());
                return Results.Problem(response.Message);
            }
        }).AllowAnonymous();

        product.MapGet("/user/{userId}", async (int userId, IProductService productService, [FromServices] ILogger<object> logger) =>
        {
            try
            {
                var productsData = await productService.GetProductsByUserIdAsync(userId);
                var response = new ApiResponse<object>(200, "User products retrieved successfully.", productsData);
                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving user products. UserId: {UserId}", userId);
                var response = new ApiResponse<object>(500, "An error occurred while retrieving user products.", new object());
                return Results.Problem(response.Message);
            }
        }).AllowAnonymous();

        product.MapPost("", async (ProductRequest product, IProductService productService, [FromServices] ILogger<object> logger) =>
        {
            try
            {
                var createdProduct = await productService.CreateProductAsync(product);
                var response = new ApiResponse<object>(201, "Product created successfully.", createdProduct);
                return Results.Created($"/products/{createdProduct.Id}", response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating product. Product: {@Product}", product);
                var response = new ApiResponse<object>(500, "An error occurred while creating the product.", new object());
                return Results.Problem(response.Message);
            }
        }).AddEndpointFilter<ValidatorFilter<ProductRequest>>();

        product.MapPut("/{id}", async (int id, Product product, IProductService productService, [FromServices] ILogger<object> logger) =>
        {
            try
            {
                if (id != product.Id)
                {
                    logger.LogWarning("Product IDs do not match. Id: {Id}, ProductId: {ProductId}", id, product.Id);
                    var response = new ApiResponse<object>(400, "Product IDs do not match.", new object());
                    return Results.BadRequest(response);
                }
                await productService.UpdateProductAsync(product);
                var successResponse = new ApiResponse<object>(204, "Product updated successfully.", new object());
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating product. Id: {ProductId}", id);
                var response = new ApiResponse<object>(500, "An error occurred while updating the product.", new object());
                return Results.Problem(response.Message);
            }
        });

        product.MapDelete("/{id}", async (int id, IProductService productService, [FromServices] ILogger<object> logger) =>
        {
            try
            {
                await productService.DeleteProductAsync(id);
                var response = new ApiResponse<object>(204, "Product deleted successfully.", new object());
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting product. Id: {ProductId}", id);
                var response = new ApiResponse<object>(500, "An error occurred while deleting the product.", new object());
                return Results.Problem(response.Message);
            }
        });
    }
}