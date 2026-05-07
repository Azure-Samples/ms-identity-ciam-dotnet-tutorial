// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Android.App;
using Microsoft.Identity.Client;

namespace SignInMaui.Platforms.Android;

/// <summary>
/// Activity that handles the MSAL redirect URI callback from the system browser
/// after Entra External ID authentication completes.
/// The intent filter is declared in AndroidManifest.xml so that scripts can
/// replace the placeholder client ID.
/// </summary>
[Activity(Exported = true, Name = "com.companyname.signinmaui.MsalActivity")]
public class MsalActivity : BrowserTabActivity
{
}
