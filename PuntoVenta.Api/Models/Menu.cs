using System.Text.Json.Serialization;

namespace PuntoVenta.Api.Models;

public partial class Menu
{
    public int IdMenu { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Ruta { get; set; }

    public int? IdPadre { get; set; }

    public int Orden { get; set; }

    public ulong Activo { get; set; }

    [JsonIgnore]
    public virtual Menu? IdPadreNavigation { get; set; }

    [JsonPropertyName("hijos")]
    public virtual ICollection<Menu> InverseIdPadreNavigation { get; set; } = new List<Menu>();
}