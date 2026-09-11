using SignInBlazorMaui.Shared.Services;

namespace SignInBlazorMaui.Web.Client.Services;

/// <summary>
/// WebAssembly implementation of <see cref="IFormFactor"/>. Runs in the browser
/// when a component is rendered with the InteractiveWebAssembly render mode.
/// </summary>
public class FormFactor : IFormFactor
{
    public string GetFormFactor() => "WebAssembly";

    public string GetPlatform() => "Browser (WASM)";
}
