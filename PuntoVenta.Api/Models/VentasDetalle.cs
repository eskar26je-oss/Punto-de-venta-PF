using System;
using System.Collections.Generic;

namespace PuntoVenta.Api.Models;

public partial class VentasDetalle
{
    public long IdVentaDetalle { get; set; }

    public int IdVenta { get; set; }

    public int IdProducto { get; set; }

    public int Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public virtual Producto IdProductoNavigation { get; set; } = null!;

    public virtual Venta IdVentaNavigation { get; set; } = null!;
}
