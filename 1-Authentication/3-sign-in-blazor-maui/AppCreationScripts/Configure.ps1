#Requires -Version 7

<#
.SYNOPSIS
    Creates Azure AD app registrations for this MAUI Blazor Hybrid sample using Microsoft Graph PowerShell.

.DESCRIPTION
    This script creates two app registrations in your Entra External ID (CIAM) tenant:
      1. A public client (MAUI app) with MSAL redirect URIs
      2. A confidential client (Web app) with OIDC redirect URI, API scope, and client secret

    It also creates a sign-up/sign-in user flow, links both apps, and patches
    all configuration files in the sample project.

    Prerequisites: Microsoft Graph PowerShell SDK
      Install-Module Microsoft.Graph.Applications -Scope CurrentUser
      Install-Module Microsoft.Graph.Identity.DirectoryManagement -Scope CurrentUser
      Install-Module Microsoft.Graph.Identity.SignIns -Scope CurrentUser

.PARAMETER TenantId
    Optional. The tenant ID (GUID) or domain (e.g., "contoso.onmicrosoft.com").

.PARAMETER MauiAppName
    Optional. The MAUI app registration display name. Defaults to "ciam-dotnet-blazor-maui".

.PARAMETER WebAppName
    Optional. The Web app registration display name. Defaults to "ciam-dotnet-blazor-maui-web".

.PARAMETER AzureEnvironmentName
    Optional. Azure cloud environment. Defaults to "Global".
    Accepted values: Global, AzureChinaCloud, AzureUSGovernment.

.EXAMPLE
    ./Configure.ps1
    ./Configure.ps1 -TenantId "contoso.onmicrosoft.com"
    ./Configure.ps1 -TenantId "contoso" -AzureEnvironmentName "AzureUSGovernment"
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string] $TenantId,

    [Parameter(Mandatory = $false)]
    [string] $MauiAppName = "ciam-dotnet-blazor-maui",

    [Parameter(Mandatory = $false)]
    [string] $WebAppName = "ciam-dotnet-blazor-maui-web",

    [Parameter(Mandatory = $false)]
    [string] $FlowName = "signup_signin",

    [Parameter(Mandatory = $false)]
    [string] $AzureEnvironmentName = "Global"
)

$ErrorActionPreference = "Stop"
$ProjectDir = Resolve-Path (Join-Path $PSScriptRoot "..")

# --- Ensure we have a tenant ---
if (-not $TenantId) {
    Write-Host ""
    Write-Host "Enter your CIAM tenant domain prefix"
    Write-Host "(e.g., 'contoso' from contoso.onmicrosoft.com): " -ForegroundColor Magenta -NoNewline
    $TenantId = Read-Host
    if ([string]::IsNullOrWhiteSpace($TenantId)) {
        Write-Error "Tenant domain is required."
        exit 1
    }
}
if ($TenantId -notmatch "\.") {
    $TenantId = "$TenantId.onmicrosoft.com"
}

# --- Ensure required modules are installed ---
$requiredModules = @("Microsoft.Graph.Applications", "Microsoft.Graph.Identity.DirectoryManagement", "Microsoft.Graph.Identity.SignIns")
foreach ($module in $requiredModules) {
    if (-not (Get-Module -ListAvailable -Name $module)) {
        Write-Host "Installing $module..."
        Install-Module $module -Scope CurrentUser -Force
    }
    Import-Module $module
}

