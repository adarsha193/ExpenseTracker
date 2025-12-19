using Microsoft.Extensions.Logging;
using ExpenseTracker.Services;
using ExpenseTracker.Converters;

namespace ExpenseTracker;

/// <summary>
/// MAUI Application Configuration and Dependency Injection Setup.
/// 
/// FEATURES INITIALIZED:
/// - Firebase Authentication & Realtime Database integration
/// - Dependency Injection container for all services
/// - Cross-platform font and UI configuration
/// - Logging for debug builds
/// 
/// KEY SERVICES REGISTERED:
/// 1. FirebaseRealtimeDbService - Handles all Firebase Auth and RTDB REST API calls
/// 2. IUserDialogService - Centralized UI alerts and dialogs
/// 3. BudgetAlertService - Monitors budget thresholds and overspending
/// 4. NotificationService - Sends local notifications for alerts
/// 5. AISuggestionsService - Generates smart budget recommendations
/// 
/// SETUP REQUIREMENTS:
/// - Update Firebase Web API Key (line 22) from Firebase Console
/// - Ensure all platforms have required SDKs installed
/// - ServiceLocator is initialized after app build for legacy code compatibility
/// </summary>
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

		// ============================================================================
		// Value Converters for Data Binding
		// ============================================================================
		// Register converters for use in XAML
		if (Application.Current?.Resources != null)
		{
			Application.Current.Resources["InvestmentReturnConverter"] = new InvestmentReturnConverter();
		}

		// ============================================================================
		// Firebase Integration - Authentication & Realtime Database
		// ============================================================================
		// REQUIRED: Set your Firebase Web API Key from Firebase Console
		// Go to: Firebase Console → Project Settings → General → Copy Web API Key
		const string firebaseWebApiKey = "AIzaSyD0IZpbPPe1v8HprK8IoEIkz0DijbBsUT0";
		
		builder.Services.AddSingleton(new HttpClient());
		builder.Services.AddSingleton(sp => 
			new FirebaseRealtimeDbService(sp.GetRequiredService<HttpClient>(), firebaseWebApiKey)
		);

		// ============================================================================
		// User Interface & Dialog Services
		// ============================================================================
		// Centralized service for alerts, confirmations, and user interactions
		builder.Services.AddSingleton<IUserDialogService, UserDialogService>();

		// ============================================================================
		// Business Logic Services - Budget, Notifications & AI
		// ============================================================================
		// Budget monitoring and overspending detection
		builder.Services.AddSingleton<BudgetAlertService>();
		
		// Local notifications for budget alerts and reminders
		builder.Services.AddSingleton<NotificationService>();
		
		// AI-powered budget recommendations and spending insights
		builder.Services.AddSingleton<AISuggestionsService>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		var app = builder.Build();

		// Service locator for parts of code that cannot use constructor injection yet.
		// This is a pragmatic bridge; prefer constructor DI for new code.
		ServiceLocator.Provider = app.Services;

		return app;
	}
}
