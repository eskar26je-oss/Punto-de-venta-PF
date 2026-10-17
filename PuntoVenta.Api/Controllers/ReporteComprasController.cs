using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Data;
using PuntoVenta.Api.Dtos;

namespace PuntoVenta.Api.Controllers;

[ApiController]
[Route("api/reportes/compras")]
[Authorize]
public class ReporteComprasController : ControllerBase
{
    private readonly PuntoVentaContext _db;

    public ReporteComprasController(PuntoVentaContext db)
    {
        _db = db;
    }

    // Fecha | No. orden | Proveedor | Productos | Unidades | Total compra, con filtros opcionales.
    [HttpGet]
    public async Task<ActionResult<ReporteComprasDto>> Get(
        [FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta, [FromQuery] int? idProveedor)
    {
        if (desde.HasValue && hasta.HasValue && desde > hasta)
            return BadRequest(new { mensaje = "La fecha inicial no puede ser mayor que la fecha final." });

        var q = _db.Compras.AsNoTracking().AsQueryable();
        if (desde.HasValue) q = q.Where(c => c.FechaOrden >= desde.Value);
        if (hasta.HasValue) q = q.Where(c => c.FechaOrden <= hasta.Value);
        if (idProveedor.HasValue) q = q.Where(c => c.IdProveedor == idProveedor.Value);

        var filas = await q
            .OrderByDescending(c => c.FechaOrden).ThenByDescending(c => c.IdCompra)
            .Select(c => new ReporteCompraFilaDto(
                c.FechaOrden, c.NoOrdenCompra,
                c.IdProveedorNavigation.Proveedor,
                c.ComprasDetalles.Sum(d => d.Cantidad),
                c.ComprasDetalles.Sum(d => d.Cantidad * d.PrecioCostoUnitario),
                c.ComprasDetalles.Select(d => new ReporteCompraProductoDto(
                    d.IdProductoNavigation.Producto1, d.Cantidad,
                    d.PrecioCostoUnitario, d.Cantidad * d.PrecioCostoUnitario)).ToList()))
            .ToListAsync();

        return Ok(new ReporteComprasDto(
            desde, hasta,
            filas.Count,
            filas.Sum(f => f.Unidades),
            filas.Sum(f => f.TotalCompra),
            filas));
    }
}
