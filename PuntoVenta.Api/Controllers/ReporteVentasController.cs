using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Data;
using PuntoVenta.Api.Dtos;

namespace PuntoVenta.Api.Controllers;

[ApiController]
[Route("api/reportes/ventas")]
[Authorize]
public class ReporteVentasController : ControllerBase
{
    private readonly PuntoVentaContext _db;

    public ReporteVentasController(PuntoVentaContext db)
    {
        _db = db;
    }

    // Fecha | Cliente | Empleado | Total Venta, con filtros opcionales.
    [HttpGet]
    public async Task<ActionResult<ReporteVentasDto>> Get(
        [FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta,
        [FromQuery] int? idCliente, [FromQuery] int? idEmpleado)
    {
        if (desde.HasValue && hasta.HasValue && desde > hasta)
            return BadRequest(new { mensaje = "La fecha inicial no puede ser mayor que la fecha final." });

        var q = _db.Ventas.AsNoTracking().AsQueryable();
        if (desde.HasValue) q = q.Where(v => v.FechaFactura >= desde.Value);
        if (hasta.HasValue) q = q.Where(v => v.FechaFactura <= hasta.Value);
        if (idCliente.HasValue) q = q.Where(v => v.IdCliente == idCliente.Value);
        if (idEmpleado.HasValue) q = q.Where(v => v.IdEmpleado == idEmpleado.Value);

        var filas = await q
            .OrderByDescending(v => v.FechaFactura).ThenByDescending(v => v.IdVenta)
            .Select(v => new ReporteVentaFilaDto(
                v.FechaFactura, v.Serie, v.NoFactura,
                v.IdClienteNavigation.Nombres + " " + v.IdClienteNavigation.Apellidos,
                v.IdEmpleadoNavigation.Nombres + " " + v.IdEmpleadoNavigation.Apellidos,
                v.VentasDetalles.Sum(d => d.Cantidad * d.PrecioUnitario)))
            .ToListAsync();

        return Ok(new ReporteVentasDto(desde, hasta, filas.Count, filas.Sum(f => f.TotalVenta), filas));
    }
}
