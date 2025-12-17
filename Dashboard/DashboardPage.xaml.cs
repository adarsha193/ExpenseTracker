using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using ExpenseTracker.Resources;

namespace ExpenseTracker;

/// <summary>
/// Converter to change text color to gray when data is 0 (indicating empty)
/// </summary>
public class DataEmptyColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is decimal decimalValue && decimalValue == 0)
        {
            return Color.FromArgb("#9CA3AF"); // Gray for empty
        }
        if (value is double doubleValue && doubleValue == 0)
        {
            return Color.FromArgb("#9CA3AF"); // Gray for empty
        }
        return Color.FromArgb("#6366F1"); // Default color for non-empty
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converter to display "Not Set" when value is 0, otherwise format as currency
/// </summary>
public class ZeroToNotSetConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        try
        {
            if (value == null)
                return "Not Set";
            
            if (value is decimal decimalValue)
            {
                return decimalValue == 0 ? "Not Set" : $"₹{decimalValue:F0}";
            }
            if (value is double doubleValue)
            {
                return doubleValue == 0 ? "Not Set" : $"₹{(decimal)doubleValue:F0}";
            }
            if (value is int intValue)
            {
                return intValue == 0 ? "Not Set" : $"₹{intValue}";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ZeroToNotSetConverter Error: {ex.Message}");
        }
        return "Not Set";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converter to display "Set Up" when value is 0, otherwise format as percentage
/// </summary>
public class ZeroToSetUpConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        try
        {
            if (value == null)
                return "Set Up";
            
            if (value is double doubleValue)
            {
                return doubleValue == 0 || double.IsNaN(doubleValue) ? "Set Up" : $"{doubleValue:F1}%";
            }
            if (value is decimal decimalValue)
            {
                return decimalValue == 0 ? "Set Up" : $"{(double)decimalValue:F1}%";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ZeroToSetUpConverter Error: {ex.Message}");
        }
        return "Set Up";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public partial class DashboardPage : ContentPage
{
    private DashboardPageViewModel _viewModel = null!;

    public DashboardPage()
    {
        InitializeComponent();
        _viewModel = new DashboardPageViewModel();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Show loading indicator
        var loadingIndicator = FindByName("LoadingIndicator") as VerticalStackLayout;
        var emptyStateIndicator = FindByName("EmptyStateIndicator") as VerticalStackLayout;
        var setupGuideBox = FindByName("SetupGuideBox") as Border;
        var recentTransactionsEmptyState = FindByName("RecentTransactionsEmptyState") as VerticalStackLayout;
        var spendingByCategorySection = FindByName("SpendingByCategorySection") as VerticalStackLayout;
        var financialOverviewSection = FindByName("FinancialOverviewSection") as VerticalStackLayout;
        
        if (loadingIndicator != null) loadingIndicator.IsVisible = true;
        if (emptyStateIndicator != null) emptyStateIndicator.IsVisible = false;
        if (setupGuideBox != null) setupGuideBox.IsVisible = false;
        if (recentTransactionsEmptyState != null) recentTransactionsEmptyState.IsVisible = true;
        if (spendingByCategorySection != null) spendingByCategorySection.IsVisible = false;
        if (financialOverviewSection != null) financialOverviewSection.IsVisible = true;
        
        System.Diagnostics.Debug.WriteLine("[Dashboard] OnAppearing started - loading data");
        
        // Load expenses from Firebase when page appears
        await _viewModel.LoadExpensesAsync();
        
        System.Diagnostics.Debug.WriteLine($"[Dashboard] After LoadExpensesAsync:");
        System.Diagnostics.Debug.WriteLine($"  - MonthlySalary: {_viewModel.MonthlySalary}");
        System.Diagnostics.Debug.WriteLine($"  - TotalInvestments: {_viewModel.TotalInvestments}");
        System.Diagnostics.Debug.WriteLine($"  - TotalSpent: {_viewModel.TotalSpent}");
        System.Diagnostics.Debug.WriteLine($"  - BudgetProgressPercentage: {_viewModel.BudgetProgressPercentage}");
        System.Diagnostics.Debug.WriteLine($"  - SavingsRate: {_viewModel.SavingsRate}");
        System.Diagnostics.Debug.WriteLine($"  - RecentExpenses.Count: {_viewModel.RecentExpenses.Count}");
        System.Diagnostics.Debug.WriteLine($"  - CategoryBreakdown.Count: {_viewModel.CategoryBreakdown.Count}");
        
        // Hide loading indicator
        if (loadingIndicator != null) loadingIndicator.IsVisible = false;
        
        // Check if this is a fresh login (no salary, investments, or expenses)
        bool hasSalary = _viewModel.MonthlySalary > 0;
        bool hasInvestments = _viewModel.TotalInvestments > 0;
        bool hasExpenses = _viewModel.RecentExpenses.Count > 0;
        bool hasBudget = _viewModel.BudgetProgressPercentage > 0;
        
        System.Diagnostics.Debug.WriteLine($"[Dashboard] State - Salary: {hasSalary}, Investments: {hasInvestments}, Expenses: {hasExpenses}, Budget: {hasBudget}");
        
        // Show setup guide if user is new (no financial data entered yet)
        bool isNewUser = !hasSalary && !hasInvestments && !hasExpenses && !hasBudget;
        System.Diagnostics.Debug.WriteLine($"[Dashboard] IsNewUser: {isNewUser}");
        if (setupGuideBox != null)
        {
            setupGuideBox.IsVisible = isNewUser;
        }
        
        // Update empty state visibility for recent transactions
        if (recentTransactionsEmptyState != null)
        {
            recentTransactionsEmptyState.IsVisible = _viewModel.RecentExpenses.Count == 0;
        }
        
        // Show spending by category section only if there's data
        if (spendingByCategorySection != null)
        {
            spendingByCategorySection.IsVisible = _viewModel.CategoryBreakdown.Count > 0;
        }
        
        // Check if expenses are empty - only show main empty state if no expenses at all
        if (!hasExpenses)
        {
            if (emptyStateIndicator != null) emptyStateIndicator.IsVisible = true;
            System.Diagnostics.Debug.WriteLine($"[Dashboard] Showing empty state - no expenses");
            return;
        }
        
        if (emptyStateIndicator != null) emptyStateIndicator.IsVisible = false;
        
        // Draw bar chart after data is loaded
        await Task.Delay(500);
        DrawBarChart();
    }

    private void DrawBarChart()
    {
        var canvas = FindByName("BarChartCanvas") as GraphicsView;
        if (canvas == null || _viewModel.CategoryBreakdown.Count == 0)
            return;

        var drawable = new BarChartDrawable(_viewModel.CategoryBreakdown);
        canvas.Drawable = drawable;
    }

    private async void OnAddExpenseClicked(object sender, EventArgs e)
    {
        if (Shell.Current != null)
        {
            await Shell.Current.GoToAsync(Strings.RouteAddExpense);
        }
    }

    private async void OnViewAllExpensesClicked(object sender, EventArgs e)
    {
        if (Shell.Current != null)
        {
            await Shell.Current.GoToAsync(Strings.RouteViewAllExpenses);
        }
    }
}

/// <summary>
/// Custom drawable for rendering bar chart
/// </summary>
public class BarChartDrawable : IDrawable
{
    private readonly IList<CategoryItem> _categoryBreakdown;
    private readonly Dictionary<string, Color> _colorMap = new();

    public BarChartDrawable(IList<CategoryItem> categoryBreakdown)
    {
        _categoryBreakdown = categoryBreakdown;
        InitializeColorMap();
    }

    private void InitializeColorMap()
    {
        _colorMap["food"] = Color.FromArgb("#EAB308");
        _colorMap["travel"] = Color.FromArgb("#0EA5E9");
        _colorMap["entertainment"] = Color.FromArgb("#EC4899");
        _colorMap["shopping"] = Color.FromArgb("#A855F7");
        _colorMap["health"] = Color.FromArgb("#6366F1");
        _colorMap["healthcare"] = Color.FromArgb("#6366F1");
        _colorMap["utilities"] = Color.FromArgb("#14B8A6");
        _colorMap["education"] = Color.FromArgb("#F59E0B");
        _colorMap["bills"] = Color.FromArgb("#10B981");
        _colorMap["other"] = Color.FromArgb("#6B7280");
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (_categoryBreakdown == null || _categoryBreakdown.Count == 0)
            return;

        canvas.SaveState();

        // Adjust padding - more bottom space for labels
        float leftPadding = 35;
        float rightPadding = 20;
        float topPadding = 20;
        float bottomPadding = 60;
        
        float chartLeft = leftPadding;
        float chartRight = dirtyRect.Width - rightPadding;
        float chartTop = topPadding;
        float chartBottom = dirtyRect.Height - bottomPadding;
        float chartHeight = chartBottom - chartTop;
        float chartWidth = chartRight - chartLeft;

        // Draw Y-axis
        canvas.StrokeColor = Color.FromArgb("#D1D5DB");
        canvas.StrokeSize = 1;
        canvas.DrawLine(chartLeft, chartTop, chartLeft, chartBottom);

        // Draw X-axis
        canvas.DrawLine(chartLeft, chartBottom, chartRight, chartBottom);

        // Calculate bar dimensions with better spacing
        int numBars = _categoryBreakdown.Count(c => c.Percentage > 0);
        if (numBars == 0) numBars = _categoryBreakdown.Count;
        
        float barWidth = Math.Min((chartWidth / (numBars + 1)) * 0.7f, 35f);
        float barSpacing = chartWidth / (numBars + 1);
        float maxPercentage = (float)_categoryBreakdown.Max(c => c.Percentage);
        if (maxPercentage == 0) maxPercentage = 100;

        // Draw grid lines first (behind bars)
        canvas.StrokeColor = Color.FromArgb("#F3F4F6");
        canvas.StrokeSize = 1;
        for (int i = 1; i < 5; i++)
        {
            float gridY = chartBottom - (chartHeight * i / 5);
            canvas.DrawLine(chartLeft, gridY, chartRight, gridY);

            // Draw percentage labels on Y-axis
            canvas.FontColor = Color.FromArgb("#9CA3AF");
            canvas.FontSize = 9;
            string percentLabel = $"{(maxPercentage * i / 5):F0}%";
            canvas.DrawString(
                percentLabel,
                3,
                gridY - 5,
                30,
                10,
                HorizontalAlignment.Right,
                VerticalAlignment.Center
            );
        }

        // Draw bars
        int barIndex = 0;
        foreach (var category in _categoryBreakdown)
        {
            if (category.Percentage <= 0)
                continue;

            float barHeight = ((float)category.Percentage / maxPercentage) * chartHeight;
            float barX = chartLeft + (barIndex + 1) * barSpacing - barWidth / 2;
            float barY = chartBottom - barHeight;

            // Get color
            Color barColor = _colorMap.ContainsKey(category.Category.ToLower())
                ? _colorMap[category.Category.ToLower()]
                : Color.FromArgb("#6B7280");

            // Draw bar with rounded top
            canvas.FillColor = barColor;
            canvas.FillRoundedRectangle(
                new RectF(barX, barY, barWidth, barHeight),
                3
            );

            // Add white border to bar
            canvas.StrokeColor = Color.FromArgb("FFFFFF");
            canvas.StrokeSize = 1f;
            canvas.DrawRoundedRectangle(
                new RectF(barX, barY, barWidth, barHeight),
                3
            );

            // Draw percentage value on top of bar (smaller, cleaner)
            if (barHeight > 15)
            {
                canvas.FontColor = Color.FromArgb("#1F2937");
                canvas.FontSize = 10;
                canvas.DrawString(
                    $"{category.Percentage:F0}%",
                    barX + barWidth / 2 - 10,
                    barY - 16,
                    30,
                    14,
                    HorizontalAlignment.Center,
                    VerticalAlignment.Top
                );
            }

            // Draw category label below axis
            canvas.FontColor = Color.FromArgb("#6B7280");
            canvas.FontSize = 10;
            string categoryLabel = category.Category.Length > 4 
                ? category.Category.Substring(0, 4).ToUpper() 
                : category.Category.ToUpper();
            
            canvas.DrawString(
                categoryLabel,
                barX + barWidth / 2 - 12,
                chartBottom + 8,
                30,
                18,
                HorizontalAlignment.Center,
                VerticalAlignment.Top
            );

            barIndex++;
        }

        canvas.RestoreState();
    }
}
