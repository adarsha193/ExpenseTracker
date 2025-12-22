# ExpenseTracker - Complete Architecture Documentation

## System Overview

ExpenseTracker is a cross-platform (.NET MAUI) personal finance management application with real-time Firebase integration. The architecture follows **MVVM (Model-View-ViewModel)** pattern with a **Service-Oriented** backend design.

```
┌──────────────────────────────────────────────────────────────┐
│                    .NET MAUI UI Layer                        │
│  (iOS, Android, macOS, Windows - Single Codebase)           │
└─────────────────────────┬──────────────────────────────────┘
                          │
┌─────────────────────────▼──────────────────────────────────┐
│              XAML Pages (Views) + Code-Behind                │
│  - LoginPage, DashboardPage, AddExpensePage, etc.           │
│  - Binding to ViewModels via Data Context                   │
└─────────────────────────┬──────────────────────────────────┘
                          │
┌─────────────────────────▼──────────────────────────────────┐
│        ViewModel Layer (INotifyPropertyChanged)              │
│  - DashboardPageViewModel                                   │
│  - Handles state management & user commands                 │
│  - Communicates with Services                               │
└─────────────────────────┬──────────────────────────────────┘
                          │
┌─────────────────────────▼──────────────────────────────────┐
│              Service Layer (Business Logic)                  │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ FirebaseRealtimeDbService (Core Backend Service)    │  │
│  │ ├─ Authentication (Login, Register, Password)       │  │
│  │ ├─ Expense Management (CRUD operations)             │  │
│  │ ├─ Budget Tracking (Set, Monitor, Alert)            │  │
│  │ ├─ Salary Management (Store & Retrieve)             │  │
│  │ └─ Investment Portfolio (Add, Update, Delete)       │  │
│  ├─────────────────────────────────────────────────────┤  │
│  │ BudgetAlertService (Budget Monitoring)              │  │
│  │ ├─ Check all budget thresholds                      │  │
│  │ ├─ Detect overspending                              │  │
│  │ └─ Calculate alert levels (Green/Yellow/Orange/Red) │  │
│  ├─────────────────────────────────────────────────────┤  │
│  │ AISuggestionsService (Smart Recommendations)        │  │
│  │ ├─ Analyze spending patterns                        │  │
│  │ ├─ Generate budget suggestions                      │  │
│  │ └─ Identify savings opportunities                   │  │
│  ├─────────────────────────────────────────────────────┤  │
│  │ NotificationService (User Alerts)                   │  │
│  │ ├─ Send budget alerts                               │  │
│  │ ├─ Send critical overspending notifications         │  │
│  │ └─ Schedule daily budget summaries                  │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────┬──────────────────────────────────┘
                          │
┌─────────────────────────▼──────────────────────────────────┐
│            Data Access Layer (REST APIs)                     │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ Firebase Authentication (Identity Toolkit API)      │  │
│  │ └─ Email/Password auth, Password reset              │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ Firebase Realtime Database (REST API)               │  │
│  │ └─ JSON data persistence & real-time sync           │  │
│  └──────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────┘
```

---

## Core Architectural Patterns

### 1. MVVM (Model-View-ViewModel)

**What it is:** A design pattern that separates UI from business logic.

**How it's used:**
- **Model**: `ExpenseData`, `UserData`, `SalaryModel`, `InvestmentModel`
- **View**: XAML pages (`DashboardPage.xaml`, `AddExpensePage.xaml`)
- **ViewModel**: `DashboardPageViewModel` - Business logic & data binding

**Data Binding Flow:**
```
View (XAML)
  ↓ (Binding)
ViewModel (Property + Commands)
  ↓ (Calls)
Services (Business Logic)
  ↓ (HTTP)
Firebase APIs
```

### 2. Dependency Injection (DI)

**What it is:** Services are provided to classes via constructor instead of being created internally.

**How it's used in MauiProgram.cs:**
```csharp
builder.Services.AddSingleton<FirebaseRealtimeDbService>();
builder.Services.AddSingleton<BudgetAlertService>();
builder.Services.AddSingleton<AISuggestionsService>();
builder.Services.AddSingleton<NotificationService>();
```

