using ExpenseTracker.Models;
using ExpenseTracker.Services;

namespace ExpenseTracker.SalaryPages;

public partial class SalaryPage : ContentPage
{
    private readonly FirebaseRealtimeDbService _firebaseService;
    private string? _currentUserId;
    private SalaryModel? _currentSalary;

    public SalaryPage()
    {
        InitializeComponent();
        _firebaseService = ServiceLocator.Provider?.GetService<FirebaseRealtimeDbService>()
            ?? throw new InvalidOperationException("FirebaseRealtimeDbService not registered");

        FrequencyPicker.ItemsSource = new List<string>
        {
            "Daily",
            "Weekly",
            "Bi-weekly",
            "Monthly",
            "Quarterly",
            "Annual"
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Get user ID from secure storage
        _currentUserId = await SecureStorage.GetAsync("user_id");
        if (!string.IsNullOrEmpty(_currentUserId))
        {
            await LoadSalaryData();
        }
    }

    private async Task LoadSalaryData()
    {
        if (string.IsNullOrEmpty(_currentUserId))
            return;

        try
        {
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;

            var response = await _firebaseService.GetSalaryAsync(_currentUserId);
            
            if (response.Success && response.Salary != null)
            {
                _currentSalary = response.Salary;
                UpdateSalaryDisplay();
            }
            else
            {
                CurrentSalaryLabel.Text = "No salary recorded yet";
                SalaryDetailsLabel.Text = "";
            }
        }
        catch (Exception ex)
        {
            var dialogService = ServiceLocator.Provider?.GetService<IUserDialogService>();
            if (dialogService != null)
                await dialogService.ShowAlertAsync("Error", $"Failed to load salary data: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private void UpdateSalaryDisplay()
    {
        if (_currentSalary == null)
            return;

        CurrentSalaryLabel.Text = $"₹{_currentSalary.Amount:N2}";
        SalaryDetailsLabel.Text = $"Frequency: {_currentSalary.Frequency}\nLast Updated: {_currentSalary.UpdatedAt:d MMM yyyy}";

        // Populate form with current values
        AmountEntry.Text = _currentSalary.Amount.ToString("F2");
        FrequencyPicker.SelectedItem = _currentSalary.Frequency;
        NotesEntry.Text = _currentSalary.Notes;
    }

    private async void OnSaveSalaryClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_currentUserId))
        {
            var dialogService = ServiceLocator.Provider?.GetService<IUserDialogService>();
            if (dialogService != null)
                await dialogService.ShowAlertAsync("Error", "User not authenticated", "OK");
            return;
        }

        var userDialogService = ServiceLocator.Provider?.GetService<IUserDialogService>();

        // Validate inputs
        if (string.IsNullOrWhiteSpace(AmountEntry.Text))
        {
            if (userDialogService != null)
                await userDialogService.ShowAlertAsync("Validation", "Please enter a salary amount", "OK");
            return;
        }

        if (!decimal.TryParse(AmountEntry.Text, out decimal amount) || amount <= 0)
        {
            if (userDialogService != null)
                await userDialogService.ShowAlertAsync("Validation", "Please enter a valid salary amount", "OK");
            return;
        }

        if (FrequencyPicker.SelectedIndex < 0)
        {
            if (userDialogService != null)
                await userDialogService.ShowAlertAsync("Validation", "Please select a payment frequency", "OK");
            return;
        }

        try
        {
            SaveButton.IsEnabled = false;
            SaveButton.Opacity = 0.5;
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;

            var salary = new SalaryModel
            {
                Id = _currentSalary?.Id,
                Amount = amount,
                Frequency = FrequencyPicker.SelectedItem.ToString(),
                StartDate = DateTime.Now,
                Notes = NotesEntry.Text,
                CreatedAt = _currentSalary?.CreatedAt ?? DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var response = await _firebaseService.SaveSalaryAsync(_currentUserId, salary);

            if (response.Success)
            {
                _currentSalary = salary;
                UpdateSalaryDisplay();
                
                if (userDialogService != null)
                    await userDialogService.ShowAlertAsync("Success", "Salary saved successfully!", "OK");
            }
            else
            {
                if (userDialogService != null)
                    await userDialogService.ShowAlertAsync("Error", response.Message, "OK");
            }
        }
        catch (Exception ex)
        {
            if (userDialogService != null)
                await userDialogService.ShowAlertAsync("Error", $"Failed to save salary: {ex.Message}", "OK");
        }
        finally
        {
            SaveButton.IsEnabled = true;
            SaveButton.Opacity = 1;
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }
}