# --- Connect to Microsoft Graph ---
Write-Host ""
Write-Host "Connecting to Microsoft Graph for tenant '$TenantId'..."
Connect-MgGraph -TenantId $TenantId `
    -Scopes "Application.ReadWrite.All", "Organization.Read.All", "IdentityUserFlow.ReadWrite.All" `
    -Environment $AzureEnvironmentName `
    -NoWelcome

$context = Get-MgContext
$TenantId = $context.TenantId
$AccountName = $context.Account

$org = Get-MgOrganization
$TenantDomain = ($org.VerifiedDomains | Where-Object { $_.IsDefault }).Name
$TenantName = $TenantDomain -replace "\.onmicrosoft\.com$", ""

Write-Host "Connected to tenant '$TenantName' ($TenantId) as '$AccountName'"

# --- Helper: Create or reuse app registration ---
function New-OrReuseApp {
    param([string]$Name, [hashtable[]]$Permissions, [switch]$IsPublicClient)

    $existing = Get-MgApplication -Filter "displayName eq '$Name'" -Top 1 -ErrorAction SilentlyContinue
    if ($existing) {
        Write-Host ""
        Write-Host "App '$Name' already exists with AppId: $($existing.AppId)"
        $response = Read-Host "Delete and recreate? (y/N)"
        if ($response -match "^[Yy]$") {
            Remove-MgApplication -ApplicationId $existing.Id
            Write-Host "Deleted existing app registration."
            $existing = $null
        }
    }

    if ($existing) {
        Write-Host "Using existing app registration '$Name'."
        return $existing
    }

    Write-Host ""
    Write-Host "Creating app registration '$Name'..."

    $params = @{
        DisplayName           = $Name
        SignInAudience        = "AzureADMyOrg"
        RequiredResourceAccess = $Permissions
    }
    if ($IsPublicClient) { $params.IsFallbackPublicClient = $true }

    $app = New-MgApplication @params
    Write-Host "Created app with AppId: $($app.AppId)"

    try {
        New-MgServicePrincipal -AppId $app.AppId -ErrorAction Stop | Out-Null
        Write-Host "Created service principal."
        Start-Sleep -Seconds 3
    } catch {
        Write-Host "Service principal already exists (non-fatal)."
    }

    return $app
}

# --- 1. Create Web app (confidential client) ---
$graphPermission = @{
    ResourceAppId  = "00000003-0000-0000-c000-000000000000"
    ResourceAccess = @(
        @{ Id = "37f7f235-527c-4136-accd-4a02d197296e"; Type = "Scope" },  # openid
        @{ Id = "7427e0e9-2fba-42fe-b0c0-848c9e6a8182"; Type = "Scope" }   # offline_access
    )
}

$webApp = New-OrReuseApp -Name $WebAppName -Permissions @($graphPermission)
$WebAppId = $webApp.AppId
$WebObjId = $webApp.Id

# Set Web redirect URI (OIDC callback) and enable ID token implicit grant
Update-MgApplication -ApplicationId $WebObjId -Web @{
    RedirectUris = @("https://localhost:7157/signin-oidc")
    ImplicitGrantSettings = @{
        EnableIdTokenIssuance = $true
    }
}
Write-Host "Set Web redirect URI: https://localhost:7157/signin-oidc (ID tokens enabled)"

# Expose an API scope: api://{webClientId}/access_as_user
$apiScopeId = [Guid]::NewGuid().ToString()
Update-MgApplication -ApplicationId $WebObjId `
    -IdentifierUris @("api://$WebAppId") `
    -Api @{
        Oauth2PermissionScopes = @(
            @{
                Id                      = $apiScopeId
                AdminConsentDisplayName = "Access as user"
                AdminConsentDescription = "Allow the MAUI app to access the Web API on behalf of the user"
                Value                   = "access_as_user"
                Type                    = "User"
                IsEnabled               = $true
            }
        )
    }
Write-Host "Exposed API scope: api://$WebAppId/access_as_user"

# Create a client secret for the Web app
$secret = Add-MgApplicationPassword -ApplicationId $WebObjId -PasswordCredential @{
    DisplayName = "Auto-generated by Configure.ps1"
}
$ClientSecret = $secret.SecretText
Write-Host "Created client secret (expires: $($secret.EndDateTime))"

# --- 2. Create MAUI app (public client) ---
$apiPermission = @{
    ResourceAppId  = $WebAppId
    ResourceAccess = @(
        @{ Id = $apiScopeId; Type = "Scope" }  # access_as_user
    )
}

$mauiApp = New-OrReuseApp -Name $MauiAppName -Permissions @($graphPermission, $apiPermission) -IsPublicClient
$MauiAppId = $mauiApp.AppId
$MauiObjId = $mauiApp.Id

# Set MAUI redirect URIs
Update-MgApplication -ApplicationId $MauiObjId -PublicClient @{
    RedirectUris = @("msal${MauiAppId}://auth", "http://localhost")
}
Write-Host "Set MAUI redirect URIs: msal${MauiAppId}://auth, http://localhost"

Write-Host ""
Write-Host "  Waiting for propagation..."
Start-Sleep -Seconds 5

# --- Patch configuration files ---
Write-Host ""
Write-Host "Updating configuration files..."

# MAUI appsettings.json
$mauiSettingsPath = Join-Path $ProjectDir "SignInBlazorMaui" "appsettings.json"
if (Test-Path $mauiSettingsPath) {
    $settings = Get-Content $mauiSettingsPath -Raw | ConvertFrom-Json
    $settings.AzureAd.Authority = "https://$TenantName.ciamlogin.com/$TenantId"
    $settings.AzureAd.ClientId = $MauiAppId
    $settings.DownstreamApi.Scopes = @("api://$WebAppId/access_as_user")
    $settings | ConvertTo-Json -Depth 10 | Set-Content $mauiSettingsPath -Encoding UTF8
    Write-Host "  ✅ SignInBlazorMaui/appsettings.json"
} else {
    Write-Host "  ❌ SignInBlazorMaui/appsettings.json — file not found"
}

# Web appsettings.json
$webSettingsPath = Join-Path $ProjectDir "SignInBlazorMaui.Web" "appsettings.json"
if (Test-Path $webSettingsPath) {
    $settings = Get-Content $webSettingsPath -Raw | ConvertFrom-Json
    $settings.AzureAd.Instance = "https://$TenantName.ciamlogin.com/"
    $settings.AzureAd.TenantId = $TenantId
    $settings.AzureAd.ClientId = $WebAppId
    $settings.AzureAd.ClientSecret = $ClientSecret
    $settings | ConvertTo-Json -Depth 10 | Set-Content $webSettingsPath -Encoding UTF8
    Write-Host "  ✅ SignInBlazorMaui.Web/appsettings.json"
} else {
    Write-Host "  ❌ SignInBlazorMaui.Web/appsettings.json — file not found"
}

# AndroidManifest.xml
$manifestPath = Join-Path $ProjectDir "SignInBlazorMaui" "Platforms" "Android" "AndroidManifest.xml"
if (Test-Path $manifestPath) {
    [xml]$manifest = Get-Content $manifestPath
    $ns = New-Object System.Xml.XmlNamespaceManager($manifest.NameTable)
    $ns.AddNamespace("android", "http://schemas.android.com/apk/res/android")
    $dataNodes = $manifest.SelectNodes("//data[@android:host='auth']", $ns)
    foreach ($node in $dataNodes) {
        $null = $node.SetAttribute("scheme", "http://schemas.android.com/apk/res/android", "msal$MauiAppId")
    }
    $manifest.Save($manifestPath)
    Write-Host "  ✅ SignInBlazorMaui/Platforms/Android/AndroidManifest.xml"
} else {
    Write-Host "  ❌ AndroidManifest.xml — file not found"
}

# iOS Info.plist
$plistPath = Join-Path $ProjectDir "SignInBlazorMaui" "Platforms" "iOS" "Info.plist"
if (Test-Path $plistPath) {
    $content = Get-Content $plistPath -Raw
    $content = $content -replace 'msalEnter_the_Application_Id_Here', "msal$MauiAppId"
    Set-Content $plistPath $content -Encoding UTF8
    Write-Host "  ✅ SignInBlazorMaui/Platforms/iOS/Info.plist"
} else {
    Write-Host "  ❌ Info.plist — file not found"
}

# --- Create user flow ---
Write-Host ""
Write-Host "Setting up sign-up/sign-in user flow..."

$existingFlows = $null
try {
    $existingFlows = Invoke-MgGraphRequest -Method GET `
        -Uri "/v1.0/identity/authenticationEventsFlows" `
        -ErrorAction Stop
} catch { }

$existingFlow = $null
if ($existingFlows.value) {
    $existingFlow = $existingFlows.value | Where-Object { $_.displayName -eq $FlowName } | Select-Object -First 1
}

# Link both apps to the flow
$appsToLink = @($MauiAppId, $WebAppId)

if ($existingFlow) {
    Write-Host "  ✅ User flow '$FlowName' already exists (id: $($existingFlow.id))."

    $linked = Invoke-MgGraphRequest -Method GET `
        -Uri "/v1.0/identity/authenticationEventsFlows/$($existingFlow.id)/conditions/applications/includeApplications"
    $linkedAppIds = @()
    if ($linked.value) {
        $linkedAppIds = $linked.value | ForEach-Object { $_.appId }
    }

    foreach ($appToLink in $appsToLink) {
        if ($linkedAppIds -contains $appToLink) {
            Write-Host "  ✅ App $appToLink already linked to flow."
        } else {
            for ($attempt = 1; $attempt -le 5; $attempt++) {
                try {
                    $null = Invoke-MgGraphRequest -Method POST `
                        -Uri "/v1.0/identity/authenticationEventsFlows/$($existingFlow.id)/conditions/applications/includeApplications" `
                        -Body @{ appId = $appToLink } `
                        -ErrorAction Stop
                    Write-Host "  ✅ App $appToLink linked to flow."
                    break
                } catch {
                    if ($attempt -lt 5) {
                        Write-Host "  ⏳ Waiting for propagation (attempt $attempt/5)..."
                        Start-Sleep -Seconds 5
                    } else {
                        Write-Host "  ❌ Could not link app $appToLink after $attempt attempts: $_" -ForegroundColor Red
                    }
                }
            }
        }
    }
} else {
    Write-Host "  Creating user flow '$FlowName'..."

    $userFlowBody = @{
        "@odata.type" = "#microsoft.graph.externalUsersSelfServiceSignUpEventsFlow"
        displayName   = $FlowName
        conditions     = @{
            applications = @{
                includeApplications = @(
                    @{ appId = $MauiAppId },
                    @{ appId = $WebAppId }
                )
            }
        }
        onAuthenticationMethodLoadStart = @{
            "@odata.type"     = "#microsoft.graph.onAuthenticationMethodLoadStartExternalUsersSelfServiceSignUp"
            identityProviders = @(
                @{ id = "EmailPassword-OAUTH" }
            )
        }
        onInteractiveAuthFlowStart = @{
            "@odata.type"    = "#microsoft.graph.onInteractiveAuthFlowStartExternalUsersSelfServiceSignUp"
            isSignUpAllowed  = $true
        }
        onAttributeCollection = @{
            "@odata.type" = "#microsoft.graph.onAttributeCollectionExternalUsersSelfServiceSignUp"
            attributes    = @(
                @{ id = "email"; displayName = "Email Address"; description = "Email address of the user"; userFlowAttributeType = "builtIn"; dataType = "string" },
                @{ id = "displayName"; displayName = "Display Name"; description = "Display Name of the User."; userFlowAttributeType = "builtIn"; dataType = "string" }
            )
            attributeCollectionPage = @{
                views = @(
                    @{
                        inputs = @(
                            @{ attribute = "email"; label = "Email Address"; inputType = "text"; hidden = $true; editable = $false; writeToDirectory = $true; required = $true },
                            @{ attribute = "displayName"; label = "Display Name"; inputType = "text"; hidden = $false; editable = $true; writeToDirectory = $true; required = $false }
                        )
                    }
                )
            }
        }
    }

    try {
        $null = Invoke-MgGraphRequest -Method POST `
            -Uri "/v1.0/identity/authenticationEventsFlows" `
            -Body $userFlowBody `
            -ErrorAction Stop
        Write-Host "  ✅ User flow '$FlowName' created and linked to both apps."
    } catch {
        Write-Host "  ❌ Failed to create user flow." -ForegroundColor Red
        Write-Host "     $_" -ForegroundColor Red
        Write-Host ""
        Write-Host "  This API requires a Microsoft Entra External ID (CIAM) tenant." -ForegroundColor Yellow
        Write-Host "  If using a standard Azure AD tenant, create the flow manually:" -ForegroundColor Yellow
        Write-Host "     Entra admin center → External Identities → User flows"
        Write-Host "     Name: $FlowName, Provider: Email with password, then link both apps"
    }
}

Write-Host ""
Write-Host "================================================================================================" -ForegroundColor Green
Write-Host "Successfully registered and configured apps"
Write-Host "  App ID (MAUI): $MauiAppName → $MauiAppId"
Write-Host "  App ID (Web):  $WebAppName → $WebAppId"
Write-Host "  API Scope:     api://$WebAppId/access_as_user"
Write-Host "  Tenant:        $TenantName ($TenantId)"
Write-Host "  Portal (MAUI): https://entra.microsoft.com/#view/Microsoft_AAD_RegisteredApps/ApplicationMenuBlade/~/Overview/appId/$MauiAppId/isMSAApp~/false"
Write-Host "  Portal (Web):  https://entra.microsoft.com/#view/Microsoft_AAD_RegisteredApps/ApplicationMenuBlade/~/Overview/appId/$WebAppId/isMSAApp~/false"
Write-Host "================================================================================================" -ForegroundColor Green

Disconnect-MgGraph | Out-Null
