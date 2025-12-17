using Microsoft.Extensions.Logging;
using ExpenseTracker.Services;

namespace ExpenseTracker;

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

		// Register Firebase Realtime Database Authentication Service
		// Firebase Web API Key from google-services.json
		const string firebaseWebApiKey = "AIzaSyD0IZpbPPe1v8HprK8IoEIkz0DijbBsUT0";
		
		builder.Services.AddSingleton(new HttpClient());
		builder.Services.AddSingleton(sp => 
			new FirebaseRealtimeDbService(sp.GetRequiredService<HttpClient>(), firebaseWebApiKey)
		);

		// Dialog service for centralized alerts and confirmations
		builder.Services.AddSingleton<IUserDialogService, UserDialogService>();

		// Budget and notification services
		builder.Services.AddSingleton<BudgetAlertService>();
		builder.Services.AddSingleton<NotificationService>();
		builder.Services.AddSingleton<AISuggestionsService>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

			var app = builder.Build();

			// Expose the built service provider for simple access from pages/viewmodels.
			// Prefer constructor injection for new code.
			ServiceLocator.Provider = app.Services;

			return app;
	}
}
