using Microsoft.Maui.Controls;
using ExpenseTracker.Services;
using ExpenseTracker.Resources;

namespace ExpenseTracker;

public partial class ForgotPasswordPage : ContentPage
{
    private readonly FirebaseRealtimeDbService _authService;
    private readonly IUserDialogService _dialogs;
    private bool _isLoading = false;

        public ForgotPasswordPage()
    {
        InitializeComponent();
        _authService = ServiceLocator.GetRequiredService<FirebaseRealtimeDbService>();
        _dialogs = ServiceLocator.GetRequiredService<IUserDialogService>();
    }

    private async void OnSendResetClicked(object sender, EventArgs e)
    {
            if (string.IsNullOrWhiteSpace(EmailEntry.Text))
            {
                await _dialogs.ShowAlertAsync(Strings.ErrorTitle, Strings.PleaseEnterEmail);
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
                ResetButton.IsEnabled = false;
                ResetButton.Opacity = 0.5;

            // Call Firebase forgot password
            var result = await _authService.ForgotPasswordAsync(EmailEntry.Text.Trim());

            if (result.Success)
            {
                await _dialogs.ShowAlertAsync(Strings.SuccessTitle, result.Message ?? Strings.ResetLinkSent);
                
                // Clear the email field
                EmailEntry.Text = string.Empty;
                
                // Navigate back to login
                await Shell.Current.GoToAsync(Strings.RouteLogin);
            }
            else
            {
                await _dialogs.ShowAlertAsync(Strings.ErrorTitle, result.Message ?? Strings.FailedToSendResetLink);
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
            ResetButton.IsEnabled = true;
            ResetButton.Opacity = 1.0;
        }
    }

    private async void OnBackToLoginTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(Strings.RouteLogin);
    }
}
