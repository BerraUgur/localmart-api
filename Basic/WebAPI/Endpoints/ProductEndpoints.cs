using WebAPI.Filters;
using WebAPI.ModelViews;
using WebAPI.Models;
using WebAPI.Services.Abstract;

namespace WebAPI.Endpoints;
public static class ProductEndpoints
{

    public static void RegisterProductEndpoints(this WebApplication app)
    {
        var product = app.MapGroup("products")
            .RequireAuthorization("multi")
            .WithTags("products");

        product.MapGet("", async (IProductService productService) =>
        {
            var products = await productService.GetAllProductsAsync();
            return Results.Ok(products);
        }).AllowAnonymous();

        product.MapGet("/{id}", async (int id, IProductService productService) =>
        {
            var product = await productService.GetProductByIdAsync(id);
            return product != null ? Results.Ok(product) : Results.NotFound();
        }).AllowAnonymous();
        
        product.MapGet("/user/{userId}", async (int userId, IProductService productService) =>
        {
            var products = await productService.GetProductsByUserIdAsync(userId);
            return Results.Ok(products);
        }).AllowAnonymous();

        product.MapPost("", async (ProductRequest product, IProductService productService) =>
        {
            var createdProduct = await productService.CreateProductAsync(product);
            return Results.Created($"/products/{createdProduct.Id}", createdProduct);
        }).AddEndpointFilter<ValidatorFilter<ProductRequest>>();

        product.MapPut("/{id}", async (int id, Product product, IProductService productService) =>
        {
            Console.WriteLine("Product: " + product.Images);
            if (id != product.Id)
            {
                return Results.BadRequest();
            }
            await productService.UpdateProductAsync(product);
            return Results.NoContent();
        });

        product.MapDelete("/{id}", async (int id, IProductService productService) =>
        {
            await productService.DeleteProductAsync(id);
            return Results.NoContent();
        });
    }
}