using PasswordManager.Model;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;

namespace PasswordManager
{
    public partial class MainPage : ContentPage
    {
        private string _storageFile;
        public ObservableCollection<PasswordModel> Passwords { get; set; } = new ObservableCollection<PasswordModel>();

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            Debug.WriteLine("OnAppearing started");

            if (!await IsUserAuthenticatedAsync())
            {
                Debug.WriteLine("User not authenticated. Redirecting to LoginPage...");
                await Navigation.PushAsync(new LoginPage());
                return;
            }

            string username = await SecureStorage.Default.GetAsync("current_user");
            Debug.WriteLine($"Current user: {username}");

            _storageFile = Path.Combine(FileSystem.AppDataDirectory, $"passwords_{username}.json");
            Debug.WriteLine($"Storage file: {_storageFile}");

            await LoadPasswordsAsync();
            Debug.WriteLine("Passwords loaded successfully.");
        }

        private async Task<bool> IsUserAuthenticatedAsync()
        {
            string username = await SecureStorage.Default.GetAsync("current_user");
            return !string.IsNullOrEmpty(username);
        }

        private async Task LoadPasswordsAsync()
        {
            try
            {
                if (File.Exists(_storageFile))
                {
                    string json = await File.ReadAllTextAsync(_storageFile);
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
                await DisplayAlert("Error", $"Error loading passwords: {ex.Message}", "OK");
            }
        }

        public async Task SavePasswordsAsync()
        {
            try
            {
                string json = JsonSerializer.Serialize(Passwords.ToList());
                await File.WriteAllTextAsync(_storageFile, json);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error saving passwords: {ex.Message}", "OK");
            }
        }

        private async void OnAddPasswordClicked(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                try
                {
                    await button.ScaleTo(0.95, 100);
                    await button.ScaleTo(1, 100);
                    await Navigation.PushAsync(new AddPasswordPage(this));
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Error navigating to AddPasswordPage: {ex.Message}", "OK");
                }
            }
        }

        private async void OnPasswordSelected(object sender, EventArgs e)
        {
            PasswordModel selectedPassword = null;

            if (e is TappedEventArgs tappedArgs && tappedArgs.Parameter is PasswordModel password)
            {
                selectedPassword = password;
            }

            if (selectedPassword == null && sender is Element element)
            {
                selectedPassword = element.BindingContext as PasswordModel;
            }

            if (selectedPassword != null)
            {
                await DisplayAlert("Password Details",
                             $"Service: {selectedPassword.Service}\nUsername: {selectedPassword.Username}\nPassword: {selectedPassword.Password}",
                             "OK");
            }
        }
        private async void OnDeletePasswordClicked(object sender, EventArgs e)
        {
            if (sender is ImageButton imageButton && imageButton.BindingContext is PasswordModel password)
            {
                bool isConfirmed = await DisplayAlert("Confirm Deletion",
                                                       "Are you sure you want to delete this password?",
                                                       "Yes",
                                                       "No");

                if (isConfirmed)
                {
                    var frame = imageButton.Parent as Frame ??
                                imageButton.Parent.Parent as Frame;

                    if (frame != null)
                    {
                        await frame.ScaleTo(0, 200);
                        await frame.FadeTo(0, 200);
                    }

                    var passwords = Passwords.ToList();
                    passwords.Remove(password);

                    Passwords.Clear();
                    foreach (var p in passwords)
                    {
                        Passwords.Add(p);
                    }

                    SavePasswordsAsync();
                }
            }
        }

        private async void OnCopyPasswordClicked(object sender, EventArgs e)
        {
            if (sender is ImageButton imageButton && imageButton.BindingContext is PasswordModel password)
            {
                await Clipboard.SetTextAsync(password.Password);
                await DisplayAlert("Copied", "Password copied to clipboard", "OK");
            }
        }
        private async void OnSaveBackupClicked(object sender, EventArgs e)
        {
            try
            {
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

                string json = JsonSerializer.Serialize(Passwords);

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

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

                await File.WriteAllTextAsync(backupPath, json);
                await DisplayAlert("Backup", $"Backup saved successfully at: {backupPath}", "OK");

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
}
