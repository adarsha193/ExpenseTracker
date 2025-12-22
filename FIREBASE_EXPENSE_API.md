# Firebase Realtime Database Complete API Reference

## Overview

The `FirebaseRealtimeDbService` is the core backend service for ExpenseTracker, providing comprehensive integration with Firebase Realtime Database. It handles **authentication, expense management, budget tracking, salary management, and investment portfolio** functionality.

**Key Features:**
- ✅ Multi-platform support (iOS, Android, macOS, Windows)
- ✅ Real-time data synchronization
- ✅ Secure authentication with Firebase Auth
- ✅ Comprehensive error handling
- ✅ CRUD operations for all entities
- ✅ Budget alert generation
- ✅ Investment tracking

This API uses **async/await**, **generics**, **LINQ**, **try-catch-finally**, and **dependency injection** patterns (see [CSHARP_CONCEPTS_EXPLAINED.md](CSHARP_CONCEPTS_EXPLAINED.md) for detailed explanations).

## Complete Database Structure

```
/users/{userId}
  ├── id: string
  ├── fullName: string
  ├── email: string
  ├── createdAt: timestamp
  ├── profileImageUrl: string (optional)
  ├── phoneNumber: string (optional)
  ├── location: string (optional)
  └── totalExpenses: number (decimal)

/expenses/{userId}/{expenseId}
  ├── id: string (auto-generated GUID)
  ├── userId: string
  ├── category: string
  ├── description: string (optional)
  ├── amount: number (decimal)
  ├── date: timestamp
  ├── icon: string (emoji, optional)
  ├── shopName: string (optional)
  ├── address: string (optional)
  ├── location: string (optional)
  ├── createdAt: timestamp
  └── modifiedAt: timestamp (optional)

/salary/{userId}/{salaryId}
  ├── id: string
  ├── userId: string
  ├── amount: number (decimal)
  ├── frequency: string (Monthly, Bi-weekly, Weekly, Daily)
  ├── startDate: timestamp
  ├── notes: string (optional)
  ├── createdAt: timestamp
  └── updatedAt: timestamp (optional)

/investments/{userId}/{investmentId}
  ├── id: string
  ├── userId: string
  ├── investmentType: string (Stocks, Bonds, Crypto, etc.)
  ├── amount: number (decimal)
  ├── returnRate: number (percentage, optional)
  ├── investmentDate: timestamp
  ├── description: string (optional)
  ├── investmentFrequency: string (One-Time or Monthly SIP)
  ├── createdAt: timestamp
  └── updatedAt: timestamp (optional)

/budgets/{userId}/{budgetId}
  ├── id: string
  ├── userId: string
  ├── category: string
  ├── allocatedAmount: number (decimal)
  ├── month: number (1-12)
  ├── year: number
  ├── notes: string (optional)
  ├── createdAt: timestamp
  └── updatedAt: timestamp (optional)
```

## Complete API Reference

