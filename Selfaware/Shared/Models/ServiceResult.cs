namespace Selfaware.Shared.Models;

public class ServiceResult<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string Message { get; set; } = string.Empty;
    public IEnumerable<string>? Errors { get; set; }

    public static ServiceResult<T> Failed(string message, IEnumerable<string>? errors = null) =>
        new()
        {
            Success = false,
            Message = message,
            Errors = errors,
        };

    public static ServiceResult<T> Ok(T data, string message = "") =>
        new()
        {
            Success = true,
            Data = data,
            Message = message,
        };
}
