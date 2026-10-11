using System;
using System.Collections.Generic;

namespace PuntoVenta.Api.Models;

public partial class Venta
{
    public int IdVenta { get; set; }

    public int NoFactura { get; set; }

    public string Serie { get; set; } = null!;

    public DateOnly FechaFactura { get; set; }

    public int IdCliente { get; set; }

    public int IdEmpleado { get; set; }

    public DateTime FechaIngreso { get; set; }

    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    public virtual Empleado IdEmpleadoNavigation { get; set; } = null!;

    public virtual ICollection<VentasDetalle> VentasDetalles { get; set; } = new List<VentasDetalle>();
}
