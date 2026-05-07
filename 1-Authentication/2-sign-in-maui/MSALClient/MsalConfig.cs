// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace SignInMaui.MSALClient;

/// <summary>
/// Configuration for Microsoft Entra External ID (CIAM) authentication,
/// loaded from the embedded appsettings.json resource.
/// </summary>
public class MsalConfig
{
    private static readonly Lazy<MsalConfig> _instance = new(Load);

    /// <summary>
    /// Gets the singleton configuration instance.
    /// </summary>
    public static MsalConfig Instance => _instance.Value;

    /// <summary>
    /// The Application (client) ID from the app registration.
    /// </summary>
    public required string ClientId { get; set; }

    /// <summary>
    /// The CIAM tenant authority URL.
    /// </summary>
    public required string Authority { get; set; }

    /// <summary>
    /// Scopes to request when acquiring tokens.
    /// </summary>
    public required string[] Scopes { get; set; }

    /// <summary>
    /// MSAL redirect URI for native apps.
    /// </summary>
    public string RedirectUri =>
#if WINDOWS
        "http://localhost";
#else
        $"msal{ClientId}://auth";
#endif

    /// <summary>
    /// iOS Keychain security group for token cache persistence.
    /// </summary>
    public const string IosKeychainSecurityGroup = "com.microsoft.adalcache";

    private static MsalConfig Load()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("SignInMaui.appsettings.json")
            ?? throw new InvalidOperationException("Embedded resource 'appsettings.json' not found.");

        var configuration = new ConfigurationBuilder()
            .AddJsonStream(stream)
            .Build();

        var azureAd = configuration.GetSection("AzureAd");
        var downstream = configuration.GetSection("DownstreamApi");

        var config = new MsalConfig
        {
            ClientId = azureAd["ClientId"]
                ?? throw new InvalidOperationException("'AzureAd:ClientId' is required in appsettings.json."),
            Authority = azureAd["Authority"]
                ?? throw new InvalidOperationException("'AzureAd:Authority' is required in appsettings.json."),
            Scopes = downstream.GetSection("Scopes").GetChildren().Select(c => c.Value!).ToArray()
                is { Length: > 0 } scopes ? scopes
                : throw new InvalidOperationException("'DownstreamApi:Scopes' is required in appsettings.json."),
        };

        return config;
    }
}
