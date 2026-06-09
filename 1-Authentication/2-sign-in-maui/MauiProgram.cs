// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Microsoft.Extensions.Logging;
using SignInMaui.MSALClient;

namespace SignInMaui;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Logging
		builder.Logging.AddDebug();

		// Register MSAL authentication services
		builder.Services.AddMsalClient();

		// Register pages (Shell caches ShellContent instances)
		builder.Services.AddSingleton<Pages.SignInPage>();
		builder.Services.AddSingleton<Pages.ClaimsPage>();

		return builder.Build();
	}
}
