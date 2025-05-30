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

    public ProductService(ApplicationDBContext context, IHttpContextAccessor contextAccessor)
    {
        _context = context;
        _contextAccessor = contextAccessor;
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        return await _context.Products.ToListAsync();
    }

    public async Task<Product> GetProductByIdAsync(int id)
    {
        return await _context.Products.Include(x =>x.Comments).ThenInclude(x=>x.User).FirstOrDefaultAsync(p => p.Id == id);
    }
    
    public async Task<IEnumerable<Product>> GetProductsByUserIdAsync(int userId)
    {
        return await _context.Products.Where(p => p.SellerUserId == userId).ToListAsync();
    }

    public async Task<Product> CreateProductAsync(ProductRequest productRequest)
    {
        string userId = _contextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        Console.WriteLine("User Id: " + userId);
        Console.WriteLine("User Id: " + int.Parse(userId) );
        var product = new Product
        {
            SellerUserId =int.Parse(userId) , 
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
    
    private bool CheckAccess(int userId)
    {
        if(_contextAccessor.HttpContext.User.IsInRole("Admin") || _contextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value == userId.ToString())
        {
            return false;
        }

        return true;
    }

    public async Task UpdateProductAsync(Product product)
    {
        if (CheckAccess(product.SellerUserId))
        {
            throw new UnauthorizedAccessException("Access denied");
        }
        
        if(product.MainImage != null && !product.MainImage.Contains("/images")){
            product.MainImage = SaveImage(product.MainImage);
        }

        //if(product.Images != null && product.Images.Any(x=>!x.Contains("/images"))){
        if(product.Images != null && product.Images.Any(x => x != null && !x.Contains("/images"))){

                product.Images = product.Images.Select(SaveImage).ToList();
        }
        else
        {
            var oldImages = _context.Products.AsNoTracking().FirstOrDefaultAsync(x=>x.Id == product.Id).Result.Images;
            product.Images = oldImages;
        }
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteProductAsync(int id)
    {
        
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            if (CheckAccess(product.SellerUserId))
            {
                throw new UnauthorizedAccessException("Access denied");
            }
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
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
