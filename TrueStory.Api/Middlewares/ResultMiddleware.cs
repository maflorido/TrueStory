using System.Text.Json;
using Products.Application;

namespace Products.Api.Middlewares;
public class ResultMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var originalBodyStream = context.Response.Body;
        var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        await next(context);
        
        memoryStream.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();

        if (!(context?.Response?.ContentType?.Equals("application/json") ?? false))
        {
            memoryStream.Seek(0, SeekOrigin.Begin);
            await memoryStream.CopyToAsync(originalBodyStream);
            return;
        }

        var result = JsonSerializer.Deserialize<HttpResult>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        
        context.Response.Body = originalBodyStream;
        context.Response.Clear();
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = result.IsSuccess ? StatusCodes.Status200OK : MapErrorCodeToStatus(result.ErrorCode);

        await context.Response.WriteAsJsonAsync(result);        
    }

    private static int MapErrorCodeToStatus(ErrorCode code) => code switch
    {
        ErrorCode.None => StatusCodes.Status200OK,
        ErrorCode.ValidationError => StatusCodes.Status400BadRequest,
        ErrorCode.NotFound => StatusCodes.Status404NotFound,
        ErrorCode.ExternalApiError => StatusCodes.Status424FailedDependency,
        ErrorCode.Unexpected => StatusCodes.Status500InternalServerError,
        _ => StatusCodes.Status500InternalServerError
    };
}

public class HttpResult
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
    public dynamic? Value { get; set; }
    public ErrorCode ErrorCode { get; set; }
    public int? TotalRecords { get; set; }
}

