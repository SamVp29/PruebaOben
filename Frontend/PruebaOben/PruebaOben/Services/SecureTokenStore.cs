using PruebaOben.Shared.Services;

namespace PruebaOben.Services;

public sealed class SecureTokenStore : ITokenStore
{
    private const string StorageKey = "pruebaoben.jwt";

    public Task<string?> GetAsync() =>
        SecureStorage.Default.GetAsync(StorageKey);

    public Task SetAsync(string token) =>
        SecureStorage.Default.SetAsync(StorageKey, token);

    public Task ClearAsync()
    {
        SecureStorage.Default.Remove(StorageKey);
        return Task.CompletedTask;
    }
}
