using System;
using System.Collections.Generic;

namespace PuntoVenta.Api.Models;

public partial class Puesto
{
    public short IdPuesto { get; set; }

    public string Puesto1 { get; set; } = null!;

    public virtual ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();
}
