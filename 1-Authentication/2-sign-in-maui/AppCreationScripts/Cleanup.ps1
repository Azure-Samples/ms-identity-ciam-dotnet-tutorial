#Requires -Version 7

<#
.SYNOPSIS
    Tears down Azure resources created by Configure.ps1 and restores placeholder values.

.DESCRIPTION
    Deletes the app registration, unlinks it from the user flow, and restores
    configuration files to their placeholder values.

    Prerequisites: Microsoft Graph PowerShell SDK
      Install-Module Microsoft.Graph.Applications -Scope CurrentUser
      Install-Module Microsoft.Graph.Identity.DirectoryManagement -Scope CurrentUser
      Install-Module Microsoft.Graph.Identity.SignIns -Scope CurrentUser

.PARAMETER TenantId
    Optional. The tenant ID or domain to clean up.

.PARAMETER AppName
    Optional. The app registration display name. Defaults to "ciam-dotnet-maui".

.PARAMETER AzureEnvironmentName
    Optional. Azure cloud environment. Defaults to "Global".
    Accepted values: Global, AzureChinaCloud, AzureUSGovernment.

.EXAMPLE
    ./Cleanup.ps1
    ./Cleanup.ps1 -TenantId "contoso.onmicrosoft.com"
    ./Cleanup.ps1 -TenantId "contoso" -AzureEnvironmentName "AzureUSGovernment"
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
Write-Host "Connected to tenant $($context.TenantId) as '$($context.Account)'"

# --- Find and delete app registrations ---
Write-Host ""
Write-Host "Looking for app registrations named '$AppName'..."

$apps = Get-MgApplication -Filter "displayName eq '$AppName'" -All

if (-not $apps -or $apps.Count -eq 0) {
    Write-Host "No app registrations found with name '$AppName'."
} else {
    # Unlink apps from user flow before deleting
    $FlowName = "signup_signin"
    try {
        $flows = Invoke-MgGraphRequest -Method GET `
            -Uri "/v1.0/identity/authenticationEventsFlows" `
            -ErrorAction Stop
        $flow = $flows.value | Where-Object { $_.displayName -eq $FlowName } | Select-Object -First 1
        if ($flow) {
            foreach ($app in $apps) {
                Write-Host "  Unlinking $($app.AppId) from user flow..."
                try {
                    Invoke-MgGraphRequest -Method DELETE `
                        -Uri "/v1.0/identity/authenticationEventsFlows/$($flow.id)/conditions/applications/includeApplications/$($app.AppId)" `
                        -ErrorAction Stop
                } catch {
                    # May not be linked — ignore
                }
            }
        }
    } catch {
        # May fail on non-CIAM tenants — ignore
    }

    # Delete app registrations
    foreach ($app in $apps) {
        Write-Host "  Deleting app: $($app.AppId)"
        Remove-MgApplication -ApplicationId $app.Id
    }
    Write-Host "  ✅ App registration(s) deleted." -ForegroundColor Green
}

# --- Restore placeholder values ---
Write-Host ""
Write-Host "Restoring placeholder values in configuration files..."

# appsettings.json
$appSettingsPath = Join-Path $ProjectDir "appsettings.json"
if (Test-Path $appSettingsPath) {
    $appSettings = Get-Content $appSettingsPath -Raw | ConvertFrom-Json
    $appSettings.AzureAd.Authority = "https://Enter_the_Tenant_Subdomain_Here.ciamlogin.com/"
    $appSettings.AzureAd.ClientId = "Enter_the_Application_Id_Here"
    $appSettings | ConvertTo-Json -Depth 10 | Set-Content $appSettingsPath -Encoding UTF8
    Write-Host "  ✅ appsettings.json" -ForegroundColor Green
}

# AndroidManifest.xml
$manifestPath = Join-Path $ProjectDir "Platforms" "Android" "AndroidManifest.xml"
if (Test-Path $manifestPath) {
    [xml]$manifest = Get-Content $manifestPath
    $ns = New-Object System.Xml.XmlNamespaceManager($manifest.NameTable)
    $ns.AddNamespace("android", "http://schemas.android.com/apk/res/android")
    $dataNodes = $manifest.SelectNodes("//data[@android:host='auth']", $ns)
    foreach ($node in $dataNodes) {
        $null = $node.SetAttribute("scheme", "http://schemas.android.com/apk/res/android", "msalEnter_the_Application_Id_Here")
    }
    $manifest.Save($manifestPath)
    Write-Host "  ✅ AndroidManifest.xml" -ForegroundColor Green
}

# Info.plist
$plistPath = Join-Path $ProjectDir "Platforms" "iOS" "Info.plist"
if (Test-Path $plistPath) {
    [xml]$plist = Get-Content $plistPath
    $strings = $plist.SelectNodes("//string")
    foreach ($s in $strings) {
        if ($s.InnerText -match "^msal") {
            $null = ($s.InnerText = "msalEnter_the_Application_Id_Here")
        }
    }
    $plist.Save($plistPath)
    Write-Host "  ✅ Info.plist" -ForegroundColor Green
}

Write-Host ""
Write-Host "=== Cleanup Complete ✅ ===" -ForegroundColor Green
Write-Host ""

Disconnect-MgGraph | Out-Null
