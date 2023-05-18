using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;

var configuration =  new ConfigurationBuilder()
     .AddJsonFile($"appsettings.json");
            
var config = configuration.Build();
var publicClientOptions = config.GetSection("AzureAd");
var scopes = new string[] { }; // by default, MSAL attaches OIDC scopes to every token request

var app = PublicClientApplicationBuilder.Create(publicClientOptions.GetValue<string>("ClientId"))
    .WithAuthority(publicClientOptions.GetValue<string>("Authority"))
    .Build();

var result = await app.AcquireTokenWithDeviceCode(scopes, async deviceCode => {
    Console.WriteLine($"In a broswer, navigate to the URL '{deviceCode.VerificationUrl}' and enter the code '{deviceCode.UserCode}'");
    await Task.FromResult(0);
}).ExecuteAsync();

Console.WriteLine($"You signed in as {result.Account.Username}");
Console.WriteLine($"{result.Account.HomeAccountId}");
Console.WriteLine("\nRetrieved ID token:");
result.ClaimsPrincipal.Claims.ToList()
    .ForEach(c => Console.WriteLine(c));
