using Products.Domain;

namespace Products.Application;

public interface IProductService
{
    Task<Result<List<Product>>> GetAsync(string? name, int page, int pageSize);
    Task<Result<Product>> CreateAsync(Product product);
    Task<Result> DeleteAsync(string id);
}