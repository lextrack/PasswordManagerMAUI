using PasswordManager.Model;
using PasswordManager.Utils;
using PasswordManager.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;

namespace PasswordManager
{
    public partial class MainPage : ContentPage
    {
        private string _storageFile;
        public ObservableCollection<PasswordModel> Passwords { get; set; } = new ObservableCollection<PasswordModel>();
        private ObservableCollection<PasswordModel> _allPasswords = new ObservableCollection<PasswordModel>();
        private string _currentSearchText = string.Empty;

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            Debug.WriteLine("OnAppearing started");

            await EncryptionHelper.InitializeAsync();

            if (!await IsUserAuthenticatedAsync())
            {
                Debug.WriteLine("User not authenticated. Redirecting to LoginPage...");
                await Navigation.PushAsync(new LoginPage());
                return;
            }

            LoadingGrid.IsVisible = true;

            try
            {
                string username = await SecureStorage.Default.GetAsync("current_user");
                Debug.WriteLine($"Current user: {username}");

                _storageFile = Path.Combine(FileSystem.AppDataDirectory, $"passwords_{username}.json");
                Debug.WriteLine($"Storage file: {_storageFile}");

                await LoadPasswordsAsync();
                Debug.WriteLine("Passwords loaded successfully.");
            }
            finally
            {
                LoadingGrid.IsVisible = false;
            }
        }

        private async Task<bool> IsUserAuthenticatedAsync()
        {
            string username = await SecureStorage.Default.GetAsync("current_user");
            return !string.IsNullOrEmpty(username);
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            SecureStorage.Default.Remove("current_user");
            await Navigation.PushAsync(new LoginPage());
        }

        private async void OnAboutClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new About());
        }

        private async Task LoadPasswordsAsync()
        {
            try
            {
                await Task.Delay(300);

                if (File.Exists(_storageFile))
                {
                    string json = await File.ReadAllTextAsync(_storageFile);
                    var loadedPasswords = JsonSerializer.Deserialize<List<PasswordModel>>(json);

                    _allPasswords.Clear();
                    Passwords.Clear();

                    foreach (var password in loadedPasswords)
                    {
                        password.Password = EncryptionHelper.Decrypt(password.Password);
                        _allPasswords.Add(password);
                        Passwords.Add(password);
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error loading passwords: {ex.Message}", "OK");
            }
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            _currentSearchText = e.NewTextValue?.ToLower() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(_currentSearchText))
            {
                // If search is empty, show all passwords
                Passwords.Clear();
                foreach (var password in _allPasswords)
                {
                    Passwords.Add(password);
                }
                return;
            }

            // Filter passwords based on search text
            var filteredPasswords = _allPasswords
                .Where(p => p.Service.ToLower().Contains(_currentSearchText))
                .ToList();

            // Update the observable collection
            Passwords.Clear();
            foreach (var password in filteredPasswords)
            {
                Passwords.Add(password);
            }
        }


        public async Task SavePasswordsAsync()
        {
            try
            {
                var encryptedPasswords = new List<PasswordModel>();

                foreach (var password in Passwords)
                {
                    // Encrypt password using EncryptionHelper
                    string encryptedPassword = EncryptionHelper.Encrypt(password.Password);
                    encryptedPasswords.Add(new PasswordModel
                    {
                        Service = password.Service,
                        Username = password.Username,
                        Password = encryptedPassword
                    });
                }

                // Serialize and store encrypted passwords
                string json = JsonSerializer.Serialize(encryptedPasswords);
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
                    var addPasswordPage = new AddPasswordPage(this);
                    await Navigation.PushAsync(addPasswordPage);

                    // When returning from AddPasswordPage, update _allPasswords
                    _allPasswords = new ObservableCollection<PasswordModel>(Passwords);
                    // Reapply current search filter
                    OnSearchTextChanged(null, new TextChangedEventArgs(string.Empty, _currentSearchText));
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

                    _allPasswords.Remove(password);
                    Passwords.Remove(password);

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

                    int addedCount = 0;
                    foreach (var newPassword in loadedPasswords)
                    {
                        bool exists = Passwords.Any(p =>
                            p.Service.Equals(newPassword.Service, StringComparison.OrdinalIgnoreCase) &&
                            p.Username.Equals(newPassword.Username, StringComparison.OrdinalIgnoreCase));

                        if (!exists)
                        {
                            Passwords.Add(newPassword);
                            _allPasswords.Add(newPassword);
                            addedCount++;
                        }
                    }

                    await SavePasswordsAsync();

                    string message = $"Backup loaded successfully!\nAdded {addedCount} new passwords.";
                    if (addedCount < loadedPasswords.Count)
                    {
                        message += $"\nSkipped {loadedPasswords.Count - addedCount} duplicate entries.";
                    }
                    await DisplayAlert("Backup", message, "OK");
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

        protected override bool OnBackButtonPressed()
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                bool shouldLogout = await DisplayAlert(
                    "Confirm Logout",
                    "If you go back, your session will be closed, are you sure?",
                    "Yes",
                    "No"
                );

                if (shouldLogout)
                {
                    SecureStorage.Default.Remove("current_user");
                    await Navigation.PopToRootAsync();
                    await Navigation.PushAsync(new LoginPage());
                }
            });

            return true;
        }

        private async void OnEditPasswordClicked(object sender, EventArgs e)
        {
            if (sender is ImageButton imageButton && imageButton.BindingContext is PasswordModel password)
            {
                await Navigation.PushAsync(new EditPasswordPage(this, password));
            }
        }
    }
}