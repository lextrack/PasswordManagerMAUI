using PasswordManager.Model;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace PasswordManager
{
    public partial class MainPage : ContentPage
    {
        private readonly string _storageFile;
        public ObservableCollection<PasswordModel> Passwords { get; set; } = new ObservableCollection<PasswordModel>();

        public MainPage()
        {
            InitializeComponent();
            _storageFile = Path.Combine(FileSystem.AppDataDirectory, "passwords.json");
            BindingContext = this;
            LoadPasswords(); // Cargar passwords al iniciar
        }

        private void LoadPasswords()
        {
            try
            {
                if (File.Exists(_storageFile))
                {
                    string json = File.ReadAllText(_storageFile);
                    var loadedPasswords = JsonSerializer.Deserialize<List<PasswordModel>>(json);
                    Passwords.Clear();
                    foreach (var password in loadedPasswords)
                    {
                        Passwords.Add(password);
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejo de errores de carga
                DisplayAlert("Error", $"Error loading passwords: {ex.Message}", "OK");
            }
        }

        public void SavePasswords()
        {
            try
            {
                string json = JsonSerializer.Serialize(Passwords.ToList());
                File.WriteAllText(_storageFile, json);
            }
            catch (Exception ex)
            {
                // Manejo de errores de guardado
                DisplayAlert("Error", $"Error saving passwords: {ex.Message}", "OK");
            }
        }

        private async void OnAddPasswordClicked(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                await button.ScaleTo(0.95, 100);
                await button.ScaleTo(1, 100);
                await Navigation.PushAsync(new AddPasswordPage(this));
            }
        }

        private void OnPasswordSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is PasswordModel selectedPassword)
            {
                DisplayAlert("Password Details",
                             $"Service: {selectedPassword.Service}\nUsername: {selectedPassword.Username}\nPassword: {selectedPassword.Password}",
                             "OK");

                // Limpiar la selección
                if (sender is CollectionView collectionView)
                {
                    collectionView.SelectedItem = null;
                }
            }
        }

        private async void OnButtonPressed(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                await button.ScaleTo(0.95, 100); // Reduce tamaño al 95% en 100ms
            }
        }

        private async void OnButtonReleased(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                await button.ScaleTo(1, 100); // Vuelve a su tamaño normal
            }
        }

        // Efecto de eliminación con fade out y reducción de tamaño
        private async void OnDeletePasswordClicked(object sender, EventArgs e)
        {
            if (sender is ImageButton imageButton && imageButton.BindingContext is PasswordModel password)
            {
                var frame = (Frame)imageButton.Parent.Parent;
                await frame.ScaleTo(0, 200);
                await frame.FadeTo(0, 200);

                if (PasswordsList.ItemsSource is IList<PasswordModel> passwords)
                {
                    passwords.Remove(password);
                    PasswordsList.ItemsSource = null;
                    PasswordsList.ItemsSource = passwords;
                    SavePasswords(); // Guardar después de eliminar
                }
            }
        }


        private async void OnSaveBackupClicked(object sender, EventArgs e)
        {
            try
            {
                // Verificar y solicitar permisos en Android
                if (DeviceInfo.Platform == DevicePlatform.Android)
                {
                    var status = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
                    if (status != PermissionStatus.Granted)
                    {
                        status = await Permissions.RequestAsync<Permissions.StorageWrite>();
                        if (status != PermissionStatus.Granted)
                        {
                            await DisplayAlert("Error", "Storage permission is required to save backup", "OK");
                            return;
                        }
                    }
                }

                // Serializar la lista de contraseñas a JSON
                string json = JsonSerializer.Serialize(Passwords);

                // Obtener la fecha y hora actual
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

                // Crear el nombre del archivo con la fecha y hora
                string fileName = $"passwords_backup_{timestamp}.json";
                string backupPath;

                if (DeviceInfo.Platform == DevicePlatform.WinUI)
                {
                    string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    backupPath = Path.Combine(documentsPath, fileName);
                }
                else if (DeviceInfo.Platform == DevicePlatform.Android)
                {
                    var downloadsPath = Path.Combine(FileSystem.AppDataDirectory, "Download");
                    if (!Directory.Exists(downloadsPath))
                    {
                        Directory.CreateDirectory(downloadsPath);
                    }
                    backupPath = Path.Combine(downloadsPath, fileName);
                }
                else
                {
                    backupPath = Path.Combine(FileSystem.AppDataDirectory, fileName);
                }

                // Guardar el archivo
                await File.WriteAllTextAsync(backupPath, json);
                await DisplayAlert("Backup", $"Backup saved successfully at: {backupPath}", "OK");

                // Preguntar si el usuario desea compartir el archivo
                bool shareFile = await DisplayAlert("Share Backup", "Do you want to share the backup file?", "Yes", "No");
                if (shareFile)
                {
                    await ShareFile(backupPath);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to save backup: {ex.Message}", "OK");
            }
        }

        private async Task ShareFile(string filePath)
        {
            try
            {
                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = "Share Backup File",
                    File = new ShareFile(filePath)
                });
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to share file: {ex.Message}", "OK");
            }
        }

        private async void OnLoadBackupClicked(object sender, EventArgs e)
        {
            try
            {
                if (DeviceInfo.Platform == DevicePlatform.Android)
                {
                    var status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
                    if (status != PermissionStatus.Granted)
                    {
                        status = await Permissions.RequestAsync<Permissions.StorageRead>();
                        if (status != PermissionStatus.Granted)
                        {
                            await DisplayAlert("Error", "Storage permission is required to load backup", "OK");
                            return;
                        }
                    }
                }

                var customFileType = new FilePickerFileType(
                    new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                { DevicePlatform.Android, new[] { "application/json" } },
                { DevicePlatform.WinUI, new[] { ".json" } }
                    });

                var options = new PickOptions
                {
                    PickerTitle = "Please select a backup file",
                    FileTypes = customFileType
                };

                var fileResult = await FilePicker.Default.PickAsync(options);
                if (fileResult != null)
                {
                    string json = await File.ReadAllTextAsync(fileResult.FullPath);
                    var loadedPasswords = JsonSerializer.Deserialize<ObservableCollection<PasswordModel>>(json);

                    Passwords.Clear();
                    foreach (var password in loadedPasswords)
                    {
                        Passwords.Add(password);
                    }

                    await DisplayAlert("Backup", "Passwords loaded successfully!", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load backup: {ex.Message}", "OK");
            }
        }
    }
}
