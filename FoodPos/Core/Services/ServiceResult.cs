using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Core.Services;

public class ServiceResult
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; }
    public int StatusCode { get; set; }

    // Constructor privado para forzar el uso de métodos estáticos
    protected ServiceResult(bool isSuccess, string errorMessage, int statusCode = 200)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        StatusCode = statusCode;
    }

    public static ServiceResult Success() => new ServiceResult(true, null, 200);
    public static ServiceResult Failure(string errorMessage, int statusCode = 400) => 
        new ServiceResult(false, errorMessage, statusCode);
}

public class ServiceResult<T> : ServiceResult
{
    public T Data { get; set; }
    private ServiceResult(T data, bool isSuccess, string errorMessage, int statusCode)
        : base(isSuccess, errorMessage, statusCode)
    {
        Data = data;
    }

    // Método de éxito (por defecto 200)
    public static ServiceResult<T> Success(T data)
        => new ServiceResult<T>(data, true, null, 200);

    // Método de fallo, aceptando un StatusCode (por defecto 400)
    public static new ServiceResult<T> Failure(string errorMessage, int statusCode = 400)
        => new ServiceResult<T>(default, false, errorMessage, statusCode);
}


