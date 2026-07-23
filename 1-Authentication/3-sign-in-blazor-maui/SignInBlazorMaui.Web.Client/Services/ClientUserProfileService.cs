using System.Net.Http.Json;
using SignInBlazorMaui.Shared.Services;

namespace SignInBlazorMaui.Web.Client.Services;

/// <summary>
/// WebAssembly implementation of <see cref="IUserProfileService"/>. Calls the
/// server's protected <c>/api/profile</c> endpoint. The request is same-origin, so
/// the browser attaches the authentication cookie automatically and no token is
/// handled by the client.
/// </summary>
public class ClientUserProfileService(HttpClient httpClient) : IUserProfileService
{
    public async Task<UserProfile> GetProfileAsync() =>
        await httpClient.GetFromJsonAsync<UserProfile>("api/profile") ?? new UserProfile();

    public async Task UpdateProfileAsync(UserProfile profile)
    {
        var response = await httpClient.PutAsJsonAsync("api/profile", profile);
        response.EnsureSuccessStatusCode();
    }
}
