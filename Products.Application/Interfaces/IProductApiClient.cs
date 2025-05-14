using Products.Domain;

namespace Products.Application;

public interface IProductApiClient
{
    Task<Result<List<Product>>> GetAllAsync();
    Task<Result<Product>> CreateAsync(Product product);
    Task<Result> DeleteAsync(string id);
}