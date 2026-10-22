using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Data;
using PuntoVenta.Api.Models;

namespace PuntoVenta.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Permiso("/productos")]
public class ProductosController : ControllerBase
{
    private readonly PuntoVentaContext _db;

    public ProductosController(PuntoVentaContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Producto>>> Get()
    {
        return await _db.Productos.OrderBy(p => p.IdProducto).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Producto>> GetPorId(int id)
    {
        var producto = await _db.Productos.FindAsync(id);
        if (producto == null) return NotFound();

        return producto;
    }

    [HttpPost]
    public async Task<ActionResult<Producto>> Post(Producto producto)
    {
        var error = await Validar(producto, 0);
        if (error is not null) return error;

        producto.IdProducto = 0;
        _db.Productos.Add(producto);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPorId), new { id = producto.IdProducto }, producto);
    }
	[RequiereAutorizacionAdmin("productos")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, Producto producto)
    {
        if (id != producto.IdProducto) return BadRequest();

        var existente = await _db.Productos.FindAsync(id);
        if (existente == null) return NotFound();

        var error = await Validar(producto, id);
        if (error is not null) return error;

        existente.Producto1 = producto.Producto1;
        existente.IdMarca = producto.IdMarca;
        existente.Descripcion = producto.Descripcion;
        existente.Imagen = producto.Imagen;
        existente.PrecioCosto = producto.PrecioCosto;
        existente.PrecioVenta = producto.PrecioVenta;
        existente.Existencia = producto.Existencia;
        existente.FechaIngreso = producto.FechaIngreso;
        await _db.SaveChangesAsync();

        return NoContent();
    }
	[RequiereAutorizacionAdmin("productos")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var producto = await _db.Productos.FindAsync(id);
        if (producto == null) return NotFound();

        try
        {
            _db.Productos.Remove(producto);
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(new { mensaje = "No se puede eliminar: el producto tiene compras o ventas registradas." });
        }

        return NoContent();
    }

    private async Task<ActionResult?> Validar(Producto p, int id)
    {
        p.Producto1 = p.Producto1.Trim();
        p.Descripcion = string.IsNullOrWhiteSpace(p.Descripcion) ? null : p.Descripcion.Trim();
        p.Imagen = string.IsNullOrWhiteSpace(p.Imagen) ? null : p.Imagen.Trim();

        if (p.Producto1.Length == 0)
            return BadRequest(new { mensaje = "El nombre del producto es obligatorio." });

        if (!await _db.Marcas.AnyAsync(m => m.IdMarca == p.IdMarca))
            return BadRequest(new { mensaje = "La marca seleccionada no existe." });

        if (p.PrecioVenta < p.PrecioCosto)
            return BadRequest(new { mensaje = "El precio de venta no puede ser menor al precio de costo." });

        if (p.FechaIngreso == default)
            p.FechaIngreso = DateTime.Now;

        if (p.FechaIngreso.Date > DateTime.Today)
            return BadRequest(new { mensaje = "La fecha de ingreso no puede ser futura." });

        if (p.FechaIngreso.Year < 2000)
            return BadRequest(new { mensaje = "La fecha de ingreso no es válida." });

        if (await _db.Productos.AnyAsync(x => x.Producto1 == p.Producto1 && x.IdMarca == p.IdMarca && x.IdProducto != id))
            return Conflict(new { mensaje = "Ya existe un producto con ese nombre en esa marca." });

        return null;
    }
}