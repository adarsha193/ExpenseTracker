using Microsoft.Maui.Controls;
using ExpenseTracker.Models;
using ExpenseTracker.Resources;

namespace ExpenseTracker;

public partial class ExpenseDetailPage : ContentPage
{
    private ExpenseData? _expenseData;

    public ExpenseDetailPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadExpenseDetails();
    }

    private void LoadExpenseDetails()
    {
        // Get the expense data passed via shell navigation
        if (Shell.Current?.CurrentState.Location.OriginalString.Contains("expensedetail") ?? false)
        {
            // Data will be passed through query parameters
            var queryId = Shell.Current.CurrentPage.GetHashCode().ToString();
            // This is handled through the route parameter
        }
    }

    public void DisplayExpense(ExpenseData expense)
    {
        _expenseData = expense;

        // Set category and icon
        CategoryLabel.Text = expense.Category;
        CategoryIcon.Text = GetIconForCategory(expense.Category);
        CategoryIconFrame.BackgroundColor = GetColorForCategory(expense.Category);

        // Set amount
        AmountLabel.Text = $"₹{expense.Amount:F2}";

        // Set date
        DateLabel.Text = expense.Date.ToString("MMM dd, yyyy 'at' HH:mm");

        // Set description
        if (!string.IsNullOrEmpty(expense.Description))
        {
            DescriptionLabel.Text = expense.Description;
        }

        // Set shop details with visibility
        if (!string.IsNullOrEmpty(expense.ShopName))
        {
            ShopNameLabel.Text = expense.ShopName;
            ShopNameFrame.IsVisible = true;
        }

        if (!string.IsNullOrEmpty(expense.Location))
        {
            LocationLabel.Text = expense.Location;
            LocationFrame.IsVisible = true;
        }

        if (!string.IsNullOrEmpty(expense.Address))
        {
            AddressLabel.Text = expense.Address;
            AddressFrame.IsVisible = true;
        }
    }

    private void OnBackClicked(object sender, EventArgs e)
    {
        Shell.Current?.GoToAsync(Strings.RouteBack);
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

    private Color GetColorForCategory(string category)
    {
        return category.ToLower() switch
        {
            "food" => Color.FromArgb("#FEF08A"),
            "travel" => Color.FromArgb("#BAE6FD"),
            "entertainment" => Color.FromArgb("#FBCFE8"),
            "shopping" => Color.FromArgb("#E9D5FF"),
            "health" => Color.FromArgb("#E0E7FF"),
            "healthcare" => Color.FromArgb("#E0E7FF"),
            "education" => Color.FromArgb("#FED7AA"),
            "utilities" => Color.FromArgb("#CCFBF1"),
            "bills" => Color.FromArgb("#D1FAE5"),
            _ => Color.FromArgb("#F3F4F6")
        };
    }
}
