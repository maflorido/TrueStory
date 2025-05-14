
namespace Products.Application;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public ErrorCode ErrorCode { get; }
    
    protected Result(bool isSuccess, string? error, ErrorCode errorCode)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorCode = errorCode;
    }

    public static Result Success() => new(true, null, ErrorCode.None);
    public static Result Failure(string error, ErrorCode errorCode = ErrorCode.Unexpected)
        => new(false, error, errorCode);
}

public class Result<T> : Result
{
    public T? Value { get; }

    public int? TotalRecords {  get; }

    protected Result(bool isSuccess, T? value, string? error, ErrorCode errorCode, int? totalRecords)
        : base(isSuccess, error, errorCode)
    {
        Value = value;
        TotalRecords = totalRecords;
    }

    public static Result<T> Success(T value)
        => new(true, value, null, ErrorCode.None, null);
    public static Result<T> PaginatedSuccess(T value, int totalRecords)
        => new(true, value, null, ErrorCode.None, totalRecords); 

    public static Result<T> Failure(string error, ErrorCode errorCode = ErrorCode.Unexpected)
        => new(false, default, error, errorCode, null);
}

public enum ErrorCode
{
    None = 0,
    ValidationError = 1,
    NotFound = 2,
    ExternalApiError = 3,
    Unauthorized = 4,
    Forbidden = 5,
    Conflict = 6,
    Unexpected = 999
}

