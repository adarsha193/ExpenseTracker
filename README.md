# ExpenseTracker

A comprehensive .NET MAUI cross-platform personal finance management application with Firebase Authentication and Realtime Database integration. Track expenses, manage budgets, monitor investments, and track salary with real-time data synchronization across iOS, Android, macOS, and Windows.

**Status**: ✅ Fully functional with comprehensive testing and documentation

---

## 📋 Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Quick Start](#quick-start)
- [Project Structure](#project-structure)
- [Architecture](#architecture)
- [Services & Components](#services--components)
- [Firebase Integration](#firebase-integration)
- [Testing](#testing)
- [Documentation](#documentation)
- [Development Guidelines](#development-guidelines)
- [Troubleshooting](#troubleshooting)
- [License](#license)

---

## ✨ Features

### Core Features
- **🔐 Authentication**: Secure user registration, login, and password recovery via Firebase Auth
- **💰 Expense Management**: Add, view, edit, and delete expenses with categories, amounts, and dates
- **📊 Dashboard**: Real-time expense summary with visual insights and statistics
- **💳 Budget Management**: Set monthly budgets by category and receive alerts on overspending
- **💼 Salary Tracking**: Track income information and salary management
- **📈 Investment Portfolio**: Monitor investments and track investment returns
- **👤 User Profile**: Manage user information and personal settings
- **🌍 Localization**: Multi-language support via resource files
- **🔒 Secure Storage**: Encrypted credential storage using MAUI SecureStorage
- **🤖 AI Suggestions**: Smart budget recommendations based on spending patterns
- **🔔 Notifications**: Real-time budget alerts and spending notifications
- **⚙️ Settings**: Customizable app preferences and help documentation

### Technical Features
- **Cross-Platform**: Single codebase for iOS, Android, macOS, and Windows
- **MVVM Architecture**: Clean separation of UI, business logic, and data
- **Dependency Injection**: Loosely coupled, testable service architecture
- **Real-time Sync**: Firebase-powered data synchronization
- **Offline Support**: Local storage with secure encryption
- **Responsive UI**: Modern XAML-based user interface
- **85%+ Test Coverage**: Comprehensive unit and integration tests

---

## 🛠️ Tech Stack

| Component | Technology |
|-----------|-----------|
| **Framework** | .NET MAUI on .NET 10.0 |
| **Language** | C# 12+ |
| **Backend** | Firebase (Authentication + Realtime Database) |
| **API Layer** | REST (HTTP) |
| **UI Markup** | XAML |
| **UI Pattern** | MVVM (Model-View-ViewModel) |
| **Local Storage** | Preferences, SecureStorage, JSON serialization |
| **Testing** | xUnit, Moq |
| **Platforms** | iOS, Android, macOS (Catalyst), Windows (UWP) |

---

## 🎯 Quick Feature Overview

```
┌──────────────────────────────────────────────────────┐
│                  ExpenseTracker App                  │
├──────────────────────────────────────────────────────┤
│                                                      │
│  🔐 Secure Authentication (Firebase)                │
│     ├─ Email/Password Login                         │
│     ├─ Account Registration                         │
│     └─ Password Recovery                            │
│                                                      │
│  💰 Expense Management                              │
│     ├─ Add/Edit/Delete Expenses                     │
│     ├─ Categorize Spending                          │
│     ├─ Search & Filter                              │
│     └─ Detailed History                             │
│                                                      │
│  📊 Dashboard                                        │
│     ├─ Total Spending Overview                      │
│     ├─ Recent Expenses                              │
│     └─ Quick Add Expense                            │
│                                                      │
│  💳 Budget Management                               │
│     ├─ Set Category Budgets                         │
│     ├─ Real-time Progress Tracking                  │
│     ├─ Budget Alerts (Red/Orange/Yellow/Green)      │
│     └─ Overspending Warnings                        │
│                                                      │
│  📈 Investment Portfolio                            │
│     ├─ Track Investments                            │
│     ├─ Calculate Returns                            │
│     └─ Portfolio Summary                            │
│                                                      │
│  💼 Salary Tracking                                 │
│     ├─ Record Income                                │
│     ├─ Track Payment Frequency                      │
│     └─ Income History                               │
│                                                      │
│  👤 User Management                                 │
│     ├─ Profile Information                          │
│     ├─ Account Statistics                           │
│     ├─ Settings & Preferences                       │
│     └─ Secure Logout                                │
│                                                      │
│  🔔 Smart Features                                  │
│     ├─ Budget Alerts                                │
│     ├─ Spending Notifications                       │
│     ├─ AI Suggestions                               │
│     └─ Real-time Sync (Firebase)                    │
│                                                      │
└──────────────────────────────────────────────────────┘
```

---

### Prerequisites

- **.NET 10.0 SDK** or later ([download](https://dotnet.microsoft.com/download))
- **A Firebase Project** with Authentication and Realtime Database enabled
- **Platform SDKs**:
  - iOS/macOS: Xcode 14+ (on macOS only)
  - Android: Android SDK + Android Studio
  - Windows: .NET Windows workload
  - macOS: .NET macOS workload

### Setup Steps

#### 1️⃣ Firebase Configuration

1. Visit [Firebase Console](https://console.firebase.google.com/)
2. Create or select a project
3. Enable **Authentication** → Email/Password provider
4. Create a **Realtime Database** in test mode
5. Copy your **Web API Key** from Project Settings → General → Your apps

#### 2️⃣ Set Firebase API Key

Open [MauiProgram.cs](MauiProgram.cs) and update the API key (around line 22):

```csharp
const string firebaseWebApiKey = "YOUR_ACTUAL_API_KEY_HERE";
```

#### 3️⃣ Restore Dependencies

```bash
dotnet restore
```

#### 4️⃣ Build & Run

**Android Emulator**:
```bash
dotnet build -f net10.0-android -c Debug
# Or for quick run:
dotnet build -t:Run -f net10.0-android
```

**iOS Simulator** (macOS only):
```bash
dotnet build -f net10.0 -r iossimulator-arm64 -c Debug
dotnet build -t:Run -f net10.0 -r iossimulator-arm64
```

**macOS (Catalyst)**:
```bash
dotnet build -f net10.0-maccatalyst -c Debug
dotnet build -t:Run -f net10.0-maccatalyst
```

**Windows**:
```bash
dotnet build -f net10.0-windows10.0.19041.0 -c Debug
dotnet build -t:Run -f net10.0-windows10.0.19041.0
```

---

## 📁 Project Structure

```
ExpenseTracker/
│
├── 📂 Models/                          # Data model definitions
│   ├── AuthModels.cs                  # Authentication request/response models
│   ├── ExpenseModel.cs                # Expense data entity
│   ├── BudgetModel.cs                 # Budget data entity
│   ├── SalaryModel.cs                 # Salary/income data entity
│   └── InvestmentModel.cs             # Investment portfolio model
│
├── 📂 Services/                        # Business logic & backend integration
│   ├── FirebaseRealtimeDbService.cs   # Firebase Auth + RTDB REST API
│   ├── BudgetAlertService.cs          # Budget monitoring & alerts
│   ├── AISuggestionsService.cs        # AI-powered spending suggestions
│   ├── NotificationService.cs         # Local notification management
│   ├── IUserDialogService.cs          # Dialog interface
│   ├── UserDialogService.cs           # Dialog implementation
│   └── ServiceLocator.cs              # Service resolution (legacy)
│
├── 📂 ViewModels/                     # MVVM logic & state management
│   └── DashboardPageViewModel.cs      # Dashboard view logic
│
├── 📂 Views/                   # Views pages
│   ├── LoginPage.xaml(.cs)            # User login
│   ├── RegistrationPage.xaml(.cs)     # New user registration
│   ├── ForgotPassword.xaml(.cs)       # Password recovery
│   └── LaunchScreenPage.xaml(.cs)     # Welcome/splash screen
│   ├── DashboardPage.xaml(.cs)        # Dashboard UI
│   └── DashboardShell.xaml(.cs)       # App shell navigation
│   ├── AddExpensePage.xaml(.cs)       # Create new expense
│   ├── ViewAllExpensesPage.xaml(.cs)  # List all expenses
│   └── ExpenseDetailPage.xaml(.cs)    # View/edit expense details
│   └── BudgetPage.xaml(.cs)           # Set & monitor budgets
│   └── SalaryPage.xaml(.cs)           # Salary configuration
│   ├── InvestmentPage.xaml(.cs)       # View investments
│   ├── AddInvestmentPage.xaml(.cs)    # Add new investment
│   └── EditInvestmentCache.cs         # Investment data cache
│   ├── ProfilePage.xaml(.cs)          # User profile
│   ├── SettingsPage.xaml(.cs)         # App settings
│   ├── HelpPage.xaml(.cs)             # Help & support
│   ├── AboutAppPage.xaml(.cs)         # About application
│   ├── ChangePasswordPage.xaml(.cs)   # Password management
│   └── TermsPage.xaml(.cs)            # Terms & conditions
│
├── 📂 Resources/                      # UI resources
│   ├── Strings/                       # Localization (.resx files)
│   ├── Images/                        # UI images & icons
│   ├── Fonts/                         # Font files
│   ├── Styles/                        # XAML theme & style dictionaries
│   ├── AppIcon/                       # App icons & branding
│   ├── Splash/                        # Splash screen designs
│   └── Raw/                           # Raw resource files
│
├── 📂 Converters/                     # XAML value converters
│   └── InvestmentReturnConverter.cs   # Format investment returns
│
├── 📂 Platforms/                      # Platform-specific code
│   ├── iOS/                           # iOS implementation
│   ├── Android/                       # Android implementation
│   ├── Windows/                       # Windows implementation
│   └── MacCatalyst/                   # macOS Catalyst implementation
│
├── 📂 ExpenseTracker.Tests/           # Unit test suite
│   ├── BudgetAlertServiceTests.cs     # Budget service tests
│   ├── FirebaseServiceTests.cs        # Firebase integration tests
│   ├── ModelsTests.cs                 # Data model tests
│   └── ExpenseTracker.Tests.csproj    # Test project file
│
├── 📂 Properties/                     # App properties
│   └── launchSettings.json            # Launch configuration
│
├── 📂 bin/ & obj/                     # Build output (generated)
│
├── 📄 App.xaml(.cs)                   # App root definition
├── 📄 AppShell.xaml(.cs)              # Authentication shell navigation
├── 📄 MauiProgram.cs                  # MAUI configuration & DI setup
├── 📄 ExpenseTracker.csproj           # Project file
├── 📄 ExpenseTracker.sln              # Solution file
│
└── 📚 Documentation Files:
    ├── README.md                      # This file
    ├── ARCHITECTURE.md                # System architecture & design patterns
    ├── PROJECT_QA.md                  # 65 Q&A on project features
    ├── CSHARP_MAUI_CONCEPTS.md        # 65 Q&A on C# & MAUI concepts
    ├── QUICK_TEST_GUIDE.md            # Testing commands & guide
    ├── FIREBASE_API_KEY_SETUP.md      # Firebase API key configuration
    ├── FIREBASE_EXPENSE_API.md        # Expense API endpoint documentation
    ├── LICENSE                        # License information
    └── run_tests.sh                   # Test automation script
```

---

## 🏗️ Architecture

### MVVM Pattern

```
┌─────────────────────────────────────────┐
│      View (XAML Pages)                  │
│  DashboardPage, ExpensePage, etc.       │
└──────────────────┬──────────────────────┘
                   │ Data Binding
                   │
┌──────────────────▼──────────────────────┐
│   ViewModel (Business Logic)            │
│  DashboardPageViewModel                 │
│  - Properties (INotifyPropertyChanged)  │
│  - Commands (ICommand)                  │
└──────────────────┬──────────────────────┘
                   │ Service Calls
                   │
┌──────────────────▼──────────────────────┐
│   Services (Business Rules)             │
│  - FirebaseRealtimeDbService            │
│  - BudgetAlertService                   │
│  - AISuggestionsService                 │
│  - NotificationService                  │
└──────────────────┬──────────────────────┘
                   │ HTTP REST Calls
                   │
┌──────────────────▼──────────────────────┐
│   Firebase APIs                         │
│  - Authentication (Identity Toolkit)    │
│  - Realtime Database (REST)             │
└─────────────────────────────────────────┘
```

### Dependency Injection

All services are registered in [MauiProgram.cs](MauiProgram.cs) with lifetimes:
- **Singleton**: Services maintaining app-wide state (FirebaseService, BudgetAlertService)
- **Transient**: Stateless utilities
- **Scoped**: Page/ViewModel-level services

### Design Patterns Used

- **MVVM**: Model-View-ViewModel pattern for UI separation
- **Service Layer**: Business logic abstracted into injectable services
- **Dependency Injection**: Loose coupling via constructor injection
- **Repository Pattern**: FirebaseRealtimeDbService as data access abstraction
- **Observer Pattern**: INotifyPropertyChanged for reactive UI updates
- **Command Pattern**: ICommand for user actions

See [ARCHITECTURE.md](ARCHITECTURE.md) for comprehensive architecture documentation.

---

## 🔧 Services & Components

### Core Services

| Service | Location | Purpose |
|---------|----------|---------|
| **FirebaseRealtimeDbService** | [Services/FirebaseRealtimeDbService.cs](Services/FirebaseRealtimeDbService.cs) | Firebase Auth + RTDB REST API integration; handles login, signup, all CRUD operations |
| **BudgetAlertService** | [Services/BudgetAlertService.cs](Services/BudgetAlertService.cs) | Monitors budget thresholds; calculates alert levels (Green/Yellow/Orange/Red) |
| **AISuggestionsService** | [Services/AISuggestionsService.cs](Services/AISuggestionsService.cs) | Analyzes spending patterns; generates budget suggestions |
| **NotificationService** | [Services/NotificationService.cs](Services/NotificationService.cs) | Sends local notifications for budget alerts and spending updates |
| **IUserDialogService** | [Services/IUserDialogService.cs](Services/IUserDialogService.cs) | Interface for displaying alerts and dialogs |

### View Models

| ViewModel | Purpose |
|-----------|---------|
| **DashboardPageViewModel** | Main dashboard state & commands; manages dashboard data binding |

### Key Pages

| Page | Purpose |
|------|---------|
| **LoginPage** | User authentication |
| **RegistrationPage** | New user account creation |
| **DashboardPage** | Main application interface with expense overview |
| **AddExpensePage** | Create new expense records |
| **ViewAllExpensesPage** | Browse expense history |
| **BudgetPage** | Set and monitor monthly budgets |
| **InvestmentPage** | View and manage investment portfolio |
| **SalaryPage** | Manage salary/income information |
| **ProfilePage** | User profile management |
| **SettingsPage** | App preferences |

---

## 🔥 Firebase Integration

The app uses Firebase for:
- **User Authentication**: Email/password registration and login via Identity Toolkit API
- **Data Persistence**: Real-time storage of expenses, budgets, investments, and user data
- **Real-time Sync**: Automatic synchronization across devices


The project includes 85%+ code coverage with unit and integration tests.

### Test Suite

Located in [ExpenseTracker.Tests/](ExpenseTracker.Tests/)

- **BudgetAlertServiceTests.cs**: Budget monitoring & alert logic
- **FirebaseServiceTests.cs**: Firebase API integration
- **ModelsTests.cs**: Data model validation

### Running Tests

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run specific test file
dotnet test --filter "BudgetAlertServiceTests"

# Run specific test method
dotnet test --filter "LoginAsync_WithValidCredentials_ReturnsSuccessResponse"

# Run with code coverage
dotnet test /p:CollectCoverage=true /p:CoverageFormat=lcov
```

Or use the provided script:
```bash
./run_tests.sh
```

See [QUICK_TEST_GUIDE.md](QUICK_TEST_GUIDE.md) for detailed testing documentation.

---

## 📚 Documentation

Comprehensive documentation is included:

| Document | Purpose |
|----------|---------|
| [ARCHITECTURE.md](ARCHITECTURE.md) | Detailed system architecture, MVVM pattern, service design |
| [QUICK_TEST_GUIDE.md](QUICK_TEST_GUIDE.md) | Testing commands, test execution guide |
| [FIREBASE_API_KEY_SETUP.md](FIREBASE_API_KEY_SETUP.md) | Step-by-step Firebase API key configuration |
| [FIREBASE_EXPENSE_API.md](FIREBASE_EXPENSE_API.md) | Complete REST API endpoint documentation |
| [SCREENSHOTS_GUIDE.md](SCREENSHOTS_GUIDE.md) | Guide for adding app screenshots to README |

---

## 👨‍💻 Development Guidelines

### Code Organization

- **Models**: Data entities in [Models/](Models/) folder
- **Services**: Business logic in [Services/](Services/) folder
- **ViewModels**: MVVM logic in [ViewModels/](ViewModels/) folder
- **Views**: XAML pages in feature folders (ExpensePages/, BudgetPages/, etc.)
- **Resources**: UI resources in [Resources/](Resources/) folder
- **Tests**: Unit tests in [ExpenseTracker.Tests/](ExpenseTracker.Tests/) folder

### Best Practices

✅ **DO**:
- Use dependency injection for loose coupling
- Implement INotifyPropertyChanged for observable properties
- Write async/await for all I/O operations
- Store UI strings in [Resources/Strings/Strings.resx](Resources/Strings.resx) for localization
- Use converters for data formatting in XAML
- Register new services in [MauiProgram.cs](MauiProgram.cs)
- Write unit tests for services and ViewModels
- Document complex business logic with comments

❌ **DON'T**:
- Hardcode UI strings in XAML (breaks localization)
- Create tight coupling between layers
- Use code-behind for business logic (use ViewModel)
- Ignore async/await (blocks UI thread)
- Access ServiceLocator in new code (use constructor DI)
- Skip error handling in service calls
- Update UI from background threads

### Adding a New Feature

1. **Define Model** in [Models/](Models/) folder
2. **Create Service** in [Services/](Services/) folder; register in [MauiProgram.cs](MauiProgram.cs)
3. **Create ViewModel** for business logic
4. **Create Pages** (XAML + code-behind) in appropriate folder
5. **Add Navigation** in [AppShell.xaml](AppShell.xaml) or [DashboardShell.xaml](DashboardShell.xaml)
6. **Add Resources** (strings, images, icons) to [Resources/](Resources/)
7. **Write Tests** in [ExpenseTracker.Tests/](ExpenseTracker.Tests/)
8. **Update Documentation**

---

## 🐛 Troubleshooting

### Common Issues

| Issue | Cause | Solution |
|-------|-------|----------|
| "Firebase API Key is invalid" | Wrong or missing API key | Update [MauiProgram.cs](MauiProgram.cs) line 22 with correct Web API Key from Firebase Console |
| "Service resolution failed" | Service not registered | Ensure service is added in [MauiProgram.cs](MauiProgram.cs) with `builder.Services.AddSingleton<IService, Service>()` |
| "Access denied" Firebase RTDB | Invalid security rules | Check Firebase console RTDB security rules; update to allow authenticated users |
| Build fails with "SDK not found" | Missing platform SDK | Install required SDKs (Xcode for iOS, Android SDK for Android, etc.) |
| "Null reference exception" | Missing await on async call | Ensure all async operations use `await` keyword |
| "UI not updating" | Property not implementing INotifyPropertyChanged | Inherit ViewModel from ObservableObject or implement INotifyPropertyChanged |
| "SecureStorage not working" | Platform-specific issue | Verify permissions in platform-specific manifest files |

### Debug Logging

Enable debug output to diagnose issues:

```csharp
System.Diagnostics.Debug.WriteLine($"Debug message: {variable}");

// In code-behind for lifecycle events
protected override void OnAppearing()
{
    base.OnAppearing();
    Debug.WriteLine("Page appeared");
}
```

### Firebase Debugging

1. Check Firebase Console → Realtime Database → Data tab for records
2. Monitor Authentication → Users tab for account issues
3. Use Firebase Console → Rules tab to verify security rules
4. Check browser DevTools → Network tab to inspect REST API calls

---

## 📄 License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.

---

