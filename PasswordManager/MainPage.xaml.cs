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
            LoadPasswords();
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
                    var frame = (Frame)imageButton.Parent.Parent;
                    await frame.ScaleTo(0, 200);
                    await frame.FadeTo(0, 200);

                    if (PasswordsList.ItemsSource is IList<PasswordModel> passwords)
                    {
                        passwords.Remove(password);
                        PasswordsList.ItemsSource = null;
                        PasswordsList.ItemsSource = passwords;
                        SavePasswords(); 
                    }
                }
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
    }
}
