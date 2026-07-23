using SignInBlazorMaui.Shared.Services;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SignInBlazorMaui.Services;

/// <summary>
/// MAUI implementation of <see cref="IUserProfileService"/>. Calls the server's
/// protected <c>/api/profile</c> endpoint, attaching the MSAL-acquired access token
/// as a bearer credential.
/// </summary>
public class UserProfileService(MsalAuthenticationStateProvider authStateProvider) : IUserProfileService
{
    public async Task<UserProfile> GetProfileAsync()
    {
        try
        {
            var httpClient = HttpClientHelper.GetHttpClient();

            var accessToken = await authStateProvider.GetAccessTokenAsync();
            if (accessToken is null)
            {
                Debug.WriteLine("No access token available for profile API call.");
                return new UserProfile();
            }

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            return await httpClient.GetFromJsonAsync<UserProfile>(HttpClientHelper.ProfileUrl) ?? new UserProfile();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"An error occurred loading the profile: {ex.Message}");
            return new UserProfile();
        }
    }

    public async Task UpdateProfileAsync(UserProfile profile)
    {
        try
        {
            var httpClient = HttpClientHelper.GetHttpClient();

            var accessToken = await authStateProvider.GetAccessTokenAsync();
            if (accessToken is null)
            {
                Debug.WriteLine("No access token available for profile API call.");
                return;
            }

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await httpClient.PutAsJsonAsync(HttpClientHelper.ProfileUrl, profile);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"An error occurred saving the profile: {ex.Message}");
        }
    }
}
