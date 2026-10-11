using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PuntoVenta.Api.Models;

public partial class Marca
{
    public short IdMarca { get; set; }

    [JsonPropertyName("nombre")]
    [Required(ErrorMessage = "El nombre de la marca es obligatorio")]
    [StringLength(50, ErrorMessage = "Máximo 50 caracteres")]
    public string Marca1 { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}