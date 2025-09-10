namespace API.Helpers.Errors;

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
            500 => "Server error. Contact the administrator."
        };
    }
}
