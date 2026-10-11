using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Data;
using PuntoVenta.Api.Models;

namespace PuntoVenta.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MarcasController : ControllerBase
{
    private readonly PuntoVentaContext _db;

    public MarcasController(PuntoVentaContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Marca>>> Get()
    {
        return await _db.Marcas.OrderBy(m => m.IdMarca).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Marca>> GetPorId(short id)
    {
        var marca = await _db.Marcas.FindAsync(id);
        if (marca == null) return NotFound();

        return marca;
    }

    [HttpPost]
    public async Task<ActionResult<Marca>> Post(Marca marca)
    {
        marca.Marca1 = marca.Marca1.Trim();
        if (marca.Marca1.Length == 0)
            return BadRequest(new { mensaje = "El nombre de la marca es obligatorio." });

        if (await _db.Marcas.AnyAsync(m => m.Marca1 == marca.Marca1))
            return Conflict(new { mensaje = "Ya existe una marca con ese nombre." });

        marca.IdMarca = 0;
        _db.Marcas.Add(marca);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPorId), new { id = marca.IdMarca }, marca);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(short id, Marca marca)
    {
        if (id != marca.IdMarca) return BadRequest();

        var existente = await _db.Marcas.FindAsync(id);
        if (existente == null) return NotFound();

        var nombre = marca.Marca1.Trim();
        if (nombre.Length == 0)
            return BadRequest(new { mensaje = "El nombre de la marca es obligatorio." });

        if (await _db.Marcas.AnyAsync(m => m.Marca1 == nombre && m.IdMarca != id))
            return Conflict(new { mensaje = "Ya existe una marca con ese nombre." });

        existente.Marca1 = nombre;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(short id)
    {
        var marca = await _db.Marcas.FindAsync(id);
        if (marca == null) return NotFound();

        try
        {
            _db.Marcas.Remove(marca);
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(new { mensaje = "No se puede eliminar: la marca está en uso por uno o más productos." });
        }

        return NoContent();
    }
}