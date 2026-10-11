using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Data;
using PuntoVenta.Api.Models;

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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Menu>>> Get()
    {
        var opciones = await _db.Menus
            .Where(m => m.Activo == 1)
            .OrderBy(m => m.Orden)
            .ToListAsync();

        return opciones.Where(m => m.IdPadre == null).ToList();
    }
}