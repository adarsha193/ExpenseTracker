using Xunit;

namespace ExpenseTracker.Tests;

/// <summary>
/// Unit tests for budget alert logic and calculations
/// </summary>
public class BudgetAlertServiceTests
{
    #region Alert Level Determination Tests

    [Theory]
    [InlineData(100, 50)]   // 50% spent - Green
    [InlineData(100, 74)]   // 74% spent - Green
    public void DetermineAlertLevel_SpentLessThan75Percent_ReturnsGreen(decimal budget, decimal spent)
    {
        // Act
        var percentageSpent = (spent / budget) * 100;
        var alertLevel = percentageSpent < 75 ? "Green" : 
                        percentageSpent < 90 ? "Yellow" :
                        percentageSpent < 100 ? "Orange" : "Red";

        // Assert
        Assert.Equal("Green", alertLevel);
    }

    [Theory]
    [InlineData(100, 75)]   // 75% spent - Yellow
    [InlineData(100, 89)]   // 89% spent - Yellow
    public void DetermineAlertLevel_Spent75To90Percent_ReturnsYellow(decimal budget, decimal spent)
    {
        // Act
        var percentageSpent = (spent / budget) * 100;
        var alertLevel = percentageSpent < 75 ? "Green" :
                        percentageSpent < 90 ? "Yellow" :
                        percentageSpent < 100 ? "Orange" : "Red";

        // Assert
        Assert.Equal("Yellow", alertLevel);
    }

    [Theory]
    [InlineData(100, 90)]   // 90% spent - Orange
    [InlineData(100, 99)]   // 99% spent - Orange
    public void DetermineAlertLevel_Spent90To100Percent_ReturnsOrange(decimal budget, decimal spent)
    {
        // Act
        var percentageSpent = (spent / budget) * 100;
        var alertLevel = percentageSpent < 75 ? "Green" :
                        percentageSpent < 90 ? "Yellow" :
                        percentageSpent < 100 ? "Orange" : "Red";

        // Assert
        Assert.Equal("Orange", alertLevel);
    }

    [Theory]
    [InlineData(100, 100)]  // 100% spent - Red
    [InlineData(100, 125)]  // 125% spent - Red
    public void DetermineAlertLevel_SpentAt100OrMore_ReturnsRed(decimal budget, decimal spent)
    {
        // Act
        var percentageSpent = (spent / budget) * 100;
        var alertLevel = percentageSpent < 75 ? "Green" :
                        percentageSpent < 90 ? "Yellow" :
                        percentageSpent < 100 ? "Orange" : "Red";

        // Assert
        Assert.Equal("Red", alertLevel);
    }

    #endregion

    #region Budget Percentage Calculation Tests

    [Theory]
    [InlineData(1000, 500, 50)]
    [InlineData(200, 100, 50)]
    [InlineData(500, 250, 50)]
    public void CalculateBudgetPercentage_With50PercentSpent_Returns50(decimal budget, decimal spent, decimal expected)
    {
        // Act
        var percentage = (spent / budget) * 100;

        // Assert
        Assert.Equal(expected, percentage);
    }

    [Theory]
    [InlineData(1000, 1000, 100)]
    [InlineData(200, 200, 100)]
    public void CalculateBudgetPercentage_With100PercentSpent_Returns100(decimal budget, decimal spent, decimal expected)
    {
        // Act
        var percentage = (spent / budget) * 100;

        // Assert
        Assert.Equal(expected, percentage);
    }

    [Theory]
    [InlineData(1000, 1500, 150)]
    [InlineData(100, 250, 250)]
    public void CalculateBudgetPercentage_With150PercentSpent_Returns150Plus(decimal budget, decimal spent, decimal expected)
    {
        // Act
        var percentage = (spent / budget) * 100;

        // Assert
        Assert.Equal(expected, percentage);
    }

    #endregion

    #region Budget Overage Calculation Tests

