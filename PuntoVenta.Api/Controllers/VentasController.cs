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
public class VentasController : ControllerBase
{
    private const string SerieDefecto = "A";
    private readonly PuntoVentaContext _db;

    public VentasController(PuntoVentaContext db)
    {
        _db = db;
    }

    // ===================== Consultas =====================

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VentaResumenDto>>> Get(
        [FromQuery] string? buscar, [FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta)
    {
        var q = _db.Ventas.AsNoTracking().AsQueryable();

        if (desde.HasValue) q = q.Where(v => v.FechaFactura >= desde.Value);
        if (hasta.HasValue) q = q.Where(v => v.FechaFactura <= hasta.Value);
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var t = buscar.Trim();
            q = q.Where(v => v.IdClienteNavigation.Nombres.Contains(t)
                          || v.IdClienteNavigation.Apellidos.Contains(t)
                          || v.NoFactura.ToString().Contains(t));
        }

        var lista = await q
            .OrderByDescending(v => v.FechaFactura).ThenByDescending(v => v.IdVenta)
            .Select(v => new VentaResumenDto(
                v.IdVenta, v.Serie, v.NoFactura, v.FechaFactura,
                v.IdClienteNavigation.Nombres + " " + v.IdClienteNavigation.Apellidos,
                v.IdEmpleadoNavigation.Nombres + " " + v.IdEmpleadoNavigation.Apellidos,
                v.VentasDetalles.Sum(d => d.Cantidad * d.PrecioUnitario)))
            .ToListAsync();

        return Ok(lista);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<VentaDto>> GetPorId(int id)
    {
        var dto = await ObtenerDto(id);
        return dto == null ? NotFound() : Ok(dto);
    }

    // ===================== Combos de apoyo (solo lectura) =====================

    [HttpGet("clientes")]
    public async Task<ActionResult<IEnumerable<OpcionDto>>> GetClientes()
    {
        return Ok(await _db.Clientes.AsNoTracking()
            .OrderBy(c => c.Nombres).ThenBy(c => c.Apellidos)
            .Select(c => new OpcionDto(c.IdCliente, c.Nombres + " " + c.Apellidos, c.Nit))
            .ToListAsync());
    }

    [HttpGet("empleados")]
    public async Task<ActionResult<IEnumerable<OpcionDto>>> GetEmpleados()
    {
        return Ok(await _db.Empleados.AsNoTracking()
            .OrderBy(e => e.Nombres).ThenBy(e => e.Apellidos)
            .Select(e => new OpcionDto(e.IdEmpleado, e.Nombres + " " + e.Apellidos, null))
            .ToListAsync());
    }

    [HttpGet("productos")]
    public async Task<ActionResult<IEnumerable<ProductoVentaDto>>> GetProductos()
    {
        return Ok(await _db.Productos.AsNoTracking()
            .OrderBy(p => p.Producto1)
            .Select(p => new ProductoVentaDto(p.IdProducto, p.Producto1, p.PrecioVenta, p.Existencia, p.Imagen))
            .ToListAsync());
    }

    // ===================== Crear / modificar / anular =====================

    [HttpPost]
    public async Task<ActionResult<VentaDto>> Post(VentaCrearDto dto)
    {
        var error = await ValidarCabecera(dto);
        if (error != null) return error;

        // Si algo falla antes del Commit, al salir se hace rollback automático.
        await using var tx = await _db.Database.BeginTransactionAsync();

        var venta = new Venta
        {
            Serie = SerieDefecto,
            FechaFactura = dto.FechaFactura,
            IdCliente = dto.IdCliente,
            IdEmpleado = dto.IdEmpleado
        };

        var errorDetalle = await AplicarDetalles(venta, dto.Detalles);
        if (errorDetalle != null) return errorDetalle;

        var ultimo = await _db.Ventas.Where(v => v.Serie == SerieDefecto)
            .MaxAsync(v => (int?)v.NoFactura) ?? 0;
        venta.NoFactura = ultimo + 1;

        _db.Ventas.Add(venta);
        try
        {
            await _db.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(new { mensaje = "No se pudo registrar la venta (número de factura duplicado). Intente de nuevo." });
        }

        return CreatedAtAction(nameof(GetPorId), new { id = venta.IdVenta }, await ObtenerDto(venta.IdVenta));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, VentaCrearDto dto)
    {
        var error = await ValidarCabecera(dto);
        if (error != null) return error;

        await using var tx = await _db.Database.BeginTransactionAsync();

        var venta = await _db.Ventas.Include(v => v.VentasDetalles)
            .FirstOrDefaultAsync(v => v.IdVenta == id);
        if (venta == null) return NotFound();

        // 1) Devolver al inventario lo que tenía la venta, 2) quitar sus líneas,
        // 3) aplicar las líneas nuevas validando existencia otra vez.
        await DevolverExistencia(venta.VentasDetalles);
        _db.VentasDetalles.RemoveRange(venta.VentasDetalles.ToList());
        await _db.SaveChangesAsync();

        venta.FechaFactura = dto.FechaFactura;
        venta.IdCliente = dto.IdCliente;
        venta.IdEmpleado = dto.IdEmpleado;

        var errorDetalle = await AplicarDetalles(venta, dto.Detalles);
        if (errorDetalle != null) return errorDetalle;

        await _db.SaveChangesAsync();
        await tx.CommitAsync();
        return NoContent();
    }

    // Anula la venta y devuelve la existencia a los productos.
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await using var tx = await _db.Database.BeginTransactionAsync();

        var venta = await _db.Ventas.Include(v => v.VentasDetalles)
            .FirstOrDefaultAsync(v => v.IdVenta == id);
        if (venta == null) return NotFound();

        await DevolverExistencia(venta.VentasDetalles);
        _db.VentasDetalles.RemoveRange(venta.VentasDetalles.ToList());
        _db.Ventas.Remove(venta);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();
        return NoContent();
    }

