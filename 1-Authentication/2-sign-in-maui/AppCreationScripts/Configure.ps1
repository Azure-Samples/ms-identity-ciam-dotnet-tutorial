#Requires -Version 7

<#
.SYNOPSIS
    Creates the Azure AD app registration for this MAUI sample using Microsoft Graph PowerShell.

.DESCRIPTION
    This script creates an app registration in your Entra External ID (CIAM) tenant,
    sets MSAL redirect URIs, grants openid/offline_access permissions, creates a
    sign-up/sign-in user flow, and patches the sample configuration files.

    Prerequisites: Microsoft Graph PowerShell SDK
      Install-Module Microsoft.Graph.Applications -Scope CurrentUser
      Install-Module Microsoft.Graph.Identity.DirectoryManagement -Scope CurrentUser
      Install-Module Microsoft.Graph.Identity.SignIns -Scope CurrentUser

.PARAMETER TenantId
    Optional. The tenant ID (GUID) or domain (e.g., "contoso.onmicrosoft.com").

.PARAMETER AppName
    Optional. The app registration display name. Defaults to "ciam-dotnet-maui".

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
    [string] $AppName = "ciam-dotnet-maui",

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

# --- Check if app already exists ---
$existingApp = Get-MgApplication -Filter "displayName eq '$AppName'" -Top 1 -ErrorAction SilentlyContinue

if ($existingApp) {
    Write-Host ""
    Write-Host "App '$AppName' already exists with AppId: $($existingApp.AppId)"
    $response = Read-Host "Delete and recreate? (y/N)"
    if ($response -match "^[Yy]$") {
        Remove-MgApplication -ApplicationId $existingApp.Id
        Write-Host "Deleted existing app registration."
        $existingApp = $null
    }
}

# --- Create or reuse app ---
if ($existingApp) {
    $AppId = $existingApp.AppId
    $ObjId = $existingApp.Id
    Write-Host "Using existing app registration."
} else {
    Write-Host ""
    Write-Host "Creating app registration '$AppName'..."

    $graphPermission = @{
        ResourceAppId  = "00000003-0000-0000-c000-000000000000"
        ResourceAccess = @(
            @{ Id = "37f7f235-527c-4136-accd-4a02d197296e"; Type = "Scope" },  # openid
            @{ Id = "7427e0e9-2fba-42fe-b0c0-848c9e6a8182"; Type = "Scope" }   # offline_access
        )
    }

    $app = New-MgApplication `
        -DisplayName $AppName `
        -SignInAudience "AzureADMyOrg" `
        -IsFallbackPublicClient `
        -RequiredResourceAccess @($graphPermission)

    $AppId = $app.AppId
    $ObjId = $app.Id
    Write-Host "Created app with AppId: $AppId"

    # Set redirect URIs
    # - msal{clientId}://auth  — iOS, Android, Mac Catalyst (custom scheme)
    # - http://localhost        — Windows (embedded WebView2 via MSAL Desktop WinUI3)
    Update-MgApplication -ApplicationId $ObjId -PublicClient @{
        RedirectUris = @("msal${AppId}://auth", "http://localhost")
    }
    Write-Host "Set redirect URIs: msal${AppId}://auth, http://localhost"

    # Create service principal
    try {
        New-MgServicePrincipal -AppId $AppId -ErrorAction Stop | Out-Null
        Write-Host "Created service principal."
        Write-Host "  Waiting for propagation..."
        Start-Sleep -Seconds 5
    } catch {
        Write-Host "Service principal already exists (non-fatal)."
    }
}

$PortalUrl = "https://entra.microsoft.com/#view/Microsoft_AAD_RegisteredApps/ApplicationMenuBlade/~/Overview/appId/$AppId/isMSAApp~/false"

# --- Patch configuration files ---
Write-Host ""
Write-Host "Updating configuration files..."

# appsettings.json
$appSettingsPath = Join-Path $ProjectDir "appsettings.json"
if (Test-Path $appSettingsPath) {
    $appSettings = Get-Content $appSettingsPath -Raw | ConvertFrom-Json
    $appSettings.AzureAd.Authority = "https://$TenantName.ciamlogin.com/"
    $appSettings.AzureAd.ClientId = $AppId
    $appSettings | ConvertTo-Json -Depth 10 | Set-Content $appSettingsPath -Encoding UTF8
    Write-Host "  ✅ appsettings.json"
} else {
    Write-Host "  ❌ appsettings.json — file not found"
}