**Benefits:**
- ✅ Easy to test (inject mocks)
- ✅ Loose coupling between components
- ✅ Single responsibility

### 3. Service-Oriented Architecture

**What it is:** Features are implemented as reusable, independent services.

**How it's used:**
- Each service handles one domain (Firebase, Budgets, AI, Notifications)
- Services are injectable and testable
- Clear separation of concerns

### 4. Repository Pattern (Implicit)

**What it is:** FirebaseRealtimeDbService acts as a data repository - abstraction over database.

**Benefits:**
- Data access logic in one place
- Easy to swap implementations
- Testable database interactions

---

## Project Structure

```
ExpenseTracker/
├── 📁 LaunchScreen/                    # Authentication Pages
│   ├── LoginPage.xaml(.cs)            # User login
│   ├── RegistrationPage.xaml(.cs)     # New user registration
│   ├── ForgotPassword.xaml(.cs)       # Password recovery
│   └── LaunchScreenPage.xaml(.cs)     # Splash/welcome screen
│
├── 📁 ExpensePages/                    # Expense Management
│   ├── AddExpensePage.xaml(.cs)       # Create new expense
│   ├── ViewAllExpensesPage.xaml(.cs)  # List all expenses
│   └── ExpenseDetailPage.xaml(.cs)    # View expense details
│
├── 📁 BudgetPages/                     # Budget Management
│   └── BudgetPage.xaml(.cs)           # Set & monitor budgets
│
├── 📁 InvestmentPages/                 # Investment Portfolio
│   ├── InvestmentPage.xaml(.cs)       # View investments
│   ├── AddInvestmentPage.xaml(.cs)    # Add new investment
│   └── EditInvestmentCache.cs         # Cache management
│
├── 📁 SalaryPages/                     # Income Management
│   └── SalaryPage.xaml(.cs)           # Manage salary info
│
├── 📁 Dashboard/                       # Main Dashboard
│   └── DashboardPage.xaml(.cs)        # Home screen with summary
│
├── 📁 DashboardViewModel/              # ViewModels
│   └── DashboardPageViewModel.cs      # Dashboard logic & data
│
├── 📁 Services/                        # Business Logic Layer
│   ├── FirebaseRealtimeDbService.cs   # ⭐ Core backend service (27 APIs)
│   ├── BudgetAlertService.cs          # Budget monitoring
│   ├── AISuggestionsService.cs        # AI recommendations
│   ├── NotificationService.cs         # Alerts & notifications
│   ├── IUserDialogService.cs          # Dialog interface
│   ├── UserDialogService.cs           # Dialog implementation
│   ├── ServiceLocator.cs              # Service provider
│   └── ...                            # Other utilities
│
├── 📁 Models/                          # Data Models
│   ├── AuthModels.cs                  # Auth request/response
│   ├── ExpenseData.cs                 # Expense entity
│   ├── SalaryModel.cs                 # Salary entity
│   ├── InvestmentModel.cs             # Investment entity
│   ├── MonthlyBudgetModel.cs          # Budget entity
│   └── Response models                # API responses
│
├── 📁 Converters/                      # XAML Value Converters
│   └── InvestmentReturnConverter.cs   # Convert return % to display
│
├── 📁 Resources/                       # UI Resources
│   ├── Styles/                        # App styling
│   ├── Fonts/                         # Custom fonts
│   ├── Images/                        # App images
│   └── Strings/                       # Localization
│
├── 📁 Platforms/                       # Platform-Specific Code
│   ├── Android/                       # Android-specific
│   ├── iOS/                           # iOS-specific
│   ├── macOS/                         # macOS-specific
│   └── Windows/                       # Windows-specific
│
├── 📁 ExpenseTracker.Tests/            # Unit Tests (280+ tests)
│   ├── FirebaseRealtimeDbServiceTests.cs
│   ├── BudgetAlertServiceTests.cs
│   ├── AISuggestionsServiceTests.cs
│   ├── NotificationServiceTests.cs
│   └── ModelTests.cs
│
├── 📄 App.xaml(.cs)                   # Application configuration
├── 📄 AppShell.xaml(.cs)              # Navigation routing
├── 📄 MauiProgram.cs                  # DI & initialization
├── 📄 ExpenseTracker.csproj           # Project configuration
└── 📄 ExpenseTracker.sln              # Solution file
```

