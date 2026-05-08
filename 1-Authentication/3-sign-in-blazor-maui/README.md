---
page_type: sample
name: A .NET MAUI Blazor Hybrid app using MSAL.NET to authenticate users with Microsoft Entra External ID
description: Sign in to a CIAM tenant using a 3-project MAUI Blazor Hybrid + Web solution
languages:
 - csharp
products:
 - msalnet
 - entra-external-id
urlFragment: ms-identity-ciam-dotnet-tutorial-3-sign-in-blazor-maui
extensions:
    services: 
    - ms-identity
    platform: 
    - DotNet
    endpoint: 
    - AAD v2.0
    level: 
    - 200
    client: 
    - MAUI Blazor Hybrid App
---

# A .NET MAUI Blazor Hybrid app using MSAL.NET to authenticate users against Microsoft Entra External ID

* [Overview](#overview)
* [Scenario](#scenario)
* [Prerequisites](#prerequisites)
* [Setup the sample](#setup-the-sample)
* [Explore the sample](#explore-the-sample)
* [About the code](#about-the-code)
* [Troubleshooting](#troubleshooting)
* [Contributing](#contributing)
* [Learn More](#learn-more)

## Overview

This sample demonstrates a **3-project Blazor Hybrid solution** that authenticates users with Microsoft Entra External ID (CIAM):

| Project | Description |
|---|---|
| **SignInBlazorMaui** | .NET MAUI Blazor Hybrid app (iOS, Android, Mac Catalyst, Windows) using MSAL.NET for native sign-in |
| **SignInBlazorMaui.Shared** | Razor Class Library with shared Blazor components (pages, layouts, services) used by both MAUI and Web |
| **SignInBlazorMaui.Web** | ASP.NET Core Blazor Server app using Microsoft.Identity.Web for OIDC sign-in, with a protected weather API |

The same Blazor UI runs natively on mobile/desktop (via MAUI) and in the browser (via Blazor Server), with platform-appropriate authentication on each.

## Scenario

1. The **MAUI app** uses MSAL.NET to sign in a user interactively and obtain a JWT [ID Token](https://aka.ms/id-tokens) from **Microsoft Entra External ID**.
1. The **Web app** uses OpenID Connect (via Microsoft.Identity.Web) to sign in browser users and exposes a protected `/api/weather` endpoint.
1. Both apps share the same Blazor components for displaying user claims, weather data, and account information.

![Scenario Image](./ReadmeFiles/topology.png)

## Prerequisites

* [Visual Studio 2022 17.14+](https://aka.ms/vsdownload) or the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) with the **MAUI** workload installed:
  * [Instructions for Windows](https://learn.microsoft.com/dotnet/maui/get-started/installation?tabs=vswin)
  * [Instructions for MacOS](https://learn.microsoft.com/dotnet/maui/get-started/installation?tabs=vsma)
* An external tenant. To create one, choose from the following methods:
    * (Recommended) Use the [Microsoft Entra External ID extension](https://aka.ms/ciamvscode/readme/marketplace) to set up an external tenant directly in Visual Studio Code.
    * [Create a new external tenant](https://learn.microsoft.com/entra/external-id/customers/how-to-create-external-tenant-portal) in the Microsoft Entra admin center.
* A user account in your **Microsoft Entra External ID** tenant.

> This sample will not work with a **personal Microsoft account**. If you're signed in to the [Azure portal](https://portal.azure.com) with a personal Microsoft account and have not created a user account in your directory before, you will need to create one before proceeding.

## Setup the sample

### Step 1: Clone or download this repository

From your shell or command line:

```console
git clone https://github.com/Azure-Samples/ms-identity-ciam-dotnet-tutorial.git
```

or download and extract the repository *.zip* file.

> :warning: To avoid path length limitations on Windows, we recommend cloning into a directory near the root of your drive.

### Step 2: Navigate to project folder

```console
cd 1-Authentication\3-sign-in-blazor-maui
```

### Step 3: Register the sample application(s) in your tenant

This sample requires **two** app registrations — a public client for the MAUI app and a confidential client for the Web app. You can:

- follow the steps below to manually register your apps
- or use the PowerShell scripts that **automatically** create both registrations, set permissions, create a user flow, and patch all configuration files

<details>
   <summary>Expand this section if you want to use the automated scripts:</summary>

> :warning: If you have never used **Microsoft Graph PowerShell** before, we recommend you go through the [App Creation Scripts Guide](./AppCreationScripts/AppCreationScripts.md) once to ensure that your environment is prepared correctly for this step.

1. Ensure that you have [PowerShell 7](https://learn.microsoft.com/powershell/scripting/install/installing-powershell-on-windows?view=powershell-7.3) or later.
1. Run the script to create your Azure AD applications and configure the code:

    ```PowerShell
    cd .\AppCreationScripts\
    .\Configure.ps1 -TenantId "[Optional] - your tenant id"
    ```

   The script will:
   - Create a **Web app** registration (`ciam-dotnet-blazor-maui-web`) with OIDC redirect URI, API scope (`access_as_user`), and client secret
   - Create a **MAUI app** registration (`ciam-dotnet-blazor-maui`) with MSAL redirect URIs and permission to the Web API
   - Create a sign-up/sign-in user flow and link both apps
   - Patch `SignInBlazorMaui/appsettings.json`, `SignInBlazorMaui.Web/appsettings.json`, `AndroidManifest.xml`, and `Info.plist`

> Other ways of running the scripts are described in [App Creation Scripts guide](./AppCreationScripts/AppCreationScripts.md).

</details>

#### Manual registration

##### Register the Web app (ciam-dotnet-blazor-maui-web)

1. Navigate to the [Azure portal](https://portal.azure.com) and select **Microsoft Entra External ID**.
1. Select **App Registrations** > **New registration**.
1. Enter name: `ciam-dotnet-blazor-maui-web`, select **Accounts in this organizational directory only**.
1. Under **Authentication** > **Web** platform, add redirect URI: `https://localhost:7157/signin-oidc`
1. Enable **ID tokens** under Implicit grant.
1. Under **Certificates & secrets**, create a new client secret and note the value.
1. Under **Expose an API**, set Application ID URI to `api://{clientId}` and add a scope `access_as_user`.
1. Under **API permissions**, add `openid` and `offline_access` from Microsoft Graph.
1. Update `SignInBlazorMaui.Web/appsettings.json` with the Instance, TenantId, ClientId, and ClientSecret.

##### Register the MAUI app (ciam-dotnet-blazor-maui)

1. Create another app registration: `ciam-dotnet-blazor-maui`, select **Accounts in this organizational directory only**.
1. Enable **Allow public client flows** (under Authentication > Advanced settings).
1. Add redirect URIs under **Mobile and desktop applications**: `msal{ClientId}://auth` and `http://localhost`
1. Under **API permissions**, add `openid`, `offline_access` from Microsoft Graph and `access_as_user` from the Web app.
1. Update `SignInBlazorMaui/appsettings.json` with Authority and ClientId.
1. Update `Platforms/Android/AndroidManifest.xml` and `Platforms/iOS/Info.plist` with the MSAL redirect scheme.

##### Create a user flow

1. In the Entra admin center, go to **External Identities** > **User flows**.
1. Create a sign-up/sign-in flow and link both app registrations to it.

### Step 4: Running the sample

#### Run the Web app

```console
cd SignInBlazorMaui.Web
dotnet run
```

Open `https://localhost:7157` in your browser.

#### Run the MAUI app

Choose the platform you want to work on by setting the startup project in the Solution Explorer. Make sure that your platform of choice is marked for build and deploy in the Configuration Manager.

Clean the solution, rebuild the solution, and run it.

## Explore the sample

### Web app

Open `https://localhost:7157`. You'll be redirected to sign in with Microsoft Entra External ID via OIDC. After signing in, you'll see the home page with your identity, and can navigate to **Weather** and **My Claims**.

### MAUI app

Click the **Sign in with Microsoft** button. On iOS/Mac Catalyst, a system browser sheet opens; on Android, a Chrome Custom Tab; on Windows, an embedded WebView2 dialog. After signing in, you can view your claims and weather data.

## About the code

### Solution structure

The solution uses a **shared Razor Class Library** pattern:

- **SignInBlazorMaui.Shared** contains all shared Blazor components (Home, Account/Claims, Weather, Counter pages) and layout (MainLayout, NavMenu). Service interfaces (`IFormFactor`, `IWeatherService`) are defined here.

- **SignInBlazorMaui** (MAUI) provides native implementations: `MsalAuthenticationStateProvider` for MSAL-based auth, `FormFactor` using `DeviceInfo`, and platform-specific code for each target (Android `MsalActivity`, iOS `OpenUrl` handler, Mac Catalyst `ASWebAuthenticationSession` workaround, Windows WinUI3).

- **SignInBlazorMaui.Web** provides server-side implementations: Microsoft.Identity.Web for OIDC auth, `FormFactor` returning "Web", `WeatherService` generating mock data, and a `/api/weather` protected endpoint.

### MAUI authentication

MSAL configuration is loaded from an embedded `appsettings.json` via `MsalConfig.cs`. The `MsalServiceExtensions.AddMsalClient()` method builds the `IPublicClientApplication` with platform-specific options:

- **iOS**: System web view via `WithSystemWebViewOptions`
- **Android**: Parent activity via `WithParentActivityOrWindow`
- **Mac Catalyst**: `ASWebAuthenticationSession` via custom `ICustomWebUi` (workaround for missing maccatalyst TFM in MSAL — [tracking issue](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues/3527))
- **Windows**: Embedded WebView2 via `WithWindowsDesktopFeatures`

Token cache persistence uses `SecureStorage` on Windows and Mac Catalyst (MSAL handles iOS Keychain and Android SharedPreferences natively).

### Web authentication

The Web project uses a dual authentication scheme:
- **OpenID Connect + Cookie** for browser users (via `AddMicrosoftIdentityWebApp`)
- **JWT Bearer** for MAUI API calls (via `AddMicrosoftIdentityWebApi`)

A policy scheme (`BearerOrCookie`) routes requests based on the `Authorization` header.

### iOS specific considerations

The `Platforms/iOS/AppDelegate.cs` must handle the MSAL redirect:

```csharp
public override bool OpenUrl(UIApplication application, NSUrl url, NSDictionary options)
{
    AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(url);
    return base.OpenUrl(application, url, options);
}
```

Enable Keychain access in `Entitlements.plist` with the `com.microsoft.adalcache` group.

### Mac Catalyst specific considerations

MSAL doesn't ship a maccatalyst TFM yet, so `MacCatalystWebUi.cs` provides an `ASWebAuthenticationSession` workaround via `ICustomWebUi`. Remove this file once MSAL ships Mac Catalyst support.

## Troubleshooting

### Some projects don't load in Visual Studio

You need the **.NET Multi-platform App UI development** [workload](https://learn.microsoft.com/en-us/visualstudio/install/modify-visual-studio?view=vs-2022) in the Visual Studio Installer.

### The project you want is not built

Right-click the solution, choose **Configuration Properties** > **Configuration** and make sure the projects and configuration you want are checked for build and deploy.

### `AADSTS700054: response_type 'id_token' is not enabled`

Enable **ID tokens** in the Web app registration under **Authentication** > **Implicit grant and hybrid flows**.

### `consent_required` error on Web

Grant admin consent for the app's permissions in the Azure portal, or sign in as an admin and consent when prompted.

## Contributing

If you'd like to contribute to this sample, see [CONTRIBUTING.MD](/CONTRIBUTING.md).

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information, see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Learn More

* [Microsoft Entra External ID documentation](https://learn.microsoft.com/entra/external-id/)
* [MSAL.NET documentation](https://learn.microsoft.com/azure/active-directory/develop/msal-net-initializing-client-applications)
* [Microsoft.Identity.Web documentation](https://learn.microsoft.com/azure/active-directory/develop/microsoft-identity-web)
* [Blazor Hybrid documentation](https://learn.microsoft.com/aspnet/core/blazor/hybrid/)
* [Token cache serialization in MSAL.NET](https://learn.microsoft.com/azure/active-directory/develop/msal-net-token-cache-serialization)
* [Building Zero Trust ready apps](https://aka.ms/ztdevsession)