    // ===================== Lógica interna =====================

    private async Task<ActionResult?> ValidarCabecera(VentaCrearDto dto)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Now);
        if (dto.FechaFactura == default)
            return BadRequest(new { mensaje = "La fecha de la venta es obligatoria." });
        if (dto.FechaFactura > hoy)
            return BadRequest(new { mensaje = "La fecha de la venta no puede ser futura." });

        if (!await _db.Clientes.AnyAsync(c => c.IdCliente == dto.IdCliente))
            return BadRequest(new { mensaje = "El cliente seleccionado no existe." });
        if (!await _db.Empleados.AnyAsync(e => e.IdEmpleado == dto.IdEmpleado))
            return BadRequest(new { mensaje = "El empleado seleccionado no existe." });

        return null;
    }

    // Valida existencia, descuenta de forma atómica y agrega las líneas a la venta.
    // El precio sale SIEMPRE de productos.precio_venta.
    private async Task<ActionResult?> AplicarDetalles(Venta venta, IEnumerable<VentaDetalleCrearDto> detalles)
    {
        // Si el mismo producto viene en varias líneas, se suman.
        var agrupados = detalles
            .GroupBy(d => d.IdProducto)
            .Select(g => new { IdProducto = g.Key, Cantidad = g.Sum(x => x.Cantidad) })
            .ToList();

        var ids = agrupados.Select(a => a.IdProducto).ToList();
        var productos = await _db.Productos.AsNoTracking()
            .Where(p => ids.Contains(p.IdProducto))
            .ToDictionaryAsync(p => p.IdProducto);

        foreach (var item in agrupados)
        {
            if (!productos.TryGetValue(item.IdProducto, out var prod))
                return BadRequest(new { mensaje = $"El producto {item.IdProducto} no existe." });

            if (prod.PrecioVenta <= 0)
                return BadRequest(new { mensaje = $"El producto «{prod.Producto1}» no tiene un precio de venta válido." });

            var cantidad = item.Cantidad;
            var idProducto = item.IdProducto;

            // UPDATE ... WHERE existencia >= cantidad: evita vender de más aunque
            // dos cajeros vendan el mismo producto al mismo tiempo.
            var filas = await _db.Productos
                .Where(p => p.IdProducto == idProducto && p.Existencia >= cantidad)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Existencia, p => p.Existencia - cantidad));

            if (filas == 0)
            {
                var disponible = await _db.Productos.Where(p => p.IdProducto == idProducto)
                    .Select(p => p.Existencia).FirstOrDefaultAsync();
                return Conflict(new
                {
                    mensaje = $"No se puede realizar la venta. Existencia insuficiente de «{prod.Producto1}». " +
                              $"Disponible: {disponible}, solicitado: {cantidad}."
                });
            }

            venta.VentasDetalles.Add(new VentasDetalle
            {
                IdProducto = idProducto,
                Cantidad = cantidad,
                PrecioUnitario = prod.PrecioVenta
            });
        }

        return null;
    }

    private async Task DevolverExistencia(IEnumerable<VentasDetalle> detalles)
    {
        var porProducto = detalles
            .GroupBy(d => d.IdProducto)
            .Select(g => new { IdProducto = g.Key, Cantidad = g.Sum(x => x.Cantidad) });

        foreach (var item in porProducto)
        {
            var idProducto = item.IdProducto;
            var cantidad = item.Cantidad;
            await _db.Productos.Where(p => p.IdProducto == idProducto)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Existencia, p => p.Existencia + cantidad));
        }
    }

    private Task<VentaDto?> ObtenerDto(int id)
    {
        return _db.Ventas.AsNoTracking()
            .Where(v => v.IdVenta == id)
            .Select(v => new VentaDto(
                v.IdVenta, v.Serie, v.NoFactura, v.FechaFactura,
                v.IdCliente, v.IdClienteNavigation.Nombres + " " + v.IdClienteNavigation.Apellidos,
                v.IdEmpleado, v.IdEmpleadoNavigation.Nombres + " " + v.IdEmpleadoNavigation.Apellidos,
                v.VentasDetalles.Sum(d => d.Cantidad * d.PrecioUnitario),
                v.VentasDetalles.Select(d => new VentaDetalleDto(
                    d.IdVentaDetalle, d.IdProducto, d.IdProductoNavigation.Producto1,
                    d.Cantidad, d.PrecioUnitario, d.Cantidad * d.PrecioUnitario)).ToList()))
            .FirstOrDefaultAsync();
    }
}
