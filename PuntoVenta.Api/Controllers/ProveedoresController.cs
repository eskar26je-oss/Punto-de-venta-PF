using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Data;
using PuntoVenta.Api.Models;

namespace PuntoVenta.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Permiso("/proveedores")]
public class ProveedoresController : ControllerBase
{
    private readonly PuntoVentaContext _db;

    public ProveedoresController(PuntoVentaContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Proveedore>>> Get()
    {
        return await _db.Proveedores.OrderBy(p => p.IdProveedor).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Proveedore>> GetPorId(int id)
    {
        var proveedor = await _db.Proveedores.FindAsync(id);
        if (proveedor == null) return NotFound();

        return proveedor;
    }

    [HttpPost]
    public async Task<ActionResult<Proveedore>> Post(Proveedore proveedor)
    {
        Limpiar(proveedor);

        if (await _db.Proveedores.AnyAsync(p => p.Proveedor == proveedor.Proveedor))
            return Conflict(new { mensaje = "Ya existe un proveedor con ese nombre." });

        if (await _db.Proveedores.AnyAsync(p => p.Nit == proveedor.Nit))
            return Conflict(new { mensaje = "Ya existe un proveedor con ese NIT." });

        proveedor.IdProveedor = 0;
        _db.Proveedores.Add(proveedor);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPorId), new { id = proveedor.IdProveedor }, proveedor);
    }
[RequiereAutorizacionAdmin("proveedores")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, Proveedore proveedor)
    {
        if (id != proveedor.IdProveedor) return BadRequest();

        var existente = await _db.Proveedores.FindAsync(id);
        if (existente == null) return NotFound();

        Limpiar(proveedor);

        if (await _db.Proveedores.AnyAsync(p => p.Proveedor == proveedor.Proveedor && p.IdProveedor != id))
            return Conflict(new { mensaje = "Ya existe un proveedor con ese nombre." });

        if (await _db.Proveedores.AnyAsync(p => p.Nit == proveedor.Nit && p.IdProveedor != id))
            return Conflict(new { mensaje = "Ya existe un proveedor con ese NIT." });

        existente.Proveedor = proveedor.Proveedor;
        existente.Nit = proveedor.Nit;
        existente.Direccion = proveedor.Direccion;
        existente.Telefono = proveedor.Telefono;
        await _db.SaveChangesAsync();

        return NoContent();
    }
[RequiereAutorizacionAdmin("proveedores")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var proveedor = await _db.Proveedores.FindAsync(id);
        if (proveedor == null) return NotFound();

        try
        {
            _db.Proveedores.Remove(proveedor);
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(new { mensaje = "No se puede eliminar: el proveedor tiene compras registradas." });
        }

        return NoContent();
    }

    private static void Limpiar(Proveedore proveedor)
    {
        proveedor.Proveedor = proveedor.Proveedor.Trim();
        proveedor.Nit = proveedor.Nit.Trim().ToUpper();
        proveedor.Direccion = string.IsNullOrWhiteSpace(proveedor.Direccion) ? null : proveedor.Direccion.Trim();
        proveedor.Telefono = string.IsNullOrWhiteSpace(proveedor.Telefono) ? null : proveedor.Telefono.Trim();
    }
}