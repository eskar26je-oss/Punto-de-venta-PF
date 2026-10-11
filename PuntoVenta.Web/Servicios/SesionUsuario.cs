using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace PuntoVenta.Web.Servicios;

public class SesionUsuario
{
    private const string Clave = "sesion";

    private readonly HttpClient _http;
    private readonly ProtectedSessionStorage _almacen;
    private bool _restaurada;

    public SesionUsuario(HttpClient http, ProtectedSessionStorage almacen)
    {
        _http = http;
        _almacen = almacen;
    }

    public string? Token { get; private set; }
    public string? Usuario { get; private set; }
    public DateTime Expira { get; private set; }

    public bool EstaAutenticado => !string.IsNullOrEmpty(Token) && Expira > DateTime.UtcNow;

    public async Task RestaurarAsync()
    {
        if (_restaurada) return;
        _restaurada = true;

        try
        {
            var resultado = await _almacen.GetAsync<DatosSesion>(Clave);
            if (resultado.Success && resultado.Value is not null && resultado.Value.Expira > DateTime.UtcNow)
            {
                Aplicar(resultado.Value.Token, resultado.Value.Usuario, resultado.Value.Expira);
            }
        }
        catch
        {
        }
    }

    public async Task IniciarAsync(string token, string usuario, DateTime expira)
    {
        Aplicar(token, usuario, expira);

        try
        {
            await _almacen.SetAsync(Clave, new DatosSesion { Token = token, Usuario = usuario, Expira = expira });
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
        _http.DefaultRequestHeaders.Authorization = null;
        _ = BorrarAlmacenAsync();
    }

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
    }
}