using Microsoft.Identity.Client;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Interop;

namespace sign_in_dotnet_wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // By default, MSAL asks for the following scopes: 
        // openid - to get an IdToken 
        // profile - to get more details in the IdToken
        // offline_access - to get a RefreshToken 
        // If more scopes are needed, they can be specified in this array
        private static readonly string[] s_scopes = [];

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Flow for acquiring an access token
        /// </summary>
        private async void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            AuthenticationResult authResult = null;
            var app = App.PublicClientApp;
            ResultText.Text = string.Empty;
            TokenInfoText.Text = string.Empty;

            IAccount firstAccount;
            
            var accounts = await app.GetAccountsAsync();
            firstAccount = accounts.FirstOrDefault();

            try
            {
                // Try to sign in silently the previously signed-in user
                authResult = await app.AcquireTokenSilent(s_scopes, firstAccount)
                    .ExecuteAsync();
            }
            catch (MsalUiRequiredException ex)
            {
                // A MsalUiRequiredException happened on AcquireTokenSilent.
                // This indicates you need to call AcquireTokenInteractive to acquire a token: 
                // either the user never signed in, or the Identity Provider wants to run extra checks on the user, like MFA
                System.Diagnostics.Debug.WriteLine($"MsalUiRequiredException: {ex.Message}");

                try
                {
                    authResult = await app.AcquireTokenInteractive(s_scopes)
                        .WithAccount(firstAccount)
                        .WithParentActivityOrWindow(new WindowInteropHelper(this).Handle) 
                        .WithPrompt(Prompt.SelectAccount)
                        .ExecuteAsync();
                }
                catch (MsalException msalex)
                {
                    ResultText.Text = $"Error Acquiring Token:{System.Environment.NewLine}{msalex}";
                }
            }
            catch (Exception ex)
            {
                ResultText.Text = $"Error Acquiring Token Silently:{System.Environment.NewLine}{ex}";
                return;
            }

            if (authResult != null)
            {
                ResultText.Text = "Sign in was successful.";
                DisplayBasicTokenInfo(authResult);
                this.SignInButton.Visibility = Visibility.Collapsed;
                this.SignOutButton.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Sign out the current user
        /// </summary>
        private async void SignOutButton_Click(object sender, RoutedEventArgs e)
        {
            var accounts = await App.PublicClientApp.GetAccountsAsync();
            if (accounts.Any())
            {
                try
                {
                    await App.PublicClientApp.RemoveAsync(accounts.FirstOrDefault());
                    this.ResultText.Text = "User has signed-out";
                    this.TokenInfoText.Text = string.Empty;
                    this.SignInButton.Visibility = Visibility.Visible;
                    this.SignOutButton.Visibility = Visibility.Collapsed;
                }
                catch (MsalException ex)
                {
                    ResultText.Text = $"Error signing-out user: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// Display basic information contained in the User 
        /// </summary>
        private void DisplayBasicTokenInfo(AuthenticationResult authResult)
        {
            TokenInfoText.Text = "";
            if (authResult != null)
            {
                TokenInfoText.Text += $"Username: {authResult.Account.Username}" + Environment.NewLine;
                TokenInfoText.Text += $"{authResult.Account.HomeAccountId}" + Environment.NewLine;
            }
        }
    }
}

