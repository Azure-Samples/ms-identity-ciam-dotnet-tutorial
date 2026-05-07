# Registering sample apps with Microsoft Entra ID

## Overview

### Quick summary

1. Install the Microsoft Graph PowerShell modules:

   ```pwsh
   Install-Module Microsoft.Graph.Applications -Scope CurrentUser
   Install-Module Microsoft.Graph.Identity.DirectoryManagement -Scope CurrentUser
   Install-Module Microsoft.Graph.Identity.SignIns -Scope CurrentUser
   ```

2. Run the script:

   ```pwsh
   cd AppCreationScripts
   ./Configure.ps1
   ```

   Or specify a tenant:

   ```pwsh
   ./Configure.ps1 -TenantId "contoso.onmicrosoft.com"
   ```

## Scripts

| Script | Description |
|---|---|
| `Configure.ps1` | Creates the app registration, sets redirect URIs, creates a user flow, and patches `appsettings.json`, `AndroidManifest.xml`, and `Info.plist` |
| `Cleanup.ps1` | Deletes the app registration, unlinks from user flow, and restores placeholder values in config files |

## Prerequisites

- [PowerShell 7+](https://learn.microsoft.com/powershell/scripting/install/installing-powershell) (cross-platform)
- Microsoft Graph PowerShell SDK modules:
  - `Microsoft.Graph.Applications`
  - `Microsoft.Graph.Identity.DirectoryManagement`
  - `Microsoft.Graph.Identity.SignIns`
- An Azure account with permissions to create app registrations

## Usage

### Option 1: Interactive (auto-detect tenant)

```pwsh
./Configure.ps1
```

A browser window will open for authentication via Microsoft Graph.

### Option 2: Specify a tenant

```pwsh
./Configure.ps1 -TenantId "contoso.onmicrosoft.com"
```

### Cleanup

To remove the app registration and restore config placeholders:

```pwsh
./Cleanup.ps1
```

Then re-run `./Configure.ps1` for a fresh setup.
