namespace ExpenseTracker.Services
{
    /// <summary>
    /// Handles local notifications and simple alert delivery for the app.
    ///
    /// This service sends budget-related alerts (warnings, critical overspend
    /// notices, and daily summaries) and uses platform APIs where available.
    /// It falls back to the app's dialog service when a system notification
    /// isn't appropriate or supported.
    ///
    /// Integration notes:
    /// - Triggered by `BudgetAlertService` when thresholds are reached.
    /// - Daily summaries are scheduled by the app and delivered through this service.
    /// - Users can control notification preferences from settings.
    /// </summary>
    public class NotificationService
    {
        private const string NotificationChannelId = "budget_alerts";
        private const string NotificationChannelName = "Budget Alerts";

        public NotificationService()
        {
            InitializeNotificationChannel();
        }

        /// <summary>
        /// Create the Android notification channel (Android 8.0+).
        /// </summary>
        private void InitializeNotificationChannel()
        {
#if __ANDROID__
            try
            {
                var channel = new Android.App.NotificationChannel(
                    NotificationChannelId,
                    NotificationChannelName,
                    Android.App.NotificationImportance.High)
                {
                    Description = "Notifications for budget alerts"
                };

                var notificationManager = Android.App.Application.Context.GetSystemService(
                    Android.Content.Context.NotificationService) as Android.App.NotificationManager;
                
                notificationManager?.CreateNotificationChannel(channel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing notification channel: {ex.Message}");
            }
#endif
        }

        /// <summary>
        /// Show a simple budget alert to the user. Uses the dialog service when
        /// a platform notification API is not available.
        /// </summary>
        public async Task SendBudgetAlertAsync(string title, string message)
        {
            try
            {
                // Use MAUI notification API
                if (Application.Current?.MainPage != null)
                {
                    var dialogService = ServiceLocator.Provider?.GetService<IUserDialogService>();
                    if (dialogService != null)
                        await dialogService.ShowAlertAsync(title, message, "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error sending notification: {ex.Message}");
            }
        }

        /// <summary>
        /// Notify the user about a critical overspend in a category.
        /// </summary>
        public async Task SendCriticalAlertAsync(string category, decimal overage)
        {
            var title = $"🚨 {category} Budget Exceeded!";
            var message = $"You've exceeded the budget by ₹{overage:N2}. Please review your spending.";
            await SendBudgetAlertAsync(title, message);
        }

        /// <summary>
        /// Send a warning when a category reaches a high percentage of its budget.
        /// </summary>
        public async Task SendWarningAsync(string category, decimal percentageUsed)
        {
            var title = $"⚠️ {category} Budget Warning";
            var message = $"You've used {percentageUsed:F1}% of your {category} budget.";
            await SendBudgetAlertAsync(title, message);
        }

        /// <summary>
        /// Deliver a daily summary listing categories that exceeded their budgets.
        /// </summary>
        public async Task SendDailySummaryAsync(List<(string Category, decimal Spent, decimal Budget)> summary)
        {
            try
            {
                var exceededCount = summary.Count(s => s.Spent > s.Budget);
                
                if (exceededCount > 0)
                {
                    var title = $"Daily Budget Summary - {exceededCount} Category Alert(s)";
                    var categories = summary
                        .Where(s => s.Spent > s.Budget)
                        .Select(s => $"• {s.Category}: ₹{s.Spent:N2} / ₹{s.Budget:N2}")
                        .ToList();
                    
                    var message = string.Join("\n", categories);
                    await SendBudgetAlertAsync(title, message);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error sending daily summary: {ex.Message}");
            }
        }

        /// <summary>
        /// Schedule a daily callback to run notification checks (platform-dependent).
        /// </summary>
        public async Task ScheduleDailyCheckAsync(Action callback, TimeSpan scheduleTime)
        {
            try
            {
                // This would use platform-specific scheduling
                // For now, we can use a simple timer approach
                var delay = CalculateNextOccurrence(scheduleTime);
                
                _ = Task.Delay(delay).ContinueWith(async _ =>
                {
                    callback?.Invoke();
                    // Reschedule for next day
                    await ScheduleDailyCheckAsync(callback, scheduleTime);
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error scheduling daily check: {ex.Message}");
            }
        }

        /// <summary>
        /// Calculate time until next occurrence of specified time
        /// </summary>
        private TimeSpan CalculateNextOccurrence(TimeSpan targetTime)
        {
            var now = DateTime.Now;
            var today = now.Date.Add(targetTime);
            
            if (today > now)
                return today - now;
            else
                return today.AddDays(1) - now;
        }
    }
}
