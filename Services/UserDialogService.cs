using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace ExpenseTracker.Services
{
    /// <summary>
    /// User Dialog Service - UI Alerts & Confirmations Feature
    /// 
    /// FEATURES:
    /// - Displays alert dialogs to the user
    /// - Shows confirmation dialogs with Yes/No options
    /// - Thread-safe execution on main UI thread
    /// - Centralized UI interaction for consistent UX
    /// 
    /// USAGE:
    /// - Display error messages
    /// - Show success confirmations
    /// - Request user confirmation for actions
    /// - Display warnings and information messages
    /// 
    /// IMPLEMENTATION:
    /// - Uses MAUI MainPage.DisplayAlert() for alerts and confirmations
    /// - All calls executed on main thread for thread safety
    /// - Graceful error handling if MainPage is not available
    /// - Works across all platforms (iOS, Android, macOS, Windows)
    /// </summary>
    public class UserDialogService : IUserDialogService
    {
        public async Task ShowAlertAsync(string title, string message, string cancel = "OK")
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    var page = Application.Current?.MainPage;
                    if (page != null)
                        await page.DisplayAlert(title, message, cancel);
                });
            }
            catch { }
        }

        public async Task<bool> ShowConfirmAsync(string title, string message, string accept = "Yes", string cancel = "No")
        {
            try
            {
                return await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    var page = Application.Current?.MainPage;
                    if (page != null)
                        return await page.DisplayAlert(title, message, accept, cancel);
                    return false;
                });
            }
            catch { return false; }
        }
    }
}
