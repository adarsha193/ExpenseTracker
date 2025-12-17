using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace ExpenseTracker.Services
{
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
