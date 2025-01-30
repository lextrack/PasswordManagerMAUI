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
}