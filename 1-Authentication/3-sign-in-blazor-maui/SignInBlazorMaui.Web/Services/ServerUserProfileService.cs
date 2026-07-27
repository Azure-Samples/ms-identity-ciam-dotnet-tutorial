using Microsoft.AspNetCore.Components.Authorization;
using SignInBlazorMaui.Shared.Services;

namespace SignInBlazorMaui.Web.Services;

/// <summary>
/// Server-side <see cref="IUserProfileService"/> used by Interactive Server
/// components. It reads and writes the shared <see cref="ProfileStore"/> in-process,
/// resolving the current user from the circuit's authentication state. No HTTP
/// round-trip and no access token are involved.
/// </summary>
public class ServerUserProfileService(
    AuthenticationStateProvider authenticationStateProvider,
    ProfileStore store) : IUserProfileService
{
    public async Task<UserProfile> GetProfileAsync() =>
        store.Get(await GetUserIdAsync());

    public async Task<bool> UpdateProfileAsync(UserProfile profile)
    {
        store.Set(await GetUserIdAsync(), profile);
        return true;
    }

    private async Task<string> GetUserIdAsync()
    {
        var state = await authenticationStateProvider.GetAuthenticationStateAsync();
        return UserId.From(state.User);
    }
}
