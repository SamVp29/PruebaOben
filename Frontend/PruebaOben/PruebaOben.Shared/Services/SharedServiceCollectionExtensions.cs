using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace PruebaOben.Shared.Services;

public static class SharedServiceCollectionExtensions
{
    public static IServiceCollection AddPruebaObenShared(
        this IServiceCollection services,
        Uri apiBaseAddress)
    {
        services.AddMudServices();
        services.AddAuthorizationCore();
        services.AddScoped<JwtAuthenticationStateProvider>();
        services.AddScoped<AuthenticationStateProvider>(provider =>
            provider.GetRequiredService<JwtAuthenticationStateProvider>());
        services.AddScoped(_ => new HttpClient { BaseAddress = apiBaseAddress });
        services.AddScoped<ApiClient>();
        return services;
    }
}
