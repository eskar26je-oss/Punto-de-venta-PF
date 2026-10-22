namespace PuntoVenta.Api.Models;

/// <summary>
/// Qué puede ver y qué puede editar un usuario en una opción del menú.
/// </summary>
public partial class Permiso
{
    public int IdPermiso { get; set; }

    public int IdUsuario { get; set; }

    public int IdMenu { get; set; }

    public ulong PuedeVer { get; set; }

    public ulong PuedeEditar { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;

    public virtual Menu IdMenuNavigation { get; set; } = null!;
}
