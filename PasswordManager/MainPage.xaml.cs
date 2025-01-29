using PasswordManager.Model;
using PasswordManager.Utils;
using System.Collections.ObjectModel;

namespace PasswordManager
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<PasswordModel> Passwords { get; set; } = new ObservableCollection<PasswordModel>();

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this; // Establece el BindingContext
        }

        private async void OnAddPasswordClicked(object sender, EventArgs e)
        {
            var addPasswordPage = new AddPasswordPage(this); // Pasa la instancia de MainPage
            await Navigation.PushAsync(addPasswordPage);
        }

        private void OnPasswordSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem is PasswordModel selectedPassword)
            {
                // Mostrar la contraseña real en un alert
                DisplayAlert("Password Details",
                             $"Service: {selectedPassword.Service}\nUsername: {selectedPassword.Username}\nPassword: {selectedPassword.Password}",
                             "OK");
            }

            // Deseleccionar el elemento para que el usuario pueda seleccionarlo nuevamente
            PasswordsList.SelectedItem = null;
        }
    }

}
