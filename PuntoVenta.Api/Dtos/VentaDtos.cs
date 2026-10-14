using System.ComponentModel.DataAnnotations;

namespace PuntoVenta.Api.Dtos;

// ---------- Entrada ----------
// El precio NO viaja en la solicitud: la API siempre usa productos.precio_venta.
public class VentaDetalleCrearDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un producto.")]
    public int IdProducto { get; set; }

    [Range(1, 100000, ErrorMessage = "La cantidad debe ser mayor a cero.")]
    public int Cantidad { get; set; }
}

public class VentaCrearDto
{
    public DateOnly FechaFactura { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un cliente.")]
    public int IdCliente { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un empleado.")]
    public int IdEmpleado { get; set; }

    [Required(ErrorMessage = "La venta debe tener al menos un producto.")]
    [MinLength(1, ErrorMessage = "La venta debe tener al menos un producto.")]
    public List<VentaDetalleCrearDto> Detalles { get; set; } = new();
}

// ---------- Salida ----------
public record VentaDetalleDto(
    long IdVentaDetalle, int IdProducto, string Producto,
    int Cantidad, decimal PrecioUnitario, decimal Subtotal);

public record VentaDto(
    int IdVenta, string Serie, int NoFactura, DateOnly FechaFactura,
    int IdCliente, string Cliente, int IdEmpleado, string Empleado,
    decimal Total, List<VentaDetalleDto> Detalles);

public record VentaResumenDto(
    int IdVenta, string Serie, int NoFactura, DateOnly FechaFactura,
    string Cliente, string Empleado, decimal Total);

// Para los combos de la pantalla de ventas
public record OpcionDto(int Id, string Nombre, string? Detalle);

public record ProductoVentaDto(
    int IdProducto, string Producto, decimal PrecioVenta, int Existencia, string? Imagen);

// Reporte de ventas
public record ReporteVentaFilaDto(
    DateOnly Fecha, string Serie, int NoFactura,
    string Cliente, string Empleado, decimal TotalVenta);

public record ReporteVentasDto(
    DateOnly? Desde, DateOnly? Hasta, int CantidadVentas,
    decimal TotalGeneral, List<ReporteVentaFilaDto> Ventas);
