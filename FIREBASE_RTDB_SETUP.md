# Firebase Realtime Database Authentication Setup Guide

## Overview
This guide explains how to set up and use Firebase Realtime Database authentication in your ExpenseTracker app with your Firebase RTDB URL: `https://expanse-tracker-2a875-default-rtdb.firebaseio.com/`

## Quick Start

### 1. Get Your Firebase Web API Key
1. Go to [Firebase Console](https://console.firebase.google.com)
2. Select your project: **expanse-tracker-2a875**
3. Go to **Project Settings** (gear icon)
4. Navigate to **Service Accounts** tab
5. Copy your **Web API Key** from the REST API section

### 2. Configure the API Key in Your App
Update `MauiProgram.cs` line 20:

```csharp
const string firebaseWebApiKey = "YOUR_FIREBASE_WEB_API_KEY"; // Replace with your actual key
```

### 3. Build and Run
```bash
dotnet build -f net10.0-ios -c Debug
dotnet build -f net10.0-android -c Debug
```

## Firebase RTDB Structure

Your Firebase Realtime Database uses the following structure:

```
expanse-tracker-2a875-default-rtdb.firebaseio.com/
├── users/
│   ├── {userId}/
│   │   ├── id: string
│   │   ├── fullName: string
│   │   ├── email: string
│   │   ├── createdAt: timestamp
│   │   ├── profileImageUrl: string (optional)
│   │   ├── lastLogin: timestamp
│   │   └── lastModified: timestamp
```

## API Methods

### Authentication

#### Login
```csharp
var result = await _authService.LoginAsync("user@example.com", "password123");

if (result.Success)
{
    var userId = result.User.Id;
    var token = result.Token;
    // User data automatically loaded from RTDB
}
```

**Endpoint:** Firebase Auth API  
**Path:** `/accounts:signInWithPassword`  
**Returns:** User data from RTDB + Auth token

---

#### Sign Up / Register
```csharp
var result = await _authService.RegisterAsync("John Doe", "john@example.com", "password123");

if (result.Success)
{
    // User created in Firebase Auth
    // User profile saved to RTDB at: /users/{userId}
    var userId = result.User.Id;
    var token = result.Token;
}
```

**Endpoint:** Firebase Auth API  
**Path:** `/accounts:signUp` → `/users/{userId}` (RTDB)  
**Returns:** User data + Auth token

---

#### Forgot Password
```csharp
var result = await _authService.ForgotPasswordAsync("user@example.com");

if (result.Success)
{
    // Email sent with password reset link
}
```

**Endpoint:** Firebase Auth API  
**Path:** `/accounts:sendOobCode`  
**Returns:** Success message

---

#### Change Password (for logged-in users)
```csharp
var token = await SecureStorage.GetAsync("auth_token");
var result = await _authService.ChangePasswordAsync(token, "newPassword123");

if (result.Success)
{
    // Password changed successfully
}
```

**Endpoint:** Firebase Auth API  
**Path:** `/accounts:update`  
**Returns:** Success message

---

#### Reset Password (with reset code from email)
```csharp
var result = await _authService.ResetPasswordAsync(resetCodeFromEmail, "newPassword123");

if (result.Success)
{

# Firebase Realtime Database

This document describes the minimal RTDB structure used by the app.

Base URL
- RTDB URL: https://expanse-tracker-2a875-default-rtdb.firebaseio.com

Primary paths
- /users/{userId} — stores user profile and metadata (id, fullName, email, createdAt, totalExpenses).
- /expenses/{userId}/{expenseId} — stores expense records for a user.

Notes
- The app updates `totalExpenses` under `/users/{userId}` when expenses change.
- Security rules should ensure users can only read/write their own records (use Firebase rules).

## Error Handling

Common error responses:

| Error | Message |
|-------|---------|
| `INVALID_PASSWORD` | Invalid email or password |
| `EMAIL_NOT_FOUND` | Email not found |
| `EMAIL_EXISTS` | This email is already registered |
| `WEAK_PASSWORD` | Password is too weak. Use at least 6 characters |
| `USER_DISABLED` | This account has been disabled |
| `TOO_MANY_ATTEMPTS_LOGIN_RETRY_AFTER` | Too many failed login attempts. Try again later. |

## Security Rules for Firebase RTDB

Recommended security rules to add to your Firebase Console:

```json
{
  "rules": {
    "users": {
      "$uid": {
        ".read": "$uid === auth.uid",
        ".write": "$uid === auth.uid",
        ".validate": "newData.hasChildren(['id', 'fullName', 'email', 'createdAt'])"
      }
    }
  }
}
```

## Code Examples

### Complete Login Flow
```csharp
public partial class LoginPage : ContentPage
{
    private readonly FirebaseRealtimeDbService _authService;

    public LoginPage()
    {
        InitializeComponent();
        _authService = MauiProgram.CreateMauiApp()
            .Services.GetRequiredService<FirebaseRealtimeDbService>();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var result = await _authService.LoginAsync(
            EmailEntry.Text,
            PasswordEntry.Text
        );

        if (result.Success && result.User != null)
        {
            // Store credentials securely
            await SecureStorage.SetAsync("auth_token", result.Token);
            await SecureStorage.SetAsync("user_id", result.User.Id);
            await SecureStorage.SetAsync("user_email", result.User.Email);

            // Navigate to dashboard
            Application.Current.Windows[0].Page = new DashboardShell();
        }
        else
        {
            await DisplayAlertAsync("Error", result.Message, "OK");
        }
    }
}
```

### Complete Sign Up Flow
```csharp
private async void OnSignUpClicked(object sender, EventArgs e)
{
    var result = await _authService.RegisterAsync(
        NameEntry.Text,      // fullName
        EmailEntry.Text,      // email
        PasswordEntry.Text    // password
    );

    if (result.Success && result.User != null)
    {
        // User created in Firebase Auth
        // User profile automatically saved to RTDB
        
        await SecureStorage.SetAsync("auth_token", result.Token);
        await SecureStorage.SetAsync("user_id", result.User.Id);

        await DisplayAlertAsync("Success", "Account created!", "OK");
        await Shell.Current.GoToAsync("///login");
    }
    else
    {
        await DisplayAlertAsync("Error", result.Message, "OK");
    }
}
```

### Logout
```csharp
private async void OnLogout()
{
    // Clear stored credentials
    SecureStorage.Remove("auth_token");
    SecureStorage.Remove("user_id");
    SecureStorage.Remove("user_email");

    // Navigate back to login
    Application.Current.Windows[0].Page = new LoginPage();
}
```

## Troubleshooting

### "Firebase API key not configured"
- You haven't set the Web API Key in `MauiProgram.cs`
- Get your key from Firebase Console > Project Settings

### "Invalid email or password"
- Email/password combination doesn't match any user
- Make sure you're using the correct credentials

### "This email is already registered"
- User already has an account with this email
- Try logging in instead of signing up

### "Password is too weak"
- Firebase requires passwords with at least 6 characters
- Use stronger passwords with mixed characters

### "Too many failed login attempts"
- You've made too many incorrect login attempts
- Wait a few minutes before trying again

## Files Modified/Created

- ✅ `Services/FirebaseRealtimeDbService.cs` (NEW)
- ✅ `LaunchScreen/LoginPage.cs` (UPDATED)
- ✅ `LaunchScreen/RegistrationPage.cs` (UPDATED)
- ✅ `LaunchScreen/ForgotPassword.cs` (UPDATED)
- ✅ `MauiProgram.cs` (UPDATED)

## Next Steps

1. Configure your Firebase Web API Key in `MauiProgram.cs`
2. Set up Firebase Realtime Database Security Rules
3. Test the login/signup flow
4. Implement additional features like profile editing
5. Add expense tracking functionality

## Resources

- [Firebase Auth REST API](https://firebase.google.com/docs/reference/rest/auth)
- [Firebase Realtime Database REST API](https://firebase.google.com/docs/database/rest)
- [Firebase Console](https://console.firebase.google.com)
