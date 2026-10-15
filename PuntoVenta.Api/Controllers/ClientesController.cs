using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Data;
using PuntoVenta.Api.Models;

namespace PuntoVenta.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClientesController : ControllerBase
{
    private readonly PuntoVentaContext _db;

    public ClientesController(PuntoVentaContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Cliente>>> Get()
    {
        return await _db.Clientes.OrderBy(c => c.IdCliente).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Cliente>> GetPorId(int id)
    {
        var cliente = await _db.Clientes.FindAsync(id);
        if (cliente == null) return NotFound();

        return cliente;
    }

    [HttpPost]
    public async Task<ActionResult<Cliente>> Post(Cliente cliente)
    {
        Limpiar(cliente);

        var error = await Validar(cliente, 0);
        if (error is not null) return error;

        cliente.IdCliente = 0;
        cliente.FechaIngreso = DateTime.Now;
        _db.Clientes.Add(cliente);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPorId), new { id = cliente.IdCliente }, cliente);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, Cliente cliente)
    {
        if (id != cliente.IdCliente) return BadRequest();

        var existente = await _db.Clientes.FindAsync(id);
        if (existente == null) return NotFound();

        Limpiar(cliente);

        var error = await Validar(cliente, id);
        if (error is not null) return error;

        existente.Nombres = cliente.Nombres;
        existente.Apellidos = cliente.Apellidos;
        existente.Nit = cliente.Nit;
        existente.Genero = cliente.Genero;
        existente.Telefono = cliente.Telefono;
        existente.CorreoElectronico = cliente.CorreoElectronico;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var cliente = await _db.Clientes.FindAsync(id);
        if (cliente == null) return NotFound();

        if (await _db.Ventas.AnyAsync(v => v.IdCliente == id))
            return Conflict(new { mensaje = "No se puede eliminar: el cliente tiene ventas registradas." });

        try
        {
            _db.Clientes.Remove(cliente);
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(new { mensaje = "No se puede eliminar: el cliente está en uso." });
        }

        return NoContent();
    }

    private async Task<ActionResult?> Validar(Cliente cliente, int idActual)
    {
        if (cliente.Nombres.Length == 0 || cliente.Apellidos.Length == 0)
            return BadRequest(new { mensaje = "Los nombres y apellidos son obligatorios." });

        if (cliente.Genero > 1)
            return BadRequest(new { mensaje = "El género no es válido." });

        if (await _db.Clientes.AnyAsync(c => c.Nit == cliente.Nit && c.IdCliente != idActual))
            return Conflict(new { mensaje = "Ya existe un cliente con ese NIT." });

        if (cliente.CorreoElectronico is not null &&
            await _db.Clientes.AnyAsync(c => c.CorreoElectronico == cliente.CorreoElectronico && c.IdCliente != idActual))
            return Conflict(new { mensaje = "Ya existe un cliente con ese correo electrónico." });

        return null;
    }

    private static void Limpiar(Cliente cliente)
    {
        cliente.Nombres = cliente.Nombres.Trim();
        cliente.Apellidos = cliente.Apellidos.Trim();
        cliente.Nit = cliente.Nit.Trim().ToUpper();
        cliente.Telefono = string.IsNullOrWhiteSpace(cliente.Telefono) ? null : cliente.Telefono.Trim();
        cliente.CorreoElectronico = string.IsNullOrWhiteSpace(cliente.CorreoElectronico)
            ? null
            : cliente.CorreoElectronico.Trim().ToLower();
    }
}
