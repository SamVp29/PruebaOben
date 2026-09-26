using PruebaOben.Application.DTOs;
using PruebaOben.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using BCrypt.Net;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using PruebaOben.Application.Interfaces;

namespace PruebaOben.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _repository;
    private readonly IJwtSettings _jwtSettings;
    public AuthService(IUserRepository repository, IJwtSettings jwtSettings)
    {
        _repository = repository;
        _jwtSettings = jwtSettings;
    }

    public async Task<string?> LoginAsync(LoginDto dto)
    {
        var user = await _repository.GetByEmailAsync(dto.email);
        
        if (user is null || !user.active)
        {
            return null;
        }
        var passwordValid = BCrypt.Net.BCrypt.Verify(
            dto.password,
            user.passwordHash
        );

        if (!passwordValid)
        {
            return null;
        }

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.email),
            new Claim(ClaimTypes.Name, user.fullname),
            new Claim(ClaimTypes.Role, user.rol)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.Key)
        );

        // firma el jwt utilizando HMAC SHA-256
        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );

        // crea el jtw con su claims, duracion y las credenciales de la firma
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        //convertirlo a texto
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}