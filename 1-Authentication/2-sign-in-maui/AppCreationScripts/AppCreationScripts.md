# Registering sample apps with Microsoft Entra External ID and updating configuration files using PowerShell

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
   ./Configure.ps1 -TenantId "contoso.onmicrosoft.com"
   ```

### More details

- [Goal of the provided scripts](#goal-of-the-provided-scripts)
  - [Presentation of the scripts](#presentation-of-the-scripts)
  - [Usage pattern for tests and DevOps scenarios](#usage-pattern-for-tests-and-devops-scenarios)
- [How to use the app creation scripts?](#how-to-use-the-app-creation-scripts)
  - [Pre-requisites](#pre-requisites)
  - [Run the script and start running](#run-the-script-and-start-running)
  - [Two ways to run the script](#two-ways-to-run-the-script)
    - [Option 1 (interactive)](#option-1-interactive)
    - [Option 2 (interactive, but create apps in a specified tenant)](#option-2-interactive-but-create-apps-in-a-specified-tenant)

## Goal of the provided scripts

### Presentation of the scripts

This sample comes with two PowerShell scripts, which automate the creation of the Microsoft Entra External ID (CIAM) app registration, and the configuration of the code for this sample. Once you run them, you will only need to build the solution and you are good to test.

These scripts are:

- `Configure.ps1` which:
  - creates a Microsoft Entra app registration with the correct permissions and redirect URIs,
  - creates a sign-up/sign-in user flow and links it to the app,
  - patches the configuration files in the sample project:
    - `appsettings.json` — sets the authority and client ID
    - `Platforms/Android/AndroidManifest.xml` — sets the MSAL redirect scheme
    - `Platforms/iOS/Info.plist` — sets the MSAL URL scheme

- `Cleanup.ps1` which:
  - deletes the app registration,
  - unlinks it from the user flow,
  - restores placeholder values in all configuration files.

### Usage pattern for tests and DevOps scenarios

If you run `Configure.ps1` and an app with the same name already exists, it will prompt you to delete and recreate it. For CI/CD scenarios, you can run `Cleanup.ps1` before `Configure.ps1` to start fresh.

## How to use the app creation scripts?

### Pre-requisites

1. [PowerShell 7](https://learn.microsoft.com/powershell/scripting/install/installing-powershell) or later (cross-platform).
1. An Azure account with permissions to create app registrations in your Microsoft Entra External ID (CIAM) tenant.

### (Optionally) install Microsoft Graph PowerShell modules

The scripts install the required PowerShell modules for the current user if needed. However, if you want to install them for all users on the machine, you can run:

```pwsh
Install-Module Microsoft.Graph.Applications
Install-Module Microsoft.Graph.Identity.DirectoryManagement
Install-Module Microsoft.Graph.Identity.SignIns
```

Or for the current user only:

```pwsh
Install-Module Microsoft.Graph.Applications -Scope CurrentUser
Install-Module Microsoft.Graph.Identity.DirectoryManagement -Scope CurrentUser
Install-Module Microsoft.Graph.Identity.SignIns -Scope CurrentUser
```

### Run the script and start running

1. Go to the `AppCreationScripts` sub-folder:

    ```pwsh
    cd AppCreationScripts
    ```

1. Run the script (see [below](#two-ways-to-run-the-script) for options).
1. Open the project and build it — all configuration files are already patched.

You're done!

### Two ways to run the script

#### Option 1 (interactive)

Run without arguments and the script will prompt you for the tenant domain prefix:

```pwsh
./Configure.ps1
```

A browser window will open for authentication via Microsoft Graph. The script will create the app in the tenant you authenticate to.

#### Option 2 (interactive, but create apps in a specified tenant)

If you want to create the app in a particular tenant, pass the `-TenantId` parameter:

```pwsh
./Configure.ps1 -TenantId "contoso.onmicrosoft.com"
```

Or just the domain prefix:

```pwsh
./Configure.ps1 -TenantId "contoso"
```

You can also specify a custom app name and user flow name:

```pwsh
./Configure.ps1 -TenantId "contoso" -AppName "my-maui-app" -FlowName "my_signup_signin"
```

### Running the script on Azure Sovereign clouds

All options above can be used on any Azure Sovereign cloud. By default, the scripts target `Global`, but this can be changed using the `-AzureEnvironmentName` parameter.

The accepted values are:

- `Global` (default)
- `AzureChinaCloud`
- `AzureUSGovernment`

Example:

```pwsh
./Cleanup.ps1 -TenantId "contoso" -AzureEnvironmentName "AzureUSGovernment"
./Configure.ps1 -TenantId "contoso" -AzureEnvironmentName "AzureUSGovernment"
```

### Cleanup

To remove the app registration and restore config placeholders:

```pwsh
./Cleanup.ps1 -TenantId "contoso"
```

Then re-run `./Configure.ps1` for a fresh setup.
