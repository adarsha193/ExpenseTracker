using ExpenseTracker.Models;

namespace ExpenseTracker.InvestmentPages
{
    /// <summary>
    /// Cache helper to pass investment data between pages.
    /// Used for Edit functionality since Shell navigation doesn't support complex object parameters.
    /// </summary>
    public static class EditInvestmentCache
    {
        public static InvestmentModel? CurrentInvestment { get; set; }

        public static void Clear()
        {
            CurrentInvestment = null;
        }
    }
}
