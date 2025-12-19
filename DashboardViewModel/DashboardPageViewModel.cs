using System.Collections.ObjectModel;
using ExpenseTracker.Models;
using ExpenseTracker.Services;

namespace ExpenseTracker;

/// <summary>
/// Represents a single expense displayed in the dashboard
/// </summary>
public class ExpenseItem
{
    public string? Category { get; set; }
    public decimal Amount { get; set; }
    public string? Icon { get; set; }
    public string? Date { get; set; }
    public string? Description { get; set; }
    public string? Id { get; set; }
    public string? ShopName { get; set; }
    public string? Address { get; set; }
    public string? Location { get; set; }
}

/// <summary>
/// Represents a spending category with its total amount and percentage breakdown
/// Used for displaying category-wise expense visualization in the dashboard
/// </summary>
public class CategoryItem : BindableObject
{
    private decimal _amount;
    private double _percentage;
    private string _category = "";
    private string _icon = "";
    private string _iconBackgroundColor = "#FEF08A";
    private string _progressColor = "#6366F1";

    public decimal Amount
    {
        get => _amount;
        set
        {
            _amount = value;
            OnPropertyChanged();
        }
    }

    public double Percentage
    {
        get => _percentage;
        set
        {
            _percentage = value;
            OnPropertyChanged();
        }
    }

    public string Category
    {
        get => _category;
        set
        {
            _category = value;
            OnPropertyChanged();
        }
    }

    public string Icon
    {
        get => _icon;
        set
        {
            _icon = value;
            OnPropertyChanged();
        }
    }

    public string IconBackgroundColor
    {
        get => _iconBackgroundColor;
        set
        {
            _iconBackgroundColor = value;
            OnPropertyChanged();
        }
    }

    public string ProgressColor
    {
        get => _progressColor;
        set
        {
            _progressColor = value;
            OnPropertyChanged();
        }
    }
}

/// <summary>
/// Converts expense percentage (0-100) to progress bar value (0.0-1.0)
/// Used for visual progress indicators in XAML binding
/// </summary>
public class ProgressConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is double percentage)
        {
            return percentage / 100.0;
        }
        return 0.0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts percentage value to XAML grid column widths (e.g., "37*,63*" for 37% and 63%)
/// Used for proportional width distribution in dashboard visuals
/// </summary>
public class PercentageWidthConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is double percentage)
        {
            // Returns grid column definitions like "37*,63*" for 37% and 63%
            return $"{percentage:F0}*,{(100 - percentage):F0}*";
        }
        return "50*,50*";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Dashboard ViewModel - Core Dashboard Feature
/// 
/// FEATURES:
/// - Displays real-time expense summary for current month
/// - Shows category-wise spending breakdown with visual progress bars
/// - Calculates and displays monthly salary, total investments, and savings rate
/// - Tracks budget progress percentage
/// - Shows 3 most recent expenses
/// - Identifies new users for onboarding
/// 
/// DATA LOADED FROM FIREBASE:
/// - User's expenses (filtered by current month)
/// - Monthly salary
/// - Investment portfolio
/// - Budget allocations
/// 
/// CALCULATIONS:
/// - Total spent: Sum of current month expenses
/// - Budget progress: (Total Spent / Total Budget) * 100
/// - Savings rate: ((Salary - Spent) / Salary) * 100
/// - Category breakdown: Grouped expenses by category with percentages
/// </summary>
public class DashboardPageViewModel : BindableObject
{
    private readonly FirebaseRealtimeDbService _authService;
    private decimal _totalSpent;
    private bool _isLoading;
    private string _userId = "";
    private decimal _monthlySalary;
    private decimal _totalInvestments;
    private double _budgetProgressPercentage;
    private double _savingsRate;

    public decimal TotalSpent
    {
        get => _totalSpent;
        set
        {
            _totalSpent = value;
            OnPropertyChanged();
        }
    }

    public decimal MonthlySalary
    {
        get => _monthlySalary;
        set
        {
            _monthlySalary = value;
            OnPropertyChanged();
        }
    }

