using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PuntoVenta.Api.Models;

public partial class Puesto
{
    public short IdPuesto { get; set; }

    [JsonPropertyName("nombre")]
    [Required(ErrorMessage = "El nombre del puesto es obligatorio")]
    [StringLength(50, ErrorMessage = "Máximo 50 caracteres")]
    public string Puesto1 { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();
}
