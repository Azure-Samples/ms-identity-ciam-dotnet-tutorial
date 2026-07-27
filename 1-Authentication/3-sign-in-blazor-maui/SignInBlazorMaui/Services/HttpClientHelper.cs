namespace SignInBlazorMaui.Services;

using System.Net;

/// <summary>
/// Helper class to manage HttpClient configuration and API endpoint URLs.
/// </summary>
internal class HttpClientHelper
{
    private static string _baseUrl = "https://localhost:7157/";
    public static string BaseUrl
    {
        get
        {
#if DEBUG
            //See: https://learn.microsoft.com/dotnet/maui/data-cloud/local-web-services
            //Android Emulator uses 10.0.2.2 to refer to localhost
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                _baseUrl = _baseUrl.Replace("localhost", "10.0.2.2");
            }
#endif
            return _baseUrl;
        }
    }
    public static string WeatherUrl => $"{BaseUrl}api/weather";
    public static string ProfileUrl => $"{BaseUrl}api/profile";

    public static HttpClient GetHttpClient()
    {
        HttpClient client;
#if WINDOWS || MACCATALYST
        client = new HttpClient();
#else
        client = new HttpClient(new HttpsClientHandlerService().GetPlatformMessageHandler());
#endif

        // The local Kestrel dev server offers both HTTP/1.1 and HTTP/2. On iOS the
        // native handler can hang negotiating HTTP/2 against the local HTTPS server,
        // so prefer HTTP/1.1 for these local development calls.
        client.DefaultRequestVersion = HttpVersion.Version11;
        client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;

        // Fail fast instead of hanging if the dev server is unreachable.
        client.Timeout = TimeSpan.FromSeconds(30);

        return client;
    }
}

internal class HttpsClientHandlerService
{
    public HttpMessageHandler GetPlatformMessageHandler()
    {
#if ANDROID
        var handler = new Xamarin.Android.Net.AndroidMessageHandler();
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
        {
            if (cert != null && cert.Issuer.Equals("CN=localhost"))
                return true;
            return errors == System.Net.Security.SslPolicyErrors.None;
        };
        return handler;
#elif IOS
        var handler = new NSUrlSessionHandler
        {
            TrustOverrideForUrl = IsHttpsLocalhost
        };
        return handler;
#else
        throw new PlatformNotSupportedException("Only Android and iOS supported.");
#endif
    }

#if IOS
    public bool IsHttpsLocalhost(NSUrlSessionHandler sender, string url, Security.SecTrust trust)
    {
        return url.StartsWith("https://localhost");
    }
#endif
}
