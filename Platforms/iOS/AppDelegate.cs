using Foundation;
using UIKit;

namespace ExpenseTracker;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override bool FinishedLaunching(UIApplication application, NSDictionary? launchOptions)
    {
        // Firebase is initialized via CocoaPods at the native level
        // No explicit C# initialization needed for iOS
        return base.FinishedLaunching(application, launchOptions);
    }
}

