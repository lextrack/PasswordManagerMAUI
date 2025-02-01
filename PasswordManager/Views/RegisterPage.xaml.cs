namespace PasswordManager.Views;

public partial class RegisterPage : ContentPage
{
    public RegisterPage()
    {
        InitializeComponent();
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        string username = UsernameEntry.Text;
        string password = PasswordEntry.Text;
        string confirmPassword = ConfirmPasswordEntry.Text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
        {
            await DisplayAlert("Error", "Please fill in all fields.", "OK");
            return;
        }

        if (password != confirmPassword)
        {
            await DisplayAlert("Error", "Passwords do not match.", "OK");
            return;
        }

        if (password.Length < 6)
        {
            await DisplayAlert("Error", "Password must be at least 6 characters long.", "OK");
            return;
        }

        bool isRegistered = await RegisterUser(username, password);

        if (isRegistered)
        {
            await DisplayAlert("Success", "User registered successfully.", "OK");

            UsernameEntry.Text = string.Empty;
            PasswordEntry.Text = string.Empty;
            ConfirmPasswordEntry.Text = string.Empty;

            await Navigation.PopAsync();
        }
        else
        {
            await DisplayAlert("Error", "Registration failed. User may already exist.", "OK");
        }
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