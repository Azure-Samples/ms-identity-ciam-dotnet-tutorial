using SignInBlazorMaui.Services;
using SignInBlazorMaui.Shared;
using SignInBlazorMaui.Shared.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;

namespace SignInBlazorMaui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        // A BlazorWebView is always interactive and, on .NET 10, does not support the
        // explicit render modes that the shared components declare. Clear the render
        // modes so those components render without a render mode inside the WebView.
        // See SignInBlazorMaui.Shared.InteractiveRenderSettings for details.
        InteractiveRenderSettings.ConfigureBlazorHybridRenderModes();

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        // Add MSAL services
        builder.Services.AddMsalClient();

        // Add Blazor authentication and authorization services
        builder.Services.AddAuthorizationCore();
        builder.Services.AddScoped<MsalAuthenticationStateProvider>();
        builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<MsalAuthenticationStateProvider>());

        // Add device-specific services used by the SignInBlazorMaui.Shared project
        builder.Services.AddSingleton<IFormFactor, FormFactor>();
        builder.Services.AddScoped<IWeatherService, WeatherService>();
        builder.Services.AddScoped<IUserProfileService, UserProfileService>();

        return builder.Build();
    }
}
