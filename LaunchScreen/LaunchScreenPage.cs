using Microsoft.Maui.Controls;
using ExpenseTracker.Resources;

namespace ExpenseTracker;

public partial class LaunchScreenPage : ContentPage
{
    public LaunchScreenPage()
    {
        InitializeComponent();
    }

    private async void OnGetStartedClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(Strings.RouteLogin);
    }
}
