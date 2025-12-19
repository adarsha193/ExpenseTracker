using ExpenseTracker.Models;

namespace ExpenseTracker.Services
{
    /// <summary>
    /// Budget Alert Service - Budget Monitoring & Alert Feature
    /// 
    /// FEATURES:
    /// - Monitors spending against user-defined budget limits
    /// - Detects overspending in real-time
    /// - Provides alert levels: Warning (90%), Critical (100%+)
    /// - Tracks overage amounts and percentages
    /// - Generates alerts for single categories or all categories
    /// 
    /// ALERT LEVELS:
    /// - Green: < 75% of budget used
    /// - Yellow: 75-90% of budget used (warning)
    /// - Orange: 90-100% of budget used (caution)
    /// - Red: > 100% of budget used (exceeded)
    /// 
    /// USAGE:
    /// - Called by DashboardPage for real-time monitoring
    /// - Used by NotificationService to send alerts
    /// - Integrated with AISuggestionsService for recommendations
    /// </summary>
    public class BudgetAlertService
    {
        private readonly FirebaseRealtimeDbService _firebaseService;

        public BudgetAlertService(FirebaseRealtimeDbService firebaseService)
        {
            _firebaseService = firebaseService ?? throw new ArgumentNullException(nameof(firebaseService));
        }

        /// <summary>
        /// Check all budget alerts for a user and return list of exceeded budgets
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
        /// Check if a specific category has exceeded its budget
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
        /// Get alert level based on spending percentage (90%, 100%, 110%+)
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
        /// Log alert to Firebase for audit trail
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
    /// Budget alert details
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
    /// Alert severity level
    /// </summary>
    public enum AlertLevel
    {
        Low = 0,      // < 90%
        Medium = 1,   // 90-99%
        High = 2,     // 100-109%
        Critical = 3  // 110%+
    }
}
