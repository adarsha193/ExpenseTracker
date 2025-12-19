namespace ExpenseTracker;

/// <summary>
/// App Shell - Navigation Structure
/// 
/// FEATURES:
/// - Declarative navigation hierarchy
/// - Tab-based navigation for main features
/// - Flyout menu for settings and profile
/// 
/// NAVIGATION STRUCTURE:
/// - Dashboard Tab: Main home page with expense summary
/// - Expenses Tab: Add and manage expenses
/// - Budget Tab: Set and monitor budgets
/// - More Tab: Salary, investments, settings, profile
/// 
/// PAGES REGISTERED:
/// - LoginPage: Authentication
/// - DashboardPage: Main dashboard
/// - AddExpensePage: New expense entry
/// - ViewAllExpensesPage: Expense list
/// - BudgetPage: Budget management
/// - SalaryPage: Income management
/// - InvestmentPage: Portfolio management
/// - ProfilePage: User information
/// - SettingsPage: App preferences
/// </summary>
public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
	}
}

