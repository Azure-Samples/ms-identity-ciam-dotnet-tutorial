using System.Net.Http.Json;
using SignInBlazorMaui.Shared.Services;

namespace SignInBlazorMaui.Web.Client.Services;

/// <summary>
/// WebAssembly implementation of <see cref="IWeatherService"/>. Calls the server's
/// protected <c>/api/weather</c> endpoint over HTTP. Because the request is
/// same-origin, the browser automatically attaches the authentication cookie, so
/// the call is authorized without the client handling any access token.
/// </summary>
public class ClientWeatherService(HttpClient httpClient) : IWeatherService
{
    public async Task<WeatherForecast[]> GetWeatherForecastsAsync() =>
        await httpClient.GetFromJsonAsync<WeatherForecast[]>("api/weather") ?? [];
}
