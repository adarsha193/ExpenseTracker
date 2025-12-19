using System.Globalization;
using ExpenseTracker.Models;

namespace ExpenseTracker.Converters;

/// <summary>
/// Converts an InvestmentModel to its expected annual return formatted as currency
/// </summary>
public class InvestmentReturnConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is InvestmentModel investment)
        {
            var expectedReturn = (investment.Amount * (investment.ReturnRate ?? 0)) / 100;
            return $"₹{expectedReturn:N2}";
        }

        return "₹0.00";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
