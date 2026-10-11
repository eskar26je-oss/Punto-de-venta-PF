using System;
using System.Collections.Generic;

namespace PuntoVenta.Api.Models;

public partial class Proveedore
{
    public int IdProveedor { get; set; }

    public string Proveedor { get; set; } = null!;

    public string Nit { get; set; } = null!;

    public string? Direccion { get; set; }

    public string? Telefono { get; set; }

    public virtual ICollection<Compra> Compras { get; set; } = new List<Compra>();
}
