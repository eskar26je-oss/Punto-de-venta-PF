using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Data;
using PuntoVenta.Api.Models;

namespace PuntoVenta.Api.Controllers;

/// <summary>
/// Administración de usuarios y sus permisos. Solo el usuario admin puede usarlo.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[SoloAdmin]
public class UsuariosController : ControllerBase
{
    private readonly PuntoVentaContext _db;

    public UsuariosController(PuntoVentaContext db)
    {
        _db = db;
    }

    // Lista de usuarios
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UsuarioResumen>>> Get()
    {
        var usuarios = await _db.Usuarios
            .OrderBy(u => u.IdUsuario)
            .Select(u => new { u.IdUsuario, u.Usuario1, u.Activo, u.FechaIngreso })
            .ToListAsync();

        var permisos = await _db.Permisos
            .Select(p => new { p.IdUsuario, p.PuedeVer, p.PuedeEditar })
            .ToListAsync();

        return usuarios.Select(u => new UsuarioResumen(
            u.IdUsuario,
            u.Usuario1,
            u.Activo == 1,
            EsAdmin(u.Usuario1),
            u.FechaIngreso,
            permisos.Count(p => p.IdUsuario == u.IdUsuario && (p.PuedeVer == 1 || p.PuedeEditar == 1)),
            permisos.Count(p => p.IdUsuario == u.IdUsuario && p.PuedeEditar == 1)))
            .ToList();
    }

    // Opciones del menú a las que se les puede dar permiso (en orden, con su nivel)
    [HttpGet("modulos")]
    public async Task<ActionResult<IEnumerable<ModuloDto>>> Modulos()
    {
        return await ObtenerModulos();
    }

    // Un usuario con sus permisos
    [HttpGet("{id:int}")]
    public async Task<ActionResult<UsuarioDetalle>> GetPorId(int id)
    {
        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario == null) return NotFound(new { mensaje = "El usuario no existe." });

        var permisos = await _db.Permisos
            .Where(p => p.IdUsuario == id)
            .Select(p => new PermisoDto(p.IdMenu, p.PuedeVer == 1 || p.PuedeEditar == 1, p.PuedeEditar == 1))
            .ToListAsync();

