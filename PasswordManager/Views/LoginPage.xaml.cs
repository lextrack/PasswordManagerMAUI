using PasswordManager.Views;

namespace PasswordManager;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        var buttons = this.GetVisualTreeDescendants().OfType<Button>();
        foreach (var button in buttons)
        {
            button.Scale = 1;
        }
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string username = UsernameEntry.Text;
        string password = PasswordEntry.Text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            await DisplayAlert("Error", "Please enter both username and password.", "OK");
            return;
        }

        bool isAuthenticated = await AuthenticateUser(username, password);

        if (isAuthenticated)
        {
            await SecureStorage.Default.SetAsync("current_user", username);
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Navigation.PushAsync(new MainPage());
            });
        }
        else
        {
            await DisplayAlert("Error", "Invalid username or password.", "OK");
        }
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            button.Scale = 1;
        }
        await Navigation.PushAsync(new RegisterPage());
    }

    private async Task<bool> AuthenticateUser(string username, string password)
    {
        string storedPassword = await SecureStorage.Default.GetAsync(username);
        return storedPassword == password;
    }

    private async void OnButtonPressed(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            await button.ScaleTo(0.95, 100);
        }
    }

    private async void OnButtonReleased(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            await button.ScaleTo(1, 100);
        }
    }
}