    public decimal TotalInvestments
    {
        get => _totalInvestments;
        set
        {
            _totalInvestments = value;
            OnPropertyChanged();
        }
    }

    public double BudgetProgressPercentage
    {
        get => _budgetProgressPercentage;
        set
        {
            _budgetProgressPercentage = value;
            OnPropertyChanged();
        }
    }

    public double SavingsRate
    {
        get => _savingsRate;
        set
        {
            _savingsRate = value;
            OnPropertyChanged();
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<ExpenseItem> RecentExpenses { get; set; }
    public ObservableCollection<CategoryItem> CategoryBreakdown { get; set; }

    /// <summary>
    /// Check if this is a fresh login (no financial data entered)
    /// </summary>
    public bool IsNewUser => 
        MonthlySalary == 0 && 
        TotalInvestments == 0 && 
        RecentExpenses.Count == 0 && 
        BudgetProgressPercentage == 0;

        public DashboardPageViewModel()
    {
        RecentExpenses = new ObservableCollection<ExpenseItem>();
        CategoryBreakdown = new ObservableCollection<CategoryItem>();
        
        try
        {
            _authService = ServiceLocator.GetRequiredService<FirebaseRealtimeDbService>();
        }
        catch
        {
            // Fallback for design time
            _authService = null!;
        }
    }

    /// <summary>
    /// Load expenses from Firebase and populate the UI
    /// </summary>
    public async Task LoadExpensesAsync()
    {
        if (_authService == null)
            return;

        IsLoading = true;
        try
        {
            // Get user ID from secure storage
            _userId = await SecureStorage.GetAsync("user_id") ?? "";
            System.Diagnostics.Debug.WriteLine($"[Dashboard] User ID: {_userId}");
            if (string.IsNullOrEmpty(_userId))
                return;

            // Load salary data
            try
            {
                var salaryResult = await _authService.GetUserSalaryAsync(_userId);
                System.Diagnostics.Debug.WriteLine($"[Dashboard] Salary Result - Success: {salaryResult.Success}, Amount: {salaryResult.Salary?.Amount}");
                if (salaryResult.Success && salaryResult.Salary != null)
                {
                    MonthlySalary = salaryResult.Salary.Amount;
                    System.Diagnostics.Debug.WriteLine($"[Dashboard] MonthlySalary set to: {MonthlySalary}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading salary: {ex.Message}");
            }

            // Load investments data
            try
            {
                var investmentsResult = await _authService.GetUserInvestmentsAsync(_userId);
                System.Diagnostics.Debug.WriteLine($"[Dashboard] Investments Result - Success: {investmentsResult.Success}, Count: {investmentsResult.Investments?.Count}");
                if (investmentsResult.Success && investmentsResult.Investments != null)
                {
                    TotalInvestments = investmentsResult.Investments.Sum(i => i.Amount);
                    System.Diagnostics.Debug.WriteLine($"[Dashboard] TotalInvestments set to: {TotalInvestments}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading investments: {ex.Message}");
            }

            // Fetch expenses from Firebase (must be before budget calculation)
            var result = await _authService.GetUserExpensesAsync(_userId);

            if (result.Success && result.Expenses != null)
            {
                System.Diagnostics.Debug.WriteLine($"[Dashboard] Total expenses from Firebase: {result.Expenses.Count}");
                
                // Filter expenses by current month only (to match Monthly Budget page)
                var now = DateTime.Now;
                var currentMonthExpenses = result.Expenses
                    .Where(e => e.Date.Month == now.Month && e.Date.Year == now.Year)
                    .ToList();
                
                System.Diagnostics.Debug.WriteLine($"[Dashboard] Current month ({now.Month}/{now.Year}) expenses: {currentMonthExpenses.Count}");
                foreach (var exp in currentMonthExpenses)
                {
                    System.Diagnostics.Debug.WriteLine($"  - {exp.Category}: ₹{exp.Amount} on {exp.Date:yyyy-MM-dd}");
                }
                
                // Update TotalSpent with current month expenses only
                TotalSpent = currentMonthExpenses.Sum(e => e.Amount);
                System.Diagnostics.Debug.WriteLine($"[Dashboard] TotalSpent (current month): {TotalSpent}");
                
                // Clear existing items
                RecentExpenses.Clear();
                CategoryBreakdown.Clear();

                // Add expenses to collection (limit to 3 most recent for dashboard)
                var recentExpenses = currentMonthExpenses
                    .OrderByDescending(e => e.Date)
                    .Take(3)
                    .ToList();

                foreach (var expense in recentExpenses)
                {
                    RecentExpenses.Add(new ExpenseItem
                    {
                        Id = expense.Id,
                        Category = expense.Category,
                        Amount = expense.Amount,
                        Description = expense.Description,
                        Date = expense.Date.ToString("MMM dd"),
                        Icon = GetIconForCategory(expense.Category),
                        ShopName = expense.ShopName,
                        Address = expense.Address,
                        Location = expense.Location
                    });
                }

                CalculateCategoryBreakdown(currentMonthExpenses);
            }

            // Load budgets to calculate progress (after expenses are loaded)
            try
            {
                var budgetsResult = await _authService.GetUserBudgetsAsync(_userId);
                System.Diagnostics.Debug.WriteLine($"[Dashboard] Budgets Result - Success: {budgetsResult.Success}, Count: {budgetsResult.Budgets?.Count}");
                if (budgetsResult.Success && budgetsResult.Budgets != null && budgetsResult.Budgets.Count > 0)
                {
                    var totalBudget = budgetsResult.Budgets.Sum(b => b.AllocatedAmount);
                    BudgetProgressPercentage = totalBudget > 0 
                        ? (double)(TotalSpent / totalBudget) * 100 
                        : 0;
                    System.Diagnostics.Debug.WriteLine($"[Dashboard] BudgetProgressPercentage set to: {BudgetProgressPercentage}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading budgets: {ex.Message}");
            }

            // Calculate savings rate
            if (MonthlySalary > 0)
            {
                decimal savings = MonthlySalary - TotalSpent;
                SavingsRate = (double)(savings / MonthlySalary) * 100;
                System.Diagnostics.Debug.WriteLine($"[Dashboard] SavingsRate set to: {SavingsRate}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading expenses: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void CalculateTotal(List<ExpenseData> expenses)
    {
        TotalSpent = expenses.Sum(x => x.Amount);
    }

    private void CalculateCategoryBreakdown(List<ExpenseData> expenses)
    {
        var categoryGroups = expenses
            .GroupBy(e => e.Category)
            .OrderByDescending(g => g.Sum(e => e.Amount))
            .ToList();

        decimal totalAmount = expenses.Sum(e => e.Amount);

        foreach (var group in categoryGroups)
        {
            decimal categoryTotal = group.Sum(e => e.Amount);
            double percentage = totalAmount > 0 ? (double)(categoryTotal / totalAmount) * 100 : 0;

            var categoryItem = new CategoryItem
            {
                Category = group.Key,
                Amount = categoryTotal,
                Percentage = percentage,
                Icon = GetIconForCategory(group.Key),
                IconBackgroundColor = GetColorForCategory(group.Key),
                ProgressColor = GetProgressColorForCategory(group.Key)
            };

            CategoryBreakdown.Add(categoryItem);
        }
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

    private string GetColorForCategory(string category)
    {
        return category.ToLower() switch
        {
            "food" => "#FEF08A",           // Yellow
            "travel" => "#DBEAFE",          // Light Blue
            "entertainment" => "#FCE7F3",   // Light Pink
            "shopping" => "#DDD6FE",        // Light Purple
            "health" => "#E0E7FF",          // Light Indigo
            "utilities" => "#CCFBF1",       // Light Teal
            _ => "#F3F4F6"                  // Gray
        };
    }

    private string GetProgressColorForCategory(string category)
    {
        return category.ToLower() switch
        {
            "food" => "#EAB308",            // Yellow
            "travel" => "#0EA5E9",          // Sky Blue
            "entertainment" => "#EC4899",   // Pink
            "shopping" => "#A855F7",        // Purple
            "health" => "#6366F1",          // Indigo
            "utilities" => "#14B8A6",       // Teal
            _ => "#6B7280"                  // Gray
        };
    }
}
