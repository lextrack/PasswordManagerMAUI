using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Utils
{
    public static class EncryptionHelper
    {
        private static byte[] Key;
        private static byte[] IV;

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

        public static string Encrypt(string plainText)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.GenerateIV(); // Generar a new IV in any new operation

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

        public static string Decrypt(string cipherText)
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;

                // Read the IV from the begining
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
    }
}
