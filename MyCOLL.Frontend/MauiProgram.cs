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
            Console.WriteLine($"API Base URL: {baseUrl}");

            // Register HttpClient with base address
            builder.Services.AddScoped(sp => 
            {
                var handler = new HttpClientHandler();
                
                // For development: Accept self-signed certificates if not using dev tunnel
                #if DEBUG
                if (!UseDevTunnel)
                {
                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                }
                #endif
                
                return new HttpClient(handler) { BaseAddress = new Uri(baseUrl) };
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

            return builder.Build();
        }

        private static string GetApiBaseUrl()
        {
            // Use Dev Tunnel URL when enabled (for testing on physical devices)
            if (UseDevTunnel)
            {
                return DevTunnelUrl;
            }

            // Platform-specific localhost URLs for emulator/simulator testing
            return DeviceInfo.Platform switch
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
        }
    }
}
