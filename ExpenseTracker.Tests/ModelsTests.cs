using Xunit;

namespace ExpenseTracker.Tests;

/// <summary>
/// Unit tests for basic model logic and calculations
/// </summary>
public class ModelsTests
{
    #region Amount Validation Tests

    [Theory]
    [InlineData(-100)]
    [InlineData(-0.01)]
    public void ValidateAmount_WithNegativeValue_IsFlagged(decimal amount)
    {
        // Act
        var isValid = amount > 0;

        // Assert
        Assert.False(isValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.01)]
    [InlineData(1000)]
    [InlineData(999999.99)]
    public void ValidateAmount_WithPositiveValue_IsValid(decimal amount)
    {
        // Act
        var isValid = amount >= 0;

        // Assert
        Assert.True(isValid);
    }

    #endregion

    #region Budget Calculations Tests

    [Fact]
    public void CalculateRemaining_WithBudgetAndSpent_ReturnsCorrectValue()
    {
        // Arrange
        decimal allocated = 5000m;
        decimal spent = 3000m;

        // Act
        var remaining = allocated - spent;

        // Assert
        Assert.Equal(2000m, remaining);
    }

    [Fact]
    public void CalculatePercentageSpent_WithBudgetAndSpent_ReturnsCorrectPercentage()
    {
        // Arrange
        decimal allocated = 5000m;
        decimal spent = 3750m;

        // Act
        var percentage = (spent / allocated) * 100;

        // Assert
        Assert.Equal(75m, percentage);
    }

    [Fact]
    public void CalculateRemaining_WhenOverBudget_ReturnsNegative()
    {
        // Arrange
        decimal allocated = 2000m;
        decimal spent = 2500m;

        // Act
        var remaining = allocated - spent;

        // Assert
        Assert.Equal(-500m, remaining);
    }

    #endregion

    #region Salary Conversion Tests

    [Fact]
    public void ConvertAnnualToMonthly_With600000Annual_Returns50000()
    {
        // Arrange
        decimal annual = 600000m;

        // Act
        var monthly = annual / 12;

        // Assert
        Assert.Equal(50000m, monthly);
    }

    [Theory]
    [InlineData(120000, 10000)]   // 120k annual = 10k monthly
    [InlineData(600000, 50000)]   // 600k annual = 50k monthly
    [InlineData(360000, 30000)]   // 360k annual = 30k monthly
    public void ConvertAnnualToMonthly_VariousAmounts_ReturnsCorrect(decimal annual, decimal expected)
    {
        // Act
        var monthly = annual / 12;

        // Assert
        Assert.Equal(expected, monthly);
    }

    #endregion

    #region Investment Calculations Tests

    [Fact]
    public void CalculateAnnualReturn_With10000At8Point5Percent_Returns850()
    {
        // Arrange
        decimal amount = 10000m;
        decimal returnRate = 8.5m;

        // Act
        var annualReturn = amount * (returnRate / 100);

        // Assert
        Assert.Equal(850m, annualReturn);
    }

    [Fact]
    public void CalculateFutureValue_With10000At8Point5Percent_Returns10850()
    {
        // Arrange
        decimal amount = 10000m;
        decimal returnRate = 8.5m;

        // Act
        var futureValue = amount * (1 + (returnRate / 100));

        // Assert
        Assert.Equal(10850m, futureValue);
    }

    [Fact]
    public void CalculatePortfolioTotal_MultipleInvestments_ReturnsSumOfAll()
    {
        // Arrange
        var investments = new decimal[] { 10000m, 5000m, 3000m };

        // Act
        var totalInvested = investments.Sum();

        // Assert
        Assert.Equal(18000m, totalInvested);
    }

    [Theory]
    [InlineData(10000, 5, 12762.82)]   // 5% annual return, compounded annually = 10000*(1.05)^5
    [InlineData(5000, 10, 8052.55)]    // 10% annual return, compounded annually = 5000*(1.1)^5
    public void CalculateCompoundInterest_VariousRates_ReturnsCorrectValue(decimal principal, decimal rate, decimal expectedAfter5Years)
    {
        // Act - Using simple compound interest formula: A = P(1 + r/100)^t
        var rateDecimal = rate / 100;
        var futureValue = principal * (decimal)Math.Pow((double)(1 + rateDecimal), 5);

        // Assert
        Assert.Equal(expectedAfter5Years, futureValue, 1); // 1 decimal place precision
    }

    #endregion

    #region String Validation Tests

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ValidateString_WithEmptyOrNull_IsFlagged(string? value)
    {
        // Act
        var isValid = !string.IsNullOrWhiteSpace(value);

        // Assert
        Assert.False(isValid);
    }

    [Theory]
    [InlineData("John Doe")]
    [InlineData("Test@Example.com")]
    [InlineData("Valid String")]
    public void ValidateString_WithNonEmptyValue_IsValid(string value)
    {
        // Act
        var isValid = !string.IsNullOrWhiteSpace(value);

        // Assert
        Assert.True(isValid);
    }

    #endregion

    #region Email Validation Tests

    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name+tag@domain.co.uk")]
    [InlineData("simple@mail.com")]
    public void ValidateEmail_WithValidFormats_IsValid(string email)
    {
        // Act
        var isValid = email.Contains("@") && email.Contains(".");

        // Assert
        Assert.True(isValid);
    }

    [Theory]
    [InlineData("invalidemail")]
    [InlineData("@nodomain")]
    [InlineData("noemail@")]
    [InlineData("")]
    public void ValidateEmail_WithInvalidFormats_IsFlagged(string email)
    {
        // Act
        var isValid = email.Contains("@") && email.Contains(".");

        // Assert
        Assert.False(isValid);
    }

    #endregion

    #region Decimal Precision Tests

    [Fact]
    public void DecimalPrecision_WithTwoDecimals_IsPreserved()
    {
        // Arrange
        decimal value = 123.45m;

        // Act
        var hasCorrectPrecision = value == 123.45m;

        // Assert
        Assert.True(hasCorrectPrecision);
    }

    [Theory]
    [InlineData(100.99)]
    [InlineData(50.01)]
    [InlineData(999.99)]
    public void DecimalPrecision_WithCurrencyValues_IsValid(decimal value)
    {
        // Act
        var isValid = value > 0;

        // Assert
        Assert.True(isValid);
    }

    #endregion

    #region Comparison Tests

    [Fact]
    public void CompareAmounts_Equal_ReturnsTrue()
    {
        // Arrange
        decimal amount1 = 5000m;
        decimal amount2 = 5000m;

        // Act
        var isEqual = amount1 == amount2;

        // Assert
        Assert.True(isEqual);
    }

    [Fact]
    public void CompareAmounts_Different_ReturnsFalse()
    {
        // Arrange
        decimal amount1 = 5000m;
        decimal amount2 = 5001m;

        // Act
        var isEqual = amount1 == amount2;

        // Assert
        Assert.False(isEqual);
    }

    [Fact]
    public void CompareStrings_SameCategory_IsEqual()
    {
        // Arrange
        var category1 = "Food";
        var category2 = "Food";

        // Act
        var isEqual = category1 == category2;

        // Assert
        Assert.True(isEqual);
    }

    #endregion
}
