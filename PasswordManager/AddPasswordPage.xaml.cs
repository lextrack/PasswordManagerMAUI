using PasswordManager.Model;

namespace PasswordManager;

public partial class AddPasswordPage : ContentPage
{
    private readonly MainPage _mainPage;

    public AddPasswordPage(MainPage mainPage)
    {
        InitializeComponent();
        _mainPage = mainPage;
        BindingContext = this;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var newPassword = new PasswordModel
        {
            Service = ServiceEntry.Text,
            Username = UsernameEntry.Text,
            Password = PasswordEntry.Text
        };

        if (_mainPage != null && _mainPage.Passwords != null)
        {
            _mainPage.Passwords.Add(newPassword);
            _mainPage.SavePasswordsAsync();
        }

        await Navigation.PopAsync();
    }

    private async void OnButtonPressed(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            await button.ScaleTo(0.95, 100); // Reduce el tamaño del botón al presionarlo
        }
    }

    private async void OnButtonReleased(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            await button.ScaleTo(1, 100); // Restaura el tamaño al soltarlo
        }
    }

}