using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace PuntoVenta.Api.Models;

public partial class Empleado
{
    public int IdEmpleado { get; set; }

    [Required(ErrorMessage = "Los nombres son obligatorios")]
    [StringLength(60, ErrorMessage = "Máximo 60 caracteres")]
    [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñÜü ]+$", ErrorMessage = "Los nombres solo pueden contener letras")]
    public string Nombres { get; set; } = null!;

    [Required(ErrorMessage = "Los apellidos son obligatorios")]
    [StringLength(60, ErrorMessage = "Máximo 60 caracteres")]
    [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñÜü ]+$", ErrorMessage = "Los apellidos solo pueden contener letras")]
    public string Apellidos { get; set; } = null!;

    [StringLength(80, ErrorMessage = "Máximo 80 caracteres")]
    public string? Direccion { get; set; }

    [RegularExpression(@"^([0-9]{8})?$", ErrorMessage = "El teléfono debe tener exactamente 8 dígitos")]
    public string? Telefono { get; set; }

    [Required(ErrorMessage = "El DPI es obligatorio")]
    [RegularExpression(@"^[0-9]{13}$", ErrorMessage = "El DPI debe tener exactamente 13 dígitos")]
    public string Dpi { get; set; } = null!;

    // 1 = Masculino, 0 = Femenino (columna bit(1))
    public ulong Genero { get; set; }

    public DateOnly FechaNacimiento { get; set; }

    [Range(1, short.MaxValue, ErrorMessage = "Seleccione un puesto")]
    public short IdPuesto { get; set; }

    public DateOnly FechaInicioLabores { get; set; }

    public DateTime FechaIngreso { get; set; }

    [JsonIgnore]
    [ValidateNever]
    public virtual Puesto IdPuestoNavigation { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();

    [JsonIgnore]
    public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();
}
