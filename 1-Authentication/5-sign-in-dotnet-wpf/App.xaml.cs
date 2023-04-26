using System.Windows;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Broker;

namespace sign_in_dotnet_wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>

    // To change from Microsoft public cloud to a national cloud, use another value of AzureCloudInstance
    public partial class App : Application
    {
        static App()
        {
            CreateApplication(true);
        }

        public static void CreateApplication(bool useWam)
        {
            var builder = PublicClientApplicationBuilder.Create(ClientId)
                .WithAuthority($"{Instance}{Tenant}")
                .WithExtraQueryParameters("dc=ESTS-PUB-EUS-AZ1-FD000-TEST1")
                .WithDefaultRedirectUri();

            //Use of Broker Requires redirect URI "ms-appx-web://microsoft.aad.brokerplugin/{client_id}" in app registration
            if (useWam)
            {
                BrokerOptions brokerOptions = new BrokerOptions(BrokerOptions.OperatingSystems.Windows);
                brokerOptions.ListOperatingSystemAccounts = true;
                builder.WithBroker(brokerOptions);
            }

            _clientApp = builder.Build();
            TokenCacheHelper.EnableSerialization(_clientApp.UserTokenCache);
        }

        // Below are the clientId (Application Id) of your app registration and the tenant information. 
        // You have to replace:
        // - the content of ClientID with the Application Id for your app registration
        // - The content of Tenant by the information about the accounts allowed to sign-in in your application:
        //   - For Work or School account in your org, use your tenant ID, or domain
        //   - for any Work or School accounts, use organizations
        //   - for any Work or School accounts, or Microsoft personal account, use common
        //   - for Microsoft Personal account, use consumers
        private static string ClientId = "Enter_the_Application_Id_Here";

        // Note: Tenant is important for the quickstart.
        private static string Tenant = "Enter_the_Tenant_Id_Here";
        private static string Instance = "https://Enter_Domain_Name_Here.ciamlogin.com/";
        private static IPublicClientApplication _clientApp;

        public static IPublicClientApplication PublicClientApp { get { return _clientApp; } }
    }
}
