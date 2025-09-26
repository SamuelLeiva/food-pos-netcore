using System.Security.Claims;

namespace API.Extensions;

public static class ClaimsPrincipalExtensions
{
    private const string UserIdClaimType = "uid";
    /// <summary>
    /// Intenta obtener el User ID del claim 'NameIdentifier' (Subject) y convertirlo a un entero.
    /// </summary>
    /// <param name="principal">El ClaimsPrincipal (User) de la solicitud.</param>
    /// <returns>El ID del usuario como entero, o -1 si no se encuentra o no es válido.</returns>
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        // El claim que almacena el ID del usuario (Subject) es ClaimTypes.NameIdentifier
        var userIdClaim = principal.FindFirst(UserIdClaimType)?.Value;

        if (int.TryParse(userIdClaim, out int userId))
        {
            return userId;
        }

        // Devolvemos un valor no válido para indicar el fallo
        return -1;
    }
}