---

## Complete Data Flow Diagrams

### 1. User Authentication Flow

```
┌─────────────────┐
│   LoginPage     │
└────────┬────────┘
         │ (Email, Password)
         ▼
┌────────────────────────────────┐
│ FirebaseRealtimeDbService      │
│ .LoginAsync()                  │
└────────┬───────────────────────┘
         │ HTTP POST
         ▼
┌─────────────────────────────────────┐
│ Firebase Identity Toolkit API       │
│ /v1/accounts:signInWithPassword     │
└────────┬────────────────────────────┘
         │ (Auth token + User ID)
         ▼
┌──────────────────────────────────┐
│ SecureStorage (Device Encrypted) │
│ - Auth Token                     │
│ - User ID                        │
└────────┬─────────────────────────┘
         │
         ▼
┌──────────────────┐
│ DashboardPage    │
│ (Authenticated)  │
└──────────────────┘
```

### 2. Add Expense Flow

```
┌──────────────────────┐
│ AddExpensePage       │
│ (User fills form)    │
└──────────┬───────────┘
           │ Expense data + Category
           ▼
┌─────────────────────────────────┐
│ FirebaseRealtimeDbService       │
│ .AddExpenseAsync()              │
│ - Generate GUID                 │
│ - Set timestamps                │
└──────────┬──────────────────────┘
           │ HTTP PUT
           ▼
┌─────────────────────────────────┐
│ Firebase RTDB                   │
│ /expenses/{userId}/{expenseId}  │
└──────────┬──────────────────────┘
           │
           ▼
┌──────────────────────────┐
│ UpdateUserTotalExpenses()│
│ (Side effect)            │
└──────────┬───────────────┘
           │ HTTP PUT
           ▼
┌──────────────────────────────┐
│ Firebase RTDB                │
│ /users/{userId}/totalExpenses│
└──────────┬───────────────────┘
           │
           ▼
┌──────────────────────────────┐
│ BudgetAlertService.Check()   │
│ (Detect overspending)        │
└──────────┬───────────────────┘
           │
           ▼
┌──────────────────────────────┐
│ NotificationService.Send()   │
│ (If budget exceeded)         │
└──────────┬───────────────────┘
           │
           ▼
┌──────────────────┐
│ User Notification│
│ (Alert Dialog)   │
└──────────────────┘
```

### 3. Budget Monitoring Flow

```
┌─────────────────────┐
│ DashboardPage       │
│ OnAppearing()       │
└──────────┬──────────┘
           │
           ▼
┌────────────────────────────────────┐
│ BudgetAlertService                 │
│ .CheckAllBudgetAlertsAsync()       │
└──────────┬───────────────────────────┘
           │ For each category:
           ▼
┌────────────────────────────────────┐
│ FirebaseRealtimeDbService          │
│ .CheckBudgetAlertAsync()           │
└──────────┬───────────────────────────┘
           │
           ├─→ GET /expenses/{userId}
           │   (Calculate spending)
           │
           ├─→ GET /budgets/{userId}
           │   (Get budget limit)
           │
           └─→ Compare & Calculate
               - Alert Level (Green/Yellow/Orange/Red)
               - Percentage Used
               - Overage Amount
           
           ▼
┌────────────────────────────────┐
│ Return BudgetAlertResponse     │
│ - HasExceeded: bool            │
│ - PercentageUsed: decimal      │
│ - Overage: decimal             │
└──────────┬─────────────────────┘
           │
           ▼
┌────────────────────────────────┐
│ DashboardPageViewModel         │
│ .AlertLevel = Yellow/Red       │
└──────────┬─────────────────────┘
           │ INotifyPropertyChanged
           ▼
┌────────────────────────────────┐
│ XAML Binding                   │
│ Displays alert color & message │
└────────────────────────────────┘
```

