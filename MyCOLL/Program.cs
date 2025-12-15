using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyCOLL.Components;
using MyCOLL.Components.Account;
using MyCOLL.Data;
using MyCOLL.Services;
using MyCOLL.UIComponents.Services;

var builder = WebApplication.CreateBuilder(args);

// ✅ Configure SignalR for file uploads (REQUIRED for InputFile)
builder.Services.Configure<Microsoft.AspNetCore.SignalR.HubOptions>(options =>
{
    options.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10 MB
});

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options =>
    {
        options.DetailedErrors = builder.Environment.IsDevelopment();
    });

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();

// Application Services
builder.Services.AddScoped<CategoriaService>();
builder.Services.AddScoped<ProdutoService>();
builder.Services.AddScoped<ModoEntregaService>();
builder.Services.AddScoped<UserAdminService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<LogService>();
builder.Services.AddScoped<EncomendaService>();
builder.Services.AddScoped<ImageUploadService>(); // ✅ Image upload service

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();


builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddSignInManager();


builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddHttpClient<CollectionApiService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5000/");
});
builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// ✅ MUST be before custom middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/logout", async (SignInManager<ApplicationUser> signInManager, HttpContext context) =>
{
    await signInManager.SignOutAsync();
    context.Response.Redirect("/Account/Login");
});

// ✅ FIXED: Authentication middleware with proper exclusions for SignalR
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value?.ToLower() ?? "";

    // Paths that MUST bypass authentication check
    var excludedPaths = new[]
    {
        "/account/login",
        "/account/logout",
        "/account/register",
        "/_blazor",        // SignalR hub - CRITICAL for Blazor Server & InputFile
        "/_framework",     // Blazor framework files
        "/_content",       // Static content from RCLs
        "/uploads",        // ✅ Image uploads folder
        "/css",
        "/js",
        "/lib",
        "/favicon",
        "/.well-known"
    };

    // Check if path should be excluded
    bool isExcluded = excludedPaths.Any(p => path.StartsWith(p));

    // Fixed boolean logic
    bool isAuthenticated = context.User.Identity?.IsAuthenticated ?? false;

    if (!isExcluded && !isAuthenticated)
    {
        context.Response.Redirect("/Account/Login");
        return;
    }

    await next();
});

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var db = services.GetRequiredService<ApplicationDbContext>();

    await db.Database.MigrateAsync();

    string[] roles = { "Admin", "Gestor", "Cliente", "Fornecedor" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // Admin
    string adminEmail = "admin@mycoll.com";
    string adminPass = "Admin123!";
    var admin = await userManager.FindByEmailAsync(adminEmail);
    if (admin == null)
    {
        admin = new ApplicationUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
        await userManager.CreateAsync(admin, adminPass);
        await userManager.AddToRoleAsync(admin, "Admin");
    }

    // Gestor
    string staffEmail = "gestor@mycoll.com";
    string staffPass = "Gestor123!";
    var staff = await userManager.FindByEmailAsync(staffEmail);
    if (staff == null)
    {
        staff = new ApplicationUser { UserName = staffEmail, Email = staffEmail, EmailConfirmed = true };
        await userManager.CreateAsync(staff, staffPass);
        await userManager.AddToRoleAsync(staff, "Gestor");
    }

    await SeedEncomendas.SeedAsync(db, userManager);
}

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();


app.Run();
