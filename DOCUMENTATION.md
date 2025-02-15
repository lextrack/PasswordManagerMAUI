# Introduccion to Simple Password Manager

This document provides an overview of the key classes in the system, focusing on those that play a fundamental role in the application's functionality. Minor implementations are excluded to maintain clarity.

The Windows version can be downloaded from GitHub, while the Android version is available on the Play Store.

## MainPage

The `MainPage.xaml.cs` file is part of a .NET MAUI application for managing passwords. Here's a breakdown of how the code works:

### Class and Properties
- **MainPage**: Inherits from `ContentPage` and represents the main page of the application.
- **Properties**:
  - `_storageFile`: Stores the path to the file where passwords are saved.
  - `Passwords`: An observable collection of `PasswordModel` objects, used for data binding.
  - `_allPasswords`: A private collection of all passwords, used for filtering.
  - `_currentSearchText`: Stores the current search text for filtering passwords.

### Constructor
- **MainPage()**: Initializes the component and sets the `BindingContext` to the current instance.

### Lifecycle Methods
- **OnAppearing()**: Called when the page appears. It initializes encryption, checks user authentication, and loads passwords from storage.

### Authentication
- **IsUserAuthenticatedAsync()**: Checks if the user is authenticated by retrieving the current user from secure storage.

### Event Handlers
- **OnLogoutClicked()**: Logs out the user by removing the current user from secure storage and navigates to the login page.
- **OnAboutClicked()**: Navigates to the About page.
- **OnAddPasswordClicked()**: Navigates to the AddPasswordPage and updates the password list when returning.
- **OnPasswordSelected()**: Displays details of the selected password.
- **OnDeletePasswordClicked()**: Deletes the selected password after confirmation.
- **OnCopyPasswordClicked()**: Copies the selected password to the clipboard.
- **OnSaveBackupClicked()**: Saves an encrypted backup of passwords to a file.
- **OnLoadBackupClicked()**: Loads passwords from an encrypted backup file.
- **OnEditPasswordClicked()**: Navigates to the EditPasswordPage for the selected password.
- **OnButtonPressed()** and **OnButtonReleased()**: Handle button press and release animations.

### Methods
- **LoadPasswordsAsync()**: Loads passwords from the storage file, decrypts them, and populates the collections.
- **SavePasswordsAsync()**: Encrypts and saves the passwords to the storage file.
- **OnSearchTextChanged()**: Filters the passwords based on the search text.
- **ShareFile()**: Shares the backup file using the device's sharing capabilities.

### Overridden Methods
- **OnBackButtonPressed()**: Handles the back button press to confirm logout and navigate to the login page.

### Summary
The `MainPage.xaml.cs` file manages the main functionality of the password manager, including loading, saving, searching, and backing up passwords. It also handles user authentication and navigation within the application.

## SecureStorageHelper
The `SecureStorageHelper` class in `SecureStorageHelper.cs` provides utility methods for saving and retrieving passwords securely using encryption. Here's a detailed explanation of how it works:

### Methods

1. **SavePasswordAsync(PasswordModel password)**
   - This method takes a `PasswordModel` object as a parameter.
   - It encrypts the password using the `EncryptionHelper.Encrypt` method.
   - It then stores the encrypted password in secure storage using `SecureStorage.Default.SetAsync`, with a key composed of the service and username.

   
```csharp
   public static async Task SavePasswordAsync(PasswordModel password)
   {
       string encryptedPassword = EncryptionHelper.Encrypt(password.Password);
       await SecureStorage.Default.SetAsync($"{password.Service}_{password.Username}", encryptedPassword);
   }
   
```

2. **GetPasswordAsync(string service, string username)**
   - This method takes the service and username as parameters.
   - It retrieves the encrypted password from secure storage using `SecureStorage.Default.GetAsync`.
   - It then decrypts the password using the `EncryptionHelper.Decrypt` method and returns the decrypted password.

   
```csharp
   public static async Task<string> GetPasswordAsync(string service, string username)
   {
       string encryptedPassword = await SecureStorage.Default.GetAsync($"{service}_{username}");
       return EncryptionHelper.Decrypt(encryptedPassword);
   }
   
```

### Usage

- **Saving a Password:**
  - Create a `PasswordModel` object with the service, username, and password.
  - Call `SavePasswordAsync` with the `PasswordModel` object to save the encrypted password in secure storage.

- **Retrieving a Password:**
  - Call `GetPasswordAsync` with the service and username to retrieve and decrypt the password from secure storage.

### Example


