using MyCOLL.UIComponents.Pages;
using MyCOLL.UIComponents.Services;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<CollectionApiService>(sp =>
{
    var client = new HttpClient { BaseAddress = new Uri("https://7rmpdxhc-7268.uks1.devtunnels.ms/") };
    var userService = sp.GetRequiredService<UserService>();
    return new CollectionApiService(client, userService);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<MyCOLL.StoreHost.Components.App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(StoreFront).Assembly);

app.Run();