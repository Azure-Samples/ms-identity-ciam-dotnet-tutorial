# Outdated Libraries Report - ms-identity-ciam-dotnet-tutorial

**Analysis Date:** September 29, 2025  
**Repository:** [Azure-Samples/ms-identity-ciam-dotnet-tutorial](https://github.com/Azure-Samples/ms-identity-ciam-dotnet-tutorial)  
**Analyzer:** GitHub Copilot Coding Agent  

## Executive Summary

This repository contains .NET tutorial samples for Microsoft Identity CIAM (Customer Identity and Access Management). The analysis reveals several outdated dependencies and version inconsistencies across the 10 C# projects that need attention to ensure security, compatibility, and optimal performance.

## Repository Structure

The repository contains:
- **10 C# projects** across authentication and authorization scenarios
- **16 PowerShell scripts** for Azure AD application setup and cleanup
- **Multiple target frameworks** including .NET 7.0 and .NET 8.0

## Key Findings

### 1. Target Framework Analysis
- **9 out of 10 projects** use .NET 8.0 as target framework ✅
- **1 MAUI project** still targets .NET 7.0 ⚠️

### 2. Package Version Inconsistencies

#### Microsoft.Identity.Web (Critical)
| Project | Current Version | Status |
|---------|----------------|---------|
| 1-Authentication/1-sign-in-aspnet-core-mvc | 3.6.2 | Outdated |
| 2-Authorization/1-call-own-api-aspnet-core-mvc/ToDoListClient | 3.6.2 | Outdated |
| 2-Authorization/1-call-own-api-aspnet-core-mvc/ToDoListAPI | 3.6.2 | Outdated |
| 2-Authorization/2-call-own-api-blazor-server/ToDoListClient | 3.6.2 | Outdated |
| 2-Authorization/2-call-own-api-blazor-server/ToDoListApi | **2.19.1** | Severely Outdated |
| 2-Authorization/3-call-own-api-dotnet-core-daemon/ToDoListAPI | 3.6.2 | Outdated |
| 2-Authorization/3-call-own-api-dotnet-core-daemon/ToDoListClient | **3.0.1** | Outdated |

**Latest Version:** 3.14.1 (as of analysis date)

#### Entity Framework Core (Critical)
| Package | Current Version | Latest Version | Status |
|---------|----------------|----------------|---------|
| Microsoft.EntityFrameworkCore | 7.0.5 | 8.0.x | Outdated |
| Microsoft.EntityFrameworkCore.InMemory | 7.0.5 | 8.0.x | Outdated |
| Microsoft.EntityFrameworkCore.Sqlite | 7.0.5 | 8.0.x | Outdated |
| Microsoft.EntityFrameworkCore.Tools | 7.0.5 | 8.0.x | Outdated |

#### ASP.NET Core Packages (Medium Priority)
| Package | Current Version | Status |
|---------|----------------|---------|
| Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation | 7.0.5 | Should be 8.0.x for .NET 8.0 |
| Microsoft.VisualStudio.Web.CodeGeneration.Design | 7.0.6 | Should be 8.0.x for .NET 8.0 |

#### MAUI Project (Medium Priority)
| Package | Current Version | Status |
|---------|----------------|---------|
| Microsoft.Identity.Client | 4.61.0 | Outdated (4.67.2 available) |
| Microsoft.Identity.Client.Extensions.Msal | 4.61.0 | Outdated (4.67.2 available) |
| Microsoft.Identity.Client.Desktop | 4.61.0 | Outdated (4.67.2 available) |
| Microsoft.Extensions.Configuration.Binder | 6.0.0 | Outdated (8.0.x available) |
| Microsoft.Extensions.Configuration.Json | 6.0.0 | Outdated (8.0.x available) |

#### Other Packages
| Package | Current Version | Status |
|---------|----------------|---------|
| NuGet.Common | 6.12.1 | Likely outdated |
| Newtonsoft.Json | 13.0.1 | Should check for latest |

### 3. PowerShell Module Dependencies

All 16 PowerShell scripts install Microsoft Graph modules without version pinning:

```powershell
Install-Module "Microsoft.Graph" -Scope CurrentUser 
Install-Module "Microsoft.Graph.Authentication" -Scope CurrentUser 
Install-Module "Microsoft.Graph.Identity.DirectoryManagement" -Scope CurrentUser 
Install-Module "Microsoft.Graph.Applications" -Scope CurrentUser 
Install-Module "Microsoft.Graph.Groups" -Scope CurrentUser 
Install-Module "Microsoft.Graph.Users" -Scope CurrentUser 
```

**Risk:** This approach may lead to inconsistent behavior across environments and time periods.

## Security Implications

1. **Critical:** Outdated Microsoft.Identity.Web versions may contain security vulnerabilities
2. **High:** EF Core 7.0.5 may have known security issues fixed in 8.0.x releases  
3. **Medium:** Unpinned PowerShell modules could introduce security risks if compromised versions are installed

## Compatibility Issues

1. **Framework Mismatch:** Using .NET 7.0 packages with .NET 8.0 projects
2. **Version Inconsistency:** Different Microsoft.Identity.Web versions across related projects
3. **MAUI Framework:** One project still targets .NET 7.0 instead of .NET 8.0

## Recommendations

### High Priority (Immediate Action Required)

1. **Standardize Microsoft.Identity.Web to 3.14.1** across all projects
   ```xml
   <PackageReference Include="Microsoft.Identity.Web" Version="3.14.1" />
   <PackageReference Include="Microsoft.Identity.Web.UI" Version="3.14.1" />
   <PackageReference Include="Microsoft.Identity.Web.DownstreamApi" Version="3.14.1" />
   ```

2. **Update Entity Framework Core to 8.0.x** for all .NET 8.0 projects
   ```xml
   <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.11" />
   <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.11" />
   <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.11" />
   <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.11" />
   ```

3. **Update ASP.NET Core packages to 8.0.x**
   ```xml
   <PackageReference Include="Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation" Version="8.0.11" />
   <PackageReference Include="Microsoft.VisualStudio.Web.CodeGeneration.Design" Version="8.0.11" />
   ```

### Medium Priority

4. **Update MAUI project to .NET 8.0**
   ```xml
   <TargetFrameworks>net8.0-android;net8.0-ios</TargetFrameworks>
   ```

5. **Update Microsoft.Identity.Client packages**
   ```xml
   <PackageReference Include="Microsoft.Identity.Client" Version="4.67.2" />
   <PackageReference Include="Microsoft.Identity.Client.Extensions.Msal" Version="4.67.2" />
   <PackageReference Include="Microsoft.Identity.Client.Desktop" Version="4.67.2" />
   ```

6. **Update configuration packages**
   ```xml
   <PackageReference Include="Microsoft.Extensions.Configuration.Binder" Version="8.0.1" />
   <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="8.0.1" />
   ```

### Low Priority

7. **Pin PowerShell module versions** for reproducibility
   ```powershell
   Install-Module "Microsoft.Graph" -RequiredVersion "2.x.x" -Scope CurrentUser 
   Install-Module "Microsoft.Graph.Authentication" -RequiredVersion "2.x.x" -Scope CurrentUser 
   ```

8. **Update NuGet.Common** to latest compatible version

## Implementation Strategy

1. **Phase 1:** Update Microsoft.Identity.Web packages across all projects
2. **Phase 2:** Update Entity Framework Core packages  
3. **Phase 3:** Update ASP.NET Core packages
4. **Phase 4:** Update MAUI project framework and packages
5. **Phase 5:** Pin PowerShell module versions

## Testing Recommendations

After updates:
1. Test all authentication flows
2. Verify API authorization functionality  
3. Test MAUI application on target platforms
4. Validate PowerShell scripts in clean environment
5. Run any existing automated tests

## Monitoring

- Set up automated dependency checking (e.g., Dependabot)
- Regularly review Microsoft security advisories
- Monitor for new releases of Microsoft.Identity.Web and related packages

---

**Note:** This analysis was performed using automated tools. Manual verification of compatibility and testing is recommended before implementing updates in production environments.