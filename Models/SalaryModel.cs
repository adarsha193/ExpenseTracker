using System;

namespace ExpenseTracker.Models
{
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
