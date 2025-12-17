# Firebase Profile Management API

## Overview

The `FirebaseRealtimeDbService` includes methods to manage user profiles in Firebase Realtime Database.

## Available Profile Methods

### 1. Update User Profile

# Firebase Profile API (short)

This file documents the profile-related methods in `FirebaseRealtimeDbService` and short examples.

Available methods
- `GetUserDataFromDb(string userId)` — returns `UserData` or `null`.
- `UpdateUserProfileAsync(string userId, string fullName, string? profileImageUrl, string idToken)` — updates profile fields.

UserData (key fields)
- `Id`, `FullName`, `Email`, `CreatedAt`, `ProfileImageUrl?`, `PhoneNumber?`, `Location?`, `TotalExpenses`.

Database path
- Profiles stored under `/users/{userId}` in the RTDB.

Minimal usage
- Load profile:
```csharp
var user = await _authService.GetUserDataFromDb(userId);
if (user != null) { NameLabel.Text = user.FullName; }
```
- Update profile:
```csharp
var res = await _authService.UpdateUserProfileAsync(userId, name, photoUrl, idToken);
if (!res.Success) await DisplayAlert("Error", res.Message, "OK");
```

Security notes
- Keep `idToken` and sensitive data in `SecureStorage`.
- Enforce write rules in Firebase security rules so users can only modify their own node.

Tips
- Cache profile data locally when possible.
- Update only changed fields to reduce writes.

See also: `FIREBASE_EXPENSE_API.md`, `FIREBASE_RTDB_SETUP.md`, `FIREBASE_API_KEY_SETUP.md`.
if (userData != null)

{
