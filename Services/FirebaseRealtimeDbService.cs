using System.Net.Http.Json;
using System.Text.Json;
using ExpenseTracker.Models;

namespace ExpenseTracker.Services;

/// <summary>
/// Firebase Realtime Database authentication service
/// Integrates with your Firebase RTDB at: https://expanse-tracker-2a875-default-rtdb.firebaseio.com/
/// </summary>
public class FirebaseRealtimeDbService
{
    private readonly HttpClient _httpClient;
    private const string FirebaseRealtimeDbUrl = "https://expanse-tracker-2a875-default-rtdb.firebaseio.com";
    private const string FirebaseAuthUrl = "https://identitytoolkit.googleapis.com/v1";
    private string? _firebaseWebApiKey;

    public FirebaseRealtimeDbService(HttpClient httpClient, string? firebaseWebApiKey = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _firebaseWebApiKey = firebaseWebApiKey;
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    /// <summary>
    /// Set Firebase Web API Key (get from Firebase Console > Project Settings)
    /// </summary>
    public void SetFirebaseWebApiKey(string apiKey)
    {
        _firebaseWebApiKey = apiKey;
    }

    /// <summary>
    /// Login user with email and password using Firebase Auth + RTDB
    /// </summary>
    public async Task<AuthResponse> LoginAsync(string email, string password)
    {
        try
        {
            if (string.IsNullOrEmpty(_firebaseWebApiKey) || _firebaseWebApiKey == "YOUR_FIREBASE_WEB_API_KEY")
                return new AuthResponse { Success = false, Message = "Firebase API key is not valid. Please pass a valid API key in MauiProgram.cs" };

            // Authenticate with Firebase Auth
            var authRequest = new
            {
                email,
                password,
                returnSecureToken = true
            };

            var url = $"{FirebaseAuthUrl}/accounts:signInWithPassword?key={_firebaseWebApiKey}";
            var response = await _httpClient.PostAsJsonAsync(url, authRequest);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var authResult = JsonSerializer.Deserialize<FirebaseAuthResponse>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (authResult?.LocalId != null)
                {
                    // Fetch user data from RTDB
                    var userData = await GetUserDataFromDb(authResult.LocalId);
                    
                    return new AuthResponse
                    {
                        Success = true,
                        Message = "Login successful",
                        User = userData ?? new UserData
                        {
                            Id = authResult.LocalId,
                            Email = email,
                            FullName = "User",
                            CreatedAt = DateTime.UtcNow
                        },
                        Token = authResult.IdToken
                    };
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorMsg = ParseFirebaseError(errorContent);
                return new AuthResponse { Success = false, Message = errorMsg };
            }

            return new AuthResponse { Success = false, Message = "Login failed" };
        }
        catch (Exception ex)
        {
            return new AuthResponse { Success = false, Message = $"Login error: {ex.Message}" };
        }
    }

    /// <summary>
    /// Register new user with email, password and store in RTDB
    /// </summary>
    public async Task<AuthResponse> RegisterAsync(string fullName, string email, string password)
    {
        try
        {
            if (string.IsNullOrEmpty(_firebaseWebApiKey) || _firebaseWebApiKey == "YOUR_FIREBASE_WEB_API_KEY")
                return new AuthResponse { Success = false, Message = "Firebase API key is not valid. Please pass a valid API key in MauiProgram.cs" };

            // Create user with Firebase Auth
            var authRequest = new
            {
                email,
                password,
                displayName = fullName,
                returnSecureToken = true
            };

            var url = $"{FirebaseAuthUrl}/accounts:signUp?key={_firebaseWebApiKey}";
            var response = await _httpClient.PostAsJsonAsync(url, authRequest);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var authResult = JsonSerializer.Deserialize<FirebaseAuthResponse>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (authResult?.LocalId != null && authResult.IdToken != null)
                {
                    // Store user data in RTDB
                    var userData = new UserData
                    {
                        Id = authResult.LocalId,
                        FullName = fullName,
                        Email = email,
                        CreatedAt = DateTime.UtcNow,
                        ProfileImageUrl = null
                    };

                    await SaveUserDataToDb(authResult.LocalId, userData, authResult.IdToken);

                    return new AuthResponse
                    {
                        Success = true,
                        Message = "Account created successfully",
                        User = userData,
                        Token = authResult.IdToken
                    };
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorMsg = ParseFirebaseError(errorContent);
                return new AuthResponse { Success = false, Message = errorMsg };
            }

            return new AuthResponse { Success = false, Message = "Registration failed" };
        }
        catch (Exception ex)
        {
            return new AuthResponse { Success = false, Message = $"Registration error: {ex.Message}" };
        }
    }

    /// <summary>
    /// Request password reset email
    /// </summary>
    public async Task<AuthResponse> ForgotPasswordAsync(string email)
    {
        try
        {
            if (string.IsNullOrEmpty(_firebaseWebApiKey) || _firebaseWebApiKey == "YOUR_FIREBASE_WEB_API_KEY")
                return new AuthResponse { Success = false, Message = "Firebase API key is not valid. Please pass a valid API key in MauiProgram.cs" };

            var request = new
            {
                requestType = "PASSWORD_RESET",
                email
            };

            var url = $"{FirebaseAuthUrl}/accounts:sendOobCode?key={_firebaseWebApiKey}";
            var response = await _httpClient.PostAsJsonAsync(url, request);

            if (response.IsSuccessStatusCode)
            {
                return new AuthResponse
                {
                    Success = true,
                    Message = "Password reset email sent successfully. Check your email for reset link."
                };
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorMsg = ParseFirebaseError(errorContent);
                return new AuthResponse { Success = false, Message = errorMsg };
            }
        }
        catch (Exception ex)
        {
            return new AuthResponse { Success = false, Message = $"Error sending reset email: {ex.Message}" };
        }
    }

    /// <summary>
    /// Change password for authenticated user
    /// </summary>
    public async Task<AuthResponse> ChangePasswordAsync(string idToken, string newPassword)
    {
        try
        {
            if (string.IsNullOrEmpty(_firebaseWebApiKey) || _firebaseWebApiKey == "YOUR_FIREBASE_WEB_API_KEY")
                return new AuthResponse { Success = false, Message = "Firebase API key is not valid. Please pass a valid API key in MauiProgram.cs" };

            var request = new
            {
                idToken,
                password = newPassword,
                returnSecureToken = true
            };

            var url = $"{FirebaseAuthUrl}/accounts:update?key={_firebaseWebApiKey}";
            var response = await _httpClient.PostAsJsonAsync(url, request);

            if (response.IsSuccessStatusCode)
            {
                return new AuthResponse
                {
                    Success = true,
                    Message = "Password changed successfully"
                };
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorMsg = ParseFirebaseError(errorContent);
                return new AuthResponse { Success = false, Message = errorMsg };
            }
        }
        catch (Exception ex)
        {
            return new AuthResponse { Success = false, Message = $"Error changing password: {ex.Message}" };
        }
    }

    /// <summary>
    /// Verify reset password code and set new password
    /// </summary>
    public async Task<AuthResponse> ResetPasswordAsync(string resetCode, string newPassword)
    {
        try
        {
            if (string.IsNullOrEmpty(_firebaseWebApiKey) || _firebaseWebApiKey == "YOUR_FIREBASE_WEB_API_KEY")
                return new AuthResponse { Success = false, Message = "Firebase API key is not valid. Please pass a valid API key in MauiProgram.cs" };

            var request = new
            {
                oobCode = resetCode,
                newPassword
            };

            var url = $"{FirebaseAuthUrl}/accounts:resetPassword?key={_firebaseWebApiKey}";
            var response = await _httpClient.PostAsJsonAsync(url, request);

            if (response.IsSuccessStatusCode)
            {
                return new AuthResponse
                {
                    Success = true,
                    Message = "Password reset successfully. You can now login with your new password."
                };
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorMsg = ParseFirebaseError(errorContent);
                return new AuthResponse { Success = false, Message = errorMsg };
            }
        }
        catch (Exception ex)
        {
            return new AuthResponse { Success = false, Message = $"Error resetting password: {ex.Message}" };
        }
    }

    /// <summary>
    /// Logout - clears local credentials and optionally performs server-side operations
    /// Note: Firebase does not provide a simple REST logout. Typical logout means
    /// clearing local tokens. Admin revoke operations require server-side Admin SDK.
    /// </summary>
    public async Task<AuthResponse> LogoutAsync()
    {
        try
        {
            // No server-side logout via REST for Firebase Auth (revocation requires Admin SDK)
            // Clear any local persisted data if needed. The service itself doesn't manage SecureStorage,
            // but we provide this helper to return a standard response for UI flows.
            await Task.CompletedTask;

            return new AuthResponse
            {
                Success = true,
                Message = "Logged out successfully"
            };
        }
        catch (Exception ex)
        {
            return new AuthResponse { Success = false, Message = $"Logout error: {ex.Message}" };
        }
    }

    /// <summary>
    /// Save user data to Firebase Realtime Database
    /// Path: /users/{userId}
    /// </summary>
    private async Task<bool> SaveUserDataToDb(string userId, UserData userData, string idToken)
    {
        try
        {
            var dbData = new
            {
                id = userData.Id,
                fullName = userData.FullName,
                email = userData.Email,
                createdAt = userData.CreatedAt.ToString("o"),
                profileImageUrl = userData.ProfileImageUrl,
                lastLogin = DateTime.UtcNow.ToString("o")
            };

            var url = $"{FirebaseRealtimeDbUrl}/users/{userId}.json?auth={idToken}";
            var response = await _httpClient.PutAsJsonAsync(url, dbData);
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving user data: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Get user data from Firebase Realtime Database
    /// Path: /users/{userId}
    /// </summary>
    public async Task<UserData?> GetUserDataFromDb(string userId)
    {
        try
        {
            var url = $"{FirebaseRealtimeDbUrl}/users/{userId}.json";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                if (content != "null")
                {
                    var userData = JsonSerializer.Deserialize<UserData>(
                        content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );
                    return userData;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting user data: {ex.Message}");
        }

        return null;
    }

    /// <summary>
    /// Update user profile data in RTDB with complete information
    /// </summary>
    public async Task<AuthResponse> UpdateUserProfileAsync(string userId, string fullName, string? email, string? phone, string? location, decimal totalExpenses, string? profileImageUrl, string idToken)
    {
        try
        {
            var updates = new
            {
                fullName,
                email,
                phoneNumber = phone,
                location,
                totalExpenses,
                profileImageUrl,
                lastModified = DateTime.UtcNow.ToString("o")
            };

            var url = $"{FirebaseRealtimeDbUrl}/users/{userId}.json?auth={idToken}";
            var response = await _httpClient.PatchAsJsonAsync(url, updates);

            if (response.IsSuccessStatusCode)
            {
                return new AuthResponse 
                { 
                    Success = true, 
                    Message = "Profile updated successfully" 
                };
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorMsg = ParseFirebaseError(errorContent);
                return new AuthResponse 
                { 
                    Success = false, 
                    Message = errorMsg 
                };
            }
        }
        catch (Exception ex)
        {
            return new AuthResponse 
            { 
                Success = false, 
                Message = $"Error updating profile: {ex.Message}" 
            };
        }
    }

    /// <summary>
    /// Get all users (admin function - ensure proper security rules)
    /// </summary>
    public async Task<List<UserData>?> GetAllUsersAsync()
    {
        try
        {
            var url = $"{FirebaseRealtimeDbUrl}/users.json";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                if (content != "null")
                {
                    var users = JsonSerializer.Deserialize<Dictionary<string, UserData>>(
                        content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );
                    return users?.Values.ToList();
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting users: {ex.Message}");
        }

        return null;
    }

    /// <summary>
    /// Parse Firebase error responses
    /// </summary>
    private string ParseFirebaseError(string errorContent)
    {
        try
        {
            // Check for API key issues first
            if (errorContent.Contains("INVALID_API_KEY", StringComparison.OrdinalIgnoreCase) ||
                errorContent.Contains("API key not valid", StringComparison.OrdinalIgnoreCase) ||
                errorContent.Contains("API_KEY_INVALID", StringComparison.OrdinalIgnoreCase))
            {
                return "Firebase API key is not valid. Please pass a valid API key in MauiProgram.cs";
            }

            var jsonDoc = JsonDocument.Parse(errorContent);
            var root = jsonDoc.RootElement;

            if (root.TryGetProperty("error", out var errorObj))
            {
                if (errorObj.TryGetProperty("message", out var message))
                {
                    var errorMsg = message.GetString() ?? "";
                    var errorMsgUpper = errorMsg.ToUpper();
                    
                    // Check for API key error in message
                    if (errorMsgUpper.Contains("API") || errorMsgUpper.Contains("KEY"))
                    {
                        return "Firebase API key is not valid. Please pass a valid API key in MauiProgram.cs";
                    }
                    
                    // Map known error codes to friendly messages
                    switch (errorMsgUpper)
                    {
                        case "INVALID_PASSWORD":
                        case "INVALID_LOGIN_CREDENTIALS":
                            return "Invalid email or password";
                        case "INVALID_EMAIL":
                            return "Invalid email format";
                        case "EMAIL_NOT_FOUND":
                            return "Email not found";
                        case "EMAIL_EXISTS":
                            return "This email is already registered";
                        case "WEAK_PASSWORD":
                            return "Password is too weak. Use at least 6 characters";
                        case "USER_DISABLED":
                            return "This account has been disabled";
                        case "TOO_MANY_ATTEMPTS_LOGIN_RETRY_AFTER":
                            return "Too many failed login attempts. Try again later.";
                        default:
                            // Fallback: make the raw message human-friendly if possible
                            try
                            {
                                var cleaned = errorMsg.Replace('_', ' ').ToLowerInvariant();
                                if (!string.IsNullOrWhiteSpace(cleaned))
                                {
                                    return char.ToUpperInvariant(cleaned[0]) + cleaned.Substring(1);
                                }
                            }
                            catch { }
                            return "An error occurred. Please try again.";
                    }
                }
            }
        }
        catch { }

        return "An error occurred. Please try again";
    }

    #region Expense Management

    /// <summary>
    /// Add a new expense to Firebase RTDB
    /// </summary>
    public async Task<ExpenseResponse> AddExpenseAsync(string userId, string category, decimal amount, string description = "", string icon = "", string shopName = "", string address = "", string location = "")
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
                return new ExpenseResponse { Success = false, Message = "User ID is required" };

            var expenseId = Guid.NewGuid().ToString();
            var expense = new ExpenseData
            {
                Id = expenseId,
                UserId = userId,
                Category = category,
                Amount = amount,
                Description = description,
                Icon = icon,
                ShopName = shopName,
                Address = address,
                Location = location,
                Date = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            var url = $"{FirebaseRealtimeDbUrl}/expenses/{userId}/{expenseId}.json";
            var response = await _httpClient.PutAsJsonAsync(url, expense);

            if (response.IsSuccessStatusCode)
            {
                // Update user's total expenses
                await UpdateUserTotalExpenses(userId);

                return new ExpenseResponse
                {
                    Success = true,
                    Message = "Expense added successfully",
                    Expense = expense
                };
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorMsg = ParseFirebaseError(errorContent);
                return new ExpenseResponse { Success = false, Message = errorMsg };
            }
        }
        catch (Exception ex)
        {
            return new ExpenseResponse { Success = false, Message = $"Error adding expense: {ex.Message}" };
        }
    }

    /// <summary>
    /// Get all expenses for a user from Firebase RTDB
    /// </summary>
    public async Task<ExpenseResponse> GetUserExpensesAsync(string userId)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
                return new ExpenseResponse { Success = false, Message = "User ID is required" };

            var url = $"{FirebaseRealtimeDbUrl}/expenses/{userId}.json";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                
                // Handle empty result
                if (content == "null" || string.IsNullOrWhiteSpace(content))
                {
                    return new ExpenseResponse
                    {
                        Success = true,
                        Message = "No expenses found",
                        Expenses = new List<ExpenseData>()
                    };
                }

                var expenses = JsonSerializer.Deserialize<Dictionary<string, ExpenseData>>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                var expenseList = expenses?.Values.OrderByDescending(e => e.Date).ToList() ?? new List<ExpenseData>();

                return new ExpenseResponse
                {
                    Success = true,
                    Message = $"Found {expenseList.Count} expenses",
                    Expenses = expenseList
                };
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorMsg = ParseFirebaseError(errorContent);
                return new ExpenseResponse { Success = false, Message = errorMsg };
            }
        }
        catch (Exception ex)
        {
            return new ExpenseResponse { Success = false, Message = $"Error fetching expenses: {ex.Message}" };
        }
    }

    /// <summary>
    /// Get a specific expense by ID
    /// </summary>
    public async Task<ExpenseResponse> GetExpenseAsync(string userId, string expenseId)
    {
        try
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(expenseId))
                return new ExpenseResponse { Success = false, Message = "User ID and Expense ID are required" };

            var url = $"{FirebaseRealtimeDbUrl}/expenses/{userId}/{expenseId}.json";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                
                if (content == "null")
                    return new ExpenseResponse { Success = false, Message = "Expense not found" };

                var expense = JsonSerializer.Deserialize<ExpenseData>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                return new ExpenseResponse
                {
                    Success = true,
                    Message = "Expense retrieved successfully",
                    Expense = expense
                };
            }
            else
            {
                return new ExpenseResponse { Success = false, Message = "Expense not found" };
            }
        }
        catch (Exception ex)
        {
            return new ExpenseResponse { Success = false, Message = $"Error fetching expense: {ex.Message}" };
        }
    }

    /// <summary>
    /// Update an existing expense
    /// </summary>
    public async Task<ExpenseResponse> UpdateExpenseAsync(string userId, string expenseId, ExpenseData expenseData)
    {
        try
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(expenseId))
                return new ExpenseResponse { Success = false, Message = "User ID and Expense ID are required" };

            expenseData.ModifiedAt = DateTime.UtcNow;
            
            var url = $"{FirebaseRealtimeDbUrl}/expenses/{userId}/{expenseId}.json";
            var response = await _httpClient.PutAsJsonAsync(url, expenseData);

            if (response.IsSuccessStatusCode)
            {
                // Update user's total expenses
                await UpdateUserTotalExpenses(userId);

                return new ExpenseResponse
                {
                    Success = true,
                    Message = "Expense updated successfully",
                    Expense = expenseData
                };
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorMsg = ParseFirebaseError(errorContent);
                return new ExpenseResponse { Success = false, Message = errorMsg };
            }
        }
        catch (Exception ex)
        {
            return new ExpenseResponse { Success = false, Message = $"Error updating expense: {ex.Message}" };
        }
    }

    /// <summary>
    /// Delete an expense
    /// </summary>
    public async Task<ExpenseResponse> DeleteExpenseAsync(string userId, string expenseId)
    {
        try
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(expenseId))
                return new ExpenseResponse { Success = false, Message = "User ID and Expense ID are required" };

            var url = $"{FirebaseRealtimeDbUrl}/expenses/{userId}/{expenseId}.json";
            var response = await _httpClient.DeleteAsync(url);

            if (response.IsSuccessStatusCode)
            {
                // Update user's total expenses
                await UpdateUserTotalExpenses(userId);

                return new ExpenseResponse
                {
                    Success = true,
                    Message = "Expense deleted successfully"
                };
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorMsg = ParseFirebaseError(errorContent);
                return new ExpenseResponse { Success = false, Message = errorMsg };
            }
        }
        catch (Exception ex)
        {
            return new ExpenseResponse { Success = false, Message = $"Error deleting expense: {ex.Message}" };
        }
    }

    /// <summary>
    /// Get total expenses summary for a user
    /// </summary>
    public async Task<ExpenseResponse> GetExpensesSummaryAsync(string userId)
    {
        try
        {
            var response = await GetUserExpensesAsync(userId);
            
            if (!response.Success || response.Expenses == null)
                return new ExpenseResponse { Success = false, Message = "Failed to fetch expenses" };

            decimal totalAmount = response.Expenses.Sum(e => e.Amount);
            var categorySummary = response.Expenses
                .GroupBy(e => e.Category)
                .OrderByDescending(g => g.Sum(e => e.Amount))
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));

            return new ExpenseResponse
            {
                Success = true,
                Message = $"Total expenses: ₹{totalAmount}",
                Expenses = response.Expenses
            };
        }
        catch (Exception ex)
        {
            return new ExpenseResponse { Success = false, Message = $"Error fetching summary: {ex.Message}" };
        }
    }

    /// <summary>
    /// Update user total expenses in their profile
    /// </summary>
    private async Task UpdateUserTotalExpenses(string userId)
    {
        try
        {
            var response = await GetUserExpensesAsync(userId);
            if (response.Success && response.Expenses != null)
            {
                decimal total = response.Expenses.Sum(e => e.Amount);
                
                var updateData = new { totalExpenses = total };
                var url = $"{FirebaseRealtimeDbUrl}/users/{userId}/totalExpenses.json";
                await _httpClient.PutAsJsonAsync(url, total);
            }
        }
        catch { }
    }

    #endregion

    #region Salary Management

    /// <summary>
    /// Save or update user's salary information
    /// </summary>
    public async Task<BasicResponse> SaveSalaryAsync(string userId, SalaryModel salary)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
                return new BasicResponse { Success = false, Message = "User ID is required" };

            if (salary == null || salary.Amount <= 0)
                return new BasicResponse { Success = false, Message = "Invalid salary amount" };

            salary.UserId = userId;
            salary.UpdatedAt = DateTime.UtcNow;
            if (salary.CreatedAt == default)
                salary.CreatedAt = DateTime.UtcNow;

            if (string.IsNullOrEmpty(salary.Id))
                salary.Id = Guid.NewGuid().ToString();

            var url = $"{FirebaseRealtimeDbUrl}/users/{userId}/salary/{salary.Id}.json";
            var response = await _httpClient.PutAsJsonAsync(url, salary);

            if (response.IsSuccessStatusCode)
                return new BasicResponse { Success = true, Message = "Salary saved successfully" };

            return new BasicResponse { Success = false, Message = "Failed to save salary" };
        }
        catch (Exception ex)
        {
            return new BasicResponse { Success = false, Message = $"Error saving salary: {ex.Message}" };
        }
    }

    /// <summary>
    /// Get user's salary information
    /// </summary>
    public async Task<SalaryResponse> GetSalaryAsync(string userId)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
                return new SalaryResponse { Success = false, Message = "User ID is required" };

            var url = $"{FirebaseRealtimeDbUrl}/users/{userId}/salary.json";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                if (content == "null")
                    return new SalaryResponse { Success = true, Message = "No salary data found", Salary = null };

                var salaries = JsonSerializer.Deserialize<Dictionary<string, SalaryModel>>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                // Return the most recent salary
                var mostRecent = salaries?.Values.OrderByDescending(s => s.CreatedAt).FirstOrDefault();
                return new SalaryResponse { Success = true, Message = "Salary fetched", Salary = mostRecent };
            }

            return new SalaryResponse { Success = false, Message = "Failed to fetch salary" };
        }
        catch (Exception ex)
        {
            return new SalaryResponse { Success = false, Message = $"Error fetching salary: {ex.Message}" };
        }
    }

    #endregion

    #region Investment Management

    /// <summary>
    /// Save or update investment
    /// </summary>
    public async Task<BasicResponse> SaveInvestmentAsync(string userId, InvestmentModel investment)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
                return new BasicResponse { Success = false, Message = "User ID is required" };

            if (investment == null || investment.Amount <= 0)
                return new BasicResponse { Success = false, Message = "Invalid investment amount" };

            investment.UserId = userId;
            investment.UpdatedAt = DateTime.UtcNow;
            if (investment.CreatedAt == default)
                investment.CreatedAt = DateTime.UtcNow;

            if (string.IsNullOrEmpty(investment.Id))
                investment.Id = Guid.NewGuid().ToString();

            var url = $"{FirebaseRealtimeDbUrl}/users/{userId}/investments/{investment.Id}.json";
            var response = await _httpClient.PutAsJsonAsync(url, investment);

            if (response.IsSuccessStatusCode)
                return new BasicResponse { Success = true, Message = "Investment saved successfully" };

            return new BasicResponse { Success = false, Message = "Failed to save investment" };
        }
        catch (Exception ex)
        {
            return new BasicResponse { Success = false, Message = $"Error saving investment: {ex.Message}" };
        }
    }

    /// <summary>
    /// Get all user's investments
    /// </summary>
    public async Task<InvestmentResponse> GetInvestmentsAsync(string userId)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
                return new InvestmentResponse { Success = false, Message = "User ID is required", Investments = new List<InvestmentModel>() };

            var url = $"{FirebaseRealtimeDbUrl}/users/{userId}/investments.json";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                if (content == "null")
                    return new InvestmentResponse { Success = true, Message = "No investments found", Investments = new List<InvestmentModel>() };

                var investments = JsonSerializer.Deserialize<Dictionary<string, InvestmentModel>>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                var investmentList = investments?.Values.OrderByDescending(i => i.CreatedAt).ToList() ?? new List<InvestmentModel>();
                return new InvestmentResponse { Success = true, Message = "Investments fetched", Investments = investmentList };
            }

            return new InvestmentResponse { Success = false, Message = "Failed to fetch investments", Investments = new List<InvestmentModel>() };
        }
        catch (Exception ex)
        {
            return new InvestmentResponse { Success = false, Message = $"Error fetching investments: {ex.Message}", Investments = new List<InvestmentModel>() };
        }
    }

    /// <summary>
    /// Delete an investment
    /// </summary>
    public async Task<BasicResponse> DeleteInvestmentAsync(string userId, string investmentId)
    {
        try
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(investmentId))
                return new BasicResponse { Success = false, Message = "User ID and Investment ID are required" };

            var url = $"{FirebaseRealtimeDbUrl}/users/{userId}/investments/{investmentId}.json";
            var response = await _httpClient.DeleteAsync(url);

            if (response.IsSuccessStatusCode)
                return new BasicResponse { Success = true, Message = "Investment deleted successfully" };

            return new BasicResponse { Success = false, Message = "Failed to delete investment" };
        }
        catch (Exception ex)
        {
            return new BasicResponse { Success = false, Message = $"Error deleting investment: {ex.Message}" };
        }
    }

    #endregion

    #region Budget Management

    /// <summary>
    /// Save or update monthly budget for a category
    /// </summary>
    public async Task<BasicResponse> SaveBudgetAsync(string userId, MonthlyBudgetModel budget)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
                return new BasicResponse { Success = false, Message = "User ID is required" };

            if (budget == null || budget.AllocatedAmount <= 0)
                return new BasicResponse { Success = false, Message = "Invalid budget amount" };

            budget.UserId = userId;
            budget.UpdatedAt = DateTime.UtcNow;
            if (budget.CreatedAt == default)
                budget.CreatedAt = DateTime.UtcNow;

            if (string.IsNullOrEmpty(budget.Id))
                budget.Id = Guid.NewGuid().ToString();

            var url = $"{FirebaseRealtimeDbUrl}/users/{userId}/budgets/{budget.Id}.json";
            var response = await _httpClient.PutAsJsonAsync(url, budget);

            if (response.IsSuccessStatusCode)
                return new BasicResponse { Success = true, Message = "Budget saved successfully" };

            return new BasicResponse { Success = false, Message = "Failed to save budget" };
        }
        catch (Exception ex)
        {
            return new BasicResponse { Success = false, Message = $"Error saving budget: {ex.Message}" };
        }
    }

    /// <summary>
    /// Get all budgets for user for a specific month/year
    /// </summary>
    public async Task<BudgetResponse> GetBudgetsAsync(string userId, int month, int year)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
                return new BudgetResponse { Success = false, Message = "User ID is required", Budgets = new List<MonthlyBudgetModel>() };

            var url = $"{FirebaseRealtimeDbUrl}/users/{userId}/budgets.json";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                if (content == "null")
                    return new BudgetResponse { Success = true, Message = "No budgets found", Budgets = new List<MonthlyBudgetModel>() };

                var budgets = JsonSerializer.Deserialize<Dictionary<string, MonthlyBudgetModel>>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                var budgetList = budgets?.Values
                    .Where(b => b.Month == month && b.Year == year)
                    .OrderBy(b => b.Category)
                    .ToList() ?? new List<MonthlyBudgetModel>();

                return new BudgetResponse { Success = true, Message = "Budgets fetched", Budgets = budgetList };
            }

            return new BudgetResponse { Success = false, Message = "Failed to fetch budgets", Budgets = new List<MonthlyBudgetModel>() };
        }
        catch (Exception ex)
        {
            return new BudgetResponse { Success = false, Message = $"Error fetching budgets: {ex.Message}", Budgets = new List<MonthlyBudgetModel>() };
        }
    }

    /// <summary>
    /// Delete a budget
    /// </summary>
    public async Task<BasicResponse> DeleteBudgetAsync(string userId, string budgetId)
    {
        try
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(budgetId))
                return new BasicResponse { Success = false, Message = "User ID and Budget ID are required" };

            var url = $"{FirebaseRealtimeDbUrl}/users/{userId}/budgets/{budgetId}.json";
            var response = await _httpClient.DeleteAsync(url);

            if (response.IsSuccessStatusCode)
                return new BasicResponse { Success = true, Message = "Budget deleted successfully" };

            return new BasicResponse { Success = false, Message = "Failed to delete budget" };
        }
        catch (Exception ex)
        {
            return new BasicResponse { Success = false, Message = $"Error deleting budget: {ex.Message}" };
        }
    }

    #endregion

    #region Budget Alert Service

    /// <summary>
    /// Check if user has exceeded budget for a category in current month
    /// </summary>
    public async Task<BudgetAlertResponse> CheckBudgetAlertAsync(string userId, string category)
    {
        try
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(category))
                return new BudgetAlertResponse { HasExceeded = false, Message = "Invalid parameters" };

            var now = DateTime.Now;
            var budgetResponse = await GetBudgetsAsync(userId, now.Month, now.Year);
            
            if (!budgetResponse.Success || budgetResponse.Budgets == null)
                return new BudgetAlertResponse { HasExceeded = false, Message = "No budget found for this month" };

            var categoryBudget = budgetResponse.Budgets.FirstOrDefault(b => b.Category == category);
            if (categoryBudget == null)
                return new BudgetAlertResponse { HasExceeded = false, Message = "No budget allocated for this category" };

            // Get expenses for this category in current month
            var expensesResponse = await GetUserExpensesAsync(userId);
            if (!expensesResponse.Success || expensesResponse.Expenses == null)
                return new BudgetAlertResponse { HasExceeded = false, Message = "Could not fetch expenses" };

            var categoryExpenses = expensesResponse.Expenses
                .Where(e => e.Category == category && e.Date.Month == now.Month && e.Date.Year == now.Year)
                .Sum(e => e.Amount);

            var percentageUsed = (categoryExpenses / categoryBudget.AllocatedAmount) * 100;
            var hasExceeded = categoryExpenses > categoryBudget.AllocatedAmount;

            return new BudgetAlertResponse
            {
                HasExceeded = hasExceeded,
                Category = category,
                BudgetAmount = categoryBudget.AllocatedAmount,
                CurrentSpending = categoryExpenses,
                Overage = hasExceeded ? categoryExpenses - categoryBudget.AllocatedAmount : 0,
                PercentageUsed = percentageUsed,
                Message = hasExceeded 
                    ? $"{category} budget exceeded! Spent ₹{categoryExpenses:F2} of ₹{categoryBudget.AllocatedAmount:F2} (Overage: ₹{categoryExpenses - categoryBudget.AllocatedAmount:F2})"
                    : $"{category} budget: ₹{categoryExpenses:F2} / ₹{categoryBudget.AllocatedAmount:F2} ({percentageUsed:F1}% used)"
            };
        }
        catch (Exception ex)
        {
            return new BudgetAlertResponse { HasExceeded = false, Message = $"Error checking budget: {ex.Message}" };
        }
    }

    #endregion

    /// <summary>
    /// Firebase Auth Response model
    /// </summary>
    private class FirebaseAuthResponse
    {
        public string? IdToken { get; set; }
        public string? Email { get; set; }
        public string? RefreshToken { get; set; }
        public string? ExpiresIn { get; set; }
        public string? LocalId { get; set; }
        public string? DisplayName { get; set; }
        public bool Registered { get; set; }
    }

    /// <summary>
    /// Get user salary information
    /// </summary>
    public async Task<SalaryResponse> GetUserSalaryAsync(string userId)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
                return new SalaryResponse { Success = false, Message = "User ID is required" };

            var url = $"{FirebaseRealtimeDbUrl}/users/{userId}/salary.json";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                
                if (content == "null" || string.IsNullOrWhiteSpace(content))
                {
                    return new SalaryResponse
                    {
                        Success = true,
                        Message = "No salary found",
                        Salary = null
                    };
                }

                // Try to deserialize as dictionary (multiple salaries) first
                try
                {
                    var salaries = JsonSerializer.Deserialize<Dictionary<string, SalaryModel>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    var mostRecent = salaries?.Values.OrderByDescending(s => s.CreatedAt).FirstOrDefault();
                    return new SalaryResponse { Success = true, Message = "Salary retrieved", Salary = mostRecent };
                }
                catch
                {
                    // If not a dictionary, deserialize as single object
                    var salary = JsonSerializer.Deserialize<SalaryModel>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return new SalaryResponse { Success = true, Message = "Salary retrieved", Salary = salary };
                }
            }
            else
            {
                return new SalaryResponse { Success = false, Message = "Salary not found" };
            }
        }
        catch (Exception ex)
        {
            return new SalaryResponse { Success = false, Message = $"Error: {ex.Message}" };
        }
    }

    /// <summary>
    /// Get user investments
    /// </summary>
    public async Task<InvestmentResponse> GetUserInvestmentsAsync(string userId)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
                return new InvestmentResponse { Success = false, Message = "User ID is required", Investments = new List<InvestmentModel>() };

            var url = $"{FirebaseRealtimeDbUrl}/users/{userId}/investments.json";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                
                if (content == "null" || string.IsNullOrWhiteSpace(content))
                {
                    return new InvestmentResponse
                    {
                        Success = true,
                        Message = "No investments found",
                        Investments = new List<InvestmentModel>()
                    };
                }

                var investments = JsonSerializer.Deserialize<Dictionary<string, InvestmentModel>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                var investmentList = investments?.Values.ToList() ?? new List<InvestmentModel>();
                return new InvestmentResponse { Success = true, Message = "Investments retrieved", Investments = investmentList };
            }
            else
            {
                return new InvestmentResponse { Success = false, Message = "Investments not found" };
            }
        }
        catch (Exception ex)
        {
            return new InvestmentResponse { Success = false, Message = $"Error: {ex.Message}" };
        }
    }

    /// <summary>
    /// Get user budgets
    /// </summary>
    public async Task<BudgetResponse> GetUserBudgetsAsync(string userId)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
                return new BudgetResponse { Success = false, Message = "User ID is required", Budgets = new List<MonthlyBudgetModel>() };

            var url = $"{FirebaseRealtimeDbUrl}/users/{userId}/budgets.json";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                
                if (content == "null" || string.IsNullOrWhiteSpace(content))
                {
                    return new BudgetResponse
                    {
                        Success = true,
                        Message = "No budgets found",
                        Budgets = new List<MonthlyBudgetModel>()
                    };
                }

                var budgets = JsonSerializer.Deserialize<Dictionary<string, MonthlyBudgetModel>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                var budgetList = budgets?.Values.ToList() ?? new List<MonthlyBudgetModel>();
                return new BudgetResponse { Success = true, Message = "Budgets retrieved", Budgets = budgetList };
            }
            else
            {
                return new BudgetResponse { Success = false, Message = "Budgets not found" };
            }
        }
        catch (Exception ex)
        {
            return new BudgetResponse { Success = false, Message = $"Error: {ex.Message}" };
        }
    }
}

/// <summary>
/// Firebase RTDB paths reference:
/// - /users/{userId} - User profile data
///   - id: string
///   - fullName: string
///   - email: string
///   - createdAt: timestamp
///   - profileImageUrl: string (optional)
///   - lastLogin: timestamp
///   - lastModified: timestamp
/// </summary>
/// 