using System.Windows;
using System.Reflection;
using Microsoft.Identity.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client.Extensions.Msal;

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

        public static IPublicClientApplication PublicClientApp { get { return _clientApp; } }


        public static void CreateApplication()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("sign_in_dotnet_wpf.appsettings.json");
            AppConfiguration = new ConfigurationBuilder()
               .AddJsonStream(stream)
               .Build();

            AzureAdConfig azureADConfig = AppConfiguration.GetSection("AzureAd").Get<AzureAdConfig>();

            var builder = PublicClientApplicationBuilder.Create(azureADConfig.ClientId)
                .WithAuthority(azureADConfig.Authority)
                .WithDefaultRedirectUri();

            _clientApp = builder.Build();

            RegisterTokenCache();
        }

        private static void RegisterTokenCache()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var appName = assembly.GetName().Name;

            // Configured only for Windows, but Mac and Linux options are available.
            // On Windows, the data is encrypted using DPAPI
            var storageProperties = new StorageCreationPropertiesBuilder(appName + ".msalcache.bin", MsalCacheHelper.UserRootDirectory)
                   .Build();

            var msalcachehelper = MsalCacheHelper.CreateAsync(storageProperties).GetAwaiter().GetResult();
            msalcachehelper.RegisterCache(_clientApp.UserTokenCache);
        }

        private static IPublicClientApplication _clientApp;
        private static IConfiguration AppConfiguration;
    }
}
