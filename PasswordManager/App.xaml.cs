namespace PasswordManager
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new LoginPage());
        }

        protected override async void OnStart()
        {
            base.OnStart();

            // Verificar si el usuario ya ha iniciado sesión
            string username = await SecureStorage.Default.GetAsync("current_user");

            if (!string.IsNullOrEmpty(username))
            {
                // Si el usuario ya ha iniciado sesión, redirigir a la página principal
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await MainPage.Navigation.PushAsync(new MainPage());
                });
            }
        }
    }
}
