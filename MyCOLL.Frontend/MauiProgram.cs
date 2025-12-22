using Microsoft.Extensions.Logging;
using MyCOLL.UIComponents.Services;
using Microsoft.AspNetCore.Components.WebView.Maui;

namespace MyCOLL.Frontend
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

            // 1. CONFIGURAÇÃO HTTPS (Porta 7268)
            // No Android usamos 10.0.2.2, no Windows localhost
            string baseUrl = DeviceInfo.Platform == DevicePlatform.Android
                ? "https://10.0.2.2:7268/"
                : "https://localhost:7268/";

            Console.WriteLine($"🔗 API Base URL: {baseUrl}");

            builder.Services.AddScoped(sp =>
            {
                // Ignorar erros de certificado (ESSENCIAL para HTTPS local no Android)
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

                var client = new HttpClient(handler)
                {
                    BaseAddress = new Uri(baseUrl),
                    Timeout = TimeSpan.FromSeconds(60)
                };

                return client;
            });

            builder.Services.AddScoped<CollectionApiService>();
            builder.Services.AddSingleton<CartService>();
            builder.Services.AddSingleton<UserService>();

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}