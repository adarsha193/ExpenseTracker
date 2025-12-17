# Firebase Expense Management API

## Overview

The `FirebaseRealtimeDbService` now includes complete expense management functionality integrated with Firebase Realtime Database.

## Database Structure

Expenses are stored in the following Firebase RTDB structure:

```
/expenses/{userId}/{expenseId}
  ├── id: string
  ├── userId: string
  ├── category: string
  ├── description: string
  ├── amount: number (decimal)
  ├── date: timestamp
  ├── icon: string (emoji)
  ├── createdAt: timestamp
  └── modifiedAt: timestamp (optional)
```

## Available Methods

### 1. Add Expense

**Method:** `AddExpenseAsync(userId, category, amount, description, icon)`

```csharp
var result = await _authService.AddExpenseAsync(
    userId: "user_id_here",
    category: "Food",
    amount: 250.50m,
    description: "Lunch at restaurant",
    icon: "🍔"
);

if (result.Success)
{
    Console.WriteLine($"Expense added: {result.Expense?.Id}");
}
else
{
    Console.WriteLine($"Error: {result.Message}");
}
```

**Parameters:**
- `userId` (string, required): The ID of the user adding the expense
- `category` (string, required): Category name (e.g., "Food", "Travel", "Entertainment")
- `amount` (decimal, required): Expense amount in rupees
- `description` (string, optional): Additional details about the expense
- `icon` (string, optional): Emoji icon representing the category (e.g., "🍔", "✈️", "🎬")

**Response:**
- `Success`: Boolean indicating operation success
- `Message`: Status message
- `Expense`: The created ExpenseData object with auto-generated ID

---

### 2. Get All User Expenses

**Method:** `GetUserExpensesAsync(userId)`

```csharp
var result = await _authService.GetUserExpensesAsync("user_id_here");

if (result.Success && result.Expenses != null)
{
    foreach (var expense in result.Expenses)
    {
        Console.WriteLine($"{expense.Icon} {expense.Category}: ₹{expense.Amount}");
    }
}
```

**Parameters:**
- `userId` (string, required): The user's ID

**Response:**
- `Success`: Boolean indicating operation success
- `Message`: Status message (includes count of expenses found)
- `Expenses`: List of ExpenseData objects sorted by date (newest first)

**Note:** Returns empty list if no expenses found, not an error.

---

### 3. Get Single Expense

**Method:** `GetExpenseAsync(userId, expenseId)`

```csharp
var result = await _authService.GetExpenseAsync("user_id_here", "expense_id_here");

if (result.Success && result.Expense != null)
{
    Console.WriteLine($"Retrieved: {result.Expense.Category} - ₹{result.Expense.Amount}");
}
```

**Parameters:**
- `userId` (string, required): The user's ID
- `expenseId` (string, required): The expense ID to retrieve

**Response:**
- `Success`: Boolean indicating operation success
- `Message`: Status message
- `Expense`: The specific ExpenseData object, or null if not found

---

### 4. Update Expense

**Method:** `UpdateExpenseAsync(userId, expenseId, expenseData)`

```csharp
var updatedExpense = new ExpenseData
{
    Id = "expense_id_here",
    UserId = "user_id_here",
    Category = "Travel",
    Amount = 500m,
    Description: "Flight ticket",
    Icon: "✈️",
    Date: DateTime.UtcNow,
    CreatedAt: DateTime.UtcNow
};

var result = await _authService.UpdateExpenseAsync(
    userId: "user_id_here",
    expenseId: "expense_id_here",
    expenseData: updatedExpense
);

if (result.Success)
{
    Console.WriteLine("Expense updated successfully");
}
```

**Parameters:**
- `userId` (string, required): The user's ID
- `expenseId` (string, required): The expense ID to update
- `expenseData` (ExpenseData, required): Updated expense data

**Response:**
- `Success`: Boolean indicating operation success
- `Message`: Status message
- `Expense`: The updated ExpenseData object

**Note:** `ModifiedAt` timestamp is automatically set to current UTC time.

---

### 5. Delete Expense

**Method:** `DeleteExpenseAsync(userId, expenseId)`

```csharp
var result = await _authService.DeleteExpenseAsync("user_id_here", "expense_id_here");

if (result.Success)
{
    Console.WriteLine("Expense deleted successfully");
}
else
{
    Console.WriteLine($"Error: {result.Message}");
}
```

**Parameters:**
- `userId` (string, required): The user's ID
- `expenseId` (string, required): The expense ID to delete

**Response:**
- `Success`: Boolean indicating operation success
- `Message`: Status message

**Note:** Automatically updates user's total expenses after deletion.

---

### 6. Get Expenses Summary

**Method:** `GetExpensesSummaryAsync(userId)`

```csharp
var result = await _authService.GetExpensesSummaryAsync("user_id_here");

if (result.Success && result.Expenses != null)
{
    decimal total = result.Expenses.Sum(e => e.Amount);
    Console.WriteLine($"Total Expenses: ₹{total}");
    
    var byCategory = result.Expenses.GroupBy(e => e.Category);
    foreach (var category in byCategory)
    {
        decimal categoryTotal = category.Sum(e => e.Amount);
        Console.WriteLine($"{category.Key}: ₹{categoryTotal}");
    }
}
```

**Parameters:**
- `userId` (string, required): The user's ID

**Response:**
- `Success`: Boolean indicating operation success
- `Message`: Includes total amount
- `Expenses`: List of all expenses (same as GetUserExpensesAsync)

