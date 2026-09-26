using Microsoft.AspNetCore.Mvc;
using PruebaOben.Application.DTOs;
using PruebaOben.Application.Interfaces;

namespace PruebaOben.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var token = await _authService.LoginAsync(dto);

        // Si no se pudo autenticar al usuario,
        // devolvemos 401 Unauthorized.
        if (token is null)
        {
            return Unauthorized(new
            {
                message = "Correo o contraseña incorrectos."
            });
        }

        // Si la autenticación fue correcta,
        // devolvemos el JWT al cliente.
        return Ok(new
        {
            token
        });
    }
}