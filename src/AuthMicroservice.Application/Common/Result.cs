namespace AuthMicroservice.Application.Common;

public class Result<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
    public string? Message { get; init; }
    public List<string> Errors { get; init; } = [];

    public static Result<T> Success(T data, string? message = null)
    {
        return new Result<T>
        {
            IsSuccess = true,
            Data = data,
            Message = message
        };
    }

    public static Result<T> Failure(string message, List<string>? errors = null)
    {
        return new Result<T>
        {
            IsSuccess = false,
            Message = message,
            Errors = errors ?? []
        };
    }

    public static Result<T> Failure(string message, string error)
    {
        return new Result<T>
        {
            IsSuccess = false,
            Message = message,
            Errors = [error]
        };
    }
}

public class Result
{
    public bool IsSuccess { get; init; }
    public string? Message { get; init; }
    public List<string> Errors { get; init; } = [];

    public static Result Success(string? message = null)
    {
        return new Result
        {
            IsSuccess = true,
            Message = message
        };
    }

    public static Result Failure(string message, List<string>? errors = null)
    {
        return new Result
        {
            IsSuccess = false,
            Message = message,
            Errors = errors ?? []
        };
    }

    public static Result Failure(string message, string error)
    {
        return new Result
        {
            IsSuccess = false,
            Message = message,
            Errors = [error]
        };
    }
}
