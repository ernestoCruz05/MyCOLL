using Microsoft.Extensions.Logging;
using MyCOLL.UIComponents.Services;

namespace MyCOLL.Frontend
{
    public static class MauiProgram
    {
        // Configure your Dev Tunnel URL here
        // Get this from Visual Studio: Tools > Options > Environment > Dev Tunnels
        // Or run: devtunnel host -p 5225
        private const string DevTunnelUrl = "https://YOUR-TUNNEL-ID.devtunnels.ms/";

        // Set to true when using Dev Tunnels for external device testing
        private const bool UseDevTunnel = false;

        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            // Configure API base URL
            string baseUrl = GetApiBaseUrl();
            Console.WriteLine($"🔗 API Base URL: {baseUrl}");

            // Register HttpClient with base address
            builder.Services.AddScoped(sp =>
            {
                var handler = new HttpClientHandler
                {
                    // Timeout aumentado para debugging
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };

                var httpClient = new HttpClient(handler)
                {
                    BaseAddress = new Uri(baseUrl),
                    Timeout = TimeSpan.FromSeconds(30)
                };

                // Log headers para debugging
                Console.WriteLine($"📡 HttpClient configured for: {httpClient.BaseAddress}");

                return httpClient;
            });

            // Register API service
            builder.Services.AddScoped<CollectionApiService>();

            // Register state management services as singletons for shared state
            builder.Services.AddSingleton<CartService>();
            builder.Services.AddSingleton<UserService>();

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

#if WINDOWS
    Microsoft.Maui.Handlers.WebViewHandler.Mapper.AppendToMapping("BlazorWebView", (handler, view) =>
    {
        if (handler.PlatformView.CoreWebView2 != null)
        {
             handler.PlatformView.CoreWebView2.Settings.IsWebMessageEnabled = true;
        }
    });
#endif


            return builder.Build();
        }

        private static string GetApiBaseUrl()
        {
            // Use Dev Tunnel URL when enabled (for testing on physical devices)
            if (UseDevTunnel)
            {
                Console.WriteLine("🌐 Using Dev Tunnel");
                return DevTunnelUrl;
            }

            // Platform-specific localhost URLs for emulator/simulator testing
            var baseUrl = DeviceInfo.Platform switch
            {
                // Android Emulator uses 10.0.2.2 to reach host machine's localhost
                var p when p == DevicePlatform.Android => "http://10.0.2.2:5225/",

                // iOS Simulator can use localhost directly
                var p when p == DevicePlatform.iOS => "http://localhost:5225/",

                // macOS can use localhost directly
                var p when p == DevicePlatform.MacCatalyst => "http://localhost:5225/",

                // Windows can use localhost directly
                var p when p == DevicePlatform.WinUI => "http://localhost:5225/",

                // Default fallback
                _ => "http://localhost:5225/"
            };

            Console.WriteLine($"📱 Platform: {DeviceInfo.Platform} → {baseUrl}");
            return baseUrl;
        }
    }
}
