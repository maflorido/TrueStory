using FluentValidation;
using FluentValidation.Results;
using Moq;
using Products.Application;
using Products.Domain;

namespace TrueStory.Tests;

public class ProductServiceTests
{
    private readonly Mock<IProductApiClient> _apiClientMock;
    private readonly Mock<IValidator<Product>> _validatorMock;
    private ProductService _productService;

    public ProductServiceTests()
    {
        _apiClientMock = new Mock<IProductApiClient>();
        _validatorMock = new Mock<IValidator<Product>>();
        _productService = new ProductService(_apiClientMock.Object, _validatorMock.Object);
    }

    [Fact]
    public async Task GetProducts_ShouldReturnFilteredResults()
    {
        // Arrange        
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<Product>(), default))
                      .ReturnsAsync(new ValidationResult());

        _apiClientMock.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(Result<List<Product>>.Success(new List<Product>
            {
                    new Product { Name = "Notebook" },
                    new Product { Name = "Smartphone" },
                    new Product { Name = "Notebook Gamer" }
            }));

        // Act
        var result = await _productService.GetAsync("Notebook", 1, 10);

        // Assert
        Assert.Equal(2, result.TotalRecords);
        Assert.All(result.Value!, p => Assert.Contains("Notebook", p.Name));
    }

    [Fact]
    public async Task GetProducts_ShouldReturnAllResults()
    {
        // Arrange        
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<Product>(), default))
                      .ReturnsAsync(new ValidationResult());

        _apiClientMock.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(Result<List<Product>>.Success(new List<Product>
            {
                    new Product { Name = "Notebook" },
                    new Product { Name = "Smartphone" },
                    new Product { Name = "Notebook Gamer" }
            }));

        // Act
        var result = await _productService.GetAsync(default, 1, 10);

        // Assert
        Assert.Equal(3, result.TotalRecords);
        Assert.All(result.Value!, p => Assert.IsType<Product>(p));
    }

    [Fact]
    public async Task GetProducts_ShouldReturnPaginatedResults()
    {
        // Arrange        
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<Product>(), default))
                      .ReturnsAsync(new ValidationResult());

        _apiClientMock.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(Result<List<Product>>.Success(new List<Product>
            {
                    new Product { Name = "Notebook" },
                    new Product { Name = "Smartphone" },
                    new Product { Name = "Notebook Gamer" }
            }));

        // Act
        var result = await _productService.GetAsync(default, 1, 2)!;

        // Assert
        Assert.Equal(3, result.TotalRecords);
        Assert.Equal(2, result.Value!.Count);

        // Act
        result = await _productService.GetAsync(default, 2, 2)!;

        // Assert
        Assert.Equal(3, result.TotalRecords);
        Assert.Single(result.Value!);
    }

    [Fact]
    public async Task GetProducts_ShouldReturnFailureResult()
    {
        // Arrange        
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<Product>(), default))
                      .ReturnsAsync(new ValidationResult());

        _apiClientMock.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(Result<List<Product>>.Failure("error"));

        // Act
        var result = await _productService.GetAsync(default, 1, 10)!;

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value!);
        Assert.Equal(ErrorCode.Unexpected, result.ErrorCode);
    }

    [Fact]
    public async Task CreateProduct_ShouldReturnSucessResult()
    {
        // Arrange
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<Product>(), default))
                      .ReturnsAsync(new ValidationResult());

        _apiClientMock.Setup(repo => repo.CreateAsync(It.IsAny<Product>()))
            .ReturnsAsync(Result<Product>.Success(new Product()));

        // Act
        var result = await _productService.CreateAsync(new Product())!;

        // Assert
        Assert.True(result.IsSuccess);
        Assert.IsType<Product>(result.Value);
        Assert.Null(result.Error);
    }

    [Fact]
    public async Task CreateProduct_ShouldReturnFailureResult()
    {
        // Arrange
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<Product>(), default))
                      .ReturnsAsync(new ValidationResult());

        _apiClientMock.Setup(repo => repo.CreateAsync(It.IsAny<Product>()))
            .ReturnsAsync(Result<Product>.Failure("error"));

        // Act
        var result = await _productService.CreateAsync(new Product())!;

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task CreateProduct_ShouldReturnValidationFailureResult()
    {
        // Arrange
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<Product>(), default))
                      .ReturnsAsync(new ValidationResult() { Errors = new() { new ValidationFailure("p1", "invalid") } });

        _apiClientMock.Setup(repo => repo.CreateAsync(It.IsAny<Product>()))
            .ReturnsAsync(Result<Product>.Success(new Product()));

        // Act
        var result = await _productService.CreateAsync(new Product())!;

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorCode.ValidationError, result.ErrorCode);
    }

    [Fact]
    public async Task DeleteProduct_ShouldReturnSuccessResult()
    {
        // Arrange
        
        _apiClientMock.Setup(repo => repo.DeleteAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _productService.DeleteAsync("abc")!;

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
    }

    [Fact]
    public async Task DeleteProduct_ShouldReturnFailureResult()
    {
        // Arrange

        _apiClientMock.Setup(repo => repo.DeleteAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Failure("error"));

        // Act
        var result = await _productService.DeleteAsync("abc")!;

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task DeleteProduct_ShouldReturnValidationFailureResult()
    {
        // Arrange

        _apiClientMock.Setup(repo => repo.DeleteAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Success);

        // Act
        var result = await _productService.DeleteAsync(string.Empty)!;

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorCode.ValidationError, result.ErrorCode);
    }
}