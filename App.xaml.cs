namespace ExpenseTracker;

/// <summary>
/// ExpenseTracker application entry point.
///
/// This class starts the MAUI app, sets up the primary navigation shell,
/// and creates the app window used across platforms. Service registration
/// and app configuration live in `MauiProgram.CreateMauiApp()`.
///
/// Notes:
/// - Configure the Firebase API key before launching (see FIREBASE_API_KEY_SETUP.md).
/// - The app's navigation and pages are declared in `AppShell`.
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
