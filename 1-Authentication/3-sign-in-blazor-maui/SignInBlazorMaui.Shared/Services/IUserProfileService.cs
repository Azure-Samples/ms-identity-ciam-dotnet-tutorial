namespace SignInBlazorMaui.Shared.Services;

/// <summary>
/// Reads and updates a per-user profile value. Each host provides its own
/// implementation:
/// <list type="bullet">
///   <item>The Web (server) implementation talks to the in-process store directly
///   and is used by Interactive Server components.</item>
///   <item>The WebAssembly and MAUI implementations call the protected
///   <c>/api/profile</c> endpoint over HTTP.</item>
/// </list>
/// </summary>
public interface IUserProfileService
{
    Task<UserProfile> GetProfileAsync();

    Task<bool> UpdateProfileAsync(UserProfile profile);
}

/// <summary>
/// A small, editable per-user value used to demonstrate reading and writing
/// server-side state across the different Blazor render modes.
/// </summary>
public class UserProfile
{
    public string DisplayName { get; set; } = string.Empty;

    public string FavoriteColor { get; set; } = string.Empty;
}
