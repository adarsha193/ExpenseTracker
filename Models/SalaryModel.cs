using System;

namespace ExpenseTracker.Models
{
    /// <summary>
    /// Salary Model - Income Tracking Data
    /// 
    /// FEATURES:
    /// - Store monthly/regular income amount
    /// - Track salary frequency (monthly, bi-weekly, weekly, daily)
    /// - Used for budget planning and savings calculations
    /// 
    /// USAGE:
    /// - Stored in Firebase: /salary/{userId}
    /// - Displayed in SalaryPage for income management
    /// - Used in dashboard for savings rate calculation
    /// - Referenced by AI service for budget recommendations
    /// </summary>
    public class SalaryModel
    {
        public string? Id { get; set; }
        public string? UserId { get; set; }
        public decimal Amount { get; set; }
        public string? Frequency { get; set; } // Monthly, Bi-weekly, Weekly, Daily
        public DateTime StartDate { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
