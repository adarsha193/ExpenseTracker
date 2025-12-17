# ExpenseTracker

ExpenseTracker is a small .NET MAUI app that demonstrates Firebase Authentication and Realtime Database for tracking personal expenses.

Quick start
- 1) Open `MauiProgram.cs` and set your Firebase Web API key (search for `firebaseWebApiKey`).
- 2) Restore and build:

```bash
dotnet restore
dotnet build -f net10.0
```

- 3) Run on a platform (example macOS/iOS simulator):

```bash
dotnet build -f net10.0 -r iossimulator-arm64
```

Project structure (high level)
- `Services/FirebaseRealtimeDbService.cs` — Firebase Auth + RTDB REST integrations (login, register, forgot/reset password, expenses, profile).
- `Resources/Strings/` — localization resource files (resx) used by XAML.
- `Dashboard/`, `LaunchScreen/`, `ExpensePages/`, `SliderView/` — UI pages and related code-behind / small viewmodels.

Where to look next
- Authentication & DB: `Services/FirebaseRealtimeDbService.cs`
- Example ViewModel: `DashboardViewModel/DashboardPageViewModel.cs`
- Localization resources: `Resources/Strings/Strings.resx`

Maintainers notes
- Prefer constructor DI for new pages/viewmodels; `Services/ServiceLocator.cs` is a small interim helper but avoid adding new callers that rely on it.
- UI strings are being migrated to `Resources/Strings/Strings.resx`. Move literal `DisplayAlert` texts and XAML strings to resx keys as you update screens.

Troubleshooting
- If you see service-resolution errors on startup, ensure `ServiceLocator.Provider` is set in `MauiProgram.cs` before resolving services.
- For Firebase permissions issues, check your RTDB rules and ensure tokens stored in `SecureStorage` are valid.

Related docs
- `FIREBASE_RTDB_SETUP.md`, `FIREBASE_API_KEY_SETUP.md`, `FIREBASE_EXPENSE_API.md`, `FIREBASE_PROFILE_API.md`

Changelog
- 2025-12-13: README rewritten for clarity and quick-start steps.
