using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile($"appsettings.json");

        var config = configuration.Build();
        var publicClientOptions = config.GetSection("AzureAd");
        var scopes = new string[] { }; // by default, MSAL attaches OIDC scopes (openid, profile, offline_access) to every token request

        // In a more complex app, you can keep the PublicClientApplication object a singleton
        var app = PublicClientApplicationBuilder.Create(publicClientOptions.GetValue<string>("ClientId"))
          .WithAuthority(publicClientOptions.GetValue<string>("Authority"))
          .Build();
        RegisterTokenCache(app);


        AuthenticationResult result;

        try
        {
            var accounts = await app.GetAccountsAsync();
            var firstAccount = accounts.FirstOrDefault();

            // Try to sign in silently the previously signed-in user
            result = await app.AcquireTokenSilent(scopes, firstAccount)
                .ExecuteAsync();
        }
        catch (MsalUiRequiredException)
        {
            // No token found in the cache or no token silent token refresh is possible
            result = await app.AcquireTokenWithDeviceCode(scopes, async deviceCode =>
            {
                Console.WriteLine($"In a broswer, navigate to the URL '{deviceCode.VerificationUrl}' and enter the code '{deviceCode.UserCode}'");
                await Task.FromResult(0);
            }).ExecuteAsync();
        }

        Console.WriteLine($"You signed in as {result.Account.Username}");
        Console.WriteLine($"{result.Account.HomeAccountId}");
        Console.WriteLine("\nRetrieved ID token:");
        result.ClaimsPrincipal.Claims.ToList()
            .ForEach(c => Console.WriteLine(c));
    }

    private static void RegisterTokenCache(IPublicClientApplication app)
    {
        
        var appName = Assembly.GetCallingAssembly().GetName().Name;

        // Configured only for Windows, but Mac and Linux options are available - see the WithMacKeyChain and WithLinuxKeyring methods
        // On Windows, the data is encrypted using DPAPI
        var storageProperties = new StorageCreationPropertiesBuilder(appName + ".msalcache.bin", MsalCacheHelper.UserRootDirectory)            
               .Build();

        var msalcachehelper = MsalCacheHelper.CreateAsync(storageProperties).GetAwaiter().GetResult();
        msalcachehelper.RegisterCache(app.UserTokenCache);
    }
}