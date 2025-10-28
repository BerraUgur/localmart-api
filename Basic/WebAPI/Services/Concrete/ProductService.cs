using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.Models;
using WebAPI.ModelViews;
using WebAPI.Services.Abstract;

namespace WebAPI.Services.Concrete;
public class ProductService : IProductService
{
    private readonly ApplicationDBContext _context;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly ILogger<ProductService> _logger;

    public ProductService(ApplicationDBContext context, IHttpContextAccessor contextAccessor, ILogger<ProductService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        try
        {
            return await _context.Products
                .Include(x => x.User)
                .Include(x => x.Comments).ThenInclude(x => x.User)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all products.");
            throw new ApplicationException("An error occurred while fetching products.", ex);
        }
    }

    public async Task<Product> GetProductByIdAsync(int id)
    {
        try
        {
            var product = await _context.Products
                .Include(x => x.Comments).ThenInclude(x => x.User)
                .Include(x => x.User)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                _logger.LogWarning($"Product with id {id} not found.");
                throw new KeyNotFoundException("Product not found.");
            }
            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching product by id {id}.");
            throw new ApplicationException("An error occurred while fetching the product.", ex);
        }
    }

    public async Task<IEnumerable<Product>> GetProductsByUserIdAsync(int userId)
    {
        try
        {
            return await _context.Products.Where(p => p.SellerUserId == userId).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching products for userId {userId}.");
            throw new ApplicationException("An error occurred while fetching user products.", ex);
        }
    }

    public async Task<Product> CreateProductAsync(ProductRequest productRequest)
    {
        if (productRequest == null) throw new ArgumentNullException(nameof(productRequest));
        try
        {
            string userId = _contextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("UserId not found in claims.");
                throw new UnauthorizedAccessException("UserId not found.");
            }

            var product = new Product
            {
                SellerUserId = int.Parse(userId),
                Name = productRequest.Name,
                Price = productRequest.Price,
                DiscountedPrice = productRequest.DiscountedPrice,
                Stock = productRequest.Stock,
                Description = productRequest.Description,
                City = productRequest.City,
                District = productRequest.District,
                MainImage = SaveImage(productRequest.MainImage),
                Images = productRequest.Images != null ? productRequest.Images.Select(SaveImage).ToList() : new List<string>()
            };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product.");
            throw new ApplicationException("An error occurred while creating the product.", ex);
        }
    }

    private bool CheckAccess(int userId)
    {
        if (_contextAccessor.HttpContext.User.IsInRole("Admin") || _contextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value == userId.ToString())
        {
            return false;
        }

        return true;
    }

    public async Task UpdateProductAsync(Product product)
    {
        if (product == null) throw new ArgumentNullException(nameof(product));
        try
        {
            // Access control: Only admin or owner can update product
            string userId = _contextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            if (!_contextAccessor.HttpContext.User.IsInRole("Admin") && userId != product.SellerUserId.ToString())
            {
                _logger.LogWarning($"Access denied for product update. SellerUserId: {product.SellerUserId}");
                throw new UnauthorizedAccessException("Access denied");
            }
            if (product.MainImage != null && !product.MainImage.Contains("/images"))
            {
                product.MainImage = SaveImage(product.MainImage);
            }
            if (product.Images != null && product.Images.Any(x => x != null && !x.Contains("/images")))
            {
                product.Images = product.Images.Select(SaveImage).ToList();
            }
            else
            {
                var oldImages = await _context.Products.AsNoTracking().Where(x => x.Id == product.Id).Select(x => x.Images).FirstOrDefaultAsync();
                product.Images = oldImages;
            }
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product.");
            throw new ApplicationException("An error occurred while updating the product.", ex);
        }
    }

    public async Task DeleteProductAsync(int id)
    {
        try
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                _logger.LogWarning($"Product with id {id} not found for deletion.");
                throw new KeyNotFoundException("Product not found.");
            }
            // Access control: Only admin or owner can delete product
            string userId = _contextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            if (!_contextAccessor.HttpContext.User.IsInRole("Admin") && userId != product.SellerUserId.ToString())
            {
                _logger.LogWarning($"Access denied for product deletion. SellerUserId: {product.SellerUserId}");
                throw new UnauthorizedAccessException("Access denied");
            }
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting product with id {id}.");
            throw new ApplicationException("An error occurred while deleting the product.", ex);
        }
    }

    private string SaveImage(string base64Image)
    {
        if (string.IsNullOrEmpty(base64Image))
        {
            return null;
        }

        var imageData = Convert.FromBase64String(Regex.Replace(base64Image, "^data:image/[a-zA-Z]+;base64,", string.Empty));
        var fileName = $"{Guid.NewGuid()}.jpg";
        var filePath = Path.Combine("wwwroot", "images", fileName);

        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        File.WriteAllBytes(filePath, imageData);

        return $"/images/{fileName}";
    }
}