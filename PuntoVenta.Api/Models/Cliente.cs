using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PuntoVenta.Api.Models;

public partial class Cliente
{
    public int IdCliente { get; set; }

    [Required(ErrorMessage = "Los nombres son obligatorios")]
    [StringLength(60, ErrorMessage = "Máximo 60 caracteres")]
    [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñÜü ]+$", ErrorMessage = "Los nombres solo pueden contener letras")]
    public string Nombres { get; set; } = null!;

    [Required(ErrorMessage = "Los apellidos son obligatorios")]
    [StringLength(60, ErrorMessage = "Máximo 60 caracteres")]
    [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñÜü ]+$", ErrorMessage = "Los apellidos solo pueden contener letras")]
    public string Apellidos { get; set; } = null!;

    [Required(ErrorMessage = "El NIT es obligatorio")]
    [StringLength(12, ErrorMessage = "Máximo 12 caracteres")]
    [RegularExpression(@"^([0-9]{1,10}-?[0-9Kk]|[Cc][Ff])$", ErrorMessage = "NIT inválido. Ejemplo: 1234567-8 o CF")]
    public string Nit { get; set; } = null!;

    // 1 = Masculino, 0 = Femenino (columna bit(1))
    public ulong Genero { get; set; }

    [RegularExpression(@"^([0-9]{8})?$", ErrorMessage = "El teléfono debe tener exactamente 8 dígitos")]
    public string? Telefono { get; set; }

    [StringLength(45, ErrorMessage = "Máximo 45 caracteres")]
    [RegularExpression(@"^([^@\s]+@[^@\s]+\.[^@\s]+)?$", ErrorMessage = "El correo electrónico no es válido")]
    public string? CorreoElectronico { get; set; }

    public DateTime FechaIngreso { get; set; }

    [JsonIgnore]
    public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();
}
