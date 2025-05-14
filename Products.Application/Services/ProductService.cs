using FluentValidation;
using Products.Domain;

namespace Products.Application;
public class ProductService(IProductApiClient client, IValidator<Product> validator) : IProductService
{
    public async Task<Result<List<Product>>> GetAsync(string? name, int page, int pageSize)
    {
        var result = await client.GetAllAsync();

        if (!result.IsSuccess)
            return result;

        //I am assuming the api always return the items sorted. If not, I would need to implement a sort policy on my end to make the pagination consistent.
        var filtered = result.Value!
            .Where(p => string.IsNullOrEmpty(name) || p.Name.Contains(name, StringComparison.OrdinalIgnoreCase));

        var paginated = filtered.Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Result<List<Product>>.PaginatedSuccess(paginated, filtered.Count());
    }

    public async Task<Result<Product>> CreateAsync(Product product)
    {
        var validationResult = await validator.ValidateAsync(product);

        if (!validationResult.IsValid)
            return Result<Product>.Failure(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)), ErrorCode.ValidationError);

        var result = await client.CreateAsync(product);

        return result;
    }

    public async Task<Result> DeleteAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return Result.Failure("Product ID is required.", ErrorCode.ValidationError);

        var result = await client.DeleteAsync(id);

        return result;
    }
}
