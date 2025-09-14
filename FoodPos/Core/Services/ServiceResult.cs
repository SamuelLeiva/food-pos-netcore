namespace Core.Services;

public class ServiceResult
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; }

    public static ServiceResult Success() => new ServiceResult { IsSuccess = true };
    public static ServiceResult Failure(string errorMessage) => new ServiceResult { IsSuccess = false, ErrorMessage = errorMessage };
}

public class ServiceResult<T>
{
    public T Data { get; set; }
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; }

    public static ServiceResult<T> Success(T data) => new ServiceResult<T> { IsSuccess = true, Data = data };
    public static ServiceResult<T> Failure(string errorMessage) => new ServiceResult<T> { IsSuccess = false, ErrorMessage = errorMessage };
}


