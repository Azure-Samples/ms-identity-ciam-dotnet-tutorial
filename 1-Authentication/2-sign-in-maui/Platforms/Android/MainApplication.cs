// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Android.App;
using Android.Runtime;

namespace SignInMaui;

[Application]
public class MainApplication(IntPtr handle, JniHandleOwnership ownership)
	: MauiApplication(handle, ownership)
{
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
