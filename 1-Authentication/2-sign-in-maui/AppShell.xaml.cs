// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using SignInMaui.Views;

namespace SignInMaui;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("mainview", typeof(MainView));
        Routing.RegisterRoute("claimsview", typeof(ClaimsView));
    }
}
