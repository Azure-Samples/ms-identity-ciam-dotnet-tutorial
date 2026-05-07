using Microsoft.Extensions.Logging;
using SignInMaui.MSALClient;

namespace SignInMaui.Pages;

public partial class ClaimsPage : ContentPage
{
    private readonly MsalTokenService _tokenService;
    private readonly ILogger<ClaimsPage> _logger;

    public IEnumerable<string> IdTokenClaims { get; set; } = ["No claims found in ID token"];

    public ClaimsPage(MsalTokenService tokenService, ILogger<ClaimsPage> logger)
    {
        _tokenService = tokenService;
        _logger = logger;
        BindingContext = this;
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        await LoadClaimsAsync();
    }

    private async Task LoadClaimsAsync()
    {
        try
        {
            _logger.LogInformation("Acquiring token...");
            var result = await _tokenService.SignInAsync();

            IdTokenClaims = result.ClaimsPrincipal.Claims.Select(c => c.Value);
            Claims.ItemsSource = IdTokenClaims;
            _logger.LogInformation("Showing {Count} claims", IdTokenClaims.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading claims");
            // Clear cached account to avoid infinite redirect loop
            await _tokenService.SignOutAsync();
            await Shell.Current.GoToAsync("//signin");
        }
    }

    private async void SignOutButton_Clicked(object sender, EventArgs e)
    {
        _logger.LogInformation("Signing out");
        await _tokenService.SignOutAsync();
        await Shell.Current.GoToAsync("//signin");
    }

    protected override bool OnBackButtonPressed() => true;
}