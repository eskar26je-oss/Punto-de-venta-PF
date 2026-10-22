using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;

namespace PuntoVenta.Api.Seguridad;

/// <summary>
/// "Permiso de un momento" que da el admin para editar o anular UN registro.
/// Es un token aparte del de inicio de sesión: dura 10 minutos y solo sirve
/// para el registro y la sección que se autorizó (por ejemplo: la venta 5).
/// </summary>
public static class TokenAutorizacion
{
    public const string Encabezado = "X-Autorizacion";
    private const string Audiencia = "PuntoVenta.Autorizacion";
    private const int Minutos = 10;

    public static string Crear(IConfiguration config, string recurso, int idRegistro)
    {
        var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var credenciales = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: Audiencia,
            claims: new[]
            {
                new Claim("recurso", recurso.ToLowerInvariant()),
                new Claim("id_registro", idRegistro.ToString())
            },
            expires: DateTime.UtcNow.AddMinutes(Minutos),
            signingCredentials: credenciales);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static bool EsValido(IConfiguration config, string? token, string recurso, int idRegistro)
    {
        if (string.IsNullOrWhiteSpace(token)) return false;

        try
        {
            var manejador = new JwtSecurityTokenHandler { MapInboundClaims = false };
            var principal = manejador.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = config["Jwt:Issuer"],
                ValidAudience = Audiencia,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!)),
                ClockSkew = TimeSpan.Zero
            }, out _);

            return principal.FindFirst("recurso")?.Value == recurso.ToLowerInvariant()
                && principal.FindFirst("id_registro")?.Value == idRegistro.ToString();
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// Para editar o anular se necesita la autorización del admin.
/// Si quien lo hace ES el admin, pasa directo.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class RequiereAutorizacionAdminAttribute : Attribute, IAuthorizationFilter
{
    public RequiereAutorizacionAdminAttribute(string recurso)
    {
        Recurso = recurso;
    }

    /// <summary>Por ejemplo "ventas" o "compras".</summary>
    public string Recurso { get; }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var usuario = context.HttpContext.User;
        if (usuario.Identity?.IsAuthenticated != true) return;
        if (UsuarioActual.EsAdmin(usuario)) return;

        var config = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var token = context.HttpContext.Request.Headers[TokenAutorizacion.Encabezado].ToString();

        var idTexto = context.RouteData.Values["id"]?.ToString();
        var idValido = int.TryParse(idTexto, out var idRegistro);

        if (!idValido || !TokenAutorizacion.EsValido(config, token, Recurso, idRegistro))
        {
            context.Result = new ObjectResult(new { mensaje = "Se necesita la autorización del administrador." })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}
