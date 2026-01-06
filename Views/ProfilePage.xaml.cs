using Microsoft.Maui.Controls;
using ExpenseTracker.Services;
using ExpenseTracker.Resources;
using ExpenseTracker.Models;
using System.Linq;

namespace ExpenseTracker;

public partial class ProfilePage : ContentPage
{
    private readonly FirebaseRealtimeDbService _authService;
    private string _userId = "";
    private string _authToken = "";

    public ProfilePage()
    {
        InitializeComponent();
        _authService = ServiceLocator.GetRequiredService<FirebaseRealtimeDbService>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadProfileAsync();
    }

    private async Task LoadProfileAsync()
    {
        try
        {
            // Show loading indicator
            var loadingIndicator = FindByName("LoadingIndicatorProfile") as VerticalStackLayout;
            var profileContent = FindByName("ProfileContent") as VerticalStackLayout;
            
            if (loadingIndicator != null) loadingIndicator.IsVisible = true;
            if (profileContent != null) profileContent.IsVisible = false;
            
            // Get user credentials from secure storage
            _userId = await SecureStorage.GetAsync("user_id") ?? "";
            _authToken = await SecureStorage.GetAsync("auth_token") ?? "";

            if (string.IsNullOrEmpty(_userId))
            {
                if (loadingIndicator != null) loadingIndicator.IsVisible = false;
                if (profileContent != null) profileContent.IsVisible = false;
                return;
            }

            // Load profile data from Firebase
            var userData = await _authService.GetUserDataFromDb(_userId);
            if (userData != null)
            {
                // Update header labels with actual user data
                HeaderNameLabel.Text = userData.FullName;
                HeaderEmailLabel.Text = userData.Email;
                
                // Update form fields
                NameEntry.Text = userData.FullName;
                EmailEntry.Text = userData.Email;
                PhoneEntry.Text = userData.PhoneNumber ?? "";
                LocationEntry.Text = userData.Location ?? "";

                // Update statistics section
                UpdateStatistics(userData);
            }
            
            // Hide loading indicator and show content
            if (loadingIndicator != null) loadingIndicator.IsVisible = false;
            if (profileContent != null) profileContent.IsVisible = true;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(Strings.ErrorTitle, string.Format(Strings.FailedToLoadProfile, ex.Message), Strings.OKButton);
            
            var loadingIndicator = FindByName("LoadingIndicatorProfile") as VerticalStackLayout;
            if (loadingIndicator != null) loadingIndicator.IsVisible = false;
        }
    }

    private void UpdateStatistics(UserData userData)
    {
        // Update total expenses
        TotalExpensesLabel.Text = $"₹{userData.TotalExpenses:F2}";

        // Update member since and duration
        if (userData.CreatedAt != DateTime.MinValue)
        {
            MemberSinceLabel.Text = userData.CreatedAt.ToString("MMM yyyy");
            
            // Calculate months since joining
            var now = DateTime.UtcNow;
            var createdDate = userData.CreatedAt.Kind == DateTimeKind.Unspecified 
                ? DateTime.SpecifyKind(userData.CreatedAt, DateTimeKind.Utc)
                : userData.CreatedAt;
            
            var monthsElapsed = (now.Year - createdDate.Year) * 12 + (now.Month - createdDate.Month);
            
            if (monthsElapsed <= 0)
                MemberDurationLabel.Text = "Recently Joined";
            else if (monthsElapsed == 1)
                MemberDurationLabel.Text = "1 Month";
            else
                MemberDurationLabel.Text = $"{monthsElapsed} Months";
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            // Get updated values
            string name = NameEntry.Text;
            string email = EmailEntry.Text;
            string phone = PhoneEntry.Text ?? "";
            string location = LocationEntry.Text ?? "";

            // Validate inputs
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email))
            {
                await DisplayAlertAsync(Strings.ValidationError, Strings.NameAndEmailRequired, Strings.OKButton);
                return;
            }

            // Validate email format
            if (!email.Contains("@"))
            {
                await DisplayAlertAsync(Strings.ValidationError, Strings.PleaseEnterValidEmail, Strings.OKButton);
                return;
            }

            // Show loading state
            var saveButton = sender as Button;
            if (saveButton != null)
            {
                saveButton.IsEnabled = false;
                saveButton.Opacity = 0.5;
            }

            // Calculate total expenses from all user expenses
            decimal totalExpenses = 0;
            var expenseResponse = await _authService.GetUserExpensesAsync(_userId);
            if (expenseResponse?.Expenses != null)
            {
                totalExpenses = expenseResponse.Expenses.Sum(e => e.Amount);
            }

            // Update profile in Firebase with all fields including total expenses
            var result = await _authService.UpdateUserProfileAsync(
                userId: _userId,
                fullName: name,
                email: email,
                phone: phone,
                location: location,
                totalExpenses: totalExpenses,
                profileImageUrl: null,
                idToken: _authToken
            );

            if (result.Success)
            {
                // Update header labels after successful save
                HeaderNameLabel.Text = name;
                HeaderEmailLabel.Text = email;

                // Update statistics
                var userData = await _authService.GetUserDataFromDb(_userId);
                if (userData != null)
                {
                    UpdateStatistics(userData);
                }

                await DisplayAlertAsync(Strings.SuccessTitle, Strings.ProfileUpdatedSuccess, Strings.OKButton);
            }
            else
            {
                await DisplayAlertAsync(Strings.ErrorTitle, result.Message, Strings.OKButton);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(Strings.ErrorTitle, string.Format(Strings.FailedToLoadProfile, ex.Message), Strings.OKButton);
        }
        finally
        {
            var saveButton = sender as Button;
            if (saveButton != null)
            {
                saveButton.IsEnabled = true;
                saveButton.Opacity = 1.0;
            }
        }
    }

    private async void OnChangePasswordClicked(object sender, EventArgs e)
    {
        // TODO: Implement password change functionality
        await DisplayAlertAsync("Change Password", Strings.PasswordChangeComingSoon, Strings.OKButton);
    }

    private async void OnDeleteAccountClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlertAsync(Strings.DeleteAccountConfirm,
            Strings.DeleteAccountMessage,
            Strings.DeleteAccountConfirm, Strings.NoButton);

        if (confirm)
        {
            // TODO: Implement account deletion
            await DisplayAlertAsync("Deleted", Strings.AccountDeleted, Strings.OKButton);
        }
    }
}