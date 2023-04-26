using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;

var configuration =  new ConfigurationBuilder()
     .AddJsonFile($"appsettings.json");
            
var config = configuration.Build();
var publicClientOptions = config.GetSection("AzureAd").Get<PublicClientApplicationOptions>()!;

var app = PublicClientApplicationBuilder.CreateWithApplicationOptions(publicClientOptions)
    .Build();

var result = await app.AcquireTokenWithDeviceCode(new [] { "openid" }, async deviceCode => {
    Console.WriteLine($"In a broswer, navigate to the URL '{deviceCode.VerificationUrl}' and enter the code '{deviceCode.UserCode}'");
    await Task.FromResult(0);
})
.ExecuteAsync();

Console.WriteLine($"You signed in as {result.Account.Username}");
Console.WriteLine($"{result.Account.HomeAccountId}");
Console.WriteLine("\nRetrieved ID token:");
Console.WriteLine(result.IdToken);