```csharp
var passwordModel = new PasswordModel
{
    Service = "exampleService",
    Username = "exampleUser",
    Password = "examplePassword"
};

// Save the password
await SecureStorageHelper.SavePasswordAsync(passwordModel);

// Retrieve the password
string password = await SecureStorageHelper.GetPasswordAsync("exampleService", "exampleUser");

```

### Summary

- The `SecureStorageHelper` class provides a simple interface for saving and retrieving passwords securely.
- It uses encryption to protect the passwords before storing them in secure storage.
- The `EncryptionHelper` class handles the encryption and decryption of passwords.
- The `SecureStorage` class provides a secure way to store and retrieve data.

## AddPasswordPage

The `AddPasswordPage.xaml.cs` file defines the behavior of the `AddPasswordPage` in a .NET MAUI application. This page allows users to add new passwords to their password manager. Here's a breakdown of how the code works:

### Class Definition

```csharp
public partial class AddPasswordPage : ContentPage
{
    private readonly MainPage _mainPage;

    public AddPasswordPage(MainPage mainPage)
    {
        InitializeComponent();
        _mainPage = mainPage;
        BindingContext = this;
    }

```
- The `AddPasswordPage` class inherits from `ContentPage`, which is a base class for pages in .NET MAUI.
- The constructor takes a `MainPage` object as a parameter and initializes the `_mainPage` field with it. This allows `AddPasswordPage` to interact with the `MainPage`.
- `InitializeComponent()` initializes the components defined in the corresponding XAML file.
- `BindingContext = this` sets the binding context of the page to itself, enabling data binding.

### Event Handlers
#### OnSaveClicked

```csharp
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

```
- This method is triggered when the save button is clicked.
- It creates a new `PasswordModel` object using the text entered in the `ServiceEntry`, `UsernameEntry`, and `PasswordEntry` fields.
- If `_mainPage` and its `Passwords` collection are not null, the new password is added to the collection, and `SavePasswordsAsync` is called to save the passwords.
- Finally, the page is popped from the navigation stack, returning to the previous page.

#### OnBackClicked

```csharp
private void OnBackClicked(object sender, EventArgs e)
{
    Navigation.PopAsync();
}

```
- This method is triggered when the back button is clicked.
- It pops the current page from the navigation stack, returning to the previous page.

#### OnButtonPressed and OnButtonReleased

```csharp
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

```
- These methods handle the visual feedback for button presses.
- `OnButtonPressed` scales the button down slightly when pressed.
- `OnButtonReleased` scales the button back to its original size when released.

### Summary
- The `AddPasswordPage` allows users to add new passwords.
- It interacts with the `MainPage` to add the new password to the collection and save it.
- It provides navigation and visual feedback for button interactions.

## EncryptionHelper

The `EncryptionHelper` class in `EncryptionHelper.cs` provides utility methods for encrypting and decrypting data, specifically for handling passwords in a secure manner. Here's a breakdown of how it works:

### Initialization

```csharp
public static async Task InitializeAsync()
{
    // Verify if the key and IV exists in SecureStorage
    if (await SecureStorage.Default.GetAsync("encryption_key") == null ||
        await SecureStorage.Default.GetAsync("encryption_iv") == null)
    {
        // Generate key and IV
        using (var aes = Aes.Create())
        {
            await SecureStorage.Default.SetAsync("encryption_key", Convert.ToBase64String(aes.Key));
            await SecureStorage.Default.SetAsync("encryption_iv", Convert.ToBase64String(aes.IV));
        }
    }

    // Recover key and IV from SecureStorage
    Key = Convert.FromBase64String(await SecureStorage.Default.GetAsync("encryption_key"));
    IV = Convert.FromBase64String(await SecureStorage.Default.GetAsync("encryption_iv"));
}

```
- **Purpose**: Initializes the encryption key and initialization vector (IV) by either generating new ones or retrieving existing ones from secure storage.
- **Steps**:
  1. Checks if the key and IV are already stored in `SecureStorage`.
  2. If not, generates a new key and IV using the `Aes` class and stores them in `SecureStorage`.
  3. Retrieves the key and IV from `SecureStorage` and converts them from Base64 strings to byte arrays.

### Encryption