### Table of Contents
1. [Authentication APIs](#authentication-apis)
2. [User Profile APIs](#user-profile-apis)
3. [Expense APIs](#expense-apis)
4. [Salary APIs](#salary-apis)
5. [Investment APIs](#investment-apis)
6. [Budget APIs](#budget-apis)
7. [Response Models](#response-models)

---

## Authentication APIs

### 1. Login User

**Method:** `LoginAsync(email, password)`

**C# Concepts Used**: Async/Await, Try-Catch, Task<T>, JSON Serialization

```csharp
var result = await _firebaseService.LoginAsync("user@example.com", "password123");

if (result.Success)
{
    // Save token to secure storage
    await SecureStorage.SetAsync("auth_token", result.Token);
    await SecureStorage.SetAsync("user_id", result.User.Id);
    
    Console.WriteLine($"Welcome, {result.User.FullName}!");
}
else
{
    Console.WriteLine($"Login failed: {result.Message}");
}
```

**Parameters:**
- `email` (string, required): User's email address
- `password` (string, required): User's password

**Returns:** `Task<AuthResponse>`
- `Success`: Boolean indicating login success
- `Message`: Status or error message
- `User`: UserData object with user information
- `Token`: Firebase Auth token for authenticated requests

**HTTP Method**: POST to Firebase Identity Toolkit  
**Endpoint**: `/v1/accounts:signInWithPassword`

---

### 2. Register New User

**Method:** `RegisterAsync(fullName, email, password)`

**C# Concepts Used**: Async/Await, Task<T>, Data Validation, String Interpolation

```csharp
var result = await _firebaseService.RegisterAsync(
    fullName: "John Doe",
    email: "john@example.com",
    password: "securePassword123"
);

if (result.Success)
{
    Console.WriteLine($"Account created: {result.User.Email}");
    // Store credentials securely
    await SecureStorage.SetAsync("auth_token", result.Token);
}
else
{
    // Handle registration errors
    Console.WriteLine($"Registration failed: {result.Message}");
}
```

**Parameters:**
- `fullName` (string, required): User's full name
- `email` (string, required): User's email (must be unique)
- `password` (string, required): Password (minimum 6 characters)

**Returns:** `Task<AuthResponse>`

**Validation:**
- Email must be valid format
- Password must be at least 6 characters
- Email must not already exist

**HTTP Method**: POST to Firebase Identity Toolkit  
**Endpoint**: `/v1/accounts:signUp`

---

### 3. Forgot Password

**Method:** `ForgotPasswordAsync(email)`

**C# Concepts Used**: Async/Await, Task<T>, Email Validation

```csharp
var result = await _firebaseService.ForgotPasswordAsync("user@example.com");

if (result.Success)
{
    await DisplayAlert("Success", "Password reset email sent", "OK");
}
else
{
    await DisplayAlert("Error", result.Message, "OK");
}
```

**Parameters:**
- `email` (string, required): Registered email address

**Returns:** `Task<AuthResponse>`

**Note:** Sends password reset link to user's email

**HTTP Method**: POST to Firebase Identity Toolkit  
**Endpoint**: `/v1/accounts:sendPasswordResetEmail`

---

### 4. Change Password

**Method:** `ChangePasswordAsync(idToken, newPassword)`

**C# Concepts Used**: Async/Await, Task<T>, Token-based Auth

```csharp
var token = await SecureStorage.GetAsync("auth_token");
var result = await _firebaseService.ChangePasswordAsync(token, "newPassword123");

if (result.Success)
{
    await DisplayAlert("Success", "Password changed successfully", "OK");
}
```

**Parameters:**
- `idToken` (string, required): User's Firebase ID token
- `newPassword` (string, required): New password (minimum 6 characters)

**Returns:** `Task<AuthResponse>`

**HTTP Method**: POST to Firebase Identity Toolkit  
**Endpoint**: `/v1/accounts:update`

---

### 5. Reset Password

**Method:** `ResetPasswordAsync(resetCode, newPassword)`

**C# Concepts Used**: Async/Await, Task<T>, Code Validation

```csharp
// Called from password reset link in email
var result = await _firebaseService.ResetPasswordAsync(resetCode, "newPassword123");

if (result.Success)
{
    // Navigate to login
    await Shell.Current.GoToAsync("login");
}
```

**Parameters:**
- `resetCode` (string, required): Code from password reset email
- `newPassword` (string, required): New password

**Returns:** `Task<AuthResponse>`

---

### 6. Logout

**Method:** `LogoutAsync()`

**C# Concepts Used**: Async/Await, Task, Cleanup

```csharp
var result = await _firebaseService.LogoutAsync();

// Clear local storage
await SecureStorage.Remove("auth_token");
await SecureStorage.Remove("user_id");

// Navigate to login
await Shell.Current.GoToAsync("login");
```

**Returns:** `Task<AuthResponse>`

**Note:** Clears local authentication state

---

## User Profile APIs

### 7. Get User Data

**Method:** `GetUserDataFromDb(userId)`

**C# Concepts Used**: Async/Await, Task<T>, JSON Deserialization, Null-Coalescing

```csharp
var userData = await _firebaseService.GetUserDataFromDb("userId123");

if (userData != null)
{
    Console.WriteLine($"User: {userData.FullName}");
    Console.WriteLine($"Email: {userData.Email}");
    Console.WriteLine($"Total Expenses: ₹{userData.TotalExpenses}");
}
```

**Parameters:**
- `userId` (string, required): User's unique ID

**Returns:** `Task<UserData?>` - User data or null if not found

**HTTP Method**: GET from Firebase RTDB  
**Endpoint**: `/users/{userId}.json`

---

### 8. Update User Profile

**Method:** `UpdateUserProfileAsync(userId, fullName, email, phone, location, totalExpenses, profileImageUrl, idToken)`

**C# Concepts Used**: Async/Await, Task<T>, Parameter Objects

```csharp
var result = await _firebaseService.UpdateUserProfileAsync(
    userId: "userId123",
    fullName: "John Doe Updated",
    email: "newemail@example.com",
    phone: "+91-9876543210",
    location: "Mumbai, India",
    totalExpenses: 5000m,
    profileImageUrl: "https://example.com/profile.jpg",
    idToken: token
);

if (result.Success)
{
    Console.WriteLine("Profile updated successfully");
}
```

**Parameters:**
- `userId` (string, required): User's ID
- `fullName` (string, required): Updated full name
- `email` (string, optional): Updated email
- `phone` (string, optional): Phone number
- `location` (string, optional): User's location
- `totalExpenses` (decimal, required): Total expenses amount
- `profileImageUrl` (string, optional): Profile picture URL
- `idToken` (string, required): Authentication token

**Returns:** `Task<AuthResponse>`

**HTTP Method**: PUT to Firebase RTDB  
**Endpoint**: `/users/{userId}.json`

---

### 9. Get All Users

**Method:** `GetAllUsersAsync()`

**C# Concepts Used**: Async/Await, Task<T>, Collections, LINQ

```csharp
var users = await _firebaseService.GetAllUsersAsync();

if (users != null)
{
    foreach (var user in users)
    {
        Console.WriteLine($"{user.FullName} - {user.Email}");
    }
}
```

**Returns:** `Task<List<UserData>?>` - List of all users or null on error

**Note:** May require admin privileges

**HTTP Method**: GET from Firebase RTDB  
**Endpoint**: `/users.json`

---

## Expense APIs

### 10. Add Expense

**Method:** `AddExpenseAsync(userId, category, amount, description, icon, shopName, address, location)`

**C# Concepts Used**: Async/Await, Task<T>, Guid Generation, Default Parameters

```csharp
var result = await _firebaseService.AddExpenseAsync(
    userId: "user123",
    category: "Food",
    amount: 450.50m,
    description: "Lunch at cafe",
    icon: "🍔",
    shopName: "Cafe Coffee Day",
    address: "Mumbai, MH",
    location: "19.0760, 72.8777"
);

if (result.Success)
{
    Console.WriteLine($"Expense ID: {result.Expense?.Id}");
}
```

**Parameters:**
- `userId` (string, required): User's ID
- `category` (string, required): Category name
- `amount` (decimal, required): Expense amount
- `description` (string, optional): Description
- `icon` (string, optional): Emoji icon
- `shopName` (string, optional): Shop/vendor name
- `address` (string, optional): Physical address
- `location` (string, optional): GPS coordinates

**Returns:** `Task<ExpenseResponse>`

**Auto-Generated Fields:**
- `id`: GUID
- `createdAt`: Current UTC time
- `date`: Current UTC time

**HTTP Method**: PUT to Firebase RTDB  
**Endpoint**: `/expenses/{userId}/{expenseId}.json`

---

### 11. Get All User Expenses

**Method:** `GetUserExpensesAsync(userId)`

**C# Concepts Used**: Async/Await, Task<T>, Collections, LINQ OrderByDescending

```csharp
var result = await _firebaseService.GetUserExpensesAsync("user123");

if (result.Success && result.Expenses != null)
{
    var foodExpenses = result.Expenses
        .Where(e => e.Category == "Food")
        .ToList();
    
    decimal total = result.Expenses.Sum(e => e.Amount);
    Console.WriteLine($"Total: ₹{total:F2}");
}
```

**Parameters:**
- `userId` (string, required): User's ID

**Returns:** `Task<ExpenseResponse>`
- `Expenses`: List sorted by date (newest first)
- Returns empty list if no expenses found

**HTTP Method**: GET from Firebase RTDB  
**Endpoint**: `/expenses/{userId}.json`

---

### 12. Get Single Expense

**Method:** `GetExpenseAsync(userId, expenseId)`

**C# Concepts Used**: Async/Await, Task<T>, Null Checking

```csharp
var result = await _firebaseService.GetExpenseAsync("user123", "expenseId");

if (result.Success && result.Expense != null)
{
    Console.WriteLine($"Amount: ₹{result.Expense.Amount}");
    Console.WriteLine($"Date: {result.Expense.Date}");
}
```

**Parameters:**
- `userId` (string, required): User's ID
- `expenseId` (string, required): Expense ID

**Returns:** `Task<ExpenseResponse>` - Single expense or null

**HTTP Method**: GET from Firebase RTDB  
**Endpoint**: `/expenses/{userId}/{expenseId}.json`

---

### 13. Update Expense

**Method:** `UpdateExpenseAsync(userId, expenseId, expenseData)`

**C# Concepts Used**: Async/Await, Task<T>, Property Assignment

```csharp
var expense = new ExpenseData
{
    Id = "expenseId",
    UserId = "user123",
    Category = "Travel",
    Amount = 2500m,
    Description = "Train ticket",
    Icon = "🚂",
    Date = DateTime.UtcNow,
    CreatedAt = DateTime.UtcNow
};

var result = await _firebaseService.UpdateExpenseAsync("user123", "expenseId", expense);

if (result.Success)
{
    Console.WriteLine("Expense updated");
}
```

**Parameters:**
- `userId` (string, required): User's ID
- `expenseId` (string, required): Expense ID to update
- `expenseData` (ExpenseData, required): Updated data

**Returns:** `Task<ExpenseResponse>`

**Auto-Updated Fields:**
- `ModifiedAt`: Current UTC time

**HTTP Method**: PUT to Firebase RTDB  
**Endpoint**: `/expenses/{userId}/{expenseId}.json`

---

### 14. Delete Expense

**Method:** `DeleteExpenseAsync(userId, expenseId)`

**C# Concepts Used**: Async/Await, Task<T>, Resource Cleanup

```csharp
var result = await _firebaseService.DeleteExpenseAsync("user123", "expenseId");

if (result.Success)
{
    Console.WriteLine("Expense deleted");
}
```

**Parameters:**
- `userId` (string, required): User's ID
- `expenseId` (string, required): Expense ID to delete

**Returns:** `Task<ExpenseResponse>`

**Side Effects:**
- Automatically updates user's total expenses

**HTTP Method**: DELETE to Firebase RTDB  
**Endpoint**: `/expenses/{userId}/{expenseId}.json`

---

### 15. Get Expenses Summary

**Method:** `GetExpensesSummaryAsync(userId)`

**C# Concepts Used**: Async/Await, Task<T>, LINQ Sum, LINQ GroupBy

```csharp
var result = await _firebaseService.GetExpensesSummaryAsync("user123");

if (result.Success && result.Expenses != null)
{
    // Calculate by category
    var categorySummary = result.Expenses
        .GroupBy(e => e.Category)
        .Select(g => new 
        { 
            Category = g.Key, 
            Total = g.Sum(e => e.Amount)
        })
        .OrderByDescending(x => x.Total);
    
    foreach (var cat in categorySummary)
    {
        Console.WriteLine($"{cat.Category}: ₹{cat.Total:F2}");
    }
}
```

**Parameters:**
- `userId` (string, required): User's ID

**Returns:** `Task<ExpenseResponse>` with all expenses for summary calculations

**HTTP Method**: GET from Firebase RTDB  
**Endpoint**: `/expenses/{userId}.json` (same as GetUserExpensesAsync)

---

## Salary APIs

### 16. Save Salary

**Method:** `SaveSalaryAsync(userId, salary)`

**C# Concepts Used**: Async/Await, Task<T>, Object Initialization, DateTime

```csharp
var salary = new SalaryModel
{
    Amount = 50000m,
    Frequency = "Monthly",
    StartDate = DateTime.Now,
    Notes = "Monthly salary from employer"
};

var result = await _firebaseService.SaveSalaryAsync("user123", salary);

if (result.Success)
{
    Console.WriteLine("Salary saved");
}
```

**Parameters:**
- `userId` (string, required): User's ID
- `salary` (SalaryModel, required): Salary details

**Returns:** `Task<BasicResponse>`

**Auto-Generated Fields:**
- `Id`: GUID if not provided
- `CreatedAt`: Current UTC time if not set
- `UpdatedAt`: Current UTC time

**Salary Frequencies:**
- Monthly
- Bi-weekly
- Weekly
- Daily

**HTTP Method**: PUT to Firebase RTDB  
**Endpoint**: `/users/{userId}/salary/{salaryId}.json`

---

### 17. Get User Salary

**Method:** `GetSalaryAsync(userId)`

**C# Concepts Used**: Async/Await, Task<T>, Dictionary Deserialization, LINQ OrderByDescending

```csharp
var result = await _firebaseService.GetSalaryAsync("user123");

if (result.Success && result.Salary != null)
{
    Console.WriteLine($"Monthly Salary: ₹{result.Salary.Amount}");
    Console.WriteLine($"Frequency: {result.Salary.Frequency}");
}
```

**Parameters:**
- `userId` (string, required): User's ID

**Returns:** `Task<SalaryResponse>`
- Returns most recent salary if multiple exist

**HTTP Method**: GET from Firebase RTDB  
**Endpoint**: `/users/{userId}/salary.json`

---

## Investment APIs

### 18. Save Investment

**Method:** `SaveInvestmentAsync(userId, investment)`

**C# Concepts Used**: Async/Await, Task<T>, Object Initialization

```csharp
var investment = new InvestmentModel
{
    InvestmentType = "Stocks",
    Amount = 100000m,
    ReturnRate = 12.5m,  // Annual return percentage
    InvestmentDate = DateTime.Now,
    Description = "Blue chip stocks portfolio",
    InvestmentFrequency = "One-Time"
};

var result = await _firebaseService.SaveInvestmentAsync("user123", investment);

if (result.Success)
{
    Console.WriteLine("Investment saved");
}
```

**Parameters:**
- `userId` (string, required): User's ID
- `investment` (InvestmentModel, required): Investment details

**Returns:** `Task<BasicResponse>`

**Investment Types:**
- Stocks
- Bonds
- Crypto
- Real Estate
- Mutual Funds
- Fixed Deposits

**Investment Frequency:**
- One-Time: Single investment
- Monthly SIP: Systematic Investment Plan

**HTTP Method**: PUT to Firebase RTDB  
**Endpoint**: `/users/{userId}/investments/{investmentId}.json`

---

### 19. Get All Investments

**Method:** `GetInvestmentsAsync(userId)`

**C# Concepts Used**: Async/Await, Task<T>, Collections, Dictionary Deserialization

```csharp
var result = await _firebaseService.GetInvestmentsAsync("user123");

if (result.Success)
{
    foreach (var investment in result.Investments)
    {
        Console.WriteLine($"{investment.InvestmentType}: ₹{investment.Amount}");
        
        if (investment.ReturnRate.HasValue)
        {
            decimal expectedReturn = investment.Amount * (investment.ReturnRate.Value / 100);
            Console.WriteLine($"Expected Return: ₹{expectedReturn:F2}");
        }
    }
}
```

**Parameters:**
- `userId` (string, required): User's ID

**Returns:** `Task<InvestmentResponse>`
- `Investments`: List sorted by creation date (newest first)

**HTTP Method**: GET from Firebase RTDB  
**Endpoint**: `/users/{userId}/investments.json`

---

### 20. Delete Investment

**Method:** `DeleteInvestmentAsync(userId, investmentId)`

**C# Concepts Used**: Async/Await, Task<T>

```csharp
var result = await _firebaseService.DeleteInvestmentAsync("user123", "investmentId");

if (result.Success)
{
    Console.WriteLine("Investment deleted");
}
```

**Parameters:**
- `userId` (string, required): User's ID
- `investmentId` (string, required): Investment ID to delete

**Returns:** `Task<BasicResponse>`

**HTTP Method**: DELETE to Firebase RTDB  
**Endpoint**: `/users/{userId}/investments/{investmentId}.json`

---

## Budget APIs

### 21. Save Budget

**Method:** `SaveBudgetAsync(userId, budget)`

**C# Concepts Used**: Async/Await, Task<T>, Object Initialization

```csharp
var budget = new MonthlyBudgetModel
{
    Category = "Food",
    AllocatedAmount = 5000m,
    Month = DateTime.Now.Month,
    Year = DateTime.Now.Year,
    Notes = "Monthly food budget"
};

var result = await _firebaseService.SaveBudgetAsync("user123", budget);

if (result.Success)
{
    Console.WriteLine("Budget saved");
}
```

**Parameters:**
- `userId` (string, required): User's ID
- `budget` (MonthlyBudgetModel, required): Budget details

**Returns:** `Task<BasicResponse>`

**Budget Categories:**
- Food
- Travel
- Entertainment
- Shopping
- Bills
- Health
- Education
- Other

**HTTP Method**: PUT to Firebase RTDB  
**Endpoint**: `/budgets/{userId}/{budgetId}.json`

---

### 22. Get Budgets

**Method:** `GetBudgetsAsync(userId, month, year)`

**C# Concepts Used**: Async/Await, Task<T>, Filtering by Month/Year

```csharp
var now = DateTime.Now;
var result = await _firebaseService.GetBudgetsAsync("user123", now.Month, now.Year);

if (result.Success && result.Budgets != null)
{
    foreach (var budget in result.Budgets)
    {
        Console.WriteLine($"{budget.Category}: ₹{budget.AllocatedAmount}");
    }
}
```

**Parameters:**
- `userId` (string, required): User's ID
- `month` (int, required): Month (1-12)
- `year` (int, required): Year (e.g., 2024)

**Returns:** `Task<BudgetResponse>`

**HTTP Method**: GET from Firebase RTDB  
**Endpoint**: `/budgets/{userId}.json`

---

### 23. Delete Budget

**Method:** `DeleteBudgetAsync(userId, budgetId)`

**C# Concepts Used**: Async/Await, Task<T>

```csharp
var result = await _firebaseService.DeleteBudgetAsync("user123", "budgetId");

if (result.Success)
{
    Console.WriteLine("Budget deleted");
}
```

**Parameters:**
- `userId` (string, required): User's ID
- `budgetId` (string, required): Budget ID to delete

**Returns:** `Task<BasicResponse>`

**HTTP Method**: DELETE to Firebase RTDB  
**Endpoint**: `/budgets/{userId}/{budgetId}.json`

---

### 24. Check Budget Alert

**Method:** `CheckBudgetAlertAsync(userId, category)`

**C# Concepts Used**: Async/Await, Task<T>, Decimal Calculations

```csharp
var result = await _firebaseService.CheckBudgetAlertAsync("user123", "Food");

if (result.HasExceeded)
{
    Console.WriteLine($"⚠️ Budget exceeded!");
    Console.WriteLine($"Budget: ₹{result.BudgetAmount}");
    Console.WriteLine($"Spent: ₹{result.CurrentSpending}");
    Console.WriteLine($"Overage: ₹{result.Overage}");
    Console.WriteLine($"Used: {result.PercentageUsed}%");
}
```

**Parameters:**
- `userId` (string, required): User's ID
- `category` (string, required): Category to check

**Returns:** `Task<BudgetAlertResponse>`
- `HasExceeded`: Boolean indicating budget status
- `BudgetAmount`: Budget limit
- `CurrentSpending`: Amount spent
- `Overage`: Amount over budget
- `PercentageUsed`: Percentage of budget used

**HTTP Method**: GET from Firebase RTDB  
**Endpoint**: `/expenses/{userId}.json` + `/budgets/{userId}.json`

---

## Response Models

### AuthResponse

```csharp
public class AuthResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public UserData? User { get; set; }
    public string? Token { get; set; }
}
```

### BasicResponse

```csharp
public class BasicResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
}
```

### ExpenseResponse

```csharp
public class ExpenseResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public ExpenseData? Expense { get; set; }
    public List<ExpenseData>? Expenses { get; set; }
}
```

### SalaryResponse

```csharp
public class SalaryResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public SalaryModel? Salary { get; set; }
}
```

### InvestmentResponse

```csharp
public class InvestmentResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public List<InvestmentModel> Investments { get; set; }
}
```

### BudgetResponse

```csharp
public class BudgetResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public List<MonthlyBudgetModel>? Budgets { get; set; }
}
```

### BudgetAlertResponse

```csharp
public class BudgetAlertResponse
{
    public bool HasExceeded { get; set; }
    public string Category { get; set; }
    public decimal BudgetAmount { get; set; }
    public decimal CurrentSpending { get; set; }
    public decimal Overage { get; set; }
    public decimal PercentageUsed { get; set; }
    public string Message { get; set; }
}
```

---

## Error Handling

All API methods follow consistent error handling patterns:

```csharp
try
{
    var result = await _firebaseService.AddExpenseAsync(...);
    
    if (result.Success)
    {
        // Handle success
    }
    else
    {
        // result.Message contains error description
        Console.WriteLine($"Error: {result.Message}");
    }
}
catch (Exception ex)
{
    // Network or unexpected errors
    Console.WriteLine($"Exception: {ex.Message}");
}
```

**Common Error Messages:**
- "User ID is required" - Missing userId parameter
- "Firebase API key is not valid" - API key not set in MauiProgram
- "Invalid email or password" - Wrong credentials
- "Email already exists" - Email already registered
- "Password too weak" - Password less than 6 characters
- Firebase RTDB permission errors - Check security rules

---

## Firebase Security Rules

```json
{
  "rules": {
    "users": {
      "$uid": {
        ".read": "$uid === auth.uid",
        ".write": "$uid === auth.uid"
      }
    },
    "expenses": {
      "$uid": {
        ".read": "$uid === auth.uid",
        ".write": "$uid === auth.uid"
      }
    },
    "salary": {
      "$uid": {
        ".read": "$uid === auth.uid",
        ".write": "$uid === auth.uid"
      }
    },
    "investments": {
      "$uid": {
        ".read": "$uid === auth.uid",
        ".write": "$uid === auth.uid"
      }
    },
    "budgets": {
      "$uid": {
        ".read": "$uid === auth.uid",
        ".write": "$uid === auth.uid"
      }
    }
  }
}
```

These rules ensure users can only access their own data.

---

## API Usage Examples

See [QUICK_TEST_GUIDE.md](QUICK_TEST_GUIDE.md) for comprehensive test examples.

See [CSHARP_CONCEPTS_EXPLAINED.md](CSHARP_CONCEPTS_EXPLAINED.md) for detailed concept explanations.

---

**Last Updated**: December 2024  
**API Version**: 1.0  
**Firebase SDK**: Latest .NET MAUI Integration

