using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PuntoVenta.Api.Data;
using PuntoVenta.Api.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<PuntoVentaContext>(opciones =>
{
    var conexion = builder.Configuration.GetConnectionString("Default");
    opciones.UseMySql(conexion, ServerVersion.AutoDetect(conexion));
});

var jwt = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opciones =>
    {
        opciones.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!)),
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<PuntoVentaContext>();

    if (!db.Usuarios.Any(u => u.Usuario1 == "admin"))
    {
        db.Usuarios.Add(new Usuario
        {
            Usuario1 = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123"),
            Activo = 1,
            FechaIngreso = DateTime.Now
        });
        db.SaveChanges();
    }
}
catch (Exception ex)
{
    app.Logger.LogError("No se pudo conectar con MySQL: {Mensaje}. Enciende MySQL y reinicia.", ex.Message);
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();