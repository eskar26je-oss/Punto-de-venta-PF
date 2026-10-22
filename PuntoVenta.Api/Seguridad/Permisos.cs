// Con este "global using" todos los controladores pueden usar [Permiso] y [SoloAdmin]
// sin agregar nada arriba de cada archivo.
global using PuntoVenta.Api.Seguridad;

using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Data;

namespace PuntoVenta.Api.Seguridad;

/// <summary>
/// Datos del usuario que hizo la petición (vienen del token JWT).
/// </summary>
public static class UsuarioActual
{
    public const string NombreAdmin = "admin";
    public const string RutaUsuarios = "/usuarios";
    public const string RutaReportes = "/reportes";

    public static bool EsAdmin(ClaimsPrincipal usuario) =>
        string.Equals(usuario.Identity?.Name, NombreAdmin, StringComparison.OrdinalIgnoreCase);

    public static int? Id(ClaimsPrincipal usuario) =>
        int.TryParse(usuario.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
}

/// <summary>
/// Revisa si el usuario tiene permiso en una opción del menú.
/// - GET (consultar) necesita "Ver".
/// - POST, PUT y DELETE (crear, editar, eliminar) necesitan "Editar".
/// El usuario admin siempre tiene acceso.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class PermisoAttribute : Attribute, IAsyncAuthorizationFilter
{
    public PermisoAttribute(string ruta)
    {
        Ruta = ruta;
    }

    /// <summary>Ruta de la opción del menú, por ejemplo "/productos".</summary>
    public string Ruta { get; }

    /// <summary>Otras opciones que también pueden CONSULTAR aquí (ej. Productos necesita leer Marcas).</summary>
    public string[] TambienVer { get; set; } = Array.Empty<string>();

    /// <summary>Otras opciones que también pueden GUARDAR aquí (ej. Ventas crea clientes).</summary>
    public string[] TambienEditar { get; set; } = Array.Empty<string>();

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var usuario = context.HttpContext.User;

        // Si no hay token, [Authorize] se encarga de responder 401.
        if (usuario.Identity?.IsAuthenticated != true) return;

        var id = UsuarioActual.Id(usuario);
        if (id is null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var db = context.HttpContext.RequestServices.GetRequiredService<PuntoVentaContext>();

        // Si el admin desactivó al usuario, se le cierra la sesión aunque su token siga vigente.
        var activo = await db.Usuarios
            .Where(u => u.IdUsuario == id)
            .Select(u => (ulong?)u.Activo)
            .FirstOrDefaultAsync();

        if (activo != 1)
        {
            context.Result = new UnauthorizedObjectResult(new { mensaje = "Tu usuario está desactivado." });
            return;
        }

        if (UsuarioActual.EsAdmin(usuario)) return;

        var metodo = context.HttpContext.Request.Method;
        var esConsulta = HttpMethods.IsGet(metodo) || HttpMethods.IsHead(metodo);

        var rutas = new List<string> { Ruta };
        rutas.AddRange(esConsulta ? TambienVer : TambienEditar);

        bool permitido;
        if (esConsulta)
        {
            permitido = await db.Permisos.AnyAsync(p =>
                p.IdUsuario == id
                && rutas.Contains(p.IdMenuNavigation.Ruta!)
                && (p.PuedeVer == 1 || p.PuedeEditar == 1));
        }
        else
        {
            permitido = await db.Permisos.AnyAsync(p =>
                p.IdUsuario == id
                && rutas.Contains(p.IdMenuNavigation.Ruta!)
                && p.PuedeEditar == 1);
        }

        if (!permitido)
        {
            var mensaje = esConsulta
                ? "No tienes permiso para ver esta información."
                : "No tienes permiso para hacer cambios aquí.";

            context.Result = new ObjectResult(new { mensaje }) { StatusCode = StatusCodes.Status403Forbidden };
        }
    }
}

/// <summary>
/// Solo el usuario admin puede entrar.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class SoloAdminAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var usuario = context.HttpContext.User;
        if (usuario.Identity?.IsAuthenticated != true) return;

        if (!UsuarioActual.EsAdmin(usuario))
        {
            context.Result = new ObjectResult(new { mensaje = "Solo el administrador puede administrar usuarios." })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}
