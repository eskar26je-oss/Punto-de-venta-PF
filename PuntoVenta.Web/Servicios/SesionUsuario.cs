using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace PuntoVenta.Web.Servicios;

public class SesionUsuario
{
    private const string Clave = "sesion";

    // Cambia cada vez que se levanta la aplicación.
    // Así, al reiniciar, las sesiones guardadas ya no sirven y hay que iniciar sesión otra vez.
    private static readonly string InstanciaServidor = Guid.NewGuid().ToString();

    private readonly HttpClient _http;
    private readonly ProtectedLocalStorage _almacen;
    private bool _restaurada;

    // Permisos del usuario (se cargan al iniciar sesión o al abrir la página)
    private List<PermisoRuta> _permisos = new();
    private HashSet<string> _modulos = new(StringComparer.OrdinalIgnoreCase);

    public SesionUsuario(HttpClient http, ProtectedLocalStorage almacen)
    {
        _http = http;
        _almacen = almacen;
    }

    public string? Token { get; private set; }
    public string? Usuario { get; private set; }
    public DateTime Expira { get; private set; }
    public bool EsAdmin { get; private set; }

    public bool EstaAutenticado => !string.IsNullOrEmpty(Token) && Expira > DateTime.UtcNow;

    public async Task RestaurarAsync()
    {
        if (_restaurada) return;
        _restaurada = true;

        try
        {
            var resultado = await _almacen.GetAsync<DatosSesion>(Clave);
            if (resultado.Success
                && resultado.Value is not null
                && resultado.Value.Expira > DateTime.UtcNow
                && resultado.Value.Instancia == InstanciaServidor)
            {
                Aplicar(resultado.Value.Token, resultado.Value.Usuario, resultado.Value.Expira);
                await CargarPermisosAsync();
            }
            else if (resultado.Success && resultado.Value is not null)
            {
                // Sesión de antes de reiniciar la aplicación: se borra
                await BorrarAlmacenAsync();
            }
        }
        catch
        {
        }
    }

    public async Task IniciarAsync(string token, string usuario, DateTime expira)
    {
        Aplicar(token, usuario, expira);
        await CargarPermisosAsync();

        try
        {
            await _almacen.SetAsync(Clave, new DatosSesion
            {
                Token = token,
                Usuario = usuario,
                Expira = expira,
                Instancia = InstanciaServidor
            });
        }
        catch
        {
        }
    }

    public void Cerrar()
    {
        Token = null;
        Usuario = null;
        Expira = default;
        EsAdmin = false;
        _permisos = new();
        _modulos.Clear();
        _http.DefaultRequestHeaders.Authorization = null;
        _http.DefaultRequestHeaders.Remove(EncabezadoAutorizacion);
        _ = BorrarAlmacenAsync();
    }

    // ---------- Autorización del admin (para editar o anular) ----------

    private const string EncabezadoAutorizacion = "X-Autorizacion";

    /// <summary>Guarda la autorización que dio el admin para mandarla a la API.</summary>
    public void UsarAutorizacion(string token)
    {
        _http.DefaultRequestHeaders.Remove(EncabezadoAutorizacion);
        _http.DefaultRequestHeaders.Add(EncabezadoAutorizacion, token);
    }

    // ---------- Permisos ----------

    /// <summary>Pide a la API qué puede ver y editar este usuario.</summary>
    public async Task CargarPermisosAsync()
    {
        EsAdmin = false;
        _permisos = new();
        _modulos.Clear();

        if (!EstaAutenticado) return;

        try
        {
            var respuesta = await _http.GetAsync("Permisos/mios");

            // La API se reinició o el token ya no sirve: hay que iniciar sesión otra vez
            if (respuesta.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                Cerrar();
                return;
            }

            var datos = respuesta.IsSuccessStatusCode
                ? await respuesta.Content.ReadFromJsonAsync<MisPermisos>()
                : null;

            if (datos is not null)
            {
                EsAdmin = datos.EsAdmin;
                _permisos = datos.Permisos ?? new();
                _modulos = new HashSet<string>(datos.Modulos ?? new(), StringComparer.OrdinalIgnoreCase);
            }
        }
        catch
        {
        }
    }

    /// <summary>Convierte "/reportes/ventas?x=1" en "/reportes".</summary>
    public static string RutaModulo(string ruta)
    {
        var limpia = ruta.Split('?', '#')[0].Trim('/');
        var primera = limpia.Split('/')[0].ToLowerInvariant();
        return "/" + primera;
    }

    public bool PuedeVer(string ruta)
    {
        var modulo = RutaModulo(ruta);
        if (EsAdmin) return true;
        if (modulo == "/usuarios") return false;
        if (!_modulos.Contains(modulo)) return true; // páginas que no están en el menú (Inicio, etc.)

        return _permisos.Any(p => string.Equals(p.Ruta, modulo, StringComparison.OrdinalIgnoreCase) && p.PuedeVer);
    }

    public bool PuedeEditar(string ruta)
    {
        var modulo = RutaModulo(ruta);
        if (EsAdmin) return true;
        if (modulo == "/usuarios") return false;
        if (!_modulos.Contains(modulo)) return true;

        return _permisos.Any(p => string.Equals(p.Ruta, modulo, StringComparison.OrdinalIgnoreCase) && p.PuedeEditar);
    }

    // ---------- Internos ----------

    private async Task BorrarAlmacenAsync()
    {
        try
        {
            await _almacen.DeleteAsync(Clave);
        }
        catch
        {
        }
    }

    private void Aplicar(string token, string usuario, DateTime expira)
    {
        Token = token;
        Usuario = usuario;
        Expira = expira;
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public class DatosSesion
    {
        public string Token { get; set; } = "";
        public string Usuario { get; set; } = "";
        public DateTime Expira { get; set; }
        public string Instancia { get; set; } = "";
    }

    public record PermisoRuta(string Ruta, bool PuedeVer, bool PuedeEditar);

    public record MisPermisos(bool EsAdmin, List<string>? Modulos, List<PermisoRuta>? Permisos);
}
