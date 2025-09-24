namespace Core.Services;

public class ServiceResult
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; }

    // Constructor privado para forzar el uso de métodos estáticos
    protected ServiceResult(bool isSuccess, string errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public static ServiceResult Success() => new ServiceResult(true, null);
    public static ServiceResult Failure(string errorMessage) => new ServiceResult(false, errorMessage);
}

public class ServiceResult<T> : ServiceResult
{
    public T Data { get; set; }
    private ServiceResult(T data, bool isSuccess, string errorMessage)
        : base(isSuccess, errorMessage)
    {
        Data = data;
    }

    public static ServiceResult<T> Success(T data) => new ServiceResult<T>(data, true, null);
    public static ServiceResult<T> Failure(string errorMessage) => new ServiceResult<T>(default, false, errorMessage);
}


