# Update Checklist - ms-identity-ciam-dotnet-tutorial

This checklist can be used to systematically update the outdated libraries identified in the analysis.

## Pre-Update Steps

- [ ] Create a backup branch or fork
- [ ] Ensure you have .NET 8.0 SDK installed
- [ ] Review current functionality that might be affected
- [ ] Plan testing strategy

## High Priority Updates

### Microsoft.Identity.Web Package Updates

Update all Microsoft.Identity.Web packages to version 3.14.1:

- [ ] **1-Authentication/1-sign-in-aspnet-core-mvc/MsIdWebApp.csproj**
  - [ ] Microsoft.Identity.Web: 3.6.2 → 3.14.1
  - [ ] Microsoft.Identity.Web.UI: 3.6.2 → 3.14.1

- [ ] **2-Authorization/1-call-own-api-aspnet-core-mvc/ToDoListClient/ToDoListClient.csproj**
  - [ ] Microsoft.Identity.Web: 3.6.2 → 3.14.1
  - [ ] Microsoft.Identity.Web.UI: 3.6.2 → 3.14.1
  - [ ] Microsoft.Identity.Web.DownstreamApi: 3.6.2 → 3.14.1

- [ ] **2-Authorization/1-call-own-api-aspnet-core-mvc/ToDoListAPI/ToDoListAPI.csproj**
  - [ ] Microsoft.Identity.Web: 3.6.2 → 3.14.1

- [ ] **2-Authorization/2-call-own-api-blazor-server/ToDoListClient/ToDoListClient.csproj**
  - [ ] Microsoft.Identity.Web: 3.6.2 → 3.14.1
  - [ ] Microsoft.Identity.Web.UI: 3.6.2 → 3.14.1
  - [ ] Microsoft.Identity.Web.DownstreamApi: 3.6.2 → 3.14.1

- [ ] **2-Authorization/2-call-own-api-blazor-server/ToDoListApi/ToDoListAPI.csproj** (Critical - very old version!)
  - [ ] Microsoft.Identity.Web: 2.19.1 → 3.14.1

- [ ] **2-Authorization/3-call-own-api-dotnet-core-daemon/ToDoListAPI/ToDoListAPI.csproj**
  - [ ] Microsoft.Identity.Web: 3.6.2 → 3.14.1

- [ ] **2-Authorization/3-call-own-api-dotnet-core-daemon/ToDoListClient/ToDoListClient.csproj**
  - [ ] Microsoft.Identity.Web: 3.0.1 → 3.14.1
  - [ ] Microsoft.Identity.Web.DownstreamApi: 3.0.1 → 3.14.1

### Entity Framework Core Updates

Update all EF Core packages from 7.0.5 to 8.0.11 (or latest 8.0.x):

- [ ] **2-Authorization/1-call-own-api-aspnet-core-mvc/ToDoListAPI/ToDoListAPI.csproj**
  - [ ] Microsoft.EntityFrameworkCore: 7.0.5 → 8.0.11
  - [ ] Microsoft.EntityFrameworkCore.InMemory: 7.0.5 → 8.0.11
  - [ ] Microsoft.EntityFrameworkCore.Sqlite: 7.0.5 → 8.0.11
  - [ ] Microsoft.EntityFrameworkCore.Tools: 7.0.5 → 8.0.11

- [ ] **2-Authorization/2-call-own-api-blazor-server/ToDoListApi/ToDoListAPI.csproj**
  - [ ] Microsoft.EntityFrameworkCore: 7.0.5 → 8.0.11
  - [ ] Microsoft.EntityFrameworkCore.InMemory: 7.0.5 → 8.0.11
  - [ ] Microsoft.EntityFrameworkCore.Sqlite: 7.0.5 → 8.0.11
  - [ ] Microsoft.EntityFrameworkCore.Tools: 7.0.5 → 8.0.11

- [ ] **2-Authorization/3-call-own-api-dotnet-core-daemon/ToDoListAPI/ToDoListAPI.csproj**
  - [ ] Microsoft.EntityFrameworkCore: 7.0.5 → 8.0.11
  - [ ] Microsoft.EntityFrameworkCore.InMemory: 7.0.5 → 8.0.11
  - [ ] Microsoft.EntityFrameworkCore.Sqlite: 7.0.5 → 8.0.11
  - [ ] Microsoft.EntityFrameworkCore.Tools: 7.0.5 → 8.0.11

