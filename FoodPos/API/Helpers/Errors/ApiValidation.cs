using API.Helpers.Response;

namespace API.Helpers.Errors;

// Errores para la validacion de DTOs
public class ApiValidation : ApiResponse
{
    public ApiValidation() : base(400)
    {

    }

    public IEnumerable<string> Errors { get; set; }
}