---

## Service Layer Details

### FirebaseRealtimeDbService (Core Service)

**Responsibility:** Handle ALL Firebase API interactions

**27 Total API Methods:**

**Authentication (6):**
- `LoginAsync` → Firebase Auth
- `RegisterAsync` → Firebase Auth
- `ForgotPasswordAsync` → Firebase Auth
- `ChangePasswordAsync` → Firebase Auth
- `ResetPasswordAsync` → Firebase Auth
- `LogoutAsync` → Local cleanup

**User Profile (3):**
- `GetUserDataFromDb` → Fetch user data
- `UpdateUserProfileAsync` → Update profile
- `GetAllUsersAsync` → Admin function

**Expense Management (6):**
- `AddExpenseAsync` → POST
- `GetUserExpensesAsync` → GET all
- `GetExpenseAsync` → GET single
- `UpdateExpenseAsync` → PUT
- `DeleteExpenseAsync` → DELETE
- `GetExpensesSummaryAsync` → Calculate summary

**Salary (2):**
- `SaveSalaryAsync` → PUT
- `GetSalaryAsync` → GET

**Investment (3):**
- `SaveInvestmentAsync` → PUT
- `GetInvestmentsAsync` → GET
- `DeleteInvestmentAsync` → DELETE

**Budget (4):**
- `SaveBudgetAsync` → PUT
- `GetBudgetsAsync` → GET by month/year
- `DeleteBudgetAsync` → DELETE
- `CheckBudgetAlertAsync` → Calculate alert

**Alternative methods (3):**
- `GetUserSalaryAsync`
- `GetUserInvestmentsAsync`
- `GetUserBudgetsAsync`

### BudgetAlertService

**Responsibility:** Monitor budgets and detect overspending

**Key Methods:**
- `CheckAllBudgetAlertsAsync()` - Check all categories
- `CheckCategoryAlertAsync()` - Check specific category
- `LogAlertAsync()` - Persist alert history

**Alert Levels:**
- 🟢 Green: < 75% used
- 🟡 Yellow: 75-90% used
- 🟠 Orange: 90-100% used
- 🔴 Red: > 100% used (Exceeded)

### AISuggestionsService

**Responsibility:** Generate smart budget recommendations

**Key Methods:**
- `GenerateBudgetSuggestionsAsync()` - Budget recommendations
- `AnalyzeSpendingTrendsAsync()` - Identify trends

**Analyses:**
- Last 3 months spending patterns
- Category-wise averages
- Savings rate calculation
- Budget vs actual comparison

### NotificationService

**Responsibility:** Send user alerts and notifications

**Key Methods:**
- `SendBudgetAlertAsync()` - General alert
- `SendCriticalAlertAsync()` - Overspending alert
- `SendWarningAsync()` - Warning notification
- `SendDailySummaryAsync()` - Daily summary
- `ScheduleDailyCheckAsync()` - Schedule checks

---

## Data Models & Response Types

### Request Models
```
LoginRequest → Email, Password
RegisterRequest → FullName, Email, Password
ForgotPasswordRequest → Email
ResetPasswordRequest → Email, ResetToken, NewPassword
```

### Response Models
```
AuthResponse
├── Success: bool
├── Message: string
├── User: UserData
└── Token: string

ExpenseResponse
├── Success: bool
├── Message: string
├── Expense: ExpenseData (single)
└── Expenses: List<ExpenseData> (multiple)

BasicResponse
├── Success: bool
└── Message: string

BudgetAlertResponse
├── HasExceeded: bool
├── BudgetAmount: decimal
├── CurrentSpending: decimal
├── Overage: decimal
├── PercentageUsed: decimal
└── Message: string
```

