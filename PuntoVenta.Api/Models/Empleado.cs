using System;
using System.Collections.Generic;

namespace PuntoVenta.Api.Models;

public partial class Empleado
{
    public int IdEmpleado { get; set; }

    public string Nombres { get; set; } = null!;

    public string Apellidos { get; set; } = null!;

    public string? Direccion { get; set; }

    public string? Telefono { get; set; }

    public string Dpi { get; set; } = null!;

    public ulong Genero { get; set; }

    public DateOnly FechaNacimiento { get; set; }

    public short IdPuesto { get; set; }

    public DateOnly FechaInicioLabores { get; set; }

    public DateTime FechaIngreso { get; set; }

    public virtual Puesto IdPuestoNavigation { get; set; } = null!;

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();

    public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();
}
