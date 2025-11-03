using FluentValidation;
using WebAPI.Models;
using WebAPI.ModelViews;

namespace WebAPI.Validators;
public class ProductRequestValidator : AbstractValidator<ProductRequest>
{
    public ProductRequestValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("Product name cannot be empty.")
            .MinimumLength(2).WithMessage("Product name must be at least 2 characters.");

        RuleFor(p => p.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero.");

        RuleFor(p => p.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Stock cannot be negative.");

        RuleFor(p => p.MainImage)
            .NotEmpty().WithMessage("Main image cannot be empty.");
    }
}