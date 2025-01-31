namespace PasswordManager;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
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
        string username = UsernameEntry.Text;
        string password = PasswordEntry.Text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            await DisplayAlert("Error", "Please enter both username and password.", "OK");
            return;
        }

        bool isRegistered = await RegisterUser(username, password);

        if (isRegistered)
        {
            await DisplayAlert("Success", "User registered successfully.", "OK");
        }
        else
        {
            await DisplayAlert("Error", "Registration failed. User may already exist.", "OK");
        }
    }

    private async Task<bool> AuthenticateUser(string username, string password)
    {
        string storedPassword = await SecureStorage.Default.GetAsync(username);

        return storedPassword == password;
    }

    private async Task<bool> RegisterUser(string username, string password)
    {
        string storedPassword = await SecureStorage.Default.GetAsync(username);

        if (storedPassword != null)
        {
            return false;
        }

        await SecureStorage.Default.SetAsync(username, password);


        string userPasswordsFile = Path.Combine(FileSystem.AppDataDirectory, $"passwords_{username}.json");
        if (!File.Exists(userPasswordsFile))
        {
            await File.WriteAllTextAsync(userPasswordsFile, "[]");
        }

        return true;
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