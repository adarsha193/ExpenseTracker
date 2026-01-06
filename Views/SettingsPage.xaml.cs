using Microsoft.Maui.Controls;
using ExpenseTracker.Services;
using ExpenseTracker.Resources;

namespace ExpenseTracker
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

 private void DarkModeSwitch_Toggled(object sender, ToggledEventArgs e)
{
    if (e.Value)   // Switch ON
    {
        Application.Current.UserAppTheme = AppTheme.Dark;
    }
    else           // Switch OFF
    {
        Application.Current.UserAppTheme = AppTheme.Light;
    }
}
        private void OnChangePasswordClicked(object sender, EventArgs e)
        {
            // Navigate to ChangePasswordPage
            Navigation.PushAsync(new ChangePasswordPage());
        }


        private void OnAboutClicked(object sender, EventArgs e)
        {
            // Navigate to AboutAppPage
            Navigation.PushAsync(new AboutAppPage());
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlertAsync(Strings.LogoutTitle, Strings.LogoutConfirm, Strings.YesButton, Strings.NoButton);

            if (!confirm)
                return;

            try
            {
                var authService = ServiceLocator.GetRequiredService<FirebaseRealtimeDbService>();
                var result = await authService.LogoutAsync();

                if (!result.Success)
                {
                    await DisplayAlertAsync(Strings.ErrorTitle, result.Message ?? Strings.LogoutFailed, Strings.OKButton);
                    return;
                }

                // Clear stored credentials locally
                SecureStorage.Remove("auth_token");
                SecureStorage.Remove("user_id");
                SecureStorage.Remove("user_email");
                SecureStorage.Remove("user_name");

                // Navigate back to login page
                if (Application.Current?.Windows.Count > 0)
                {
                    Application.Current.Windows[0].Page = new AppShell();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync(Strings.ErrorTitle, string.Format(Strings.LogoutError, ex.Message), Strings.OKButton);
            }
        }
    }
}
