using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Data;
using PuntoVenta.Api.Dtos;
using PuntoVenta.Api.Models;

namespace PuntoVenta.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
	[Permiso("/compras")]
public class ComprasController : ControllerBase
{
    // true  = al guardar una compra, productos.precio_costo pasa a ser el costo de esa compra.
    // false = la compra solo suma existencia y no toca el precio de costo del producto.
    private static readonly bool ActualizarPrecioCosto = true;

    private readonly PuntoVentaContext _db;

    public ComprasController(PuntoVentaContext db)
    {
        _db = db;
    }

    // ===================== Consultas =====================

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CompraResumenDto>>> Get(
        [FromQuery] string? buscar, [FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta)
    {
        var q = _db.Compras.AsNoTracking().AsQueryable();

        if (desde.HasValue) q = q.Where(c => c.FechaOrden >= desde.Value);
        if (hasta.HasValue) q = q.Where(c => c.FechaOrden <= hasta.Value);
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var t = buscar.Trim();
            q = q.Where(c => c.IdProveedorNavigation.Proveedor.Contains(t)
                          || c.NoOrdenCompra.ToString().Contains(t));
        }

        var lista = await q
            .OrderByDescending(c => c.FechaOrden).ThenByDescending(c => c.IdCompra)
            .Select(c => new CompraResumenDto(
                c.IdCompra, c.NoOrdenCompra, c.FechaOrden,
                c.IdProveedorNavigation.Proveedor,
                c.ComprasDetalles.Sum(d => d.Cantidad),
                c.ComprasDetalles.Sum(d => d.Cantidad * d.PrecioCostoUnitario)))
            .ToListAsync();

        return Ok(lista);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CompraDto>> GetPorId(int id)
    {
        var dto = await ObtenerDto(id);
        return dto == null ? NotFound() : Ok(dto);
    }

    // ===================== Combos de apoyo (solo lectura) =====================

    [HttpGet("proveedores")]
    public async Task<ActionResult<IEnumerable<OpcionDto>>> GetProveedores()
    {
        return Ok(await _db.Proveedores.AsNoTracking()
            .OrderBy(p => p.Proveedor)
            .Select(p => new OpcionDto(p.IdProveedor, p.Proveedor, p.Nit))
            .ToListAsync());
    }

    [HttpGet("productos")]
    public async Task<ActionResult<IEnumerable<ProductoCompraDto>>> GetProductos()
    {
        return Ok(await _db.Productos.AsNoTracking()
            .OrderBy(p => p.Producto1)
            .Select(p => new ProductoCompraDto(p.IdProducto, p.Producto1, p.PrecioCosto, p.PrecioVenta, p.Existencia))
            .ToListAsync());
    }

    // ===================== Crear / modificar / anular =====================

    [HttpPost]
    public async Task<ActionResult<CompraDto>> Post(CompraCrearDto dto)
    {
        var error = await ValidarCabecera(dto);
        if (error != null) return error;

        var lineas = Agrupar(dto.Detalles);
        var errorProductos = await ValidarProductos(lineas);
        if (errorProductos != null) return errorProductos;

        // Si algo falla antes del Commit, al salir se hace rollback automático.
        await using var tx = await _db.Database.BeginTransactionAsync();

        var compra = new Compra
        {
            FechaOrden = dto.FechaOrden,
            IdProveedor = dto.IdProveedor
        };

        foreach (var l in lineas)
        {
            compra.ComprasDetalles.Add(new ComprasDetalle
            {
                IdProducto = l.IdProducto,
                Cantidad = l.Cantidad,
                PrecioCostoUnitario = l.PrecioCostoUnitario
            });
        }

        // La compra SUMA existencia a cada producto.
        var cambios = lineas.ToDictionary(l => l.IdProducto, l => l.Cantidad);
        await AjustarExistencia(cambios, "registrar");
        await AplicarPrecioCosto(lineas);

        // Número de orden correlativo (último + 1).
        var ultimo = await _db.Compras.MaxAsync(c => (int?)c.NoOrdenCompra) ?? 0;
        compra.NoOrdenCompra = ultimo + 1;

        _db.Compras.Add(compra);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return CreatedAtAction(nameof(GetPorId), new { id = compra.IdCompra }, await ObtenerDto(compra.IdCompra));
    }
[RequiereAutorizacionAdmin("compras")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, CompraCrearDto dto)
    {
        var error = await ValidarCabecera(dto);
        if (error != null) return error;

        var lineas = Agrupar(dto.Detalles);
        var errorProductos = await ValidarProductos(lineas);
        if (errorProductos != null) return errorProductos;

        await using var tx = await _db.Database.BeginTransactionAsync();

        var compra = await _db.Compras.Include(c => c.ComprasDetalles)
            .FirstOrDefaultAsync(c => c.IdCompra == id);
        if (compra == null) return NotFound();

        // Diferencia por producto = cantidad nueva - cantidad anterior.
        // Si la diferencia es negativa se descuenta de la existencia (validando que alcance).
        var cambios = new Dictionary<int, int>();
        foreach (var d in compra.ComprasDetalles)
            cambios[d.IdProducto] = cambios.GetValueOrDefault(d.IdProducto) - d.Cantidad;
        foreach (var l in lineas)
            cambios[l.IdProducto] = cambios.GetValueOrDefault(l.IdProducto) + l.Cantidad;

        var errorExistencia = await AjustarExistencia(cambios, "modificar");
        if (errorExistencia != null) return errorExistencia;

        await AplicarPrecioCosto(lineas);

        // Reemplazar las líneas de la compra.
        _db.ComprasDetalles.RemoveRange(compra.ComprasDetalles.ToList());
        await _db.SaveChangesAsync();

        compra.FechaOrden = dto.FechaOrden;
        compra.IdProveedor = dto.IdProveedor;
        foreach (var l in lineas)
        {
            compra.ComprasDetalles.Add(new ComprasDetalle
            {
                IdProducto = l.IdProducto,
                Cantidad = l.Cantidad,
                PrecioCostoUnitario = l.PrecioCostoUnitario
            });
        }

        await _db.SaveChangesAsync();
        await tx.CommitAsync();
        return NoContent();
    }

    // Anula la compra y descuenta del inventario lo que había ingresado.
    // Si ya se vendió parte de esa mercadería y no alcanza la existencia, no se permite.
    [RequiereAutorizacionAdmin("compras")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await using var tx = await _db.Database.BeginTransactionAsync();

        var compra = await _db.Compras.Include(c => c.ComprasDetalles)
            .FirstOrDefaultAsync(c => c.IdCompra == id);
        if (compra == null) return NotFound();

        var cambios = compra.ComprasDetalles
            .GroupBy(d => d.IdProducto)
            .ToDictionary(g => g.Key, g => -g.Sum(d => d.Cantidad));

        var errorExistencia = await AjustarExistencia(cambios, "anular");
        if (errorExistencia != null) return errorExistencia;

        _db.ComprasDetalles.RemoveRange(compra.ComprasDetalles.ToList());
        _db.Compras.Remove(compra);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();
        return NoContent();
    }

    // ===================== Lógica interna =====================

    private async Task<ActionResult?> ValidarCabecera(CompraCrearDto dto)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Now);
        if (dto.FechaOrden == default)
            return BadRequest(new { mensaje = "La fecha de la orden es obligatoria." });
        if (dto.FechaOrden > hoy)
            return BadRequest(new { mensaje = "La fecha de la orden no puede ser futura." });

