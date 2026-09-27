using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace PruebaOben.Shared.Services;

public sealed class ApiClient(
    HttpClient httpClient,
    ITokenStore tokenStore,
    JwtAuthenticationStateProvider authenticationStateProvider)
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    public async Task LoginAsync(LoginRequest request)
    {
        using var response = await httpClient.PostAsJsonAsync(
            "api/auth/login", request, JsonOptions);
        await EnsureSuccessAsync(response);

        var login = await response.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions)
            ?? throw new ApiException("La API devolvió una respuesta de login vacía.", response.StatusCode);

        if (string.IsNullOrWhiteSpace(login.token))
        {
            throw new ApiException("La API no devolvió un token JWT.", response.StatusCode);
        }

        await tokenStore.SetAsync(login.token);
        await authenticationStateProvider.RefreshAsync();
    }

    public async Task<IReadOnlyList<UserDto>> GetUsersAsync(bool includeDeleted = false) =>
        await SendAsync<IReadOnlyList<UserDto>>(
            HttpMethod.Get,
            includeDeleted ? "api/users?includeDeleted=true" : "api/users");

    public async Task<UserDto> CreateUserAsync(CreateUserRequest request) =>
        await SendAsync<UserDto>(HttpMethod.Post, "api/users", request);

    public async Task UpdateUserAsync(int id, UpdateUserRequest request) =>
        await SendAsync(HttpMethod.Put, $"api/users/{id}", request);

    public async Task SetUserActiveAsync(UserDto user, bool active) =>
        await UpdateUserAsync(user.id, new UpdateUserRequest
        {
            username = user.username,
            fullname = user.fullname,
            email = user.email,
            rol = user.rol,
            active = active
        });

    public async Task DeleteUserAsync(int id) =>
        await SendAsync(HttpMethod.Delete, $"api/users/{id}");

    public async Task PermanentlyDeleteUserAsync(int id) =>
        await SendAsync(HttpMethod.Delete, $"api/users/{id}/permanent");

    public async Task<AuditPageDto> GetAuditAsync(
        int page,
        int pageSize,
        string? action = null)
    {
        var uri = $"api/audit?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(action))
        {
            uri += $"&action={Uri.EscapeDataString(action)}";
        }

        return await SendAsync<AuditPageDto>(HttpMethod.Get, uri);
    }

    public async Task LogoutAsync()
    {
        await tokenStore.ClearAsync();
        authenticationStateProvider.NotifySignedOut();
    }

    private async Task<T> SendAsync<T>(
        HttpMethod method,
        string uri,
        object? body = null)
    {
        using var response = await SendRequestAsync(method, uri, body);
        await EnsureSuccessAsync(response);

        return await response.Content.ReadFromJsonAsync<T>(JsonOptions)
            ?? throw new ApiException("La API devolvió una respuesta vacía.", response.StatusCode);
    }

    private async Task SendAsync(HttpMethod method, string uri, object? body = null)
    {
        using var response = await SendRequestAsync(method, uri, body);
        await EnsureSuccessAsync(response);
    }

    private async Task<HttpResponseMessage> SendRequestAsync(
        HttpMethod method,
        string uri,
        object? body)
    {
        var token = await tokenStore.GetAsync();
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ApiException("La sesión expiró. Inicia sesión nuevamente.", HttpStatusCode.Unauthorized);
        }

        using var request = new HttpRequestMessage(method, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: JsonOptions);
        }

        return await httpClient.SendAsync(request);
    }

    private async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var message = await ReadErrorMessageAsync(response);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            await tokenStore.ClearAsync();
            authenticationStateProvider.NotifySignedOut();
        }

        throw new ApiException(message, response.StatusCode);
    }

    private static async Task<string> ReadErrorMessageAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        if (!string.IsNullOrWhiteSpace(body))
        {
            try
            {
                using var json = JsonDocument.Parse(body);
                if (json.RootElement.TryGetProperty("message", out var message)
                    && message.ValueKind == JsonValueKind.String)
                {
                    return message.GetString()!;
                }

                if (json.RootElement.TryGetProperty("title", out var title)
                    && title.ValueKind == JsonValueKind.String)
                {
                    return title.GetString()!;
                }
            }
            catch (JsonException)
            {
                return body;
            }
        }

        return $"La API respondió {(int)response.StatusCode} ({response.ReasonPhrase}).";
    }
}

public sealed class ApiException(string message, HttpStatusCode statusCode)
    : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}