---

## ExpenseData Model

```csharp
public class ExpenseData
{
    public string? Id { get; set; }                    // Auto-generated GUID
    public string UserId { get; set; }                // User who created the expense
    public string Category { get; set; }              // e.g., "Food", "Travel"
    public string? Description { get; set; }          // Optional description
    public decimal Amount { get; set; }               // Amount in rupees
    public DateTime Date { get; set; }                // When the expense occurred
    public string? Icon { get; set; }                 // Emoji icon (e.g., "🍔")
    public DateTime CreatedAt { get; set; }           // When created
    public DateTime? ModifiedAt { get; set; }         // Last modified timestamp
}
```

---

## ExpenseResponse Model

```csharp
public class ExpenseResponse
{
    public bool Success { get; set; }                 // Operation success status
    public string Message { get; set; }               // Status/error message
    public ExpenseData? Expense { get; set; }         // Single expense (add, update, get)
    public List<ExpenseData>? Expenses { get; set; }  // Multiple expenses (list, summary)
}
```

---

## Common Categories and Icons

Recommended category/icon combinations:

| Category | Icon | Example |
|----------|------|---------|
| Food | 🍔 | Meals, restaurants |
| Travel | ✈️ | Flights, uber, gas |
| Entertainment | 🎬 | Movies, games |
| Shopping | 🛍️ | Clothes, groceries |
| Health | 💊 | Medicine, gym |
| Utilities | 💡 | Electricity, water |
| Other | 📝 | Miscellaneous |

---

## Integration Examples

### Display in Dashboard

```csharp
// In DashboardPage.xaml.cs
public partial class DashboardPage : ContentPage
{
    private readonly FirebaseRealtimeDbService _authService;
    
    public DashboardPage()
    {
        InitializeComponent();
        _authService = MauiProgram.CreateMauiApp()
            .Services.GetRequiredService<FirebaseRealtimeDbService>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        var userId = await SecureStorage.GetAsync("user_id");
        if (!string.IsNullOrEmpty(userId))
        {
            var result = await _authService.GetUserExpensesAsync(userId);
            
            if (result.Success && result.Expenses != null)
            {
                ExpensesCollectionView.ItemsSource = result.Expenses;
                
                decimal total = result.Expenses.Sum(e => e.Amount);
                TotalLabel.Text = $"Total: ₹{total:F2}";
            }
        }
    }
}
```

### Add Expense Form

```csharp
// In AddExpensePage.xaml.cs
private async void OnSaveExpenseClicked(object sender, EventArgs e)
{
    if (!decimal.TryParse(AmountEntry.Text, out decimal amount))
    {
        await DisplayAlert("Error", "Invalid amount", "OK");
        return;
    }

    var userId = await SecureStorage.GetAsync("user_id");
    
    var result = await _authService.AddExpenseAsync(
        userId: userId,
        category: CategoryPicker.SelectedItem?.ToString() ?? "Other",
        amount: amount,
        description: DescriptionEntry.Text,
        icon: GetIconForCategory(CategoryPicker.SelectedItem?.ToString() ?? "Other")
    );

    if (result.Success)
    {
        await DisplayAlert("Success", "Expense added", "OK");
        await Shell.Current.GoToAsync("dashboard");
    }
    else
    {
        await DisplayAlert("Error", result.Message, "OK");
    }
}

private string GetIconForCategory(string category)
{
    return category switch
    {
        "Food" => "🍔",
        "Travel" => "✈️",
        "Entertainment" => "🎬",
        "Shopping" => "🛍️",
        "Health" => "💊",
        "Utilities" => "💡",
        _ => "📝"
    };
}
```

---

## Error Handling

All methods return `ExpenseResponse` with:
- **Success**: `true` for successful operations
- **Success**: `false` for errors
- **Message**: Descriptive error message

**Common Errors:**
- "User ID is required" - Missing user ID parameter
- "User ID and Expense ID are required" - Missing required IDs
- "Expense not found" - Specified expense doesn't exist
- Firebase API errors (see authentication docs)

---

## Firebase Security Rules

Recommended Realtime Database security rules:

```json
{
  "rules": {
    "expenses": {
      "$uid": {
        ".read": "$uid === auth.uid",
        ".write": "$uid === auth.uid",
        ".validate": "root.child('users').child($uid).exists()"
      }
    },
    "users": {
      "$uid": {
        ".read": "$uid === auth.uid",
        ".write": "$uid === auth.uid"
      }
    }
  }
}
```

These rules ensure:
- Users can only read/write their own expenses
- Expenses are only created under valid user IDs
- User data is protected

---

## Data Persistence

All expense data is persisted in Firebase Realtime Database at:
- **URL**: `https://expanse-tracker-2a875-default-rtdb.firebaseio.com/expenses`

Data is automatically synced across all instances of the app for the logged-in user.

---

## Performance Tips

1. **Use GetUserExpensesAsync()** for listing - automatically sorted by date
2. **Cache results locally** if needed with MAUI's Preferences
3. **Use GetExpensesSummaryAsync()** for dashboard statistics
4. **Delete expenses carefully** - immediately updates user total

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Expenses not showing | Verify user ID is correctly stored in SecureStorage |
| Add expense fails | Check Firebase API key is set in MauiProgram.cs |
| Firebase permission errors | Review security rules in Firebase Console |
| Total expenses not updating | Wait a moment, automatic update is delayed |
| Empty list returned | No expenses exist yet for this user (not an error) |

