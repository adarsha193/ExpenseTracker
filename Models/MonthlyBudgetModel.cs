using System;

namespace ExpenseTracker.Models
{
    /// <summary>
    /// Monthly Budget Model - Budget Allocation Data
    /// 
    /// FEATURES:
    /// - Define budget limits for each spending category
    /// - Support multiple months and years
    /// - Categories: Food, Travel, Entertainment, Shopping, Bills, Health, Education, Other
    /// 
    /// USAGE:
    /// - Stored in Firebase: /budgets/{userId}/{categoryId}
    /// - Displayed in BudgetPage for budget management
    /// - Used by BudgetAlertService for overspending detection
    /// - Tracked in dashboard for budget progress visualization
    /// 
    /// ALERTS:
    /// - Warning at 90% of budget
    /// - Critical alert when budget exceeded
    /// - Used by NotificationService for user notifications
    /// </summary>
    public class MonthlyBudgetModel
    {
        public string? Id { get; set; }
        public string? UserId { get; set; }
        public string? Category { get; set; } // Food, Travel, Entertainment, Shopping, Bills, Health, Education, Other
        public decimal AllocatedAmount { get; set; }
        public int Month { get; set; } // 1-12
        public int Year { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
