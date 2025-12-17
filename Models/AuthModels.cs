namespace ExpenseTracker.Models;

/// <summary>
/// Request model for user login
/// </summary>
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Request model for user registration
/// </summary>
public class RegisterRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Request model for forgot password
/// </summary>
public class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Request model for password reset
/// </summary>
public class ResetPasswordRequest
{
    public string Email { get; set; } = string.Empty;
    public string ResetToken { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

/// <summary>
/// Response model for authentication
/// </summary>
public class AuthResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public UserData? User { get; set; }
    public string? Token { get; set; }
}

/// <summary>
/// User data response
/// </summary>
public class UserData
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Location { get; set; }
    public decimal TotalExpenses { get; set; }
}

/// <summary>
/// Expense data model
/// </summary>
public class ExpenseData
{
    public string? Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string? Icon { get; set; }
    public string? ShopName { get; set; }
    public string? Address { get; set; }
    public string? Location { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

/// <summary>
/// Generic API response wrapper
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
}

/// <summary>
/// Response for expense operations
/// </summary>
public class ExpenseResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public ExpenseData? Expense { get; set; }
    public List<ExpenseData>? Expenses { get; set; }
}

/// <summary>
/// Generic basic response
/// </summary>
public class BasicResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Response for salary operations
/// </summary>
public class SalaryResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public SalaryModel? Salary { get; set; }
}

/// <summary>
/// Response for investment operations
/// </summary>
public class InvestmentResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<InvestmentModel> Investments { get; set; } = new();
}

/// <summary>
/// Response for budget operations
/// </summary>
public class BudgetResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<MonthlyBudgetModel> Budgets { get; set; } = new();
}

/// <summary>
/// Response for budget alert check
/// </summary>
public class BudgetAlertResponse
{
    public bool HasExceeded { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal BudgetAmount { get; set; }
    public decimal CurrentSpending { get; set; }
    public decimal Overage { get; set; }
    public decimal PercentageUsed { get; set; }
    public string Message { get; set; } = string.Empty;
}
