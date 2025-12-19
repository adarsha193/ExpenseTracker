using System.Threading.Tasks;

namespace ExpenseTracker.Services
{
    /// <summary>
    /// Interface for user dialog service - Abstraction for UI alerts and confirmations
    /// Allows for mock implementations in testing and consistent UI interactions
    /// </summary>
    public interface IUserDialogService
    {
        /// <summary>
        /// Display a message alert dialog
        /// </summary>
        Task ShowAlertAsync(string title, string message, string cancel = "OK");

        /// <summary>
        /// Display a confirmation dialog and return user's choice
        /// </summary>
        Task<bool> ShowConfirmAsync(string title, string message, string accept = "Yes", string cancel = "No");

        /// <summary>
        /// Display a confirmation alert with Yes/No buttons
        /// </summary>
        Task<bool> ShowConfirmationAsync(string title, string message);

        /// <summary>
        /// Display an action sheet with multiple options
        /// </summary>
        Task<string?> ShowActionSheetAsync(string title, string cancel, string? destruction, string[] buttons);
    }
}

