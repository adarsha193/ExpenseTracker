using ExpenseTracker.Models;
using ExpenseTracker.Services;
using Microsoft.Maui.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ExpenseTracker.BudgetPages;

/// <summary>
/// Converter to cap progress bar at 100% (1.0) for display
/// </summary>
public class ProgressConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is double percentage)
        {
            return Math.Min(percentage, 1.0); // Cap at 100% for progress bar
        }
        return 0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converter to change progress bar color when budget is exceeded
/// </summary>
public class ExceededColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is bool isExceeded)
        {
            return isExceeded ? Color.FromArgb("#FF5252") : Color.FromArgb("#9C27B0");
        }
        return Color.FromArgb("#9C27B0");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converter to change percentage label color when budget is exceeded
/// </summary>
public class PercentageColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is bool isExceeded)
        {
            return isExceeded ? Color.FromArgb("#FF5252") : Color.FromArgb("#999999");
        }
        return Color.FromArgb("#999999");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BudgetItemViewModel
{
    public string? Id { get; set; }
    public string? Category { get; set; }
    public decimal AllocatedAmount { get; set; }
    public decimal CurrentSpending { get; set; }
    public double SpendingPercentage { get; set; }
    public bool IsExceeded { get; set; }
    public string? AlertMessage { get; set; }
}

public partial class BudgetPage : ContentPage, INotifyPropertyChanged
{
    private readonly FirebaseRealtimeDbService _firebaseService;
    private string? _currentUserId;
    private List<BudgetItemViewModel> _budgets = new();