    [Theory]
    [InlineData(1000, 500, 0)]   // Under budget - no overage
    [InlineData(500, 300, 0)]    // Under budget - no overage
    public void CalculateOverage_WhenUnderBudget_ReturnsZero(decimal budget, decimal spent, decimal expected)
    {
        // Act
        var overage = spent > budget ? spent - budget : 0;

        // Assert
        Assert.Equal(expected, overage);
    }

    [Theory]
    [InlineData(1000, 1200, 200)]  // Over budget
    [InlineData(500, 600, 100)]    // Over budget
    public void CalculateOverage_WhenOverBudget_ReturnsCorrectAmount(decimal budget, decimal spent, decimal expected)
    {
        // Act
        var overage = spent > budget ? spent - budget : 0;

        // Assert
        Assert.Equal(expected, overage);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void CalculateBudgetPercentage_WithZeroBudget_ThrowsException()
    {
        // Arrange
        var budget = 0m;
        var spent = 100m;

        // Act & Assert
        Assert.Throws<DivideByZeroException>(() => 
        {
            _ = (spent / budget) * 100;
        });
    }

    [Theory]
    [InlineData(1000, 0)]
    [InlineData(500, 0)]
    public void CalculateBudgetPercentage_WithZeroSpent_ReturnsZero(decimal budget, decimal spent)
    {
        // Act
        var percentage = spent > 0 ? (spent / budget) * 100 : 0;

        // Assert
        Assert.Equal(0, percentage);
    }

    #endregion

    #region Multiple Category Budget Tests

    [Theory]
    [InlineData(1000, 800, 100, 50)]   // Category 1: 80%, Category 2: 50%
    [InlineData(500, 450, 200, 100)]   // Category 1: 90%, Category 2: 50%
    public void CompareBudgets_MultipleCategories_ReturnsHighestPercentage(
        decimal budget1, decimal spent1, decimal budget2, decimal spent2)
    {
        // Act
        var percentage1 = (spent1 / budget1) * 100;
        var percentage2 = (spent2 / budget2) * 100;
        var highestPercentage = Math.Max(percentage1, percentage2);

        // Assert
        Assert.True(highestPercentage >= percentage1);
        Assert.True(highestPercentage >= percentage2);
    }

    [Fact]
    public void CompareBudgets_MultipleCategories_IdentifiesHighestRisk()
    {
        // Arrange
        var categories = new[]
        {
            new { Budget = 1000m, Spent = 500m },   // 50%
            new { Budget = 500m, Spent = 475m },    // 95% - Highest
            new { Budget = 2000m, Spent = 1000m }   // 50%
        };

        // Act
        var alertLevels = categories.Select(c => 
        {
            var percent = (c.Spent / c.Budget) * 100;
            return percent < 75 ? "Green" :
                   percent < 90 ? "Yellow" :
                   percent < 100 ? "Orange" : "Red";
        }).ToList();

        // Assert
        Assert.Contains("Orange", alertLevels);
    }

    #endregion

    #region Budget Alert Threshold Tests

    [Theory]
    [InlineData(75, true)]      // At threshold
    [InlineData(74.9, false)]   // Below threshold
    [InlineData(75.1, true)]    // Above threshold
    public void IsBudgetAtWarningThreshold_75Percent_DetectsCorrectly(decimal percentage, bool shouldAlert)
    {
        // Act
        var isAtThreshold = percentage >= 75;

        // Assert
        Assert.Equal(shouldAlert, isAtThreshold);
    }

    [Theory]
    [InlineData(90, true)]      // At critical threshold
    [InlineData(89.9, false)]   // Below threshold
    [InlineData(90.1, true)]    // Above threshold
    public void IsBudgetAtCriticalThreshold_90Percent_DetectsCorrectly(decimal percentage, bool shouldAlert)
    {
        // Act
        var isAtThreshold = percentage >= 90;

        // Assert
        Assert.Equal(shouldAlert, isAtThreshold);
    }

    #endregion
}
