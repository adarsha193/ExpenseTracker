using Microsoft.Maui.Controls;
using ExpenseTracker.Services;
using ExpenseTracker.Resources;

namespace ExpenseTracker;

public partial class AddExpensePage : ContentPage
{
    private readonly FirebaseRealtimeDbService _authService;

    public AddExpensePage()
    {
        InitializeComponent();
        _authService = ServiceLocator.GetRequiredService<FirebaseRealtimeDbService>();
    }

    private async void OnSaveExpenseClicked(object sender, EventArgs e)
    {
        // Validate inputs
        if (CategoryPicker.SelectedIndex == -1)
        {
            await DisplayAlertAsync(Strings.ValidationError, Strings.PleaseSelectCategory, Strings.OKButton);
            return;
        }

        if (string.IsNullOrWhiteSpace(AmountEntry.Text))
        {
            await DisplayAlertAsync(Strings.ValidationError, Strings.PleaseEnterAmount, Strings.OKButton);
            return;
        }

        if (!decimal.TryParse(AmountEntry.Text, out decimal amount) || amount <= 0)
        {
            await DisplayAlertAsync(Strings.ValidationError, Strings.PleaseEnterValidAmount, Strings.OKButton);
            return;
        }

        // Get form data
        string? selectedItem = CategoryPicker.SelectedItem as string;
        string category = selectedItem ?? "Other";
        string description = DescriptionEditor.Text ?? string.Empty;
        string shopName = ShopNameEntry.Text ?? string.Empty;
        string address = AddressEntry.Text ?? string.Empty;
        string location = LocationEntry.Text ?? string.Empty;

        // Get user ID from secure storage
        var userId = await SecureStorage.GetAsync("user_id");
        if (string.IsNullOrEmpty(userId))
        {
            await DisplayAlertAsync(Strings.ErrorTitle, Strings.UserNotFound, Strings.OKButton);
            return;
        }

        // Show loading indicator
        AmountEntry.IsEnabled = false;
        CategoryPicker.IsEnabled = false;
        DescriptionEditor.IsEnabled = false;
        ShopNameEntry.IsEnabled = false;
        AddressEntry.IsEnabled = false;
        LocationEntry.IsEnabled = false;

        try
        {
            // Get icon for category
            string icon = GetIconForCategory(category);

            // Save to Firebase
            var result = await _authService.AddExpenseAsync(
                userId: userId,
                category: category,
                amount: amount,
                description: description,
                icon: icon,
                shopName: shopName,
                address: address,
                location: location
            );

            if (result.Success)
            {
                await DisplayAlertAsync(Strings.SuccessTitle, string.Format(Strings.ExpenseSavedSuccess, amount, category), Strings.OKButton);

                // Clear form
                AmountEntry.Text = string.Empty;
                CategoryPicker.SelectedIndex = -1;
                DescriptionEditor.Text = string.Empty;
                ShopNameEntry.Text = string.Empty;
                AddressEntry.Text = string.Empty;
                LocationEntry.Text = string.Empty;

                // Navigate back to dashboard
                if (Shell.Current != null)
                {
                    await Shell.Current.GoToAsync(Strings.RouteBack);
                }
            }
            else
            {
                await DisplayAlertAsync(Strings.ErrorTitle, result.Message, Strings.OKButton);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(Strings.ErrorTitle, string.Format(Strings.FailedToSaveExpense, ex.Message), Strings.OKButton);
        }
        finally
        {
            AmountEntry.IsEnabled = true;
            CategoryPicker.IsEnabled = true;
            DescriptionEditor.IsEnabled = true;
            ShopNameEntry.IsEnabled = true;
            AddressEntry.IsEnabled = true;
            LocationEntry.IsEnabled = true;
        }
    }

    private string GetIconForCategory(string category)
    {
        return category.ToLower() switch
        {
            "food" => "🍔",
            "travel" => "✈️",
            "entertainment" => "🎬",
            "shopping" => "🛍️",
            "health" => "⚕️",
            "healthcare" => "⚕️",
            "education" => "📚",
            "utilities" => "💡",
            "bills" => "📋",
            _ => "📝"
        };
    }
}
