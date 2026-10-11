using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PuntoVenta.Api.Data;

namespace PuntoVenta.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly PuntoVentaContext _db;
    private readonly IConfiguration _config;

    public AuthController(PuntoVentaContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest peticion)
    {
        var usuario = await _db.Usuarios
            .FirstOrDefaultAsync(u => u.Usuario1 == peticion.Usuario);

        if (usuario == null
            || usuario.Activo == 0
            || !BCrypt.Net.BCrypt.Verify(peticion.Contrasena, usuario.PasswordHash))
        {
            return Unauthorized(new { mensaje = "Usuario o contraseña incorrectos" });
        }

        var minutos = int.Parse(_config["Jwt:Minutos"] ?? "120");
        var expira = DateTime.UtcNow.AddMinutes(minutos);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
            new Claim(ClaimTypes.Name, usuario.Usuario1)
        };

        var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var credenciales = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expira,
            signingCredentials: credenciales);

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new LoginResponse(jwt, usuario.Usuario1, expira));
    }

    [HttpGet("yo")]
    [Authorize]
    public IActionResult Yo()
    {
        return Ok(new { mensaje = "Tienes acceso", usuario = User.Identity?.Name });
    }
}

public class LoginRequest
{
    [Required(ErrorMessage = "El usuario es obligatorio")]
    [StringLength(30, ErrorMessage = "Máximo 30 caracteres")]
    public string Usuario { get; set; } = "";

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [StringLength(50, ErrorMessage = "Máximo 50 caracteres")]
    public string Contrasena { get; set; } = "";
}

public record LoginResponse(string Token, string Usuario, DateTime Expira);