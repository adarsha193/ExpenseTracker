using System;

namespace ExpenseTracker.Models
{
    /// <summary>
    /// Investment Model - Investment Portfolio Data
    /// 
    /// FEATURES:
    /// - Track various investment types (stocks, bonds, crypto, real estate, mutual funds)
    /// - Store investment amount and expected return rate
    /// - Track investment dates and total portfolio value
    /// 
    /// USAGE:
    /// - Stored in Firebase: /investments/{userId}/{investmentId}
    /// - Displayed in InvestmentPage for portfolio overview
    /// - Included in dashboard financial summary
    /// </summary>
    public class InvestmentModel
    {
        public string? Id { get; set; }
        public string? UserId { get; set; }
        public string? InvestmentType { get; set; } // Stocks, Bonds, Crypto, Real Estate, Mutual Funds, etc.
        public decimal Amount { get; set; }
        public decimal? ReturnRate { get; set; } // Annual return percentage
        public DateTime InvestmentDate { get; set; }
        public string? Description { get; set; }
        public string? InvestmentFrequency { get; set; } = "One-Time"; // One-Time or Monthly SIP
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
