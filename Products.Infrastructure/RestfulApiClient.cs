using System.Net.Http.Json;
using Products.Application;
using Products.Domain;

namespace Products.Infrastructure;
public class RestfulApiClient(HttpClient httpClient) : IProductApiClient
{
    public async Task<Result<List<Product>>> GetAllAsync()
    {
        try
        {
            var response = await httpClient.GetAsync("/objects");

            if (!response.IsSuccessStatusCode)
                return Result<List<Product>>.Failure(
                    $"Failed to retrieve products. Status code: {response.StatusCode}",
                    ErrorCode.ExternalApiError
                );

            var data = await response.Content.ReadFromJsonAsync<List<Product>>();

            return Result<List<Product>>.Success(data ?? []);
        }
        catch (Exception ex)
        {
            return Result<List<Product>>.Failure(
                $"Unexpected error: {ex.Message}",
                ErrorCode.Unexpected
            );
        }
    }

    public async Task<Result<Product>> CreateAsync(Product product)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("/objects", new
            {
                name = product.Name,
                data = product.Data
            });

            if (!response.IsSuccessStatusCode)
                return Result<Product>.Failure(
                    $"Failed to create product. Status code: {response.StatusCode}",
                    ErrorCode.ExternalApiError
                );

            var created = await response.Content.ReadFromJsonAsync<Product>();

            if (created is null)
                return Result<Product>.Failure("Invalid response from API.", ErrorCode.ExternalApiError);

            return Result<Product>.Success(created);
        }
        catch (Exception ex)
        {
            return Result<Product>.Failure(
                $"Unexpected error: {ex.Message}",
                ErrorCode.Unexpected
            );
        }
    }

    public async Task<Result> DeleteAsync(string id)
    {
        try
        {
            var response = await httpClient.DeleteAsync($"/objects/{id}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Result.Failure("Product not found.", ErrorCode.NotFound);

            if (!response.IsSuccessStatusCode)
                return Result.Failure($"Failed to delete product. Status: {response.StatusCode}", ErrorCode.ExternalApiError);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(
                $"Unexpected error: {ex.Message}",
                ErrorCode.Unexpected
            );
        }
    }
}
