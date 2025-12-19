namespace ExpenseTracker;

/// <summary>
/// Main Application class for ExpenseTracker MAUI app.
/// 
/// FEATURES:
/// - Initializes the app shell which provides navigation between pages
/// - Manages the app lifecycle and window creation
/// - Serves as the entry point for the cross-platform MAUI application
/// 
/// SETUP:
/// - Ensure Firebase API key is configured in MauiProgram.cs before app startup
/// - All services are registered in MauiProgram.CreateMauiApp()
/// - The app uses AppShell for declarative navigation between feature pages
/// </summary>
public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window
        {
            Page = new AppShell()
        };
    }
}
