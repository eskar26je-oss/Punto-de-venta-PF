using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Data;
using PuntoVenta.Api.Models;

namespace PuntoVenta.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
	[Permiso("/puestos", TambienVer = new[] { "/empleados" })]
public class PuestosController : ControllerBase
{
    private readonly PuntoVentaContext _db;

    public PuestosController(PuntoVentaContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Puesto>>> Get()
    {
        return await _db.Puestos.OrderBy(p => p.IdPuesto).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Puesto>> GetPorId(short id)
    {
        var puesto = await _db.Puestos.FindAsync(id);
        if (puesto == null) return NotFound();

        return puesto;
    }

    [HttpPost]
    public async Task<ActionResult<Puesto>> Post(Puesto puesto)
    {
        puesto.Puesto1 = puesto.Puesto1.Trim();
        if (puesto.Puesto1.Length == 0)
            return BadRequest(new { mensaje = "El nombre del puesto es obligatorio." });

        if (await _db.Puestos.AnyAsync(p => p.Puesto1 == puesto.Puesto1))
            return Conflict(new { mensaje = "Ya existe un puesto con ese nombre." });

        puesto.IdPuesto = 0;
        _db.Puestos.Add(puesto);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPorId), new { id = puesto.IdPuesto }, puesto);
    }
[RequiereAutorizacionAdmin("puestos")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(short id, Puesto puesto)
    {
        if (id != puesto.IdPuesto) return BadRequest();

        var existente = await _db.Puestos.FindAsync(id);
        if (existente == null) return NotFound();

        var nombre = puesto.Puesto1.Trim();
        if (nombre.Length == 0)
            return BadRequest(new { mensaje = "El nombre del puesto es obligatorio." });

        if (await _db.Puestos.AnyAsync(p => p.Puesto1 == nombre && p.IdPuesto != id))
            return Conflict(new { mensaje = "Ya existe un puesto con ese nombre." });

        existente.Puesto1 = nombre;
        await _db.SaveChangesAsync();

        return NoContent();
    }
[RequiereAutorizacionAdmin("puestos")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(short id)
    {
        var puesto = await _db.Puestos.FindAsync(id);
        if (puesto == null) return NotFound();

        var empleados = await _db.Empleados.CountAsync(e => e.IdPuesto == id);
        if (empleados > 0)
            return Conflict(new { mensaje = $"No se puede eliminar: el puesto está asignado a {empleados} empleado(s)." });

        try
        {
            _db.Puestos.Remove(puesto);
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(new { mensaje = "No se puede eliminar: el puesto está en uso." });
        }

        return NoContent();
    }
}
