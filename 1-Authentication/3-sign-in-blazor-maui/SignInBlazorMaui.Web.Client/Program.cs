using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SignInBlazorMaui.Shared.Services;
using SignInBlazorMaui.Web.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Blazor authentication/authorization. The server serializes the authentication
// state (see AddAuthenticationStateSerialization in the Web project) and the
// WebAssembly client deserializes it here. No tokens are stored in the browser —
// the browser only ever holds the server's authentication cookie.
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

// HttpClient targeting the host origin. Same-origin requests automatically send
// the authentication cookie, so calls to the protected /api/* endpoints are
// authorized without the client ever handling a token.
builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Device-specific services used by the SignInBlazorMaui.Shared project.
builder.Services.AddSingleton<IFormFactor, FormFactor>();
builder.Services.AddScoped<IWeatherService, ClientWeatherService>();
builder.Services.AddScoped<IUserProfileService, ClientUserProfileService>();

await builder.Build().RunAsync();
