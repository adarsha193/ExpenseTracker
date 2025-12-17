using System;
using Microsoft.Maui.Controls;
using ExpenseTracker.Services;
using ExpenseTracker.Resources;
using ExpenseTracker.SalaryPages;
using ExpenseTracker.InvestmentPages;
using ExpenseTracker.BudgetPages;

namespace ExpenseTracker
{
    public partial class DashboardShell : Shell
    {
        public static readonly BindableProperty UserNameProperty =
            BindableProperty.Create(nameof(UserName), typeof(string), typeof(DashboardShell), "Welcome User");

        public static readonly BindableProperty UserEmailProperty =
            BindableProperty.Create(nameof(UserEmail), typeof(string), typeof(DashboardShell), "user@example.com");

        public string UserName
        {
            get => (string)GetValue(UserNameProperty);
            set => SetValue(UserNameProperty, value);
        }

        public string UserEmail
        {
            get => (string)GetValue(UserEmailProperty);
            set => SetValue(UserEmailProperty, value);
        }

        public DashboardShell()
        {
            InitializeComponent();
            BindingContext = this;
            LoadUserData();
        }

        private async void LoadUserData()
        {
            try
            {
                // Get user data from SecureStorage
                var userName = await SecureStorage.GetAsync("user_name");
                var userEmail = await SecureStorage.GetAsync("user_email");

                if (!string.IsNullOrEmpty(userName))
                    UserName = userName;

                if (!string.IsNullOrEmpty(userEmail))
                    UserEmail = userEmail;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading user data: {ex.Message}");
            }
        }

        private async void OnMenuItemClicked(object sender, EventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                switch (menuItem.Text)
                {
                    case "Profile":
                        await DisplayAlertAsync("Profile", Strings.ProfilePageComingSoon, Strings.OKButton);
                        break;

                    case "Settings":
                        await DisplayAlertAsync("Settings", Strings.SettingsPageComingSoon, Strings.OKButton);
                        break;

                    case "Help":
                        await DisplayAlertAsync("Help", Strings.HelpPageComingSoon, Strings.OKButton);
                        break;
                }
            }
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

                // Clear stored credentials
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
