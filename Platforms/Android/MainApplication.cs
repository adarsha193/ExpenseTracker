using Android.App;
using Android.Runtime;
using System;

namespace ExpenseTracker;

[Application]
public class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
    }

    public override void OnCreate()
    {
        base.OnCreate();

        // Firebase is initialized via google-services.json and native SDKs
        // No explicit C# initialization needed
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}

