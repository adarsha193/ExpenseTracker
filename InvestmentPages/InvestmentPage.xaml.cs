using ExpenseTracker.Models;
using ExpenseTracker.Services;

namespace ExpenseTracker.InvestmentPages;

public partial class InvestmentPage : ContentPage
{
    private readonly FirebaseRealtimeDbService _firebaseService;
    private readonly IUserDialogService _dialogService;
    private string? _currentUserId;
    private List<InvestmentModel> _investments = new();

    public InvestmentPage()
    {
        InitializeComponent();
        _firebaseService = ServiceLocator.Provider?.GetService<FirebaseRealtimeDbService>()
            ?? throw new InvalidOperationException("FirebaseRealtimeDbService not registered");
        _dialogService = ServiceLocator.Provider?.GetService<IUserDialogService>()
            ?? throw new InvalidOperationException("IUserDialogService not registered");
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        _currentUserId = await SecureStorage.GetAsync("user_id");
        if (!string.IsNullOrEmpty(_currentUserId))
        {
            await LoadInvestments();
        }
    }

    private async Task LoadInvestments()
    {
        if (string.IsNullOrEmpty(_currentUserId))
            return;

        try
        {
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;
            EmptyStateIndicator.IsVisible = false;

            var response = await _firebaseService.GetInvestmentsAsync(_currentUserId);
            
            if (response.Success && response.Investments != null)
            {
                _investments = response.Investments.ToList();
                InvestmentsCollectionView.ItemsSource = _investments;

                if (_investments.Count > 0)
                {
                    EmptyStateIndicator.IsVisible = false;
                    decimal totalValue = _investments.Sum(i => i.Amount);
                    decimal totalExpectedReturn = _investments.Sum(i => (i.Amount * (i.ReturnRate ?? 0)) / 100);
                    
                    TotalInvestmentLabel.Text = $"₹{totalValue:N2}";
                    ExpectedReturnLabel.Text = $"₹{totalExpectedReturn:N2}";
                    InvestmentCountLabel.Text = $"{_investments.Count} investment{(_investments.Count != 1 ? "s" : "")}";
                }
                else
                {
                    EmptyStateIndicator.IsVisible = true;
                    TotalInvestmentLabel.Text = "₹0.00";
                    ExpectedReturnLabel.Text = "₹0.00";
                    InvestmentCountLabel.Text = "No investments";
                }
            }
            else
            {
                EmptyStateIndicator.IsVisible = true;
                TotalInvestmentLabel.Text = "₹0.00";
                ExpectedReturnLabel.Text = "₹0.00";
                InvestmentCountLabel.Text = "No investments";
            }
        }
        catch (Exception ex)
        {
            if (_dialogService != null)
                await _dialogService.ShowAlertAsync("Error", $"Failed to load investments: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private async void OnAddInvestmentClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_currentUserId))
        {
            await _dialogService.ShowAlertAsync("Error", "User not authenticated", "OK");
            return;
        }

        await Shell.Current.GoToAsync("///add-investment");
    }

    private async void OnInvestmentSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is InvestmentModel investment)
        {
            InvestmentsCollectionView.SelectedItem = null;
            
            var action = await _dialogService.ShowActionSheetAsync(
                "Investment Options",
                "Cancel",
                "Delete",
                new[] { "View Details", "Edit" });

            if (action == "View Details")
            {
                await ShowInvestmentDetails(investment);
            }
            else if (action == "Edit")
            {
                // Store the investment in a static property for passing between pages
                EditInvestmentCache.CurrentInvestment = investment;
                await Shell.Current.GoToAsync("///add-investment");
            }
            else if (action == "Delete")
            {
                await DeleteInvestment(investment);
            }
        }
    }

    private async Task ShowInvestmentDetails(InvestmentModel investment)
    {
        if (investment == null) return;

        var expectedReturn = (investment.Amount * (investment.ReturnRate ?? 0)) / 100;
        var futureValue = investment.Amount + expectedReturn;

        var message = $"Investment Type: {investment.InvestmentType}\n" +
                      $"Amount: ₹{investment.Amount:N2}\n" +
                      $"Return Rate: {investment.ReturnRate:F2}% per annum\n" +
                      $"Expected Annual Return: ₹{expectedReturn:N2}\n" +
                      $"Projected Value: ₹{futureValue:N2}\n" +
                      $"Date: {investment.InvestmentDate:d MMM yyyy}\n" +
                      (string.IsNullOrEmpty(investment.Description) ? "" : $"\nDescription: {investment.Description}");

        await _dialogService.ShowAlertAsync("Investment Details", message, "Close");
    }

    private async Task DeleteInvestment(InvestmentModel investment)
    {
        if (investment?.Id == null) return;

        var confirmed = await _dialogService.ShowConfirmationAsync(
            "Delete Investment",
            $"Are you sure you want to delete this {investment.InvestmentType} investment?");

        if (!confirmed) return;

        try
        {
            var response = await _firebaseService.DeleteInvestmentAsync(_currentUserId!, investment.Id);
            
            if (response.Success)
            {
                await _dialogService.ShowAlertAsync("Success", "Investment deleted successfully!", "OK");
                await LoadInvestments();
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", response.Message ?? "Failed to delete investment", "OK");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Failed to delete investment: {ex.Message}", "OK");
        }
    }
}
