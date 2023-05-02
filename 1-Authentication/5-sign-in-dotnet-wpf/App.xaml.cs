using System.Windows;
using System.Reflection;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Broker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

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
            CreateApplication();
        }

        public static void CreateApplication()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("call_own_api_dotnet_wpf.appsettings.json");
            AppConfiguration = new ConfigurationBuilder()
               .AddJsonStream(stream)
               .Build();

            AzureAdConfig azureADConfig = AppConfiguration.GetSection("AzureAd").Get<AzureAdConfig>();

            var builder = PublicClientApplicationBuilder.Create(azureADConfig.ClientId)
                .WithAuthority(azureADConfig.Authority)
                .WithExtraQueryParameters("dc=ESTS-PUB-EUS-AZ1-FD000-TEST1")
                .WithDefaultRedirectUri();

            _clientApp = builder.Build();
            TokenCacheHelper.EnableSerialization(_clientApp.UserTokenCache);
        }
        
        private static IPublicClientApplication _clientApp;
        private static IConfiguration AppConfiguration;
        public static IPublicClientApplication PublicClientApp { get { return _clientApp; } }
    }
}