```csharp
public static string Encrypt(string plainText)
{
    using (Aes aesAlg = Aes.Create())
    {
        aesAlg.Key = Key;
        aesAlg.GenerateIV(); // Generate a new IV for each operation

        ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

        using (MemoryStream msEncrypt = new MemoryStream())
        {
            msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);

            using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
                using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(plainText);
                }
                return Convert.ToBase64String(msEncrypt.ToArray());
            }
        }
    }
}

```
- **Purpose**: Encrypts a plain text string.
- **Steps**:
  1. Creates an `Aes` instance and sets its key.
  2. Generates a new IV for the encryption operation.
  3. Creates an encryptor using the key and IV.
  4. Writes the IV to the memory stream.
  5. Encrypts the plain text and writes it to the memory stream.
  6. Converts the encrypted data to a Base64 string and returns it.

### Decryption

```csharp
public static string Decrypt(string cipherText)
{
    byte[] cipherBytes = Convert.FromBase64String(cipherText);

    using (Aes aesAlg = Aes.Create())
    {
        aesAlg.Key = Key;

        // Read the IV from the beginning
        byte[] iv = new byte[aesAlg.IV.Length];
        Array.Copy(cipherBytes, iv, iv.Length);
        aesAlg.IV = iv;

        ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

        using (MemoryStream msDecrypt = new MemoryStream(cipherBytes, iv.Length, cipherBytes.Length - iv.Length))
        {
            using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
            {
                using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                {
                    return srDecrypt.ReadToEnd();
                }
            }
        }
    }
}

```
- **Purpose**: Decrypts a Base64 encoded cipher text string.
- **Steps**:
  1. Converts the Base64 encoded cipher text to a byte array.
  2. Creates an `Aes` instance and sets its key.
  3. Extracts the IV from the beginning of the cipher text byte array.
  4. Creates a decryptor using the key and IV.
  5. Decrypts the cipher text and reads the plain text from the memory stream.
  6. Returns the decrypted plain text.

### Encrypting and Decrypting Backups

```csharp
public static string EncryptBackup(IEnumerable<PasswordModel> passwords)
{
    var backupPasswords = passwords.Select(p => new PasswordModel
    {
        Service = p.Service,
        Username = p.Username,
        Password = Encrypt(p.Password)  // Encrypt each password
    }).ToList();

    return JsonSerializer.Serialize(backupPasswords);
}

public static List<PasswordModel> DecryptBackup(string encryptedJson)
{
    var backupPasswords = JsonSerializer.Deserialize<List<PasswordModel>>(encryptedJson);

    return backupPasswords.Select(p => new PasswordModel
    {
        Service = p.Service,
        Username = p.Username,
        Password = Decrypt(p.Password)  // Decrypt each password
    }).ToList();
}

```
- **EncryptBackup**:
  - **Purpose**: Encrypts a list of `PasswordModel` objects and serializes them to a JSON string.
  - **Steps**:
    1. Encrypts each password in the list.
    2. Serializes the list of encrypted `PasswordModel` objects to a JSON string.
    3. Returns the JSON string.

- **DecryptBackup**:
  - **Purpose**: Deserializes a JSON string to a list of `PasswordModel` objects and decrypts their passwords.
  - **Steps**:
    1. Deserializes the JSON string to a list of `PasswordModel` objects.
    2. Decrypts each password in the list.
    3. Returns the list of decrypted `PasswordModel` objects.

### Summary
The `EncryptionHelper` class provides methods to securely encrypt and decrypt individual strings and lists of `PasswordModel` objects, using AES encryption with keys and IVs stored in secure storage. This ensures that sensitive data, such as passwords, are protected both in storage and during transmission.

## LoginPage

The `LoginPage.xaml.cs` file defines the behavior of the `LoginPage` in a .NET MAUI application. Here's a breakdown of how the code works:

### Class Definition

```csharp
public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

```
- The `LoginPage` class inherits from `ContentPage`, which is a base class for pages in .NET MAUI.
- The constructor initializes the page by calling `InitializeComponent()`, which sets up the UI components defined in the corresponding XAML file.

### OnAppearing Method

```csharp
protected override void OnAppearing()
{
    base.OnAppearing();

    var buttons = this.GetVisualTreeDescendants().OfType<Button>();
    foreach (var button in buttons)
    {
        button.Scale = 1;
    }
}

```
- This method is called when the page appears.
- It resets the scale of all buttons on the page to 1, ensuring they are at their default size.

### OnLoginClicked Method

