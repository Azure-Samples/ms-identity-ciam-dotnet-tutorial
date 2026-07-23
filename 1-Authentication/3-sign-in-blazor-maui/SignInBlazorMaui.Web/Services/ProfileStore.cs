using System.Collections.Concurrent;
using SignInBlazorMaui.Shared.Services;

namespace SignInBlazorMaui.Web.Services;

/// <summary>
/// In-memory, per-user profile store shared across every server circuit and API
/// call. In a real app this would be backed by a database. It exists to show that
/// Interactive Server components and API endpoints alike have direct, in-process
/// access to server-side state.
/// </summary>
public class ProfileStore
{
    private readonly ConcurrentDictionary<string, UserProfile> _profiles = new();

    public UserProfile Get(string userId) =>
        _profiles.TryGetValue(userId, out var profile)
            ? Copy(profile)
            : new UserProfile();

    public void Set(string userId, UserProfile profile) =>
        _profiles[userId] = Copy(profile);

    private static UserProfile Copy(UserProfile profile) => new()
    {
        DisplayName = profile.DisplayName,
        FavoriteColor = profile.FavoriteColor,
    };
}
