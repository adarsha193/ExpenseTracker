using System.Threading.Tasks;

namespace ExpenseTracker.Services
{
    /// <summary>
    /// Abstraction for showing alerts, confirmations, and action sheets to the user.
    ///
    /// Implementations can target platform dialogs or in-app UI, and can be
    /// mocked in tests to avoid UI dependencies.
    /// </summary>
    public interface IUserDialogService
    {
        /// <summary>
        /// Show a simple alert with a single dismiss button.
        /// </summary>
        Task ShowAlertAsync(string title, string message, string cancel = "OK");

        /// <summary>
        /// Show a confirmation dialog and return true if the user accepts.
        /// </summary>
        Task<bool> ShowConfirmAsync(string title, string message, string accept = "Yes", string cancel = "No");

        /// <summary>
        /// Convenience method for a Yes/No confirmation dialog.
        /// </summary>
        Task<bool> ShowConfirmationAsync(string title, string message);

        /// <summary>
        /// Show an action sheet with multiple options and return the selected value.
        /// </summary>
        Task<string?> ShowActionSheetAsync(string title, string cancel, string? destruction, string[] buttons);
    }
}

