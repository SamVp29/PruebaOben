namespace PruebaOben.Shared.Services;

public interface ITokenStore
{
    Task<string?> GetAsync();
    Task SetAsync(string token);
    Task ClearAsync();
}