### ASP.NET Core Package Updates

Update ASP.NET Core packages from 7.0.x to 8.0.x:

- [ ] **Multiple ToDoListAPI projects**
  - [ ] Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation: 7.0.5 → 8.0.11
  - [ ] Microsoft.VisualStudio.Web.CodeGeneration.Design: 7.0.6 → 8.0.11

## Medium Priority Updates

### MAUI Project Framework Update

- [ ] **1-Authentication/2-sign-in-maui/sign-in-maui.csproj**
  - [ ] Update TargetFrameworks: net7.0-android;net7.0-ios → net8.0-android;net8.0-ios
  - [ ] Update Windows framework: net7.0-windows10.0.19041.0 → net8.0-windows10.0.19041.0

### MAUI Project Package Updates

- [ ] **1-Authentication/2-sign-in-maui/sign-in-maui.csproj**
  - [ ] Microsoft.Identity.Client: 4.61.0 → 4.67.2
  - [ ] Microsoft.Identity.Client.Extensions.Msal: 4.61.0 → 4.67.2
  - [ ] Microsoft.Identity.Client.Desktop: 4.61.0 → 4.67.2
  - [ ] Microsoft.Extensions.Configuration.Binder: 6.0.0 → 8.0.1
  - [ ] Microsoft.Extensions.Configuration.Json: 6.0.0 → 8.0.1

### WPF Project Updates

- [ ] **1-Authentication/5-sign-in-dotnet-wpf/sign-in-dotnet-wpf.csproj**
  - [ ] Microsoft.Identity.Client: 4.67.2 → (verify if latest)
  - [ ] Microsoft.Identity.Client.Extensions.Msal: 4.67.2 → (verify if latest)
  - [ ] Microsoft.Extensions.Configuration: 8.0.0 → 9.0.0 (if compatible)
  - [ ] Microsoft.Extensions.Configuration.Binder: 8.0.1 → 9.0.0 (if compatible)
  - [ ] Microsoft.Extensions.Configuration.Json: 8.0.1 → 9.0.0 (if compatible)

### Device Code Project Updates

- [ ] **1-Authentication/4-sign-in-device-code/MsIdBrowserlessApp.csproj**
  - [ ] Microsoft.Extensions.Configuration: 9.0.0 → (latest if newer)
  - [ ] Microsoft.Extensions.Configuration.Binder: 9.0.0 → (latest if newer)
  - [ ] Microsoft.Extensions.Configuration.Json: 9.0.0 → (latest if newer)

## Low Priority Updates

### Other Package Updates

- [ ] **NuGet.Common Updates**
  - [ ] NuGet.Common: 6.12.1 → (check latest version)

- [ ] **Newtonsoft.Json Updates**
  - [ ] Newtonsoft.Json: 13.0.1 → (check latest version)

### PowerShell Module Version Pinning

Pin versions in all PowerShell scripts (16 files total):

- [ ] **1-Authentication scripts** (8 files)
- [ ] **2-Authorization scripts** (8 files)

Example change:
```powershell
# From:
Install-Module "Microsoft.Graph" -Scope CurrentUser 

# To:
Install-Module "Microsoft.Graph" -RequiredVersion "2.x.x" -Scope CurrentUser 
```

## Testing After Updates

- [ ] Build all projects successfully
- [ ] Test authentication flows in ASP.NET Core MVC app
- [ ] Test authentication flows in Blazor Server app  
- [ ] Test API authorization functionality
- [ ] Test MAUI application on target platforms (if available)
- [ ] Test WPF application
- [ ] Test device code flow
- [ ] Test daemon application
- [ ] Validate PowerShell scripts in clean environment

## Verification Commands

After each update, run these commands to verify:

```bash
# Build specific project
dotnet build path/to/project.csproj

# Build all projects
dotnet build --configuration Release

# Check for vulnerabilities
dotnet list package --vulnerable

# Check for outdated packages
dotnet list package --outdated
```

## Notes

- Always update packages incrementally and test after each major change
- Some packages may have breaking changes between major versions
- Review release notes for any breaking changes before updating
- Consider updating in a development environment first
- The .NET 8.0 projects should use .NET 8.0 compatible package versions where possible