### Entity Models
```
UserData
├── Id: string
├── FullName: string
├── Email: string
├── ProfileImageUrl: string
├── PhoneNumber: string
├── Location: string
├── TotalExpenses: decimal
└── CreatedAt: DateTime

ExpenseData
├── Id: string (GUID)
├── UserId: string (FK)
├── Category: string
├── Amount: decimal
├── Description: string
├── Icon: string (emoji)
├── ShopName: string
├── Address: string
├── Location: string (GPS)
├── Date: DateTime
├── CreatedAt: DateTime
└── ModifiedAt: DateTime

SalaryModel
├── Id: string
├── UserId: string (FK)
├── Amount: decimal
├── Frequency: string (Monthly/Bi-weekly/Weekly/Daily)
├── StartDate: DateTime
├── Notes: string
├── CreatedAt: DateTime
└── UpdatedAt: DateTime

InvestmentModel
├── Id: string
├── UserId: string (FK)
├── InvestmentType: string (Stocks/Bonds/Crypto/Real Estate/Mutual Funds)
├── Amount: decimal
├── ReturnRate: decimal
├── InvestmentDate: DateTime
├── InvestmentFrequency: string (One-Time/Monthly SIP)
├── Description: string
├── CreatedAt: DateTime
└── UpdatedAt: DateTime

MonthlyBudgetModel
├── Id: string
├── UserId: string (FK)
├── Category: string
├── AllocatedAmount: decimal
├── Month: int (1-12)
├── Year: int
├── Notes: string
├── CreatedAt: DateTime
└── UpdatedAt: DateTime
```

---

## Firebase Database Structure

```json
{
  "users": {
    "{userId}": {
      "id": "user_guid",
      "fullName": "John Doe",
      "email": "john@example.com",
      "phoneNumber": "+91-9876543210",
      "location": "Mumbai, India",
      "profileImageUrl": "https://...",
      "totalExpenses": 25000,
      "createdAt": "2024-12-21T10:30:00Z"
    }
  },
  
  "expenses": {
    "{userId}": {
      "{expenseId}": {
        "id": "expense_guid",
        "userId": "{userId}",
        "category": "Food",
        "amount": 450.50,
        "description": "Lunch at cafe",
        "icon": "🍔",
        "shopName": "Cafe Coffee Day",
        "address": "Mumbai, MH",
        "location": "19.0760, 72.8777",
        "date": "2024-12-21T12:00:00Z",
        "createdAt": "2024-12-21T12:05:00Z",
        "modifiedAt": null
      }
    }
  },
  
  "salary": {
    "{userId}": {
      "{salaryId}": {
        "id": "salary_guid",
        "userId": "{userId}",
        "amount": 50000,
        "frequency": "Monthly",
        "startDate": "2024-01-01T00:00:00Z",
        "notes": "Monthly salary",
        "createdAt": "2024-12-21T10:00:00Z",
        "updatedAt": "2024-12-21T10:00:00Z"
      }
    }
  },
  
  "investments": {
    "{userId}": {
      "{investmentId}": {
        "id": "investment_guid",
        "userId": "{userId}",
        "investmentType": "Stocks",
        "amount": 100000,
        "returnRate": 12.5,
        "investmentDate": "2024-01-15T00:00:00Z",
        "investmentFrequency": "One-Time",
        "description": "Blue chip portfolio",
        "createdAt": "2024-12-21T10:00:00Z",
        "updatedAt": "2024-12-21T10:00:00Z"
      }
    }
  },
  
  "budgets": {
    "{userId}": {
      "{budgetId}": {
        "id": "budget_guid",
        "userId": "{userId}",
        "category": "Food",
        "allocatedAmount": 5000,
        "month": 12,
        "year": 2024,
        "notes": "Monthly food budget",
        "createdAt": "2024-12-21T10:00:00Z",
        "updatedAt": "2024-12-21T10:00:00Z"
      }
    }
  }
}
```

---

## Security Architecture

### Authentication
- **Method:** Firebase Authentication with Email/Password
- **Token Storage:** SecureStorage (OS-level encryption)
- **Token Usage:** All RTDB requests include auth token
- **Session:** Auto-logout on app exit

