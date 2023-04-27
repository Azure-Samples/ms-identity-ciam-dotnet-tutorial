// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using SignInMaui.MSALClient;
using Microsoft.Identity.Client;

namespace SignInMaui.Views
{
    public partial class MainView : ContentPage
    {
        public MainView()
        {
            InitializeComponent();

            IAccount cachedUserAccount = PublicClientSingleton.Instance.MSALClientHelper.FetchSignedInUserFromCache().Result;

            _ = Dispatcher.DispatchAsync(async () =>
            {
                if (cachedUserAccount == null)
                {
                    SignInButton.IsEnabled = true;
                }
                else
                {
                    await Shell.Current.GoToAsync("claimsview");
                }
            });
        }

        private async void OnSignInClicked(object sender, EventArgs e)
        {
            await PublicClientSingleton.Instance.AcquireTokenSilentAsync();
            await Shell.Current.GoToAsync("claimsview");
        }
        protected override bool OnBackButtonPressed() { return true; }

    }
}
