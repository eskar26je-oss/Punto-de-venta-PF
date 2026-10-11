using System;
using System.Collections.Generic;

namespace PuntoVenta.Api.Models;

public partial class ComprasDetalle
{
    public long IdCompraDetalle { get; set; }

    public int IdCompra { get; set; }

    public int IdProducto { get; set; }

    public int Cantidad { get; set; }

    public decimal PrecioCostoUnitario { get; set; }

    public virtual Compra IdCompraNavigation { get; set; } = null!;

    public virtual Producto IdProductoNavigation { get; set; } = null!;
}
