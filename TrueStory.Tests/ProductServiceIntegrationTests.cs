using Products.Application;
using Products.Domain;

namespace TrueStory.Tests;

public class ProductServiceIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IProductService _productService;

    public ProductServiceIntegrationTests(IntegrationTestFixture fixture)
    {
        _productService = fixture.ProductService;
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateProduct_WhenValid()
    {
        var product = new Product
        {
            Name = "Produto Integração " + Guid.NewGuid(),
            Data = "123"
        };

        var result = await _productService.CreateAsync(product);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(product.Name, result.Value!.Name);
    }

    [Fact]
    public async Task CreateAsync_ShouldNotCreateProduct_WhenInvalid()
    {
        var product = new Product
        {
            Data = "123"
        };

        var result = await _productService.CreateAsync(product);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorCode.ValidationError, result.ErrorCode);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnPaginatedProducts()
    {
        var result = await _productService.GetAsync(null, page: 1, pageSize: 10);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.InRange(result.Value!.Count, 0, 10);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFailure_WhenIdIsInvalid()
    {
        var result = await _productService.DeleteAsync("invalido-id");

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.NotFound, result.ErrorCode);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFailure_WhenIdIsEmpty()
    {
        var result = await _productService.DeleteAsync("");

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.ValidationError, result.ErrorCode);
    }
}
