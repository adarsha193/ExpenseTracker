using ExpenseTracker.Models;

namespace ExpenseTracker.Services
{
    /// <summary>
    /// AI Suggestions Service - Smart Financial Recommendations Feature
    /// 
    /// FEATURES:
    /// - Generates personalized budget recommendations based on spending patterns
    /// - Analyzes last 3 months of expense data to identify trends
    /// - Calculates savings rate and provides improvement suggestions
    /// - Identifies overspending categories and suggests budget adjustments
    /// - Detects underspent categories for reallocation opportunities
    /// 
    /// SUGGESTION TYPES:
    /// - IncreaseCategory: User overspends regularly, increase budget
    /// - DecreaseCategory: User underspends, reduce budget and reallocate
    /// - IncreaseSavings: Low savings rate, increase savings target
    /// 
    /// PRIORITY LEVELS:
    /// - High: Critical overspending or very low savings rate
    /// - Medium: Regular overspending but manageable
    /// - Low: Optimization suggestions
    /// 
    /// DATA ANALYZED:
    /// - Monthly spending patterns per category
    /// - Average monthly spending
    /// - Savings rate (savings/income %)
    /// - Current budget allocations
    /// </summary>
    public class AISuggestionsService
    {
        private readonly FirebaseRealtimeDbService _firebaseService;

        public AISuggestionsService(FirebaseRealtimeDbService firebaseService)
        {
            _firebaseService = firebaseService ?? throw new ArgumentNullException(nameof(firebaseService));
        }

        /// <summary>
        /// Generate personalized budget suggestions based on spending patterns
        /// </summary>
        public async Task<List<BudgetSuggestion>> GenerateBudgetSuggestionsAsync(string userId)
        {
            var suggestions = new List<BudgetSuggestion>();

            try
            {
                // Get last 3 months of expenses
                var expenses = await GetRecentExpensesAsync(userId);
                var salary = await GetUserSalaryAsync(userId);
                var budgets = await GetCurrentBudgetsAsync(userId);

                if (expenses.Count == 0 || salary == null)
                    return suggestions;

                // Analyze spending by category
                var categoryAnalysis = AnalyzeSpendingByCategory(expenses);
                var savingsRate = CalculateSavingsRate(salary.Amount, expenses);

                // Generate suggestions
                foreach (var category in categoryAnalysis.Keys)
                {
                    var analysis = categoryAnalysis[category];
                    var currentBudget = budgets.FirstOrDefault(b => b.Category == category);

                    // Suggestion 1: If overspending frequently
                    if (analysis.AverageMonthlySpending > (currentBudget?.AllocatedAmount ?? 0))
                    {
                        var recommendedAmount = analysis.AverageMonthlySpending * 1.1m; // 10% buffer
                        suggestions.Add(new BudgetSuggestion
                        {
                            Type = SuggestionType.IncreaseCategory,
                            Category = category,
                            CurrentAmount = currentBudget?.AllocatedAmount ?? 0,
                            SuggestedAmount = recommendedAmount,
                            Reason = $"You've been spending ₹{analysis.AverageMonthlySpending:N2}/month on {category}. Consider increasing budget to ₹{recommendedAmount:N2} to accommodate your needs.",
                            Priority = analysis.AverageMonthlySpending > (currentBudget?.AllocatedAmount ?? 0) * 1.2m ? Priority.High : Priority.Medium
                        });
                    }

                    // Suggestion 2: If underspending
                    if (analysis.AverageMonthlySpending < (currentBudget?.AllocatedAmount ?? 0) * 0.5m && analysis.MonthsTracked >= 2)
                    {
                        var recommendedAmount = analysis.AverageMonthlySpending * 1.2m; // 20% buffer for occasional needs
                        suggestions.Add(new BudgetSuggestion
                        {
                            Type = SuggestionType.DecreaseCategory,
                            Category = category,
                            CurrentAmount = currentBudget?.AllocatedAmount ?? 0,
                            SuggestedAmount = recommendedAmount,
                            Reason = $"Your actual {category} spending (₹{analysis.AverageMonthlySpending:N2}/month) is much lower than your budget. You could reduce to ₹{recommendedAmount:N2}.",
                            Priority = Priority.Low
                        });
                    }
                }

                // Suggestion 3: Savings recommendation
                if (savingsRate < 0.1m) // Less than 10% savings
                {
                    suggestions.Add(new BudgetSuggestion
                    {
                        Type = SuggestionType.IncreasesSavings,
                        Reason = $"Your savings rate is only {savingsRate:P1}. Consider aiming for at least 20% of your income (₹{salary.Amount * 0.2m:N2}) in savings.",
                        Priority = Priority.High
                    });
                }

                return suggestions.OrderByDescending(s => (int)s.Priority).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error generating suggestions: {ex.Message}");
                return suggestions;
            }
        }

