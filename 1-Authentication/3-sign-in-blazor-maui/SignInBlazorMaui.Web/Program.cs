using SignInBlazorMaui.Shared.Services;
using SignInBlazorMaui.Web.Components;
using SignInBlazorMaui.Web.Services;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    // Serialize the server-side authentication state so it flows to WebAssembly
    // components without the browser needing to hold any tokens. SerializeAllClaims
    // makes the full set of user claims available to WASM-rendered components.
    .AddAuthenticationStateSerialization(options => options.SerializeAllClaims = true);

// Add device-specific services used by the SignInBlazorMaui.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();
builder.Services.AddScoped<IWeatherService, WeatherService>();

// Per-user profile value. The store is a process-wide singleton (stand-in for a
// database); the Interactive Server components use ServerUserProfileService to read
// and write it in-process.
builder.Services.AddSingleton<ProfileStore>();
builder.Services.AddScoped<IUserProfileService, ServerUserProfileService>();

builder.Services.AddCascadingAuthenticationState();

// Dual authentication: OIDC + Cookie for web browser, JWT Bearer for MAUI API calls.
// A policy scheme routes requests to the correct handler based on the Authorization header.
var authBuilder = builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = "BearerOrCookie";
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddPolicyScheme("BearerOrCookie", "Bearer or Cookie", options =>
    {
        options.ForwardDefaultSelector = context =>
        {
            var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
            if (authHeader?.StartsWith("Bearer ") == true)
                return JwtBearerDefaults.AuthenticationScheme;
            return CookieAuthenticationDefaults.AuthenticationScheme;
        };
    });

// OpenID Connect + Cookie for web browser users
authBuilder.AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

// Forward prompt=create to the CIAM authorize endpoint (for direct register links)
// and handle authentication failures gracefully (e.g. user cancels consent)
builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.Events ??= new OpenIdConnectEvents();
    var existingHandler = options.Events.OnRedirectToIdentityProvider;
    options.Events.OnRedirectToIdentityProvider = async context =>
    {
        if (context.Properties.Items.TryGetValue("prompt", out var prompt))
        {
            context.ProtocolMessage.Prompt = prompt;
        }
        if (existingHandler != null)
            await existingHandler(context);
    };

    options.Events.OnRemoteFailure = context =>
    {
        // Log the real reason so authentication failures aren't silently swallowed.
        var logger = context.HttpContext.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("OidcRemoteFailure");
        logger.LogError(context.Failure, "OIDC remote failure: {Message}", context.Failure?.Message);

        context.Response.Redirect("/");
        context.HandleResponse();
        return Task.CompletedTask;
    };
});

// JWT Bearer validation for MAUI client API calls
authBuilder.AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"),
    jwtBearerScheme: JwtBearerDefaults.AuthenticationScheme);

builder.Services.AddAuthorization();

// For more information on OpenAPI support in ASP.NET Core,
// see OpenAPI support in ASP.NET Core API apps at
// https://learn.microsoft.com/aspnet/core/fundamentals/openapi/overview
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

// The OpenID Connect sign-in/sign-out callbacks arrive as a cross-site form POST
// from the identity provider (response_mode=form_post). .NET 11 adds automatic
// cross-origin CSRF protection that records an invalid antiforgery verdict for such
// cross-site posts, which would stop the OpenID Connect handler from reading the
// response form. These callbacks have their own CSRF protection built into the
// OIDC protocol (the 'state' parameter and correlation/nonce cookies validated by
// the handler), so mark the antiforgery verdict valid for those paths only.
app.Use(async (context, next) =>
{
    var path = context.Request.Path;
    if (path.StartsWithSegments("/signin-oidc") ||
        path.StartsWithSegments("/signout-callback-oidc"))
    {
        context.Features.Set<IAntiforgeryValidationFeature>(new AllowAntiforgeryValidationFeature());
    }

    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(
        typeof(SignInBlazorMaui.Shared._Imports).Assembly,
        typeof(SignInBlazorMaui.Web.Client._Imports).Assembly);

// Login endpoint: triggers OIDC redirect to Entra External ID
app.MapGet("/authentication/login", async (HttpContext context, string? returnUrl) =>
{
    await context.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme,
        new AuthenticationProperties { RedirectUri = returnUrl ?? "/" });
});

// Register endpoint: same OIDC flow with sign-up hints for CIAM
app.MapGet("/authentication/register", async (HttpContext context, string? returnUrl) =>
{
    var properties = new AuthenticationProperties { RedirectUri = returnUrl ?? "/" };
    properties.Items["prompt"] = "create";
    await context.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, properties);
});

// Logout endpoint: clears cookie and signs out of Entra
app.MapPost("/authentication/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    await context.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme,
        new AuthenticationProperties { RedirectUri = "/" });
});

// Add the weather API endpoint and require authorization
app.MapGet("/api/weather", async (IWeatherService weatherService) =>
{
    var forecasts = await weatherService.GetWeatherForecastsAsync();
    return TypedResults.Ok(forecasts);
}).RequireAuthorization();

// Profile API used by the MAUI client (and any WebAssembly client). Reads and writes
// the same server-side ProfileStore that the Interactive Server components use.
// The state-changing PUT disables antiforgery because it is already protected by
// authentication (a bearer token from MAUI, or the same-origin auth cookie).
app.MapGet("/api/profile", (HttpContext context, ProfileStore store) =>
    TypedResults.Ok(store.Get(UserId.From(context.User))))
    .RequireAuthorization();

app.MapPut("/api/profile", (HttpContext context, ProfileStore store, UserProfile profile) =>
{
    store.Set(UserId.From(context.User), profile);
    return TypedResults.NoContent();
}).RequireAuthorization().DisableAntiforgery();

app.Run();

/// <summary>
/// Antiforgery verdict that reports success. Used to mark the OpenID Connect
/// callback paths as valid so .NET 11's automatic cross-origin CSRF protection
/// doesn't block the identity provider's legitimate form_post callback. The OIDC
/// protocol provides its own CSRF protection via the 'state' parameter and the
/// correlation/nonce cookies that the handler validates.
/// </summary>
sealed class AllowAntiforgeryValidationFeature : IAntiforgeryValidationFeature
{
    public bool IsValid => true;

    public Exception? Error => null;
}
