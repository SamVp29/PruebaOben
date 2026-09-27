using Microsoft.JSInterop;

namespace PruebaOben.Shared.Services;

public sealed class WebTokenStore(IJSRuntime jsRuntime) : ITokenStore
{
    private const string StorageKey = "pruebaoben.jwt";

    public Task<string?> GetAsync() =>
        jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", StorageKey).AsTask();

    public Task SetAsync(string token) =>
        jsRuntime.InvokeVoidAsync("sessionStorage.setItem", StorageKey, token).AsTask();

    public Task ClearAsync() =>
        jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", StorageKey).AsTask();
}
