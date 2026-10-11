using System;
using System.Collections.Generic;

namespace PuntoVenta.Api.Models;

public partial class Compra
{
    public int IdCompra { get; set; }

    public int NoOrdenCompra { get; set; }

    public int IdProveedor { get; set; }

    public DateOnly FechaOrden { get; set; }

    public DateTime FechaIngreso { get; set; }

    public virtual ICollection<ComprasDetalle> ComprasDetalles { get; set; } = new List<ComprasDetalle>();

    public virtual Proveedore IdProveedorNavigation { get; set; } = null!;
}
