using System;
using System.Collections.Generic;

namespace PuntoVenta.Api.Models;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public string Usuario1 { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public ulong Activo { get; set; }

    public int? IdEmpleado { get; set; }

    public DateTime FechaIngreso { get; set; }

    public virtual Empleado? IdEmpleadoNavigation { get; set; }
}