        /// <summary>
        /// Analyze spending trends and provide insights
        /// </summary>
        public async Task<SpendingInsight> AnalyzeSpendingTrendsAsync(string userId)
        {
            var insight = new SpendingInsight();

            try
            {
                var expenses = await GetRecentExpensesAsync(userId);
                if (expenses.Count == 0)
                    return insight;

                var total = expenses.Sum(e => e.Amount);
                var average = expenses.Average(e => e.Amount);
                var maxCategory = expenses.GroupBy(e => e.Category)
                    .OrderByDescending(g => g.Sum(e => e.Amount))
                    .FirstOrDefault();

                insight.TotalSpent = total;
                insight.AverageDailySpending = (double)average;
                insight.HighestSpendingCategory = maxCategory?.Key ?? "Unknown";
                insight.HighestSpendingAmount = maxCategory?.Sum(e => e.Amount) ?? 0;
                insight.Insight = $"Your spending is trending high in {insight.HighestSpendingCategory}. Consider reviewing this category for potential savings.";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error analyzing trends: {ex.Message}");
            }

            return insight;
        }

        private async Task<List<ExpenseData>> GetRecentExpensesAsync(string userId)
        {
            var response = await _firebaseService.GetUserExpensesAsync(userId);
            if (response.Success && response.Expenses != null)
            {
                var threeMonthsAgo = DateTime.Now.AddMonths(-3);
                return response.Expenses
                    .Where(e => e.Date >= threeMonthsAgo)
                    .ToList();
            }
            return new List<ExpenseData>();
        }

        private async Task<SalaryModel?> GetUserSalaryAsync(string userId)
        {
            var response = await _firebaseService.GetSalaryAsync(userId);
            return response.Success ? response.Salary : null;
        }

        private async Task<List<MonthlyBudgetModel>> GetCurrentBudgetsAsync(string userId)
        {
            var now = DateTime.Now;
            var response = await _firebaseService.GetBudgetsAsync(userId, now.Month, now.Year);
            return response.Success ? response.Budgets : new List<MonthlyBudgetModel>();
        }

        private Dictionary<string, CategoryAnalysis> AnalyzeSpendingByCategory(List<ExpenseData> expenses)
        {
            var analysis = new Dictionary<string, CategoryAnalysis>();

            foreach (var expense in expenses)
            {
                if (!analysis.ContainsKey(expense.Category))
                {
                    analysis[expense.Category] = new CategoryAnalysis();
                }

                analysis[expense.Category].TotalSpent += expense.Amount;
                analysis[expense.Category].MonthsTracked = (DateTime.Now - expenses.Min(e => e.Date)).Days / 30 + 1;
            }

            foreach (var category in analysis.Keys)
            {
                analysis[category].AverageMonthlySpending = analysis[category].TotalSpent / analysis[category].MonthsTracked;
            }

            return analysis;
        }

        private decimal CalculateSavingsRate(decimal monthlySalary, List<ExpenseData> expenses)
        {
            if (monthlySalary <= 0)
                return 0;

            var monthlyExpenses = expenses.Count > 0 ? expenses.Average(e => e.Amount) : 0;
            var savings = monthlySalary - (decimal)monthlyExpenses;
            return savings / monthlySalary;
        }

        private class CategoryAnalysis
        {
            public decimal TotalSpent { get; set; }
            public decimal AverageMonthlySpending { get; set; }
            public int MonthsTracked { get; set; }
        }
    }

    /// <summary>
    /// Budget suggestion model
    /// </summary>
    public class BudgetSuggestion
    {
        public SuggestionType Type { get; set; }
        public string? Category { get; set; }
        public decimal CurrentAmount { get; set; }
        public decimal SuggestedAmount { get; set; }
        public string? Reason { get; set; }
        public Priority Priority { get; set; }
    }

    /// <summary>
    /// Spending insights
    /// </summary>
    public class SpendingInsight
    {
        public decimal TotalSpent { get; set; }
        public double AverageDailySpending { get; set; }
        public string? HighestSpendingCategory { get; set; }
        public decimal HighestSpendingAmount { get; set; }
        public string? Insight { get; set; }
    }

    public enum SuggestionType
    {
        IncreaseCategory,
        DecreaseCategory,
        IncreasesSavings,
        AdjustFrequency
    }

    public enum Priority
    {
        Low = 0,
        Medium = 1,
        High = 2
    }
}
