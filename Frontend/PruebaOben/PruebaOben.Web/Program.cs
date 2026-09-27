using PruebaOben.Web.Components;
using PruebaOben.Shared.Services;

var builder = WebApplication.CreateBuilder(args);

var apiBaseAddress = builder.Configuration["Api:BaseAddress"]
    ?? throw new InvalidOperationException(
        "Configure Api:BaseAddress para que la aplicación web pueda llamar a la API.");

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddPruebaObenShared(new Uri(apiBaseAddress));
builder.Services.AddScoped<ITokenStore, WebTokenStore>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddAdditionalAssemblies(
        typeof(PruebaOben.Shared._Imports).Assembly)
    .AddInteractiveServerRenderMode();

app.Run();
