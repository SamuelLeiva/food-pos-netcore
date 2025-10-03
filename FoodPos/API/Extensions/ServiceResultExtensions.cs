using API.Helpers.Response;
using Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Extensions;

public static class ServiceResultExtensions
{
    // Extensión para ServiceResult<T>
    public static ObjectResult ToActionResult<T>(this ServiceResult<T> result)
    {
        // Si es éxito, devuelve Ok(200), Created(201), o similar.
        // Aquí asumimos que si es éxito (200/201), el controlador lo maneja explícitamente.
        // Este método se centra en manejar los fallos (4xx/5xx).

        if (result.IsSuccess)
        {
            // Para resultados exitosos, creamos un 200 OK con el cuerpo ApiResponse<T>
            var successResponse = new ApiResponse<T>(result.StatusCode, "Operation successful.", result.Data);
            return new OkObjectResult(successResponse) { StatusCode = result.StatusCode };
        }
        else
        {
            // Para resultados fallidos (4xx/5xx), usamos el código del ServiceResult.
            var errorResponse = new ApiResponse(result.StatusCode, result.ErrorMessage);

            // Retorna un ObjectResult con el StatusCode y el cuerpo uniforme (ApiResponse)
            return new ObjectResult(errorResponse) { StatusCode = result.StatusCode };
        }
    }

    // Extensión para ServiceResult sin tipo (útil para DELETE exitoso 204 o fallo)
    public static ActionResult ToActionResult(this ServiceResult result)
    {
        if (result.IsSuccess)
        {
            // Para DELETE exitoso, devolvemos 204 NoContent, que no tiene cuerpo
            return new NoContentResult();
        }
        else
        {
            // Para resultados fallidos (4xx/5xx)
            var errorResponse = new ApiResponse(result.StatusCode, result.ErrorMessage);
            return new ObjectResult(errorResponse) { StatusCode = result.StatusCode };
        }
    }
}
