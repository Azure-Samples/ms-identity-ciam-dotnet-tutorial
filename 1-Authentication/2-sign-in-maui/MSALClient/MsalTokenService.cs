// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace SignInMaui.MSALClient;

/// <summary>
/// Service that wraps MSAL token acquisition, sign-in, and sign-out operations.
/// Registered as a singleton in the DI container.
/// </summary>
public class MsalTokenService(IPublicClientApplication msalClient, ILogger<MsalTokenService> logger)
{
    /// <summary>
    /// Gets the scopes configured for authentication.
    /// </summary>
    public string[] Scopes => MsalConfig.Instance.Scopes;

    /// <summary>
    /// Attempts to sign in silently using cached accounts, then falls back
    /// to interactive sign-in if no cached token is available.
    /// </summary>
    public async Task<AuthenticationResult> SignInAsync()
    {
        var account = await GetAccountAsync();

        try
        {
            if (account is not null)
            {
                return await msalClient
                    .AcquireTokenSilent(Scopes, account)
                    .ExecuteAsync();
            }
            else
            {
                return await SignInInteractiveAsync();
            }
        }
        catch (MsalUiRequiredException ex)
        {
            logger.LogWarning(ex, "Silent auth failed, falling back to interactive");
            return await msalClient
                .AcquireTokenInteractive(Scopes)
                .WithPlatformOptions()
                .ExecuteAsync();
        }
    }

    /// <summary>
    /// Starts an interactive sign-in flow with platform-specific UI.
    /// </summary>
    public async Task<AuthenticationResult> SignInInteractiveAsync()
    {
        if (msalClient.IsUserInteractive())
        {
            return await msalClient.AcquireTokenInteractive(Scopes)
                .WithPlatformOptions()
                .ExecuteAsync();
        }

        // Fallback to device code for non-interactive environments
        return await msalClient.AcquireTokenWithDeviceCode(Scopes, dcr =>
        {
            Console.WriteLine(dcr.Message);
            return Task.CompletedTask;
        }).ExecuteAsync();
    }

    /// <summary>
    /// Signs out the current user by removing all cached accounts.
    /// </summary>
    public async Task SignOutAsync()
    {
        foreach (var account in await msalClient.GetAccountsAsync())
        {
            await msalClient.RemoveAsync(account);
        }
    }

    /// <summary>
    /// Gets the first cached account, if available.
    /// If multiple accounts exist (unexpected), clears all and returns null.
    /// </summary>
    public async Task<IAccount?> GetAccountAsync()
    {
        var accounts = (await msalClient.GetAccountsAsync()).ToList();

        if (accounts.Count > 1)
        {
            foreach (var acc in accounts)
            {
                await msalClient.RemoveAsync(acc);
            }
            return null;
        }

        return accounts.SingleOrDefault();
    }
}
