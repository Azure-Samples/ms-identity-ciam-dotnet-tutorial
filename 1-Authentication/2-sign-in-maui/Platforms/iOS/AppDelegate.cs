// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Foundation;
using SignInMaui.MSALClient;
using UIKit;

namespace SignInMaui;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
