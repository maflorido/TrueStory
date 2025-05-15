using System.Net;
using System.Text.Json;
using Moq;
using Moq.Protected;
using Products.Application;
using Products.Domain;
using Products.Infrastructure;

namespace TrueStory.Tests;

public class RestfulApiClientTests
{
    private static HttpClient CreateHttpClient(HttpResponseMessage responseMessage)
    {
        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(responseMessage);

        return new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://fake.api")
        };
    }

    private static HttpClient CreateHttpClientException()
    {
        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException("Simulated network failure"));

        return new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://fake.api")
        };
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnSuccess_WhenApiReturnsData()
    {
        var expectedProducts = new List<Product> { new() { Name = "Test", Data = "123" } };
        var json = JsonSerializer.Serialize(expectedProducts);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json)
        };
        response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        var client = new RestfulApiClient(CreateHttpClient(response));

        var result = await client.GetAllAsync();

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value);
        Assert.Equal("Test", result.Value[0].Name);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnFailure_WhenApiFails()
    {
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        var client = new RestfulApiClient(CreateHttpClient(response));

        var result = await client.GetAllAsync();

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.ExternalApiError, result.ErrorCode);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnFailure_WhenApiUnexpectedError()
    {
        var client = new RestfulApiClient(CreateHttpClientException());

        var result = await client.GetAllAsync();

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.Unexpected, result.ErrorCode);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnSuccess_WhenApiReturnsCreated()
    {
        var input = new Product { Name = "New", Data = "456" };
        var created = new Product { Name = "New", Data = "456" };
        var json = JsonSerializer.Serialize(created);
        var response = new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(json)
        };
        response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        var client = new RestfulApiClient(CreateHttpClient(response));

        var result = await client.CreateAsync(input);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("New", result.Value.Name);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnFailure_WhenResponseIsNull()
    {
        var response = new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent("")
        };
        var client = new RestfulApiClient(CreateHttpClient(response));

        var result = await client.CreateAsync(new Product { Name = "Fail", Data = "x" });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.Unexpected, result.ErrorCode);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnFailure_WhenHttpStatusIsInvaid()
    {
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("")
        };
        var client = new RestfulApiClient(CreateHttpClient(response));

        var result = await client.CreateAsync(new Product { Name = "Fail", Data = "x" });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.ExternalApiError, result.ErrorCode);
    }

    [Fact]
    public async Task CrreateAsync_ShouldReturnFailure_WhenApiUnexpectedError()
    {
        var client = new RestfulApiClient(CreateHttpClientException());

        var result = await client.CreateAsync(new Product { Name = "Fail", Data = "x" });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.Unexpected, result.ErrorCode);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnSuccess_WhenDeletedSuccessfully()
    {
        var response = new HttpResponseMessage(HttpStatusCode.NoContent);
        var client = new RestfulApiClient(CreateHttpClient(response));

        var result = await client.DeleteAsync("123");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnNotFound_WhenItemDoesNotExist()
    {
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);
        var client = new RestfulApiClient(CreateHttpClient(response));

        var result = await client.DeleteAsync("not-found-id");

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.NotFound, result.ErrorCode);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFailure_WhenApiError()
    {
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
        var client = new RestfulApiClient(CreateHttpClient(response));

        var result = await client.DeleteAsync("invalid-id");

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.ExternalApiError, result.ErrorCode);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFailure_WhenApiUnexpectedError()
    {
        var client = new RestfulApiClient(CreateHttpClientException());

        var result = await client.DeleteAsync("invalid-id");

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.Unexpected, result.ErrorCode);
    }
}
