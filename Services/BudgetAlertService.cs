using ExpenseTracker.Models;

namespace ExpenseTracker.Services
{
    /// <summary>
    /// Monitors user budgets and produces alerts when limits are approached or exceeded.
    ///
    /// The service checks spending against configured budgets, categorizes the
    /// alert level (warning, high, critical), and returns alert details that other
    /// parts of the app (for example, `NotificationService` or the dashboard)
    /// can consume.
    ///
    /// Typical uses:
    /// - Periodic checks from the dashboard or a scheduled task
    /// - On-demand checks for a specific category
    /// - Integrates with AI suggestions and notification delivery
    /// </summary>
    public class BudgetAlertService
    {
        private readonly FirebaseRealtimeDbService _firebaseService;

        public BudgetAlertService(FirebaseRealtimeDbService firebaseService)
        {
            _firebaseService = firebaseService ?? throw new ArgumentNullException(nameof(firebaseService));
        }

        /// <summary>
        /// Check every budget for the given user and return alerts for budgets that
        /// have been exceeded.
        /// </summary>
        public async Task<List<BudgetAlert>> CheckAllBudgetAlertsAsync(string userId)
        {
            var alerts = new List<BudgetAlert>();
            
            if (string.IsNullOrEmpty(userId))
                return alerts;

            try
            {
                var now = DateTime.Now;
                var budgetResponse = await _firebaseService.GetBudgetsAsync(userId, now.Month, now.Year);
                
                if (!budgetResponse.Success || budgetResponse.Budgets == null)
                    return alerts;

                foreach (var budget in budgetResponse.Budgets)
                {
                    var alert = await _firebaseService.CheckBudgetAlertAsync(userId, budget.Category);
                    
                    if (alert.HasExceeded)
                    {
                        alerts.Add(new BudgetAlert
                        {
                            Category = alert.Category,
                            BudgetAmount = alert.BudgetAmount,
                            CurrentSpending = alert.CurrentSpending,
                            OverageAmount = alert.Overage,
                            PercentageExceeded = (alert.Overage / alert.BudgetAmount) * 100,
                            AlertLevel = GetAlertLevel(alert.PercentageUsed),
                            Message = alert.Message,
                            CheckedAt = DateTime.Now
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking budget alerts: {ex.Message}");
            }

            return alerts;
        }

        /// <summary>
        /// Check a single category and return an alert if it has exceeded the budget.
        /// </summary>
        public async Task<BudgetAlert?> CheckCategoryAlertAsync(string userId, string category)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(category))
                return null;

            try
            {
                var alert = await _firebaseService.CheckBudgetAlertAsync(userId, category);
                
                if (alert.HasExceeded)
                {
                    return new BudgetAlert
                    {
                        Category = alert.Category,
                        BudgetAmount = alert.BudgetAmount,
                        CurrentSpending = alert.CurrentSpending,
                        OverageAmount = alert.Overage,
                        PercentageExceeded = (alert.Overage / alert.BudgetAmount) * 100,
                        AlertLevel = GetAlertLevel(alert.PercentageUsed),
                        Message = alert.Message,
                        CheckedAt = DateTime.Now
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking category alert: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Map a spending percentage to an `AlertLevel` value.
        /// </summary>
        private AlertLevel GetAlertLevel(decimal percentageUsed)
        {
            if (percentageUsed >= 110m)
                return AlertLevel.Critical; // 110%+ - Red alert
            else if (percentageUsed >= 100m)
                return AlertLevel.High; // 100% - Orange alert
            else if (percentageUsed >= 90m)
                return AlertLevel.Medium; // 90% - Yellow warning
            else
                return AlertLevel.Low; // Normal
        }

        /// <summary>
        /// Record an alert to the backend (placeholder for analytics/audit logging).
        /// </summary>
        public async Task LogAlertAsync(string userId, BudgetAlert alert)
        {
            try
            {
                // Could log to Firebase for analytics
                // This is a placeholder for future analytics logging
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error logging alert: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Details describing a single budget alert.
    /// </summary>
    public class BudgetAlert
    {
        public string? Category { get; set; }
        public decimal BudgetAmount { get; set; }
        public decimal CurrentSpending { get; set; }
        public decimal OverageAmount { get; set; }
        public decimal PercentageExceeded { get; set; }
        public AlertLevel AlertLevel { get; set; }
        public string? Message { get; set; }
        public DateTime CheckedAt { get; set; }
    }

    /// <summary>
    /// Severity level for budget alerts.
    /// </summary>
    public enum AlertLevel
    {
        Low = 0,      // < 90%
        Medium = 1,   // 90-99%
        High = 2,     // 100-109%
        Critical = 3  // 110%+
    }
}
