using PruebaOben.Application.Interfaces;

namespace PruebaOben.Api.Configuration;

public class JwtSettings : IJwtSettings
{
    public string Key { get; set; } = string.Empty;
}