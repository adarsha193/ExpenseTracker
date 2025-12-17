# Firebase API Key Setup

## Issue
The app is currently showing: **"API key is not valid please pass valid api key error"**

This happens because the Firebase Web API Key in `MauiProgram.cs` is still set to the placeholder: `"YOUR_FIREBASE_WEB_API_KEY"`

## Solution

### Step 1: Get Your Firebase Web API Key

1. Go to [Firebase Console](https://console.firebase.google.com/)
2. Select your project: **expanse-tracker-2a875**
3. Click on **Project Settings** (gear icon) in the top-left
4. Go to the **"General"** tab
5. Scroll down to find **"Your apps"** section
6. Look for your **Web App** configuration
7. Find the `apiKey` value in the config
   - It looks something like: `AIzaSyD...` (a long string starting with `AIzaSy`)

### Step 2: Update MauiProgram.cs

1. Open `/Users/adarshahebbar/Documents/Maui.net/ExpenseTracker/MauiProgram.cs`
2. Find line 22:
   ```csharp
   const string firebaseWebApiKey = "YOUR_FIREBASE_WEB_API_KEY";
   ```
3. Replace it with your actual API key:
   ```csharp
   const string firebaseWebApiKey = "AIzaSyD..."; // Replace with your actual key
   ```

### Alternative: Set via Environment Variable

If you prefer not to hardcode the API key, you can use an environment variable:

```csharp
const string firebaseWebApiKey = Environment.GetEnvironmentVariable("FIREBASE_API_KEY") ?? "YOUR_FIREBASE_WEB_API_KEY";
```

Then set the `FIREBASE_API_KEY` environment variable on your system.

## Error Messages

The app now provides better error messages for API key issues:

- **"Firebase API key is not valid. Please pass a valid API key in MauiProgram.cs"** - The API key is either empty, placeholder text, or invalid
- **"API_KEY_INVALID"** - Firebase rejected the API key (invalid key format)

## Test After Setup

1. Rebuild the app: `dotnet build`
2. Try logging in with valid Firebase credentials
3. If successful, you'll be able to access the dashboard
4. If you see the error again, double-check:
   - API key is copied correctly (no extra spaces)
   - You're using the **Web API Key**, not the **Server API Key**
   - The key starts with `AIzaSy` or similar
   - The Firebase project has **Authentication** enabled
   - The Firebase project has **Realtime Database** enabled

## Firebase Realtime Database URL

Your RTDB is correctly configured at:
- **URL**: `https://expanse-tracker-2a875-default-rtdb.firebaseio.com/`
- **Authentication**: Firebase Auth REST API
- **Data Storage**: `/users/{userId}` path in RTDB

## Security Note

**Do not** commit your API key to version control. Consider:
1. Using environment variables (recommended)
2. Using a local configuration file (add to `.gitignore`)
3. Using Azure Key Vault or similar secure storage for production

## Troubleshooting

### Still getting "API key is not valid" error?

1. **Verify API Key Format**: The key should be 40+ characters
   ```
   Valid: AIzaSyD_abc123XYZ...
   Invalid: YOUR_FIREBASE_WEB_API_KEY
   ```

2. **Check Firebase Console**:
   - Go to Authentication > Sign-in method
   - Ensure Email/Password provider is enabled
   
3. **Check Realtime Database**:
   - Go to Realtime Database
   - Ensure database is created and accessible
   - Check Rules tab - should allow read/write for authenticated users:
   ```json
   {
     "rules": {
       "users": {
         "$uid": {
           ".read": "$uid === auth.uid",
           ".write": "$uid === auth.uid"
         }
       }
     }
   }
   ```

4. **Clear App Cache**: Sometimes cached data causes issues
   - iOS: Reinstall the app
   - Android: Settings > Apps > ExpenseTracker > Storage > Clear Cache

## Files Modified

- `MauiProgram.cs` - Added better API key validation
- `Services/FirebaseRealtimeDbService.cs` - Enhanced error messages for API key issues
