using Microsoft.Extensions.Logging;
using MyCOLL.UIComponents.Services;


namespace MyCOLL.Frontend
{
    public static class MauiProgram
    {
        // Configure your Dev Tunnel URL here
        // NOTE: I removed the extra space you had after "https://"
        private const string DevTunnelUrl = "https://7rmpdxhc-7268.uks1.devtunnels.ms";

        // Set to true to use the Tunnel (works for Android, iOS, and Windows)
        private const bool UseDevTunnel = true;

        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            // 1. Get the correct URL (Tunnel or Localhost)
            string baseUrl = GetApiBaseUrl();




            Console.WriteLine($"🔗 API Base URL: {baseUrl}");

            // 2. Register HttpClient with the special headers
            builder.Services.AddScoped(sp =>
            {

                var handler = new HttpClientHandler();

                // Allow self-signed certs (useful if you ever switch back to https localhost)
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

                var client = new HttpClient(handler)
                {
                    BaseAddress = new Uri(baseUrl),
                    // Increased timeout to handle Tunnel latency
                    Timeout = TimeSpan.FromSeconds(60)
                };

                // CRITICAL: This header bypasses the Microsoft "Anti-Phishing" warning page
                client.DefaultRequestHeaders.Add("X-Tunnel-Skip-AntiPhishing-Page", "true");

                // User-Agent helps some firewalls identify your app
                client.DefaultRequestHeaders.Add("User-Agent", "MyCOLL-Mobile-App");

                Console.WriteLine($"📡 HttpClient configured for: {client.BaseAddress}");
                return client;
            });

            // 3. Register your Application Services
            builder.Services.AddScoped<CollectionApiService>();
            builder.Services.AddSingleton<CartService>();
            builder.Services.AddSingleton<UserService>();

            // 4. Register WebView
            builder.Services.AddMauiBlazorWebView();

#if DEBUG

            builder.Logging.AddDebug();
#endif

            // 5. Windows-Specific Fix for Mixed Content (HTTP images)
#if WINDOWS
            Microsoft.Maui.Handlers.WebViewHandler.Mapper.AppendToMapping("BlazorWebView", (handler, view) =>
            {
                // This allows the WebView to load HTTP images on Windows
                if (handler.PlatformView.CoreWebView2 != null)
                {
                    handler.PlatformView.CoreWebView2.Settings.IsWebMessageEnabled = true;
                }
});
#endif

#if ANDROID
            // CORREÇÃO: Usar 'BlazorWebViewMapper' em vez de 'Mapper'
            Microsoft.AspNetCore.Components.WebView.Maui.BlazorWebViewHandler.BlazorWebViewMapper.AppendToMapping("BlazorWebViewMixedContent", (handler, view) =>
            {
                // Acede ao controlo nativo Android (WebView) e ativa o modo Mixed Content
                handler.PlatformView.Settings.MixedContentMode = Android.Webkit.MixedContentHandling.AlwaysAllow;
            });
#endif

            return builder.Build();
        }

        private static string GetApiBaseUrl()
{
    // Priority 1: Use Dev Tunnel if enabled
    if (UseDevTunnel)
    {
        Console.WriteLine("🌐 Using Dev Tunnel configuration");
        return DevTunnelUrl;
    }

    // Priority 2: Use Platform-specific Localhost (Fallback)
    var baseUrl = DeviceInfo.Platform switch
    {
        var p when p == DevicePlatform.Android => "http://10.0.2.2:5225/",
        var p when p == DevicePlatform.iOS => "http://localhost:5225/",
        var p when p == DevicePlatform.MacCatalyst => "http://localhost:5225/",
        var p when p == DevicePlatform.WinUI => "http://localhost:5225/",
        _ => "http://localhost:5225/"
    };

    Console.WriteLine($"📱 Local Platform URL: {baseUrl}");
    return baseUrl;
}
    }
}