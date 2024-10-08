---
page_type: sample
name: An ASP.NET Core web app authenticating users against Microsoft Entra External ID using Microsoft Identity Web
description: 
languages:
 - csharp
products:
 - entra-external-id
 - microsoft-identity-web
urlFragment: ms-identity-ciam-dotnet-tutorial-1-sign-in-aspnet-core-mvc
extensions:
    services: 
    - ms-identity
    platform: 
    - DotNet
    endpoint: 
    - AAD v2.0
    level: 
    - 100
    client: 
    - ASP.NET Core web app
---

# An ASP.NET Core web app authenticating users against Microsoft Entra External ID using Microsoft Identity Web

* [Overview](#overview)
* [Scenario](#scenario)
* [Prerequisites](#prerequisites)
* [Register the web app](#register-the-web-application-in-your-tenant)
* [Add app client secret](#add-app-client-secret)
* [Grant API permissions](#grant-api-permissions)
* [Create a user flow](#create-a-user-flow)

* [About the code](#about-the-code)
* [Contributing](#contributing)
* [Learn More](#learn-more)

## Overview

This sample demonstrates an ASP.NET Core web app that authenticates users against Microsoft Entra External ID with the help of [Microsoft.Identity.Web](https://github.com/AzureAD/microsoft-identity-web).

## Scenario

| Instruction | Description |
| --- | --- |
| **Use case** | This code sample applies to external tenant configuration uses cases. For workforce use cases, refer to [Sign in users and call the Microsoft Graph API from an ASP.NET Core web app](https://learn.microsoft.com/en-us/entra/identity-platform/quickstart-web-app-dotnet-core-sign-in) |
| **Scenario** | Sign-in a user to an ASP.NET Core web app using Microsoft Entra External ID |
| **Product documentation** | Explore  [Microsoft Entra External ID documentation](https://learn.microsoft.com/entra/external-id/customers/) |

## Prerequisites

* An IDE such as [Visual Studio](https://visualstudio.microsoft.com/downloads/) or [Visual Studio Code](https://code.visualstudio.com/download)
* [.NET Core SDK](https://www.microsoft.com/net/learn/get-started)
* An external tenant. To create one, choose from the following methods:
    * (Recommended) Use the [Microsoft Entra External ID extension](https://aka.ms/ciamvscode/readme/marketplace) to set up an external tenant directly in Visual Studio Code.
    * [Create a new external tenant](https://learn.microsoft.com/entra/external-id/customers/how-to-create-external-tenant-portal) in the Microsoft Entra admin center.
* A user account in your **Microsoft Entra External ID** tenant.

## Register the web application in your tenant

You can register an app in your tenant automatically by using Microsoft Graph PowerShell or via the Microsoft Entra admin center.

When you use Microsoft Graph PowerShell, you automatically register the applications and related objects app secrets, then modify your project config files, so you can run the app without any further action:

* To register your app in the Microsoft Entra admin center use the steps in [Sign in users for a sample ASP.NET web app in an external tenant](https://learn.microsoft.com/en-us/entra/external-id/customers/sample-web-app-dotnet-sign-in#register-the-web-app)
* To register and configure your app automatically,

<details>
   <summary>Expand this section</summary>

> :warning: If you have never used **Microsoft Graph PowerShell** before, we recommend you go through the [App Creation Scripts Guide](./AppCreationScripts/AppCreationScripts.md) once to ensure that your environment is prepared correctly for this step.

1. Ensure that you have [PowerShell 7](https://learn.microsoft.com/powershell/scripting/install/installing-powershell-on-windows?view=powershell-7.3) or later.
1. Run the script to create your Microsoft Entra application and configure the code of the sample application accordingly.
1. For interactive process -in PowerShell, run:

    ```PowerShell
    cd .\AppCreationScripts\
    .\Configure.ps1 -TenantId "[Optional] - your tenant id" -AzureEnvironmentName "[Optional] - Azure environment, defaults to 'Global'"
    ```

> Other ways of running the scripts are described in [App Creation Scripts guide](./AppCreationScripts/AppCreationScripts.md). The scripts also provide a guide to automated application registration, configuration and removal which can help in your CI/CD scenarios.

> :information_source: This sample can make use of client certificates. You can use **AppCreationScripts** to register a Microsoft Entra application with certificates. See: [How to use certificates instead of client secrets](./README-use-certificate.md)

</details>

## Add app client secret

To create a client secret for the registered application, use the steps in [Add a client secret](https://learn.microsoft.com/en-us/entra/external-id/customers/sample-web-app-dotnet-sign-in#add-app-client-secret)

## Grant API permissions

To grant delegated permissions, use the steps in [Grant API permissions](https://learn.microsoft.com/en-us/entra/external-id/customers/sample-web-app-dotnet-sign-in#grant-api-permissions).

## Create a user flow

To create a user flow a customer can use to sign in or sign up for an application, use the steps in [Create a user flow](https://learn.microsoft.com/en-us/entra/external-id/customers/sample-web-app-dotnet-sign-in#create-a-user-flow)

## Clone or download sample web application

To get the code for this sample web app, navigate to the **<> Code** icon on this page do either of the following;
- Copy the **HTTPS** and clone using the URL in your terminal

    ```console
    git clone https://github.com/Azure-Samples/ms-identity-ciam-dotnet-tutorial.git
    ```

- [Download the .zip file](https://github.com/Azure-Samples/ms-identity-ciam-dotnet-tutorial/archive/refs/heads/main.zip) and extract it to a folder where the total length of the path is 260 or fewer characters.

## Configure the application

You'll need to update your sample app so that it uses the settings of the web app that you registered. To do so, use the steps in [Configure the application](https://learn.microsoft.com/en-us/entra/external-id/customers/sample-web-app-dotnet-sign-in#configure-the-application).

## Run and test the sample

You can now test the sample web app. Open your web browser and navigate to <https://localhost:7274> and sign in with an account registered to the external tenant.

> :information_source: Did the sample not work for you as expected? Then please reach out to us using the [GitHub Issues](../../../../issues) page.

## We'd love your feedback!

Were we successful in addressing your learning objective? Consider taking a moment to [share your experience with us](https://forms.microsoft.com/Pages/DesignPageV2.aspx?subpage=design&m2=1&id=v4j5cvGGr0GRqy180BHbR9p5WmglDttMunCjrD00y3NUMlJETFFSQVQ4SjBGQk9aVUhPS0JUOUJUUi4u).

## Troubleshooting

<details>
	<summary>Expand for troubleshooting info</summary>


ASP.NET Core applications create session cookies that represent the identity of the caller. Some Safari users using iOS 12 had issues which are described in ASP.NET Core #4467 and the Web kit bugs database Bug 188165 - iOS 12 Safari breaks ASP.NET Core 2.1 OIDC authentication.

If your web site needs to be accessed from users using iOS 12, you probably want to disable the SameSite protection, but also ensure that state changes are protected with CSRF anti-forgery mechanism. See the how to fix section of Microsoft Security Advisory: iOS12 breaks social, WSFed and OIDC logins #4647

To provide feedback on or suggest features for Microsoft Entra ID, visit [User Voice page](https://feedback.azure.com/d365community/forum/79b1327d-d925-ec11-b6e6-000d3a4f06a4).
</details>

## About the code

<details>
	<summary>Expand to learn more about the code</summary>

This sample shows how to use the OpenID Connect ASP.NET Core middleware to sign in users from a single Microsoft Entra External ID tenant. The middleware is initialized in the `Program.cs` file by passing it the Client ID of the app, and the URL of the Microsoft Entra tenant where the app is registered. These values are  read from the `appsettings.json` file. The middleware takes care of:

- Downloading the Microsoft Entra metadata, finding the signing keys, and finding the issuer name for the tenant.
- Processing OpenID Connect sign-in responses by validating the signature and issuer in an incoming JWT, extracting the user's claims, and putting the claims in `ClaimsPrincipal.Current`.
- Integrating with the session cookie ASP.NET Core middleware to establish a session for the user.

You can trigger the middleware to send an OpenID Connect sign-in request by decorating a class or method with the `[Authorize]` attribute or by issuing a challenge (see the [AccountController.cs](https://github.com/aspnet/AspNetCore/blob/master/src/Azure/AzureAD/Authentication.AzureAD.UI/src/Areas/AzureAD/Controllers/AccountController.cs) file which is part of ASP.NET Core):

The middleware in this project is created as a part of the open-source [ASP.NET Core Security](https://github.com/aspnet/aspnetcore) project.

These steps are encapsulated in the [Microsoft.Identity.Web](https://github.com/AzureAD/microsoft-identity-web/wiki) library.

</details>

## Deploying Web app to Azure App Service

There is one web app in this sample. To deploy it to **Azure App Services**, you'll need to:

- create an **Azure App Service**
- publish the projects to the **App Services**, and
- update its client(s) to call the website instead of the local environment.

<details>
	<summary>Expand to learn more about how to publish your files</summary>

### Publish your files (ciam-aspnet-webapp)

#### Publish using Visual Studio

Follow the link to [Publish with Visual Studio](https://docs.microsoft.com/visualstudio/deployment/quickstart-deploy-to-azure).

#### Publish using Visual Studio Code

1. Install the Visual Studio Code extension [Azure App Service](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azureappservice).
1. Follow the link to [Publish with Visual Studio Code](https://docs.microsoft.com/aspnet/core/tutorials/publish-to-azure-webapp-using-vscode)

### Update the app registration

1. Navigate back to to the [Azure portal](https://portal.azure.com).
In the left-hand navigation pane, select the **Microsoft Entra ID** service, and then select **App registrations (Preview)**.
1. In the resulting screen, select the `ciam-aspnet-webapp` application.
1. In the app's registration screen, select **Authentication** in the menu.
    1. In the **Redirect URIs** section, update the reply URLs to match the site URL of your Azure deployment. For example:
        1. `https://ciam-aspnet-webapp.azurewebsites.net/`
        1. `https://ciam-aspnet-webapp.azurewebsites.net/signin-oidc`
    1. Update the **Front-channel logout URL** fields with the address of your service, for example [https://ciam-aspnet-webapp.azurewebsites.net](https://ciam-aspnet-webapp.azurewebsites.net)

> :warning: If your app is using an *in-memory* storage, **Azure App Services** will spin down your web site if it is inactive, and any records that your app was keeping will be empty. In addition, if you increase the instance count of your website, requests will be distributed among the instances. Your app's records, therefore, will not be the same on each instance.

</details>

## Contributing

If you'd like to contribute to this sample, see [CONTRIBUTING.MD](/CONTRIBUTING.md).

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information, see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## See also

* [Customize the default branding](https://learn.microsoft.com/en-us/entra/external-id/customers/how-to-customize-branding-customers)
* [OAuth 2.0 device authorization grant flow](https://learn.microsoft.com/en-us/entra/identity-platform/v2-oauth2-device-code)
* [Building Zero Trust ready apps](https://learn.microsoft.com/en-us/security/zero-trust/deploy/identity)
* [Microsoft.Identity.Web](https://aka.ms/microsoft-identity-web)
