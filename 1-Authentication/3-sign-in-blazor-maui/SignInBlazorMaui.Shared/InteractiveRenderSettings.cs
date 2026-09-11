using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace SignInBlazorMaui.Shared;

/// <summary>
/// Indirection for the Blazor interactive render modes so that the shared Razor
/// components can declare a per-page <c>@rendermode</c> and still be hosted by the
/// MAUI Blazor Hybrid app.
///
/// Why this is needed on .NET 10
/// -----------------------------
/// The shared components in this RCL are rendered by three hosts: the Blazor Web
/// App (server), its WebAssembly client, and the MAUI <c>BlazorWebView</c>. On the
/// web, per-page render modes such as <c>InteractiveWebAssembly</c> are exactly what
/// we want. But a <c>BlazorWebView</c> is *always* interactive and, on .NET 10, its
/// renderer throws when it encounters a component that specifies a render mode:
///
///     NotSupportedException: Cannot supply a component of type '...' because the
///     current platform does not support the render mode 'InteractiveWebAssembly'.
///
/// To let the same components run in all three hosts, the pages reference these
/// nullable properties (via <c>@using static</c> in <c>_Imports.razor</c>) instead of
/// the real <see cref="RenderMode"/> values. On the web the properties keep their
/// default (real) render modes, so per-page interactivity works as intended. In the
/// MAUI app, <see cref="ConfigureBlazorHybridRenderModes"/> is called at startup to
/// set them all to <c>null</c>, which means "no render mode" — the components simply
/// render interactively in the WebView.
///
/// On .NET 11 this workaround is no longer required: dotnet/aspnetcore#65876 makes
/// the Blazor Hybrid renderer treat render modes as no-ops, so the shared components
/// can use the built-in <see cref="RenderMode"/> values directly and this class can
/// be removed.
/// </summary>
public static class InteractiveRenderSettings
{
    public static IComponentRenderMode? InteractiveServer { get; set; } = RenderMode.InteractiveServer;

    public static IComponentRenderMode? InteractiveAuto { get; set; } = RenderMode.InteractiveAuto;

    public static IComponentRenderMode? InteractiveWebAssembly { get; set; } = RenderMode.InteractiveWebAssembly;

    /// <summary>
    /// Clears the render modes so shared components render without a render mode.
    /// Call this once from the MAUI app's startup (<c>MauiProgram.CreateMauiApp</c>),
    /// because a <c>BlazorWebView</c> is always interactive and does not support
    /// explicit render modes on .NET 10.
    /// </summary>
    public static void ConfigureBlazorHybridRenderModes()
    {
        InteractiveServer = null;
        InteractiveAuto = null;
        InteractiveWebAssembly = null;
    }
}
