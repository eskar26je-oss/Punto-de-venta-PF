using System;
using System.Collections.Generic;

namespace PuntoVenta.Api.Models;

public partial class Producto
{
    public int IdProducto { get; set; }

    public string Producto1 { get; set; } = null!;

    public short IdMarca { get; set; }

    public string? Descripcion { get; set; }

    public string? Imagen { get; set; }

    public decimal PrecioCosto { get; set; }

    public decimal PrecioVenta { get; set; }

    public int Existencia { get; set; }

    public DateTime FechaIngreso { get; set; }

    public virtual ICollection<ComprasDetalle> ComprasDetalles { get; set; } = new List<ComprasDetalle>();

    public virtual Marca IdMarcaNavigation { get; set; } = null!;

    public virtual ICollection<VentasDetalle> VentasDetalles { get; set; } = new List<VentasDetalle>();
}
