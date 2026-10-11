using System;
using System.Collections.Generic;

namespace PuntoVenta.Api.Models;

public partial class Cliente
{
    public int IdCliente { get; set; }

    public string Nombres { get; set; } = null!;

    public string Apellidos { get; set; } = null!;

    public string Nit { get; set; } = null!;

    public ulong Genero { get; set; }

    public string? Telefono { get; set; }

    public string? CorreoElectronico { get; set; }

    public DateTime FechaIngreso { get; set; }

    public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();
}
