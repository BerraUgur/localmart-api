using FluentValidation;
using WebAPI.Models;
using WebAPI.ModelViews;

namespace WebAPI.Validators;
public class ProductValidator : AbstractValidator<ProductRequest>
{
    public ProductValidator()
    {
        // RuleFor(p => p.Name).NotEmpty().WithMessage("Ürün adı boş olamaz.");
        // RuleFor(p => p.Price).GreaterThan(0).WithMessage("Fiyat sıfırdan büyük olmalıdır.");
        // RuleFor(p => p.Stock).GreaterThanOrEqualTo(0).WithMessage("Stok adedi negatif olamaz.");
        // RuleFor(p => p.MainImage).NotNull().NotEmpty().WithMessage("Ana resim boş olamaz.");
    }
}