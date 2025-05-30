using System.ComponentModel.DataAnnotations.Schema;
using WebAPI.Security;

namespace WebAPI.Models;

public class Product
    {
        public int Id { get; set; }
        public int SellerUserId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public decimal DiscountedPrice { get; set; }
        public int Stock { get; set; }
        public string Description { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string? MainImage { get; set; }
        public List<string>? Images { get; set; }
        public virtual ICollection<Comment> Comments { get; set; } = null!;
        
        [ForeignKey("SellerUserId")]
        public virtual User User { get; set; } = null!;
    }