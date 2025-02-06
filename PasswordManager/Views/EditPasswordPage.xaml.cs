using PasswordManager.Model;

namespace PasswordManager.Views;

public partial class EditPasswordPage : ContentPage
{
    private readonly MainPage _mainPage;
    private readonly PasswordModel _originalPassword;
    private readonly PasswordModel _editingPassword;

    public EditPasswordPage(MainPage mainPage, PasswordModel password)
    {
        InitializeComponent();
        _mainPage = mainPage;
        _originalPassword = password;
        _editingPassword = new PasswordModel
        {
            Service = password.Service,
            Username = password.Username,
            Password = password.Password
        };
        BindingContext = _editingPassword;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_editingPassword.Service) ||
            string.IsNullOrWhiteSpace(_editingPassword.Username) ||
            string.IsNullOrWhiteSpace(_editingPassword.Password))
        {
            await DisplayAlert("Error", "All fields are required", "OK");
            return;
        }

        // Update the original password with new values
        _originalPassword.Service = _editingPassword.Service;
        _originalPassword.Username = _editingPassword.Username;
        _originalPassword.Password = _editingPassword.Password;

        // Save changes to storage
        await _mainPage.SavePasswordsAsync();

        await Navigation.PopAsync();
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private void OnTogglePasswordVisibilityClicked(object sender, EventArgs e)
    {
        PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
        TogglePasswordButton.Text = PasswordEntry.IsPassword ? "👁" : "🔒";
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