using System.ComponentModel.DataAnnotations;

namespace PuntoVenta.Api.Dtos;

// NOTA: OpcionDto ya existe en VentaDtos.cs y se reutiliza para el combo de proveedores.

// ---------- Entrada ----------
// A diferencia de la venta, en la compra SÍ viaja el precio:
// es el costo que cobró el proveedor en esa orden.
public class CompraDetalleCrearDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un producto.")]
    public int IdProducto { get; set; }

    [Range(1, 100000, ErrorMessage = "La cantidad debe ser mayor a cero.")]
    public int Cantidad { get; set; }

    [Range(0.01, 999999.99, ErrorMessage = "El precio de costo debe ser mayor a cero.")]
    public decimal PrecioCostoUnitario { get; set; }
}

public class CompraCrearDto
{
    public DateOnly FechaOrden { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un proveedor.")]
    public int IdProveedor { get; set; }

    [Required(ErrorMessage = "La compra debe tener al menos un producto.")]
    [MinLength(1, ErrorMessage = "La compra debe tener al menos un producto.")]
    public List<CompraDetalleCrearDto> Detalles { get; set; } = new();
}

// ---------- Salida ----------
public record CompraDetalleDto(
    long IdCompraDetalle, int IdProducto, string Producto,
    int Cantidad, decimal PrecioCostoUnitario, decimal Subtotal);

public record CompraDto(
    int IdCompra, int NoOrdenCompra, DateOnly FechaOrden,
    int IdProveedor, string Proveedor,
    decimal Total, List<CompraDetalleDto> Detalles);

public record CompraResumenDto(
    int IdCompra, int NoOrdenCompra, DateOnly FechaOrden,
    string Proveedor, int Unidades, decimal Total);

// Para el combo de productos de la pantalla de compras
public record ProductoCompraDto(
    int IdProducto, string Producto, decimal PrecioCosto, decimal PrecioVenta, int Existencia);

// ---------- Reporte de compras ----------
public record ReporteCompraProductoDto(
    string Producto, int Cantidad, decimal PrecioCostoUnitario, decimal Subtotal);

public record ReporteCompraFilaDto(
    DateOnly Fecha, int NoOrdenCompra, string Proveedor,
    int Unidades, decimal TotalCompra, List<ReporteCompraProductoDto> Productos);

public record ReporteComprasDto(
    DateOnly? Desde, DateOnly? Hasta, int CantidadCompras, int TotalUnidades,
    decimal TotalGeneral, List<ReporteCompraFilaDto> Compras);
