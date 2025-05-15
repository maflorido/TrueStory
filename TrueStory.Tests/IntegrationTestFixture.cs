using FluentValidation;
using Products.Application;
using Products.Domain;
using Products.Infrastructure;

namespace TrueStory.Tests;

public class IntegrationTestFixture : IDisposable
{
    public IProductService ProductService { get; }

    private readonly HttpClient _httpClient;

    public IntegrationTestFixture()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://api.restful-api.dev/")
        };

        var client = new RestfulApiClient(_httpClient);

        var validator = new InlineValidator<Product>();
        validator.RuleFor(p => p.Name).NotEmpty();
        
        ProductService = new ProductService(client, validator);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
