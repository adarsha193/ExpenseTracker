using System;

namespace ExpenseTracker.Models
{
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
