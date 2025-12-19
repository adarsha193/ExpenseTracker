# Architecture Overview

## System Layers

```
┌─────────────────────────────────┐
│  UI Layer (XAML Pages)          │
│  LoginPage, DashboardPage, etc. │
└─────────────────┬───────────────┘
                  │
┌─────────────────▼───────────────┐
│  ViewModel Layer                │
│  DashboardPageViewModel         │
└─────────────────┬───────────────┘
                  │
┌─────────────────▼───────────────┐
│  Service Layer                  │
│  • FirebaseRealtimeDbService    │
│  • BudgetAlertService           │
│  • AISuggestionsService         │
│  • NotificationService          │
└─────────────────┬───────────────┘
                  │
┌─────────────────▼───────────────┐
│  Data Layer (Firebase REST)     │
│  Realtime Database              │
└─────────────────────────────────┘
```

## Key Data Flows

### User Login
```
LoginPage → FirebaseRealtimeDbService.LoginAsync()
  → Firebase Auth API
  → Fetch user profile from RTDB
  → Store token in SecureStorage
  → Navigate to Dashboard
```

### Add Expense
```
AddExpensePage → FirebaseRealtimeDbService.AddExpenseAsync()
  → Firebase RTDB (create expense record)
  → BudgetAlertService (check if budget exceeded)
  → NotificationService (send alert if needed)
  → DashboardPage (refresh data)
```

### Budget Monitoring
```
DashboardPage (on load)
  → BudgetAlertService.CheckAllBudgetAlertsAsync()
  → Compare spending vs budget limits
  → Return alerts with overage amounts
  → Display warnings to user
```

## Core Services

| Service | Purpose |
|---------|---------|
| **FirebaseRealtimeDbService** | All Firebase API calls (auth, CRUD for expenses, budgets, profiles, etc.) |
| **BudgetAlertService** | Detect overspending and calculate alert levels |
| **AISuggestionsService** | Generate budget recommendations based on spending patterns |
| **NotificationService** | Send local alerts when budgets exceeded |
| **UserDialogService** | Centralized UI alerts and confirmations |

## Firebase Database Structure

```
/users/{userId}
├── id, email, fullName, phoneNumber, location

/expenses/{userId}/{expenseId}
├── id, userId, category, description, amount, date, icon

/budgets/{userId}/{budgetId}
├── id, userId, category, allocatedAmount, month, year

/salary/{userId}
├── id, userId, amount, frequency

/investments/{userId}/{investmentId}
├── id, userId, investmentType, amount, returnRate
```

## Dependency Injection

Services are registered in `MauiProgram.cs`:

```csharp
builder.Services.AddSingleton<FirebaseRealtimeDbService>();
builder.Services.AddSingleton<BudgetAlertService>();
builder.Services.AddSingleton<AISuggestionsService>();
builder.Services.AddSingleton<NotificationService>();
builder.Services.AddSingleton<IUserDialogService, UserDialogService>();
```

Use constructor injection in pages and viewmodels:

```csharp
public DashboardPage(FirebaseRealtimeDbService firebase)
{
    _firebase = firebase;
}
```

## Features by Layer

| Feature | Service | UI Page |
|---------|---------|---------|
| Authentication | FirebaseRealtimeDbService | LoginPage |
| Expense Management | FirebaseRealtimeDbService | AddExpensePage, ViewAllExpensesPage |
| Dashboard | FirebaseRealtimeDbService | DashboardPage |
| Budget Alerts | BudgetAlertService | BudgetPage |
| AI Recommendations | AISuggestionsService | (Future) |
| Notifications | NotificationService | (System-wide) |

## Security Notes

- **Auth Tokens**: Stored in `SecureStorage` (encrypted)
- **API Calls**: All use HTTPS with Firebase REST API
- **Input Validation**: All user inputs validated before API calls
- **Error Messages**: User-friendly (never expose technical details)
- **Sensitive Data**: Never logged or stored in Preferences

## Adding New Features

1. Create page in appropriate folder (e.g., `/FeaturePages`)
2. Add methods to `FirebaseRealtimeDbService` if needed
3. Register new services in `MauiProgram.cs`
4. Use constructor DI in pages
5. Add high-level comments to new classes

## Error Handling

All service methods return Response objects:

```csharp
public class ExpenseResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public List<ExpenseData>? Expenses { get; set; }
}
```

This consistent pattern allows clean error handling in UI.
