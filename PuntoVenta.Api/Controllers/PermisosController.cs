using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Data;

namespace PuntoVenta.Api.Controllers;

/// <summary>
/// Le dice a la página web qué puede ver y editar el usuario que inició sesión.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PermisosController : ControllerBase
{
    private readonly PuntoVentaContext _db;

    public PermisosController(PuntoVentaContext db)
    {
        _db = db;
    }

    [HttpGet("mios")]
    public async Task<ActionResult<MisPermisos>> Mios()
    {
        var id = UsuarioActual.Id(User);
        if (id is null) return Unauthorized();

        // Todas las opciones del menú que se controlan con permisos
        var modulos = await _db.Menus
            .Where(m => m.Activo == 1 && m.Ruta != null)
            .Select(m => m.Ruta!)
            .ToListAsync();

        if (UsuarioActual.EsAdmin(User))
        {
            return new MisPermisos(true, modulos, new List<PermisoRuta>());
        }

        var permisos = await _db.Permisos
            .Where(p => p.IdUsuario == id)
            .Select(p => new PermisoRuta(
                p.IdMenuNavigation.Ruta ?? "",
                p.PuedeVer == 1 || p.PuedeEditar == 1,
                p.PuedeEditar == 1))
            .ToListAsync();

        return new MisPermisos(false, modulos, permisos);
    }
}

public record PermisoRuta(string Ruta, bool PuedeVer, bool PuedeEditar);

public record MisPermisos(bool EsAdmin, List<string> Modulos, List<PermisoRuta> Permisos);
