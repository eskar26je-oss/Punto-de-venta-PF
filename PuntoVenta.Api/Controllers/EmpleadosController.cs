using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Data;
using PuntoVenta.Api.Models;

namespace PuntoVenta.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmpleadosController : ControllerBase
{
    private readonly PuntoVentaContext _db;

    public EmpleadosController(PuntoVentaContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Empleado>>> Get()
    {
        return await _db.Empleados.OrderBy(e => e.IdEmpleado).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Empleado>> GetPorId(int id)
    {
        var empleado = await _db.Empleados.FindAsync(id);
        if (empleado == null) return NotFound();

        return empleado;
    }

    [HttpPost]
    public async Task<ActionResult<Empleado>> Post(Empleado empleado)
    {
        Limpiar(empleado);

        var error = await Validar(empleado, 0);
        if (error is not null) return error;

        empleado.IdEmpleado = 0;
        empleado.FechaIngreso = DateTime.Now;
        _db.Empleados.Add(empleado);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPorId), new { id = empleado.IdEmpleado }, empleado);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, Empleado empleado)
    {
        if (id != empleado.IdEmpleado) return BadRequest();

        var existente = await _db.Empleados.FindAsync(id);
        if (existente == null) return NotFound();

        Limpiar(empleado);

        var error = await Validar(empleado, id);
        if (error is not null) return error;

        existente.Nombres = empleado.Nombres;
        existente.Apellidos = empleado.Apellidos;
        existente.Direccion = empleado.Direccion;
        existente.Telefono = empleado.Telefono;
        existente.Dpi = empleado.Dpi;
        existente.Genero = empleado.Genero;
        existente.FechaNacimiento = empleado.FechaNacimiento;
        existente.IdPuesto = empleado.IdPuesto;
        existente.FechaInicioLabores = empleado.FechaInicioLabores;
        // FechaIngreso no se modifica: es la fecha en que se registró el empleado.
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var empleado = await _db.Empleados.FindAsync(id);
        if (empleado == null) return NotFound();

        if (await _db.Ventas.AnyAsync(v => v.IdEmpleado == id))
            return Conflict(new { mensaje = "No se puede eliminar: el empleado tiene ventas registradas." });

        if (await _db.Usuarios.AnyAsync(u => u.IdEmpleado == id))
            return Conflict(new { mensaje = "No se puede eliminar: el empleado tiene un usuario del sistema asignado." });

        try
        {
            _db.Empleados.Remove(empleado);
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(new { mensaje = "No se puede eliminar: el empleado está en uso." });
        }

        return NoContent();
    }

    private async Task<ActionResult?> Validar(Empleado empleado, int idActual)
    {
        if (empleado.Nombres.Length == 0 || empleado.Apellidos.Length == 0)
            return BadRequest(new { mensaje = "Los nombres y apellidos son obligatorios." });

        if (empleado.Genero > 1)
            return BadRequest(new { mensaje = "El género no es válido." });

        if (!await _db.Puestos.AnyAsync(p => p.IdPuesto == empleado.IdPuesto))
            return BadRequest(new { mensaje = "El puesto seleccionado no existe." });

        var hoy = DateOnly.FromDateTime(DateTime.Today);

        if (empleado.FechaNacimiento > hoy.AddYears(-18))
            return BadRequest(new { mensaje = "El empleado debe ser mayor de edad (18 años o más)." });

        if (empleado.FechaNacimiento < hoy.AddYears(-100))
            return BadRequest(new { mensaje = "La fecha de nacimiento no es válida." });

        if (empleado.FechaInicioLabores > hoy)
            return BadRequest(new { mensaje = "La fecha de inicio de labores no puede ser futura." });

        if (empleado.FechaInicioLabores <= empleado.FechaNacimiento)
            return BadRequest(new { mensaje = "La fecha de inicio de labores debe ser posterior a la fecha de nacimiento." });

        if (await _db.Empleados.AnyAsync(e => e.Dpi == empleado.Dpi && e.IdEmpleado != idActual))
            return Conflict(new { mensaje = "Ya existe un empleado con ese DPI." });

        return null;
    }

    private static void Limpiar(Empleado empleado)
    {
        empleado.Nombres = empleado.Nombres.Trim();
        empleado.Apellidos = empleado.Apellidos.Trim();
        empleado.Dpi = empleado.Dpi.Trim();
        empleado.Direccion = string.IsNullOrWhiteSpace(empleado.Direccion) ? null : empleado.Direccion.Trim();
        empleado.Telefono = string.IsNullOrWhiteSpace(empleado.Telefono) ? null : empleado.Telefono.Trim();
    }
}
