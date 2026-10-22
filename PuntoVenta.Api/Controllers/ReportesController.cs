using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Data;

namespace PuntoVenta.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
	[Permiso("/reportes")]
public class ReportesController : ControllerBase
{
    private readonly PuntoVentaContext _db;

    public ReportesController(PuntoVentaContext db)
    {
        _db = db;
    }

    [HttpGet("productos")]
    public async Task<ActionResult<IEnumerable<ReporteProductoDto>>> Productos()
    {
        return await _db.Productos
            .OrderBy(p => p.Producto1)
            .Select(p => new ReporteProductoDto
            {
                IdProducto = p.IdProducto,
                Producto = p.Producto1,
                Marca = p.IdMarcaNavigation.Marca1,
                Existencia = p.Existencia,
                PrecioCosto = p.PrecioCosto,
                PrecioVenta = p.PrecioVenta
            })
            .ToListAsync();
    }

    [HttpGet("existencias")]
    public async Task<ActionResult<IEnumerable<ReporteExistenciaDto>>> Existencias()
    {
        return await _db.Productos
            .OrderBy(p => p.Existencia)
            .ThenBy(p => p.Producto1)
            .Select(p => new ReporteExistenciaDto
            {
                IdProducto = p.IdProducto,
                Producto = p.Producto1,
                Marca = p.IdMarcaNavigation.Marca1,
                Comprado = p.ComprasDetalles.Sum(c => c.Cantidad),
                Vendido = p.VentasDetalles.Sum(v => v.Cantidad),
                Existencia = p.Existencia,
                PrecioCosto = p.PrecioCosto
            })
            .ToListAsync();
    }
}

public class ReporteProductoDto
{
    public int IdProducto { get; set; }
    public string Producto { get; set; } = "";
    public string Marca { get; set; } = "";
    public int Existencia { get; set; }
    public decimal PrecioCosto { get; set; }
    public decimal PrecioVenta { get; set; }
}

public class ReporteExistenciaDto
{
    public int IdProducto { get; set; }
    public string Producto { get; set; } = "";
    public string Marca { get; set; } = "";
    public int Comprado { get; set; }
    public int Vendido { get; set; }
    public int Existencia { get; set; }
    public decimal PrecioCosto { get; set; }
}