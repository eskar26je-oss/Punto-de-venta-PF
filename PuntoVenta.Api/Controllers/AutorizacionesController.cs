using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Data;

namespace PuntoVenta.Api.Controllers;

/// <summary>
/// El admin escribe su usuario y contraseña para autorizar que otro usuario
/// edite o anule un registro (por ejemplo, una venta).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AutorizacionesController : ControllerBase
{
    // Secciones donde se pide autorización para editar o eliminar
    private static readonly string[] RecursosPermitidos =
    {
        "productos", "marcas", "ventas", "clientes", "empleados",
        "puestos", "compras", "proveedores"
    };

    private readonly PuntoVentaContext _db;
    private readonly IConfiguration _config;

    public AutorizacionesController(PuntoVentaContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [HttpPost]
    public async Task<IActionResult> Post(SolicitudAutorizacion datos)
    {
        var recurso = datos.Recurso.Trim().ToLowerInvariant();

        if (!RecursosPermitidos.Contains(recurso) || datos.IdRegistro <= 0)
            return BadRequest(new { mensaje = "Solicitud de autorización no válida." });

        if (!string.Equals(datos.Usuario.Trim(), UsuarioActual.NombreAdmin, StringComparison.OrdinalIgnoreCase))
            return Unauthorized(new { mensaje = "Solo el administrador puede autorizar." });

        var admin = await _db.Usuarios.FirstOrDefaultAsync(u => u.Usuario1 == UsuarioActual.NombreAdmin);

        if (admin == null || admin.Activo == 0 || !BCrypt.Net.BCrypt.Verify(datos.Contrasena, admin.PasswordHash))
            return Unauthorized(new { mensaje = "Usuario o contraseña del administrador incorrectos." });

        var token = TokenAutorizacion.Crear(_config, recurso, datos.IdRegistro);
        return Ok(new { token });
    }
}

public class SolicitudAutorizacion
{
    [Required(ErrorMessage = "Escribe el usuario del administrador")]
    public string Usuario { get; set; } = "";

    [Required(ErrorMessage = "Escribe la contraseña del administrador")]
    public string Contrasena { get; set; } = "";

    [Required]
    public string Recurso { get; set; } = "";

    public int IdRegistro { get; set; }
}
