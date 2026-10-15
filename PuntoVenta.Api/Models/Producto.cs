using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace PuntoVenta.Api.Models;

public partial class Producto
{
    public int IdProducto { get; set; }

    [JsonPropertyName("producto")]
    [Required(ErrorMessage = "El nombre del producto es obligatorio")]
    [StringLength(50, ErrorMessage = "Máximo 50 caracteres")]
    public string Producto1 { get; set; } = null!;

    [Range(1, short.MaxValue, ErrorMessage = "Seleccione una marca")]
    public short IdMarca { get; set; }

    [StringLength(100, ErrorMessage = "Máximo 100 caracteres")]
    public string? Descripcion { get; set; }

    [StringLength(30, ErrorMessage = "Máximo 30 caracteres")]
    [RegularExpression(@"^(/images/productos/[0-9a-f]{7}\.(jpg|jpeg|png|webp))?$", ErrorMessage = "Ruta de imagen inválida")]
    public string? Imagen { get; set; }

    [Range(0.01, 999999.99, ErrorMessage = "El precio de costo debe ser mayor a cero")]
    public decimal PrecioCosto { get; set; }

    [Range(0.01, 999999.99, ErrorMessage = "El precio de venta debe ser mayor a cero")]
    public decimal PrecioVenta { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "La existencia no puede ser negativa")]
    public int Existencia { get; set; }

    public DateTime FechaIngreso { get; set; }

    [JsonIgnore]
    public virtual ICollection<ComprasDetalle> ComprasDetalles { get; set; } = new List<ComprasDetalle>();

    [JsonIgnore]
    [ValidateNever]
    public virtual Marca IdMarcaNavigation { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<VentasDetalle> VentasDetalles { get; set; } = new List<VentasDetalle>();
}