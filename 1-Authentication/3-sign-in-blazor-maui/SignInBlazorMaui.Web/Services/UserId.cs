using System.Security.Claims;

namespace SignInBlazorMaui.Web.Services;

/// <summary>
/// Resolves a stable, unique identifier for the signed-in user from the available
/// claims. Microsoft Entra External ID issues the object identifier (<c>oid</c>);
/// the other claim types are checked as fallbacks.
/// </summary>
public static class UserId
{
    public static string From(ClaimsPrincipal user) =>
        user.FindFirst("oid")?.Value
        ?? user.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value
        ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? user.FindFirst("sub")?.Value
        ?? user.Identity?.Name
        ?? "unknown";
}
