using System.Diagnostics;
using PuntoVenta.Web.Components;
using PuntoVenta.Web.Servicios;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient("PuntoVentaApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:5280/api/");
});
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("PuntoVentaApi"));

builder.Services.AddScoped<SesionUsuario>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    var carpetaApi = Path.GetFullPath(
        Path.Combine(app.Environment.ContentRootPath, "..", "PuntoVenta.Api"));

    if (Directory.Exists(carpetaApi))
    {
        var procesoApi = Process.Start(new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "watch --non-interactive run",
            WorkingDirectory = carpetaApi,
            UseShellExecute = false
        });

        app.Lifetime.ApplicationStopping.Register(() =>
        {
            try { procesoApi?.Kill(entireProcessTree: true); } catch { }
        });
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();