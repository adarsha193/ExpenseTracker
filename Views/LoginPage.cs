using Microsoft.Maui.Controls;
using ExpenseTracker.Services;
using ExpenseTracker.Resources;

#if __IOS__
using UIKit;
using Foundation;
#endif

namespace ExpenseTracker;

/// <summary>
/// Login Page - User Authentication Feature
/// 
/// FEATURES:
/// - Email and password authentication via Firebase
/// - "Forgot Password" link for password recovery
/// - "Register" link for new user signup
/// - Platform-specific password field handling (iOS autofill suppression)
/// - Error messaging and loading state indicators
/// - Secure credential handling
/// 
/// FLOW:
/// 1. User enters email and password
/// 2. OnLoginClicked validates input
/// 3. Calls FirebaseRealtimeDbService.LoginAsync()
/// 4. On success: Navigate to DashboardPage
/// 5. On failure: Display error message
/// 
/// SECURITY:
/// - Passwords never logged or stored locally
/// - Auth token stored in SecureStorage
/// - HTTPS only communication with Firebase
/// </summary>
public partial class LoginPage : ContentPage
{
    private readonly FirebaseRealtimeDbService _authService;
    private readonly IUserDialogService _dialogs;
    private bool _isLoading = false;

    public LoginPage()
    {
        InitializeComponent();
        _authService = ServiceLocator.GetRequiredService<FirebaseRealtimeDbService>();
        _dialogs = ServiceLocator.GetRequiredService<IUserDialogService>();

        // Disable strong password suggestions on iOS
        DisablePasswordAutofill();
    }

    /// <summary>
    /// Disables the automatic strong password suggestion on iOS
    /// </summary>
    private void DisablePasswordAutofill()
    {
#if __IOS__
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (PasswordEntry.Handler?.PlatformView is UITextField passwordField)
            {
                passwordField.TextContentType = UITextContentType.Password;
                if (passwordField.RespondsToSelector(new ObjCRuntime.Selector("setPasswordRules:")))
                {
                    passwordField.PasswordRules = null;
                }
            }
        });
#endif
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(EmailEntry.Text) || string.IsNullOrWhiteSpace(PasswordEntry.Text))
        {
            await _dialogs.ShowAlertAsync(Strings.ErrorTitle, Strings.PleaseEnterEmailAndPassword);
            return;
        }

        if (_isLoading)
            return;

        _isLoading = true;
        try
        {
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;
            LoginButton.IsEnabled = false;
            LoginButton.Opacity = 0.5;

            var result = await _authService.LoginAsync(
                EmailEntry.Text.Trim(),
                PasswordEntry.Text
            );

            if (result.Success && result.User != null)
            {
                if (!string.IsNullOrEmpty(result.Token))
                    await SecureStorage.SetAsync("auth_token", result.Token);

                await SecureStorage.SetAsync("user_id", result.User.Id);
                await SecureStorage.SetAsync("user_email", result.User.Email);
                await SecureStorage.SetAsync("user_name", result.User.FullName);

                // await _dialogs.ShowAlertAsync(Strings.SuccessTitle, Strings.AccountCreated);

                if (Application.Current?.Windows.Count > 0)
                {
                    Application.Current.Windows[0].Page = new DashboardShell();
                }
            }
            else
            {
                await _dialogs.ShowAlertAsync(Strings.ErrorTitle, result.Message ?? Strings.SignIn);
            }
        }
        catch (Exception ex)
        {
            await _dialogs.ShowAlertAsync(Strings.ErrorTitle, string.Format(Strings.AnErrorOccurred, ex.Message));
        }
        finally
        {
            _isLoading = false;
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            LoginButton.IsEnabled = true;
            LoginButton.Opacity = 1.0;
        }
    }

    private async void OnForgotPasswordTapped(object sender, EventArgs e)
    {
        if (Shell.Current != null)
        {
            await Shell.Current.GoToAsync(Strings.RouteForgotPassword);
        }
        else
        {
            await Navigation.PushAsync(new ForgotPasswordPage());
        }
    }

    private async void OnSignUpTapped(object sender, EventArgs e)
    {
        if (Shell.Current != null)
        {
            await Shell.Current.GoToAsync(Strings.RouteSignup);
        }
        else
        {
            await Navigation.PushAsync(new SignUpPage());
        }
    }
}
