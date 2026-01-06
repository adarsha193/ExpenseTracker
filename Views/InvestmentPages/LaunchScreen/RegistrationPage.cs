using Microsoft.Maui.Controls;
using ExpenseTracker.Services;
using ExpenseTracker.Resources;

#if __IOS__
using UIKit;
using Foundation;
#endif

namespace ExpenseTracker;

public partial class SignUpPage : ContentPage
{
    private readonly FirebaseRealtimeDbService _authService;
    private readonly IUserDialogService _dialogs;
    private bool _isLoading = false;

        public SignUpPage()
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
        // Access the native UITextField for password fields and disable strong password suggestions
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (PasswordEntry.Handler?.PlatformView is UITextField passwordField)
            {
                passwordField.TextContentType = UITextContentType.Password;
                // Remove password rules to disable strong password suggestions
                if (passwordField.RespondsToSelector(new ObjCRuntime.Selector("setPasswordRules:")))
                {
                    passwordField.PasswordRules = null;
                }
            }

            if (ConfirmPasswordEntry.Handler?.PlatformView is UITextField confirmField)
            {
                confirmField.TextContentType = UITextContentType.Password;
                if (confirmField.RespondsToSelector(new ObjCRuntime.Selector("setPasswordRules:")))
                {
                    confirmField.PasswordRules = null;
                }
            }
        });
#endif
    }

    private async void OnSignUpClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameEntry.Text) || 
            string.IsNullOrWhiteSpace(EmailEntry.Text) || 
            string.IsNullOrWhiteSpace(PasswordEntry.Text) ||
            string.IsNullOrWhiteSpace(ConfirmPasswordEntry.Text))
        {
                await _dialogs.ShowAlertAsync(Strings.ErrorTitle, Strings.PleaseFillAllFields);
            return;
        }

        if (PasswordEntry.Text != ConfirmPasswordEntry.Text)
        {
                await _dialogs.ShowAlertAsync(Strings.ErrorTitle, Strings.PasswordsDoNotMatch);
            return;
        }

        if (_isLoading)
            return;

        _isLoading = true;
        try
        {
            // Show loading indicator
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;
            SignUpButton.IsEnabled = false;
            SignUpButton.Opacity = 0.5;

            // Call Firebase register
            var result = await _authService.RegisterAsync(
                NameEntry.Text.Trim(),
                EmailEntry.Text.Trim(),
                PasswordEntry.Text
            );

            if (result.Success && result.User != null)
            {
                // Store token if provided
                if (!string.IsNullOrEmpty(result.Token))
                {
                    await SecureStorage.SetAsync("auth_token", result.Token);
                }

                // Store user info
                await SecureStorage.SetAsync("user_id", result.User.Id);
                await SecureStorage.SetAsync("user_email", result.User.Email);
                await SecureStorage.SetAsync("user_name", result.User.FullName);

                await _dialogs.ShowAlertAsync(Strings.SuccessTitle, Strings.AccountCreated);
                await Shell.Current.GoToAsync(Strings.RouteLogin);
            }
            else
            {
                await _dialogs.ShowAlertAsync(Strings.ErrorTitle, result.Message ?? Strings.RegistrationFailed);
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
            SignUpButton.IsEnabled = true;
            SignUpButton.Opacity = 1.0;
        }
    }

    private async void OnSignInTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(Strings.RouteLogin);
    }
}
