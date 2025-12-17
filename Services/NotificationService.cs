namespace ExpenseTracker.Services
{
    /// <summary>
    /// Service for handling local notifications
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
        /// Initialize notification channel (required for Android 8.0+)
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
        /// Send budget alert notification
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
        /// Send critical overspending alert
        /// </summary>
        public async Task SendCriticalAlertAsync(string category, decimal overage)
        {
            var title = $"🚨 {category} Budget Exceeded!";
            var message = $"You've exceeded the budget by ₹{overage:N2}. Please review your spending.";
            await SendBudgetAlertAsync(title, message);
        }

        /// <summary>
        /// Send warning when approaching budget limit (90%)
        /// </summary>
        public async Task SendWarningAsync(string category, decimal percentageUsed)
        {
            var title = $"⚠️ {category} Budget Warning";
            var message = $"You've used {percentageUsed:F1}% of your {category} budget.";
            await SendBudgetAlertAsync(title, message);
        }

        /// <summary>
        /// Send daily budget summary
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
        /// Schedule daily notification check (if supported by platform)
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
