using PasswordManager.Model;

namespace PasswordManager.Utils
{
    public static class SecureStorageHelper
    {
        public static async Task SavePasswordAsync(PasswordModel password)
        {
            string encryptedPassword = EncryptionHelper.Encrypt(password.Password);
            await SecureStorage.Default.SetAsync($"{password.Service}_{password.Username}", encryptedPassword);
        }

        public static async Task<string> GetPasswordAsync(string service, string username)
        {
            string encryptedPassword = await SecureStorage.Default.GetAsync($"{service}_{username}");
            return EncryptionHelper.Decrypt(encryptedPassword);
        }
    }
}
