using ExpenseTracker.Models;
using ExpenseTracker.Services;

namespace ExpenseTracker.InvestmentPages;

public partial class InvestmentPage : ContentPage
{
    private readonly FirebaseRealtimeDbService _firebaseService;
    private string? _currentUserId;
    private List<InvestmentModel> _investments = new();

    public InvestmentPage()
    {
        InitializeComponent();
        _firebaseService = ServiceLocator.Provider?.GetService<FirebaseRealtimeDbService>()
            ?? throw new InvalidOperationException("FirebaseRealtimeDbService not registered");
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
                    TotalInvestmentLabel.Text = $"₹{totalValue:N2}";
                    InvestmentCountLabel.Text = $"{_investments.Count} investment{(_investments.Count != 1 ? "s" : "")}";
                }
                else
                {
                    EmptyStateIndicator.IsVisible = true;
                    TotalInvestmentLabel.Text = "₹0.00";
                    InvestmentCountLabel.Text = "No investments";
                }
            }
            else
            {
                EmptyStateIndicator.IsVisible = true;
                TotalInvestmentLabel.Text = "₹0.00";
                InvestmentCountLabel.Text = "No investments";
            }
        }
        catch (Exception ex)
        {
            var dialogService = ServiceLocator.Provider?.GetService<IUserDialogService>();
            if (dialogService != null)
                await dialogService.ShowAlertAsync("Error", $"Failed to load investments: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private async void OnAddInvestmentClicked(object? sender, EventArgs e)
    {
        var dialogService = ServiceLocator.Provider?.GetService<IUserDialogService>();
        
        if (string.IsNullOrEmpty(_currentUserId))
        {
            if (dialogService != null)
                await dialogService.ShowAlertAsync("Error", "User not authenticated", "OK");
            return;
        }

        // Navigate to investment detail page or show a modal form
        // For now, we'll show an alert
        if (dialogService != null)
            await dialogService.ShowAlertAsync("Coming Soon", "Investment form will be available soon", "OK");
    }
}
