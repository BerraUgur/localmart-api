using WebAPI.Models;
using WebAPI.ModelViews;

namespace WebAPI.Services.Abstract;
public interface IProductService
{
    Task<IEnumerable<Product>> GetAllProductsAsync();
    Task<Product> GetProductByIdAsync(int id);
    Task<Product> CreateProductAsync(ProductRequest product);
    Task UpdateProductAsync(Product product);
    Task DeleteProductAsync(int id);
    Task<IEnumerable<Product>> GetProductsByUserIdAsync(int userId);
}