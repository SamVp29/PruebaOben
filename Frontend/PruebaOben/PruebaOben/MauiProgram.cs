using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using PruebaOben.Services;
using PruebaOben.Shared.Services;

namespace PruebaOben
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            var settingsFile = "api-config.json";
#if DEBUG
            settingsFile = "api-config.Development.json";
#endif

            using (var apiSettings = FileSystem.OpenAppPackageFileAsync(settingsFile)
                .GetAwaiter().GetResult())
            {
                builder.Configuration.AddJsonStream(apiSettings);
            }

            var apiBaseAddress = DeviceInfo.Platform == DevicePlatform.Android
                ? builder.Configuration["Api:AndroidBaseAddress"]
                : builder.Configuration["Api:DesktopBaseAddress"];

            apiBaseAddress ??= builder.Configuration["Api:BaseAddress"];
            if (string.IsNullOrWhiteSpace(apiBaseAddress))
            {
                throw new InvalidOperationException(
                    $"Configure la dirección de la API en Resources/Raw/{settingsFile}.");
            }

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddPruebaObenShared(new Uri(apiBaseAddress));
            builder.Services.AddScoped<ITokenStore, SecureTokenStore>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
