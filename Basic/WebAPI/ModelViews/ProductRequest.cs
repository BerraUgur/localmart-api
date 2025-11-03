namespace WebAPI.ModelViews;

public class ProductRequest
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public decimal DiscountedPrice { get; set; }
    public int Stock { get; set; }
    public string Description { get; set; }
    public string City { get; set; }
    public string District { get; set; }
    public string MainImage { get; set; }
    public List<string> Images { get; set; }
}