        if (!await _db.Proveedores.AnyAsync(p => p.IdProveedor == dto.IdProveedor))
            return BadRequest(new { mensaje = "El proveedor seleccionado no existe." });

        if (dto.Detalles == null || dto.Detalles.Count == 0)
            return BadRequest(new { mensaje = "La compra debe tener al menos un producto." });

        if (dto.Detalles.Any(d => d.Cantidad <= 0))
            return BadRequest(new { mensaje = "La cantidad de cada producto debe ser mayor a cero." });
        if (dto.Detalles.Any(d => d.PrecioCostoUnitario <= 0))
            return BadRequest(new { mensaje = "El precio de costo de cada producto debe ser mayor a cero." });

        return null;
    }

    private async Task<ActionResult?> ValidarProductos(List<LineaCompra> lineas)
    {
        var ids = lineas.Select(l => l.IdProducto).ToList();
        var existentes = await _db.Productos.AsNoTracking()
            .Where(p => ids.Contains(p.IdProducto))
            .Select(p => p.IdProducto)
            .ToListAsync();

        var faltante = ids.Except(existentes).FirstOrDefault();
        if (faltante != 0)
            return BadRequest(new { mensaje = $"El producto {faltante} no existe." });

        return null;
    }

    // Si el mismo producto viene en varias líneas se suman las cantidades
    // y se usa el último precio de costo ingresado.
    private static List<LineaCompra> Agrupar(IEnumerable<CompraDetalleCrearDto> detalles)
    {
        return detalles
            .GroupBy(d => d.IdProducto)
            .Select(g => new LineaCompra(
                g.Key,
                g.Sum(x => x.Cantidad),
                Math.Round(g.Last().PrecioCostoUnitario, 2)))
            .ToList();
    }

    // cambio > 0 suma existencia; cambio < 0 resta solo si alcanza (UPDATE ... WHERE existencia >= cantidad).
    private async Task<ActionResult?> AjustarExistencia(Dictionary<int, int> cambios, string accion)
    {
        foreach (var item in cambios)
        {
            var idProducto = item.Key;
            var cambio = item.Value;
            if (cambio == 0) continue;

            if (cambio > 0)
            {
                await _db.Productos.Where(p => p.IdProducto == idProducto)
                    .ExecuteUpdateAsync(s => s.SetProperty(p => p.Existencia, p => p.Existencia + cambio));
                continue;
            }

            var quitar = -cambio;
            var filas = await _db.Productos
                .Where(p => p.IdProducto == idProducto && p.Existencia >= quitar)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Existencia, p => p.Existencia - quitar));

            if (filas == 0)
            {
                var prod = await _db.Productos.AsNoTracking()
                    .Where(p => p.IdProducto == idProducto)
                    .Select(p => new { p.Producto1, p.Existencia })
                    .FirstOrDefaultAsync();

                return Conflict(new
                {
                    mensaje = $"No se puede {accion} la compra. Ya se vendieron unidades de «{prod?.Producto1}». " +
                              $"Existencia actual: {prod?.Existencia}, se necesitan descontar: {quitar}."
                });
            }
        }

        return null;
    }

    private async Task AplicarPrecioCosto(List<LineaCompra> lineas)
    {
        if (!ActualizarPrecioCosto) return;

        foreach (var l in lineas)
        {
            var idProducto = l.IdProducto;
            var precio = l.PrecioCostoUnitario;
               var precioVenta = Math.Round(precio * 1.25m, 2); // costo + 25%
   await _db.Productos.Where(p => p.IdProducto == idProducto)
       .ExecuteUpdateAsync(s => s
           .SetProperty(p => p.PrecioCosto, precio)
           .SetProperty(p => p.PrecioVenta, precioVenta));
        }
    }

    private Task<CompraDto?> ObtenerDto(int id)
    {
        return _db.Compras.AsNoTracking()
            .Where(c => c.IdCompra == id)
            .Select(c => new CompraDto(
                c.IdCompra, c.NoOrdenCompra, c.FechaOrden,
                c.IdProveedor, c.IdProveedorNavigation.Proveedor,
                c.ComprasDetalles.Sum(d => d.Cantidad * d.PrecioCostoUnitario),
                c.ComprasDetalles.Select(d => new CompraDetalleDto(
                    d.IdCompraDetalle, d.IdProducto, d.IdProductoNavigation.Producto1,
                    d.Cantidad, d.PrecioCostoUnitario, d.Cantidad * d.PrecioCostoUnitario)).ToList()))
            .FirstOrDefaultAsync();
    }

    private record LineaCompra(int IdProducto, int Cantidad, decimal PrecioCostoUnitario);
}
