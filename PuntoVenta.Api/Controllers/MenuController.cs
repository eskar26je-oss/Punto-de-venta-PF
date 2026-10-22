using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Data;

namespace PuntoVenta.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MenuController : ControllerBase
{
    private readonly PuntoVentaContext _db;

    public MenuController(PuntoVentaContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Devuelve solo las opciones del menú que el usuario puede ver.
    /// El admin ve todo, incluida la opción "Usuarios".
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MenuDto>>> Get()
    {
        var opciones = await _db.Menus
            .AsNoTracking()
            .Where(m => m.Activo == 1)
            .OrderBy(m => m.Orden)
            .ToListAsync();

        HashSet<int> visibles;

        if (UsuarioActual.EsAdmin(User))
        {
            visibles = opciones.Select(o => o.IdMenu).ToHashSet();
        }
        else
        {
            var id = UsuarioActual.Id(User);

            var permitidos = await _db.Permisos
                .Where(p => p.IdUsuario == id && (p.PuedeVer == 1 || p.PuedeEditar == 1))
                .Select(p => p.IdMenu)
                .ToListAsync();

            visibles = permitidos.ToHashSet();

            // La opción "Usuarios" es solo para el admin
            visibles.RemoveWhere(idMenu =>
                opciones.Any(o => o.IdMenu == idMenu && o.Ruta == UsuarioActual.RutaUsuarios));
        }

        // Arma el árbol. Si el usuario no puede ver una opción "padre" pero sí alguna "hija",
        // el padre aparece sin enlace para que el menú no se rompa.
        List<MenuDto> Construir(int? idPadre) =>
            opciones
                .Where(o => o.IdPadre == idPadre)
                .Select(o => new { Opcion = o, Hijos = Construir(o.IdMenu) })
                .Where(x => visibles.Contains(x.Opcion.IdMenu) || x.Hijos.Any())
                .Select(x => new MenuDto(
                    x.Opcion.IdMenu,
                    x.Opcion.Nombre.Trim(),
                    visibles.Contains(x.Opcion.IdMenu) ? x.Opcion.Ruta : null,
                    x.Opcion.Orden,
                    x.Hijos))
                .ToList();

        return Construir(null);
    }
}

public record MenuDto(int IdMenu, string Nombre, string? Ruta, int Orden, List<MenuDto> Hijos);
