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
        try
        {
            await Navigation.PushAsync(new LoginPage());
        }
        catch (Exception ex)
        {
            // here i have to display exception message in alert  
            // App resourece not found use hardcoded string
            await DisplayAlert("Error", ex.Message, "OK");

        } 
        
    }
}