    public List<BudgetItemViewModel> Budgets
    {
        get => _budgets;
        set 
        { 
            _budgets = value;
            OnPropertyChanged();
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public BudgetPage()
    {
        InitializeComponent();
        _firebaseService = ServiceLocator.Provider?.GetService<FirebaseRealtimeDbService>()
            ?? throw new InvalidOperationException("FirebaseRealtimeDbService not registered");

        MonthYearLabel.Text = DateTime.Now.ToString("MMMM yyyy");
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        _currentUserId = await SecureStorage.GetAsync("user_id");
        if (!string.IsNullOrEmpty(_currentUserId))
        {
            await LoadBudgets();
        }
    }

    private async Task LoadBudgets()
    {
        if (string.IsNullOrEmpty(_currentUserId))
        {
            System.Diagnostics.Debug.WriteLine("[BudgetPage] LoadBudgets - No user ID");
            return;
        }

        try
        {
            System.Diagnostics.Debug.WriteLine($"[BudgetPage] LoadBudgets started for user: {_currentUserId}");
            
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;
            EmptyStateIndicator.IsVisible = false;
            BudgetsCollectionView.IsVisible = false;

            var now = DateTime.Now;
            System.Diagnostics.Debug.WriteLine($"[BudgetPage] Fetching budgets for {now.Month}/{now.Year}");
            
            var budgetResponse = await _firebaseService.GetBudgetsAsync(_currentUserId, now.Month, now.Year);
            var expensesResponse = await _firebaseService.GetUserExpensesAsync(_currentUserId);
            
            System.Diagnostics.Debug.WriteLine($"[BudgetPage] BudgetResponse - Success: {budgetResponse.Success}, Count: {budgetResponse.Budgets?.Count}");
            System.Diagnostics.Debug.WriteLine($"[BudgetPage] ExpensesResponse - Success: {expensesResponse.Success}, Count: {expensesResponse.Expenses?.Count}");
            
            _budgets.Clear();

            if (budgetResponse.Success && budgetResponse.Budgets != null && budgetResponse.Budgets.Count > 0)
            {
                decimal totalBudget = 0;
                decimal totalSpent = 0;
                
                System.Diagnostics.Debug.WriteLine($"[BudgetPage] Processing {budgetResponse.Budgets.Count} budgets for {now.Month}/{now.Year}");

                // Calculate total spent from ALL expenses in current month (not just budgeted categories)
                if (expensesResponse.Success && expensesResponse.Expenses != null)
                {
                    var allMonthExpenses = expensesResponse.Expenses
                        .Where(e => e.Date.Month == now.Month && e.Date.Year == now.Year)
                        .ToList();
                    
                    totalSpent = allMonthExpenses.Sum(e => e.Amount);
                    System.Diagnostics.Debug.WriteLine($"[BudgetPage] Total expenses in {now.Month}/{now.Year}: {totalSpent} (from {allMonthExpenses.Count} total expenses)");
                    
                    foreach (var exp in allMonthExpenses)
                    {
                        System.Diagnostics.Debug.WriteLine($"  - {exp.Category}: ₹{exp.Amount} on {exp.Date:yyyy-MM-dd}");
                    }
                }

                foreach (var budget in budgetResponse.Budgets)
                {
                    // Calculate current spending for this category
                    decimal categorySpent = 0;
                    if (expensesResponse.Success && expensesResponse.Expenses != null)
                    {
                        categorySpent = expensesResponse.Expenses
                            .Where(e => e.Category == budget.Category && 
                                   e.Date.Month == now.Month && 
                                   e.Date.Year == now.Year)
                            .Sum(e => e.Amount);
                        
                        System.Diagnostics.Debug.WriteLine($"[BudgetPage] {budget.Category}: {categorySpent}");
                    }

                    double spendingPercentage = budget.AllocatedAmount > 0 
                        ? (double)(categorySpent / budget.AllocatedAmount)
                        : 0;

                    bool isExceeded = categorySpent > budget.AllocatedAmount;
                    string alertMessage = isExceeded 
                        ? $"⚠️ Exceeded by ₹{(categorySpent - budget.AllocatedAmount):N2}" 
                        : "";

                    _budgets.Add(new BudgetItemViewModel
                    {
                        Id = budget.Id,
                        Category = budget.Category,
                        AllocatedAmount = budget.AllocatedAmount,
                        CurrentSpending = categorySpent,
                        SpendingPercentage = Math.Min(spendingPercentage, 1.5), // Allow up to 150% for visual overflow
                        IsExceeded = isExceeded,
                        AlertMessage = alertMessage
                    });

                    totalBudget += budget.AllocatedAmount;
                }

                Budgets = _budgets;  // Use property to trigger binding update via INotifyPropertyChanged
                System.Diagnostics.Debug.WriteLine($"[BudgetPage] Total Spent (from all month expenses): {totalSpent}");
                System.Diagnostics.Debug.WriteLine($"[BudgetPage] Set Budgets property to list with {_budgets.Count} items");
                
                BudgetsCollectionView.IsVisible = true;  // Show the collection view
                TotalBudgetLabel.Text = $"₹{totalBudget:N2}";
                TotalSpentLabel.Text = $"₹{totalSpent:N2}";

                System.Diagnostics.Debug.WriteLine($"[BudgetPage] Loaded {_budgets.Count} budgets, showing CollectionView");

                // Show alert summary if any category exceeded
                var exceededCategories = _budgets.Where(b => b.IsExceeded).ToList();
                if (exceededCategories.Count > 0)
                {
                    AlertSummaryBox.IsVisible = true;
                    var alertMessages = exceededCategories
                        .Select(b => $"{b.Category}: ₹{(b.CurrentSpending - b.AllocatedAmount):N2} over")
                        .ToList();
                    AlertSummaryLabel.Text = $"{exceededCategories.Count} categor{(exceededCategories.Count > 1 ? "ies" : "y")} exceeded:\n• " + string.Join("\n• ", alertMessages);
                }
                else
                {
                    AlertSummaryBox.IsVisible = false;
                }

                EmptyStateIndicator.IsVisible = false;
            }
            else
            {
                AlertSummaryBox.IsVisible = false;
                EmptyStateIndicator.IsVisible = true;
                TotalBudgetLabel.Text = "₹0.00";
                TotalSpentLabel.Text = "₹0.00";
            }
        }
        catch (Exception ex)
        {
            var dialogService = ServiceLocator.Provider?.GetService<IUserDialogService>();
            if (dialogService != null)
                await dialogService.ShowAlertAsync("Error", $"Failed to load budgets: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private async void OnAddCategoryClicked(object? sender, EventArgs e)
    {
        var dialogService = ServiceLocator.Provider?.GetService<IUserDialogService>();
        
        if (string.IsNullOrEmpty(_currentUserId))
        {
            if (dialogService != null)
                await dialogService.ShowAlertAsync("Error", "User not authenticated", "OK");
            return;
        }

        // Show category selection and budget amount input
        var categories = new List<string>
        {
            "Food",
            "Travel",
            "Entertainment",
            "Shopping",
            "Bills",
            "Health",
            "Education",
            "Other"
        };

        var selectedCategory = await DisplayActionSheetAsync("Select Category", "Cancel", null, categories.ToArray());
        
        if (selectedCategory == "Cancel" || string.IsNullOrEmpty(selectedCategory))
            return;

        // Check if category already has budget
        var existingBudget = _budgets.FirstOrDefault(b => b.Category == selectedCategory);
        if (existingBudget != null)
        {
            if (dialogService != null)
                await dialogService.ShowAlertAsync("Info", $"Budget for {selectedCategory} already exists", "OK");
            return;
        }

        // Prompt for budget amount
        string? result = await DisplayPromptAsync(
            "Add Budget",
            $"Enter monthly budget for {selectedCategory} (₹)",
            "Add",
            "Cancel",
            "0",
            10,
            Keyboard.Numeric);

        if (string.IsNullOrEmpty(result) || !decimal.TryParse(result, out decimal amount) || amount <= 0)
        {
            if (dialogService != null)
                await dialogService.ShowAlertAsync("Validation", "Please enter a valid amount", "OK");
            return;
        }

        try
        {
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;

            var budget = new MonthlyBudgetModel
            {
                Category = selectedCategory,
                AllocatedAmount = amount,
                Month = DateTime.Now.Month,
                Year = DateTime.Now.Year,
                Notes = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var response = await _firebaseService.SaveBudgetAsync(_currentUserId, budget);

            if (response.Success)
            {
                if (dialogService != null)
                    await dialogService.ShowAlertAsync("Success", "Budget added successfully!", "OK");
                
                await LoadBudgets();
            }
            else
            {
                if (dialogService != null)
                    await dialogService.ShowAlertAsync("Error", response.Message, "OK");
            }
        }
        catch (Exception ex)
        {
            if (dialogService != null)
                await dialogService.ShowAlertAsync("Error", $"Failed to add budget: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }
}
