using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PuntoVenta.Api.Models;

public partial class Proveedore
{
    public int IdProveedor { get; set; }

    [Required(ErrorMessage = "El nombre del proveedor es obligatorio")]
    [StringLength(60, ErrorMessage = "Máximo 60 caracteres")]
    public string Proveedor { get; set; } = null!;

    [Required(ErrorMessage = "El NIT es obligatorio")]
    [StringLength(12, ErrorMessage = "Máximo 12 caracteres")]
    [RegularExpression(@"^[0-9]{1,10}-?[0-9Kk]$", ErrorMessage = "NIT inválido. Ejemplo: 1234567-8")]
    public string Nit { get; set; } = null!;

    [StringLength(80, ErrorMessage = "Máximo 80 caracteres")]
    public string? Direccion { get; set; }

    [RegularExpression(@"^([0-9]{8})?$", ErrorMessage = "El teléfono debe tener exactamente 8 dígitos")]
    public string? Telefono { get; set; }

    [JsonIgnore]
    public virtual ICollection<Compra> Compras { get; set; } = new List<Compra>();
}