### Data Security
- **Encryption in Transit:** HTTPS for all Firebase REST calls
- **Encryption at Rest:** Firebase default encryption
- **Access Control:** Firebase security rules per user ID

### Firebase Security Rules
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

### Input Validation
- All user inputs validated before API calls
- Email format validation
- Password minimum length checking
- Amount decimal validation
- Null checks throughout

### Error Messages
- User-friendly error descriptions
- Never expose technical details
- Logging for debugging (debug builds only)

---

## Key C# Concepts Used

1. **Async/Await** - All service methods are async
2. **Task & Task<T>** - Return types for async operations
3. **Properties & Auto-Properties** - Data encapsulation
4. **LINQ** - Filtering, grouping, sorting data
5. **Events & INotifyPropertyChanged** - UI binding
6. **Generics** - Type-safe collections
7. **Interfaces** - Service contracts
8. **Classes & Inheritance** - Object-oriented design
9. **Collections** - Lists, Dictionaries for data storage
10. **Null-Coalescing** - Safe null handling
11. **ICommand & Delegates** - UI command binding
12. **String Interpolation** - Readable string formatting
13. **Try-Catch-Finally** - Error handling
14. **Using Statements** - Resource cleanup
15. **Dependency Injection** - Service injection
16. **Extension Methods** - Helper functions
17. **Enums** - Alert levels, investment types
18. **Method Overloading** - Multiple signatures
19. **Lambda Expressions** - LINQ queries
20. **Tuples** - Multi-value returns

---

## Testing Architecture

**Framework:** xUnit, Moq, FluentAssertions

**Coverage:** 280+ unit tests with 85%+ code coverage

**Test Categories:**
- Authentication flows (15 tests)
- Expense CRUD (20 tests)
- Budget monitoring (50 tests)
- AI suggestions (40 tests)
- Notifications (35 tests)
- Data models (50 tests)
- ViewModels (50 tests)

**Running Tests:**
```bash
cd ExpenseTracker
dotnet test
```

---

## Cross-Platform Considerations

### Platform-Specific Code
Located in `Platforms/` folder:
- **Android/** - Android-specific implementations
- **iOS/** - iOS-specific implementations
- **macOS/** - macOS-specific implementations
- **Windows/** - Windows-specific implementations

### Shared Code
- All services (100% shared)
- All models (100% shared)
- All ViewModels (100% shared)
- XAML pages (100% shared with platform fallbacks)

### Target Frameworks
- .NET 8.0+
- iOS 14.2+
- Android 21+
- macOS 11+
- Windows 10+

---

## Performance Optimizations

1. **HTTP Caching** - HttpClient reuses connections
2. **Local Caching** - Expenses cached in ViewModel
3. **LINQ Optimization** - Use Where before Select
4. **Async All I/O** - No blocking calls
5. **Lazy Loading** - Load data on demand
6. **Collection Virtualization** - XAML CollectionView optimization

---

## Future Architecture Improvements

1. **Offline Support** - Local SQLite cache with sync
2. **Real-time Sync** - Firebase Realtime listeners
3. **Advanced Caching** - Multi-level caching strategy
4. **GraphQL** - Replace REST with GraphQL
5. **CQRS Pattern** - Separate read/write models
6. **Event Sourcing** - Audit trail for all operations
7. **Microservices** - Split backend services
8. **CI/CD Pipeline** - GitHub Actions automation

---

## Documentation References

- **[FIREBASE_EXPENSE_API.md](FIREBASE_EXPENSE_API.md)** - Complete API reference (27 methods)
- **[CSHARP_CONCEPTS_EXPLAINED.md](CSHARP_CONCEPTS_EXPLAINED.md)** - C# patterns & concepts
- **[QUICK_TEST_GUIDE.md](QUICK_TEST_GUIDE.md)** - Testing instructions
- **[FIREBASE_API_KEY_SETUP.md](FIREBASE_API_KEY_SETUP.md)** - API key configuration
- **[README.md](README.md)** - Project overview

---

**Last Updated:** December 2024  
**Architecture Version:** 2.0 - Complete & Comprehensive  
**Status:** Production Ready ✅
