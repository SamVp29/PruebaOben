using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace PruebaOben.Shared.Services;

public sealed class JwtAuthenticationStateProvider(ITokenStore tokenStore)
    : AuthenticationStateProvider
{
    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await tokenStore.GetAsync();
        if (string.IsNullOrWhiteSpace(token))
        {
            return Anonymous;
        }

        try
        {
            var principal = CreatePrincipal(token);
            if (principal is null)
            {
                await tokenStore.ClearAsync();
                return Anonymous;
            }

            return new AuthenticationState(principal);
        }
        catch (FormatException)
        {
            await tokenStore.ClearAsync();
            return Anonymous;
        }
        catch (JsonException)
        {
            await tokenStore.ClearAsync();
            return Anonymous;
        }
    }

    public async Task RefreshAsync()
    {
        var state = await GetAuthenticationStateAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(state));
    }

    public void NotifySignedOut() =>
        NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));

    private static ClaimsPrincipal? CreatePrincipal(string token)
    {
        var parts = token.Split('.');
        if (parts.Length != 3)
        {
            return null;
        }

        var payload = parts[1].Replace('-', '+').Replace('_', '/');
        payload = payload.PadRight(payload.Length + ((4 - payload.Length % 4) % 4), '=');
        using var json = JsonDocument.Parse(Encoding.UTF8.GetString(
            Convert.FromBase64String(payload)));

        if (!json.RootElement.TryGetProperty("exp", out var expiresAt)
            || !expiresAt.TryGetInt64(out var unixTime))
        {
            return null;
        }

        DateTimeOffset expiration;
        try
        {
            expiration = DateTimeOffset.FromUnixTimeSeconds(unixTime);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }

        if (expiration <= DateTimeOffset.UtcNow)
        {
            return null;
        }

        var claims = new List<Claim>();
        AddClaim(json.RootElement, "sub", ClaimTypes.NameIdentifier, claims);
        AddClaim(json.RootElement, "email", ClaimTypes.Email, claims);
        AddClaim(json.RootElement, "unique_name", ClaimTypes.Name, claims);
        AddClaim(json.RootElement, "name", ClaimTypes.Name, claims);
        AddClaim(json.RootElement, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",
            ClaimTypes.Name, claims);
        AddClaim(json.RootElement, "role", ClaimTypes.Role, claims);
        AddClaim(json.RootElement, "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
            ClaimTypes.Role, claims);

        return claims.Count == 0
            ? null
            : new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
    }

    private static void AddClaim(
        JsonElement payload,
        string propertyName,
        string claimType,
        ICollection<Claim> claims)
    {
        if (payload.TryGetProperty(propertyName, out var value)
            && value.ValueKind == JsonValueKind.String
            && !string.IsNullOrWhiteSpace(value.GetString())
            && !claims.Any(claim => claim.Type == claimType))
        {
            claims.Add(new Claim(claimType, value.GetString()!));
        }
    }
}
