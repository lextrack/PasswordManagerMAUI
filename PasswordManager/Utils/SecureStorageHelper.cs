using PasswordManager.Model;

namespace PasswordManager.Utils
{
    public static class SecureStorageHelper
    {
        public static async Task SavePasswordAsync(PasswordModel password)
        {
            await SecureStorage.Default.SetAsync($"{password.Service}_{password.Username}", password.Password);
        }

        public static async Task<string> GetPasswordAsync(string service, string username)
        {
            return await SecureStorage.Default.GetAsync($"{service}_{username}");
        }
    }
}
