namespace BuildingBlocks.Shared.DTOs;

public class ResponseWrapper<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ResponseWrapper<T> Success(T data, string? message = null) => new()
    {
        IsSuccess = true,
        Data = data,
        Message = message
    };

    public static ResponseWrapper<T> Fail(string message, List<string>? errors = null) => new()
    {
        IsSuccess = false,
        Message = message,
        Errors = errors
    };
}

public class ResponseWrapper
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ResponseWrapper Success(string? message = null) => new()
    {
        IsSuccess = true,
        Message = message
    };

    public static ResponseWrapper Fail(string message, List<string>? errors = null) => new()
    {
        IsSuccess = false,
        Message = message,
        Errors = errors
    };
}