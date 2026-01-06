using Microsoft.Maui.Controls;
using ExpenseTracker.Services;
using ExpenseTracker.Resources;
using ExpenseTracker.Models;

namespace ExpenseTracker;

public partial class ViewAllExpensesPage : ContentPage
{
    private readonly FirebaseRealtimeDbService _authService;
    private List<ExpenseData> _allExpenses = new();
    private string _userId = "";

        public ViewAllExpensesPage()
    {
        InitializeComponent();
        _authService = ServiceLocator.GetRequiredService<FirebaseRealtimeDbService>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadExpensesAsync();
    }

    private async Task LoadExpensesAsync()
    {
        try
        {
            // Show loading indicator
            var loadingIndicator = FindByName("LoadingIndicatorExpenses") as VerticalStackLayout;
            var emptyState = FindByName("EmptyStateExpenses") as VerticalStackLayout;
            
            if (loadingIndicator != null) loadingIndicator.IsVisible = true;
            if (emptyState != null) emptyState.IsVisible = false;
            
            // Get user ID
            _userId = await SecureStorage.GetAsync("user_id") ?? "";
            if (string.IsNullOrEmpty(_userId))
            {
                if (loadingIndicator != null) loadingIndicator.IsVisible = false;
                if (emptyState != null) emptyState.IsVisible = true;
                return;
            }

            // Fetch expenses from Firebase
            var result = await _authService.GetUserExpensesAsync(_userId);

            if (result.Success && result.Expenses != null && result.Expenses.Count > 0)
            {
                _allExpenses = result.Expenses;
                DisplayExpenses(_allExpenses);
                
                if (loadingIndicator != null) loadingIndicator.IsVisible = false;
                if (emptyState != null) emptyState.IsVisible = false;
            }
            else
            {
                _allExpenses.Clear();
                ExpensesCollectionView.ItemsSource = null;
                TotalExpensesLabel.Text = "Total Expenses: ₹0";
                
                if (loadingIndicator != null) loadingIndicator.IsVisible = false;
                if (emptyState != null) emptyState.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(Strings.ErrorTitle, string.Format(Strings.FailedToLoadExpenses, ex.Message), Strings.OKButton);
            
            var loadingIndicator = FindByName("LoadingIndicatorExpenses") as VerticalStackLayout;
            if (loadingIndicator != null) loadingIndicator.IsVisible = false;
        }
    }

    private void DisplayExpenses(List<ExpenseData> expenses)
    {
        var expenseItems = expenses
            .OrderByDescending(e => e.Date)
            .Select(e => new ExpenseItem
            {
                Id = e.Id,
                Category = e.Category,
                Amount = e.Amount,
                Date = e.Date.ToString("MMM dd, yyyy 'at' HH:mm"),
                Icon = GetIconForCategory(e.Category),
                Description = e.Description,
                ShopName = e.ShopName,
                Address = e.Address,
                Location = e.Location
            })
            .ToList();

        ExpensesCollectionView.ItemsSource = expenseItems;
        
        // Update total expenses display
        decimal total = expenses.Sum(e => e.Amount);
        TotalExpensesLabel.Text = $"Total Expenses: ₹{total:F2}";
    }

    // Event handler for View button click
    private async void OnViewExpenseClicked(object sender, TappedEventArgs e)
    {
        try
        {
            var label = (Label)sender;
            var expense = (ExpenseItem)label.BindingContext;

            // Find the original expense data
            var expenseData = _allExpenses.FirstOrDefault(ex => ex.Id == expense.Id);
            if (expenseData != null)
            {
                // Navigate to detail page and pass the expense data
                var detailPage = new ExpenseDetailPage();
                detailPage.DisplayExpense(expenseData);
                await Navigation.PushAsync(detailPage);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(Strings.ErrorTitle, string.Format(Strings.FailedToViewExpense, ex.Message), Strings.OKButton);
        }
    }

    // Search functionality
    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        string searchText = e.NewTextValue?.ToLower() ?? "";
        PerformSearch(searchText);
    }

    private void OnSearchButtonPressed(object sender, EventArgs e)
    {
        string searchText = SearchBar.Text?.ToLower() ?? "";
        PerformSearch(searchText);
    }

    private void PerformSearch(string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            // Show all expenses if search is empty
            DisplayExpenses(_allExpenses);
            return;
        }

        // Search by category or amount
        var filteredExpenses = _allExpenses
            .Where(e => 
                e.Category.ToLower().Contains(searchText) ||
                e.Amount.ToString().Contains(searchText) ||
                (e.Description?.ToLower() ?? "").Contains(searchText)
            )
            .ToList();

        DisplayExpenses(filteredExpenses);
    }

    private string GetIconForCategory(string category)
    {
        return category.ToLower() switch
        {
            "food" => "🍔",
            "travel" => "✈️",
            "entertainment" => "🎬",
            "shopping" => "🛍️",
            "health" => "⚕️",
            "healthcare" => "⚕️",
            "education" => "📚",
            "utilities" => "💡",
            "bills" => "📋",
            _ => "📝"
        };
    }
}

/// <summary>
/// Value converter to check if a string has content
/// </summary>
public class StringLengthConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is string str)
        {
            return !string.IsNullOrWhiteSpace(str);
        }
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}