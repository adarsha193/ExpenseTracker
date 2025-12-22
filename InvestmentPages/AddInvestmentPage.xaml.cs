using ExpenseTracker.Models;
using ExpenseTracker.Services;

namespace ExpenseTracker.InvestmentPages;

public partial class AddInvestmentPage : ContentPage
{
    private readonly FirebaseRealtimeDbService _firebaseService;
    private readonly IUserDialogService _dialogService;
    private string? _currentUserId;
    private InvestmentModel? _editingInvestment;

    public AddInvestmentPage(InvestmentModel? investmentToEdit = null)
    {
        InitializeComponent();
        
        _firebaseService = ServiceLocator.Provider?.GetService<FirebaseRealtimeDbService>()
            ?? throw new InvalidOperationException("FirebaseRealtimeDbService not registered");
        _dialogService = ServiceLocator.Provider?.GetService<IUserDialogService>()
            ?? throw new InvalidOperationException("IUserDialogService not registered");

        _editingInvestment = investmentToEdit;
        
        if (_editingInvestment != null)
        {
            Title = "Edit Investment";
            SaveButton.Text = "Update Investment";
        }

        // Update summary when amount or return rate changes
        AmountEntry.TextChanged += (s, e) => UpdateSummary();
        ReturnRateEntry.TextChanged += (s, e) => UpdateSummary();
        
        // Add input validation for amount entry
        AmountEntry.TextChanged += (s, e) => ValidateDecimalInput(AmountEntry);
        
        // Add input validation for return rate entry
        ReturnRateEntry.TextChanged += (s, e) => ValidateDecimalInput(ReturnRateEntry);
    }

    private void ValidateDecimalInput(Entry entry)
    {
        if (string.IsNullOrEmpty(entry.Text))
            return;

        // Allow decimal format with optional negative (for validation purposes)
        if (!decimal.TryParse(entry.Text, out var value))
        {
            entry.Text = entry.Text.Substring(0, entry.Text.Length - 1);
        }
        else if (entry == ReturnRateEntry && value < 0)
        {
            // Don't allow negative return rates
            entry.Text = entry.Text.Substring(0, entry.Text.Length - 1);
            MainThread.BeginInvokeOnMainThread(async () =>
                await _dialogService.ShowAlertAsync("Validation", "Return rate cannot be negative", "OK"));
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        _currentUserId = await SecureStorage.GetAsync("user_id");
        
        // Set default date to today if not already set
        if (DatePicker.Date == default)
        {
            DatePicker.Date = DateTime.Today;
        }
        
        // Check if there's an investment in the cache (from Edit)
        if (_editingInvestment == null && EditInvestmentCache.CurrentInvestment != null)
        {
            _editingInvestment = EditInvestmentCache.CurrentInvestment;
            EditInvestmentCache.Clear();
            
            Title = "Edit Investment";
            SaveButton.Text = "Update Investment";
        }
        
        if (_editingInvestment != null)
        {
            LoadInvestmentData();
        }
    }

    private void LoadInvestmentData()
    {
        if (_editingInvestment == null) return;

        InvestmentFrequencyPicker.SelectedItem = _editingInvestment.InvestmentFrequency ?? "One-Time";
        InvestmentTypePicker.SelectedItem = _editingInvestment.InvestmentType;
        AmountEntry.Text = _editingInvestment.Amount.ToString("F2");
        ReturnRateEntry.Text = _editingInvestment.ReturnRate?.ToString("F2") ?? "";
        DatePicker.Date = _editingInvestment.InvestmentDate != default ? _editingInvestment.InvestmentDate : DateTime.Today;
        DescriptionEditor.Text = _editingInvestment.Description ?? "";
        
        UpdateSummary();
    }

    private void UpdateSummary()
    {
        if (decimal.TryParse(AmountEntry.Text, out var amount) &&
            decimal.TryParse(ReturnRateEntry.Text, out var returnRate))
        {
            var expectedReturn = (amount * returnRate) / 100;
            SummaryAmountLabel.Text = $"₹{amount:N2}";
            SummaryReturnLabel.Text = $"₹{expectedReturn:N2}";
        }
        else
        {
            SummaryAmountLabel.Text = "₹0.00";
            SummaryReturnLabel.Text = "₹0.00";
        }
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        if (!ValidateInput())
            return;

        try
        {
            SaveButton.IsEnabled = false;

            var investment = _editingInvestment ?? new InvestmentModel();
            
            // Get selected items safely
            var frequencyIndex = InvestmentFrequencyPicker.SelectedIndex;
            var typeIndex = InvestmentTypePicker.SelectedIndex;
            
            investment.InvestmentFrequency = frequencyIndex >= 0 
                ? InvestmentFrequencyPicker.Items[frequencyIndex] 
                : "One-Time";
            
            investment.InvestmentType = typeIndex >= 0 
                ? InvestmentTypePicker.Items[typeIndex] 
                : "Other";
            
            if (decimal.TryParse(AmountEntry.Text, out var amount))
                investment.Amount = amount;
            
            if (decimal.TryParse(ReturnRateEntry.Text, out var returnRate))
                investment.ReturnRate = returnRate;
            
            investment.InvestmentDate = DatePicker.Date ?? DateTime.Today;
            investment.Description = DescriptionEditor.Text;

            var response = await _firebaseService.SaveInvestmentAsync(_currentUserId!, investment);

            if (response.Success)
            {
                await _dialogService.ShowAlertAsync("Success", 
                    _editingInvestment == null ? "Investment added successfully!" : "Investment updated successfully!", 
                    "OK");
                EditInvestmentCache.Clear();
                await Shell.Current.GoToAsync("///");
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", response.Message ?? "Failed to save investment", "OK");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Error", $"Failed to save investment: {ex.Message}", "OK");
        }
        finally
        {
            SaveButton.IsEnabled = true;
        }
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///");
    }

    private bool ValidateInput()
    {
        // Validate investment frequency is selected
        if (InvestmentFrequencyPicker.SelectedIndex < 0)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
                await _dialogService.ShowAlertAsync("Validation", "Please select investment type (One-Time or Monthly SIP)", "OK"));
            return false;
        }

        // Validate investment type is selected
        if (InvestmentTypePicker.SelectedIndex < 0)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
                await _dialogService.ShowAlertAsync("Validation", "Please select an investment category", "OK"));
            return false;
        }

        // Validate amount
        if (string.IsNullOrWhiteSpace(AmountEntry.Text) || !decimal.TryParse(AmountEntry.Text, out var amount) || amount <= 0)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
                await _dialogService.ShowAlertAsync("Validation", "Please enter a valid investment amount", "OK"));
            return false;
        }

        return true;
    }
}
