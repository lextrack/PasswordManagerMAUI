using PasswordManager.Model;

namespace PasswordManager;

public partial class AddPasswordPage : ContentPage
{
    private readonly MainPage _mainPage;

    public AddPasswordPage(MainPage mainPage)
    {
        InitializeComponent();
        _mainPage = mainPage; // Guarda la referencia de MainPage
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var newPassword = new PasswordModel
        {
            Service = ServiceEntry.Text,
            Username = UsernameEntry.Text,
            Password = PasswordEntry.Text
        };

        // Agrega la nueva contraseņa a la lista en MainPage
        _mainPage.Passwords.Add(newPassword);

        await Navigation.PopAsync();
    }
}