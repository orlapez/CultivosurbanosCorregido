using System.Security.Claims;

namespace Terpy.API.Helpers
{
    public static class UsuarioActualExtensions
    {
        public const string RolAdministrador = "Administrador";

        public static string? ObtenerUsuarioId(this ClaimsPrincipal user)
            => user.FindFirstValue(ClaimTypes.NameIdentifier);

        public static bool EsAdministrador(this ClaimsPrincipal user)
            => user.IsInRole(RolAdministrador);
    }
}