# AndroidManifest.xml
$manifestPath = Join-Path $ProjectDir "Platforms" "Android" "AndroidManifest.xml"
if (Test-Path $manifestPath) {
    [xml]$manifest = Get-Content $manifestPath
    $ns = New-Object System.Xml.XmlNamespaceManager($manifest.NameTable)
    $ns.AddNamespace("android", "http://schemas.android.com/apk/res/android")
    $dataNodes = $manifest.SelectNodes("//data[@android:host='auth']", $ns)
    foreach ($node in $dataNodes) {
        $null = $node.SetAttribute("scheme", "http://schemas.android.com/apk/res/android", "msal$AppId")
    }
    $manifest.Save($manifestPath)
    Write-Host "  ✅ AndroidManifest.xml"
} else {
    Write-Host "  ❌ AndroidManifest.xml — file not found"
}

# Info.plist
$plistPath = Join-Path $ProjectDir "Platforms" "iOS" "Info.plist"
if (Test-Path $plistPath) {
    [xml]$plist = Get-Content $plistPath
    $strings = $plist.SelectNodes("//string")
    foreach ($s in $strings) {
        if ($s.InnerText -match "^msal") {
            $null = ($s.InnerText = "msal$AppId")
        }
    }
    $plist.Save($plistPath)
    Write-Host "  ✅ Info.plist"
} else {
    Write-Host "  ❌ Info.plist — file not found"
}

# --- Create user flow ---
Write-Host ""
Write-Host "Setting up sign-up/sign-in user flow..."

# Check if flow already exists
$existingFlows = $null
try {
    $existingFlows = Invoke-MgGraphRequest -Method GET `
        -Uri "/v1.0/identity/authenticationEventsFlows" `
        -ErrorAction Stop
} catch {
    # May fail on non-CIAM tenants
}

$existingFlow = $null
if ($existingFlows.value) {
    $existingFlow = $existingFlows.value | Where-Object { $_.displayName -eq $FlowName } | Select-Object -First 1
}

if ($existingFlow) {
    Write-Host "  ✅ User flow '$FlowName' already exists (id: $($existingFlow.id))."

    # Ensure app is linked
    $linked = Invoke-MgGraphRequest -Method GET `
        -Uri "/v1.0/identity/authenticationEventsFlows/$($existingFlow.id)/conditions/applications/includeApplications"
    $linkedAppIds = @()
    if ($linked.value) {
        $linkedAppIds = $linked.value | ForEach-Object { $_.appId }
    }

    if ($linkedAppIds -contains $AppId) {
        Write-Host "  ✅ App already linked to flow."
    } else {
        $linked = $false
        for ($attempt = 1; $attempt -le 5; $attempt++) {
            try {
                $null = Invoke-MgGraphRequest -Method POST `
                    -Uri "/v1.0/identity/authenticationEventsFlows/$($existingFlow.id)/conditions/applications/includeApplications" `
                    -Body @{ appId = $AppId } `
                    -ErrorAction Stop
                Write-Host "  ✅ App linked to existing flow."
                $linked = $true
                break
            } catch {
                if ($attempt -lt 5) {
                    Write-Host "  ⏳ Waiting for service principal to propagate (attempt $attempt/5)..."
                    Start-Sleep -Seconds 5
                } else {
                    Write-Host "  ❌ Could not link app to flow after $attempt attempts: $_" -ForegroundColor Red
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
                    @{ appId = $AppId }
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
        $result = Invoke-MgGraphRequest -Method POST `
            -Uri "/v1.0/identity/authenticationEventsFlows" `
            -Body $userFlowBody `
            -ErrorAction Stop
        Write-Host "  ✅ User flow '$FlowName' created and linked to app."
    } catch {
        Write-Host "  ❌ Failed to create user flow." -ForegroundColor Red
        Write-Host "     $_" -ForegroundColor Red
        Write-Host ""
        Write-Host "  This API requires a Microsoft Entra External ID (CIAM) tenant." -ForegroundColor Yellow
        Write-Host "  If using a standard Azure AD tenant, create the flow manually:" -ForegroundColor Yellow
        Write-Host "     Entra admin center → External Identities → User flows"
        Write-Host "     Name: $FlowName, Provider: Email with password, then link '$AppName'"
    }
}

$PortalUrl = "https://entra.microsoft.com/#view/Microsoft_AAD_RegisteredApps/ApplicationMenuBlade/~/Overview/appId/$AppId/isMSAApp~/false"

Write-Host ""
Write-Host "================================================================================================" -ForegroundColor Green
Write-Host "Successfully registered and configured '$AppName'"
Write-Host "  App ID:  $AppId"
Write-Host "  Tenant:  $TenantName ($TenantId)"
Write-Host "  Portal:  $PortalUrl"
Write-Host "================================================================================================" -ForegroundColor Green

Disconnect-MgGraph | Out-Null
