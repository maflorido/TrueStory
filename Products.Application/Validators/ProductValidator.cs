using FluentValidation;
using Products.Domain;

namespace Products.Application;

public class ProductValidator : AbstractValidator<Product>
{
    public ProductValidator()
    {
        RuleFor(p => p.Name).NotEmpty().WithMessage("Name is required");
    }
}