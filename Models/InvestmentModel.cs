using System;

namespace ExpenseTracker.Models
{
    public class InvestmentModel
    {
        public string? Id { get; set; }
        public string? UserId { get; set; }
        public string? InvestmentType { get; set; } // Stocks, Bonds, Crypto, Real Estate, Mutual Funds, etc.
        public decimal Amount { get; set; }
        public decimal? ReturnRate { get; set; } // Annual return percentage
        public DateTime InvestmentDate { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