        return new UsuarioDetalle(usuario.IdUsuario, usuario.Usuario1, usuario.Activo == 1, EsAdmin(usuario.Usuario1), permisos);
    }

    // Crear usuario
    [HttpPost]
    public async Task<IActionResult> Post(UsuarioGuardar datos)
    {
        var nombre = datos.Usuario.Trim();

        if (string.IsNullOrWhiteSpace(datos.Contrasena) || datos.Contrasena.Length < 6)
            return BadRequest(new { mensaje = "La contraseña debe tener al menos 6 caracteres." });

        if (await _db.Usuarios.AnyAsync(u => u.Usuario1 == nombre))
            return Conflict(new { mensaje = "Ya existe un usuario con ese nombre." });

        var usuario = new Usuario
        {
            Usuario1 = nombre,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(datos.Contrasena),
            Activo = datos.Activo ? 1UL : 0UL,
            FechaIngreso = DateTime.Now
        };

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();

        await GuardarPermisos(usuario.IdUsuario, datos.Permisos);

        return Ok(new { idUsuario = usuario.IdUsuario, mensaje = "Usuario creado." });
    }

    // Editar usuario (si la contraseña viene vacía, no se cambia)
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, UsuarioGuardar datos)
    {
        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario == null) return NotFound(new { mensaje = "El usuario no existe." });

        var nombre = datos.Usuario.Trim();
        var esAdmin = EsAdmin(usuario.Usuario1);

        if (!string.IsNullOrEmpty(datos.Contrasena))
        {
            if (datos.Contrasena.Length < 6)
                return BadRequest(new { mensaje = "La contraseña debe tener al menos 6 caracteres." });

            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(datos.Contrasena);
        }

        if (esAdmin)
        {
            // Al admin solo se le puede cambiar la contraseña: siempre tiene todos los permisos.
            await _db.SaveChangesAsync();
            return Ok(new { mensaje = "Usuario actualizado." });
        }

        if (!string.Equals(nombre, usuario.Usuario1, StringComparison.OrdinalIgnoreCase)
            && await _db.Usuarios.AnyAsync(u => u.Usuario1 == nombre && u.IdUsuario != id))
        {
            return Conflict(new { mensaje = "Ya existe un usuario con ese nombre." });
        }

        if (EsAdmin(nombre))
            return BadRequest(new { mensaje = "Ese nombre está reservado para el administrador." });

        usuario.Usuario1 = nombre;
        usuario.Activo = datos.Activo ? 1UL : 0UL;
        await _db.SaveChangesAsync();

        await GuardarPermisos(id, datos.Permisos);

        return Ok(new { mensaje = "Usuario actualizado." });
    }

    // ---------- Ayudantes ----------

    private static bool EsAdmin(string nombre) =>
        string.Equals(nombre, UsuarioActual.NombreAdmin, StringComparison.OrdinalIgnoreCase);

    private async Task<List<ModuloDto>> ObtenerModulos()
    {
        var opciones = await _db.Menus
            .AsNoTracking()
            .Where(m => m.Activo == 1 && m.Ruta != UsuarioActual.RutaUsuarios)
            .OrderBy(m => m.Orden)
            .ToListAsync();

        var resultado = new List<ModuloDto>();

        void Agregar(int? idPadre, int nivel)
        {
            foreach (var o in opciones.Where(o => o.IdPadre == idPadre))
            {
                resultado.Add(new ModuloDto(
                    o.IdMenu,
                    o.Nombre.Trim(),
                    o.Ruta,
                    nivel,
                    // Los reportes solo se consultan, no se editan
                    o.Ruta != UsuarioActual.RutaReportes));

                Agregar(o.IdMenu, nivel + 1);
            }
        }

        Agregar(null, 0);
        return resultado;
    }

    /// <summary>Reemplaza todos los permisos del usuario por los nuevos.</summary>
    private async Task GuardarPermisos(int idUsuario, List<PermisoDto> permisos)
    {
        var modulos = await ObtenerModulos();

        var anteriores = await _db.Permisos.Where(p => p.IdUsuario == idUsuario).ToListAsync();
        _db.Permisos.RemoveRange(anteriores);

        foreach (var p in permisos)
        {
            var modulo = modulos.FirstOrDefault(m => m.IdMenu == p.IdMenu);
            if (modulo == null) continue;

            var editar = p.PuedeEditar && modulo.PermiteEditar;
            var ver = p.PuedeVer || editar; // si puede editar, también puede ver

            if (!ver) continue;

            _db.Permisos.Add(new Permiso
            {
                IdUsuario = idUsuario,
                IdMenu = p.IdMenu,
                PuedeVer = ver ? 1UL : 0UL,
                PuedeEditar = editar ? 1UL : 0UL
            });
        }

        await _db.SaveChangesAsync();
    }
}

// ---------- Datos que se envían y reciben ----------

public record UsuarioResumen(int IdUsuario, string Usuario, bool Activo, bool EsAdmin, DateTime FechaIngreso,
    int OpcionesQueVe, int OpcionesQueEdita);

public record UsuarioDetalle(int IdUsuario, string Usuario, bool Activo, bool EsAdmin, List<PermisoDto> Permisos);

public record PermisoDto(int IdMenu, bool PuedeVer, bool PuedeEditar);

public record ModuloDto(int IdMenu, string Nombre, string? Ruta, int Nivel, bool PermiteEditar);

public class UsuarioGuardar
{
    [Required(ErrorMessage = "El usuario es obligatorio")]
    [StringLength(30, MinimumLength = 3, ErrorMessage = "El usuario debe tener entre 3 y 30 caracteres")]
    public string Usuario { get; set; } = "";

    [StringLength(50, ErrorMessage = "Máximo 50 caracteres")]
    public string? Contrasena { get; set; }

    public bool Activo { get; set; } = true;

    public List<PermisoDto> Permisos { get; set; } = new();
}
