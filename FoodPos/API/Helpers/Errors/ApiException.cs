using API.Helpers.Response;

namespace API.Helpers.Errors;

// Clase usada en Exception Middleware
public class ApiException : ApiResponse
{
    public ApiException(int statusCode, string message = null, string details = null) : base(statusCode, message)
    {
        Details = details;
    }

    public string Details { get; set; }
}
