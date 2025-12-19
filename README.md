# ExpenseTracker

A .NET MAUI cross-platform expense tracking application with Firebase Authentication and Realtime Database integration. Manage expenses, budgets, investments, and salary tracking with real-time data synchronization.

## Features

- **Authentication**: User registration, login, and password recovery via Firebase Auth
- **Expense Management**: Add, view, edit, and delete expenses with categories and detailed breakdowns
- **Dashboard**: Real-time expense summary and visualization
- **Budget Management**: Set and monitor monthly budgets with alerts
- **Salary & Investment Tracking**: Track income and investment portfolio
- **User Profile**: Manage user information and settings
- **Localization**: Multi-language support via resource files
- **Secure Storage**: Credentials stored securely using MAUI SecureStorage
- **Cross-Platform**: Runs on iOS, Android, macOS, and Windows

## Prerequisites

- .NET 10.0 SDK or later
- A Firebase project with Realtime Database enabled
- Xcode (for iOS/macOS builds on macOS)
- Android SDK (for Android builds)

## Quick Start

### 1. Firebase Configuration

1. Go to [Firebase Console](https://console.firebase.google.com/)
2. Create or select your project
3. Enable **Authentication** (Email/Password)
4. Create a **Realtime Database**
5. Copy your Web API Key from Project Settings → General → Your apps

### 2. Set Firebase API Key

Open [MauiProgram.cs](MauiProgram.cs) and update line 22:

```csharp
const string firebaseWebApiKey = "YOUR_ACTUAL_API_KEY_HERE";
```

### 3. Restore and Build

```bash
# Restore NuGet packages
dotnet restore

# Build for .NET 10.0
dotnet build -f net10.0
```

### 4. Run on Your Platform

**iOS Simulator (macOS)**:
```bash
dotnet build -f net10.0 -r iossimulator-arm64 -c Debug
```

**Android Emulator**:
```bash
dotnet build -f net10.0-android -c Debug
```

**Windows**:
```bash
dotnet build -f net10.0-windows10.0.19041.0 -c Debug
```

**macOS (Catalyst)**:
```bash
dotnet build -f net10.0-maccatalyst -c Debug
```

## Project Structure

```
ExpenseTracker/
├── Models/                          # Data models (Auth, Expense, Budget, Salary, Investment)
├── Services/                        # Business logic layer
│   ├── FirebaseRealtimeDbService.cs # Firebase Auth + RTDB API integration
│   ├── BudgetAlertService.cs        # Budget monitoring
│   ├── AISuggestionsService.cs      # AI-powered suggestions
│   ├── NotificationService.cs       # Local notifications
│   └── UserDialogService.cs         # UI dialogs
├── LaunchScreen/                    # Authentication pages (Login, Register, Forgot Password)
├── Dashboard/                       # Main dashboard page
├── ExpensePages/                    # Expense management UI
├── BudgetPages/                     # Budget tracking UI
├── SalaryPages/                     # Salary management UI
├── InvestmentPages/                 # Investment tracking UI
├── SliderView/                      # Settings, Profile, Help, About pages
├── Resources/
│   ├── Strings/                     # Localization (resx files)
│   ├── Images/                      # UI images and icons
│   ├── Styles/                      # XAML resource dictionaries
│   └── AppIcon/                     # App icons and splash screens
└── Platforms/                       # Platform-specific code (Android, iOS, macOS, Windows)
```

## Key Components

| Component | Purpose |
|-----------|---------|
| [FirebaseRealtimeDbService](Services/FirebaseRealtimeDbService.cs) | Handles all Firebase Auth and RTDB REST API calls |
| [DashboardPageViewModel](DashboardViewModel/DashboardPageViewModel.cs) | Main dashboard data binding logic |
| [BudgetAlertService](Services/BudgetAlertService.cs) | Monitors and alerts on budget thresholds |
| [AISuggestionsService](Services/AISuggestionsService.cs) | Provides smart spending insights |
| [ServiceLocator](Services/ServiceLocator.cs) | Simple service resolution (legacy - prefer constructor DI) |

## Firebase Integration

The app uses Firebase REST APIs for:
- User authentication (signup, login, password reset)
- CRUD operations for expenses, profiles, and budgets
- Real-time data synchronization

**Database Structure**:
```
/expenses/{userId}/{expenseId}
├── id, userId, category, description, amount, date, icon, createdAt, modifiedAt

/users/{userId}
├── id, email, fullName, phoneNumber, location, totalExpenses, createdAt
```

See [FIREBASE_INTEGRATION_SUMMARY.md](FIREBASE_INTEGRATION_SUMMARY.md) for detailed API documentation.

## Development Guidelines

- **Dependency Injection**: Use constructor injection for new pages/viewmodels. `ServiceLocator` is for legacy code only.
- **Localization**: Store UI strings in [Resources/Strings/Strings.resx](Resources/Strings/Strings.resx), not hardcoded in XAML.
- **Service Registration**: Add new services in [MauiProgram.cs](MauiProgram.cs).

## Troubleshooting

| Issue | Solution |
|-------|----------|
| "API key is not valid" | Ensure Firebase Web API Key is set correctly in [MauiProgram.cs](MauiProgram.cs) |
| Service resolution errors at startup | Verify `ServiceLocator.Provider` is initialized in `MauiProgram.CreateMauiApp()` |
| Firebase RTDB access denied | Check RTDB security rules and ensure auth tokens are valid |
| Build fails on platform X | Ensure you have the required SDKs (Xcode for iOS, Android SDK for Android, etc.) |

## Documentation

- [FIREBASE_RTDB_SETUP.md](FIREBASE_RTDB_SETUP.md) — Firebase Realtime Database setup
- [FIREBASE_API_KEY_SETUP.md](FIREBASE_API_KEY_SETUP.md) — API key configuration
- [FIREBASE_EXPENSE_API.md](FIREBASE_EXPENSE_API.md) — Expense API endpoints
- [FIREBASE_PROFILE_API.md](FIREBASE_PROFILE_API.md) — Profile API endpoints
- [FIREBASE_INTEGRATION_SUMMARY.md](FIREBASE_INTEGRATION_SUMMARY.md) — Complete integration overview
- [ARCHITECTURE.md](ARCHITECTURE.md) — Application architecture and design patterns
- [CODING_STANDARDS.md](CODING_STANDARDS.md) — Code standards and best practices

## License

See [LICENSE](LICENSE) for details.
