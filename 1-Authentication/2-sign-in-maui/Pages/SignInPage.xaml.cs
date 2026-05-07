// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using SignInMaui.MSALClient;

namespace SignInMaui.Pages;

public partial class SignInPage : ContentPage
{
    private readonly MsalTokenService _tokenService;
    private readonly ILogger<SignInPage> _logger;
    private bool _isNavigating;

    public SignInPage(MsalTokenService tokenService, ILogger<SignInPage> logger)
    {
        _tokenService = tokenService;
        _logger = logger;
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (_isNavigating) return;

        try
        {
            var account = await _tokenService.GetAccountAsync();
            if (account is not null)
            {
                _logger.LogInformation("Cached account: {Account}, navigating to claims", account.Username);
                _isNavigating = true;
                await Shell.Current.GoToAsync("//claims");
            }
            else
            {
                SignInButton.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking cached account");
            SignInButton.IsEnabled = true;
        }
        finally
        {
            _isNavigating = false;
        }
    }

    private async void OnSignInClicked(object sender, EventArgs e)
    {
        try
        {
            SignInButton.IsEnabled = false;
            _logger.LogInformation("Sign-in clicked");
            await _tokenService.SignInAsync();
            _logger.LogInformation("Sign-in succeeded");
            await Shell.Current.GoToAsync("//claims");
        }
        catch (MsalClientException ex) when (ex.ErrorCode == "authentication_canceled")
        {
            _logger.LogInformation("Sign-in canceled by user");
            SignInButton.IsEnabled = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sign-in failed");
            SignInButton.IsEnabled = true;
        }
    }
}
