using System.Windows.Input;

namespace PasswordManager.Views;

public partial class About : ContentPage
{
    public ICommand OpenGitHubCommand { get; }
    public ICommand OpenPayPalCommand { get; }

    public About()
    {
        InitializeComponent();
        OpenGitHubCommand = new Command(async () => await OpenGitHub());
        OpenPayPalCommand = new Command(async () => await OpenPayPal());
        BindingContext = this;
    }

    private async Task OpenGitHub()
    {
        try
        {
            Uri uri = new Uri("https://github.com/lextrack/PasswordManagerMAUI");
            await Browser.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Could not open the link.", "OK");
        }
    }

    private async Task OpenPayPal()
    {
        try
        {
            Uri uri = new Uri("https://www.paypal.me/lextrack");
            await Browser.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Could not open the link.", "OK");
        }
    }
}