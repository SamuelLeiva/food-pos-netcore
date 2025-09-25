namespace API.Helpers.Response;

// Clase no genérica para errores
public class ApiResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; }

    public ApiResponse(int statusCode, string message = null)
    {
        StatusCode = statusCode;
        Message = message ?? GetDefaultMessage(statusCode);
    }

    private string GetDefaultMessage(int statusCode)
    {
        return statusCode switch
        {
            400 => "You have done an incorrect request.",
            401 => "User not authorized.",
            404 => "The resource requested does not exist.",
            405 => "Http method not allowed in the server.",
            500 => "Server error. Contact the administrator.",
            _ => "Unknown error."
        };
    }
}

// Clase genérica para respuestas exitosas con datos
public class ApiResponse<T>
{
    public int StatusCode { get; set; }
    public string Message { get; set; }
    public T Data { get; set; }

    public ApiResponse(int statusCode, string message = null, T data = default)
    {
        StatusCode = statusCode;
        Message = message;
        Data = data;
    }
}