```csharp
private async void OnLoginClicked(object sender, EventArgs e)
{
    string username = UsernameEntry.Text;
    string password = PasswordEntry.Text;

    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
    {
        await DisplayAlert("Error", "Please enter both username and password.", "OK");
        return;
    }

    bool isAuthenticated = await AuthenticateUser(username, password);

    if (isAuthenticated)
    {
        await SecureStorage.Default.SetAsync("current_user", username);
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await Navigation.PushAsync(new MainPage());
        });
    }
    else
    {
        await DisplayAlert("Error", "Invalid username or password.", "OK");
    }
}

```
- This method handles the login button click event.
- It retrieves the username and password from the UI.
- If either field is empty, it displays an error message.
- It calls `AuthenticateUser` to check the credentials.
- If authentication is successful, it stores the username in secure storage and navigates to the `MainPage`.
- If authentication fails, it displays an error message.

### OnRegisterClicked Method

```csharp
private async void OnRegisterClicked(object sender, EventArgs e)
{
    if (sender is Button button)
    {
        button.Scale = 1;
    }
    await Navigation.PushAsync(new RegisterPage());
}

```
- This method handles the register button click event.
- It resets the button scale and navigates to the `RegisterPage`.

### AuthenticateUser Method

```csharp
private async Task<bool> AuthenticateUser(string username, string password)
{
    string storedPassword = await SecureStorage.Default.GetAsync(username);
    return storedPassword == password;
}

```
- This method checks if the provided username and password match the stored credentials.
- It retrieves the stored password from secure storage and compares it with the provided password.

### OnButtonPressed and OnButtonReleased Methods

```csharp
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

```
- These methods handle the button press and release events.
- They animate the button scale to provide visual feedback when the button is pressed and released.

Overall, this code manages user interactions on the login page, including login and registration, and provides visual feedback for button presses.

## PasswordModel

The `PasswordModel` class in `PasswordModel.cs` is a simple model class used to represent a password entry for a service. Here's a breakdown of its components:

### Properties
1. **Service**: A string property to store the name of the service (e.g., "Gmail").
2. **Username**: A string property to store the username associated with the service.
3. **Password**: A string property to store the password for the service.

### Computed Property
4. **CensoredPassword**: A read-only property that returns a censored version of the password. If the `Password` property is empty or null, it returns an empty string; otherwise, it returns "*****".

### Methods
5. **Equals**: An overridden method to compare two `PasswordModel` objects. It checks if the `Service`, `Username`, and `Password` properties are equal.
6. **GetHashCode**: An overridden method to generate a hash code for the `PasswordModel` object. It combines the hash codes of the `Service`, `Username`, and `Password` properties.

### Example Usage
This class can be used to create instances representing different password entries and can be compared for equality or used in collections that require hash codes (e.g., dictionaries or hash sets).

### Code Example

```csharp
var password1 = new PasswordModel
{
    Service = "Gmail",
    Username = "user1",
    Password = "password123"
};

var password2 = new PasswordModel
{
    Service = "Gmail",
    Username = "user1",
    Password = "password123"
};

bool areEqual = password1.Equals(password2); // True
string censored = password1.CensoredPassword; // "*****"

```

This class is straightforward and provides basic functionality for handling password entries in a .NET MAUI application.

## MainActivity

The `MainActivity.cs` file defines the main activity for the Android platform in a .NET MAUI project. Here's a breakdown of how the code works:

### Imports

```csharp
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

```
These `using` directives import necessary namespaces for Android application development.

### Namespace and Class Definition

```csharp
namespace PasswordManager
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

#if ANDROID
            // Avoid screenshots
            Window.SetFlags(WindowManagerFlags.Secure, WindowManagerFlags.Secure);
#endif
        }
    }
}

```

### Activity Attribute
The `Activity` attribute above the `MainActivity` class provides metadata about the activity:
- `Theme = "@style/Maui.SplashTheme"`: Sets the theme for the activity.
- `MainLauncher = true`: Indicates that this activity is the main entry point of the application.
- `LaunchMode = LaunchMode.SingleTop`: Ensures that if an instance of the activity already exists at the top of the stack, a new instance will not be created.
- `ConfigurationChanges`: Specifies the configuration changes that the activity will handle itself, preventing it from being restarted.

### MainActivity Class
The `MainActivity` class inherits from `MauiAppCompatActivity`, which is a base class for activities in .NET MAUI.

### OnCreate Method
The `OnCreate` method is overridden to perform initialization when the activity is created:
- `base.OnCreate(savedInstanceState)`: Calls the base class's `OnCreate` method to ensure proper initialization.
- The `#if ANDROID` directive ensures that the enclosed code is only compiled for the Android platform.
- `Window.SetFlags(WindowManagerFlags.Secure, WindowManagerFlags.Secure)`: Sets a flag to prevent the activity's window from being captured in screenshots or viewed on non-secure displays.

This setup ensures that the main activity is properly configured and secure for the Android platform in a .NET MAUI application.