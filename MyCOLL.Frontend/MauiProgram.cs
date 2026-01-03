using Microsoft.Extensions.Logging;
using MyCOLL.UIComponents.Services;
using Microsoft.AspNetCore.Components.Authorization;

namespace MyCOLL.Frontend
{
    public static class MauiProgram
    {
        private const string DevTunnelUrl = "https://7rmpdxhc-7268.uks1.devtunnels.ms/";
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

            string baseUrl = GetApiBaseUrl();

            builder.Services.AddScoped(sp =>
            {
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

                var client = new HttpClient(handler)
                {
                    BaseAddress = new Uri(baseUrl),
                    Timeout = TimeSpan.FromSeconds(60)
                };

                client.DefaultRequestHeaders.Add("X-Tunnel-Skip-AntiPhishing-Page", "true");
                client.DefaultRequestHeaders.Add("User-Agent", "MyCOLL-Mobile-App");

                return client;
            });

            builder.Services.AddScoped<CollectionApiService>();
            builder.Services.AddSingleton<CartService>();
            builder.Services.AddScoped<UserService>();
            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<CustomAuthStateProvider>();
            builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthStateProvider>());

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
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

#if ANDROID
            Microsoft.AspNetCore.Components.WebView.Maui.BlazorWebViewHandler.BlazorWebViewMapper.AppendToMapping("BlazorWebViewMixedContent", (handler, view) =>
            {
                handler.PlatformView.Settings.MixedContentMode = Android.Webkit.MixedContentHandling.AlwaysAllow;
            });
#endif

            return builder.Build();
        }

        private static string GetApiBaseUrl()
        {
            if (UseDevTunnel)
                return DevTunnelUrl;

            return DeviceInfo.Platform switch
            {
                var p when p == DevicePlatform.Android => "http://10.0.2.2:5225/",
                var p when p == DevicePlatform.iOS => "http://localhost:5225/",
                var p when p == DevicePlatform.MacCatalyst => "http://localhost:5225/",
                var p when p == DevicePlatform.WinUI => "http://localhost:5225/",
                _ => "http://localhost:5225/"
            };
        }
    }
}