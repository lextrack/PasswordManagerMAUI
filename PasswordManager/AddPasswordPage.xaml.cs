using PasswordManager.Model;

namespace PasswordManager;

public partial class AddPasswordPage : ContentPage
{
    private readonly MainPage _mainPage;

    public AddPasswordPage(MainPage mainPage)
    {
        InitializeComponent();
        _mainPage = mainPage;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var newPassword = new PasswordModel
        {
            Service = ServiceEntry.Text,
            Username = UsernameEntry.Text,
            Password = PasswordEntry.Text
        };

        _mainPage.Passwords.Add(newPassword);
        (_mainPage as MainPage)?.SavePasswords();
        await Navigation.PopAsync();
    }

    private async void OnButtonPressed(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            await button.ScaleTo(0.95, 100); // Reduce el tama�o del bot�n al presionarlo
        }
    }

    private async void OnButtonReleased(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            await button.ScaleTo(1, 100); // Restaura el tama�o al soltarlo
        }
    }

}