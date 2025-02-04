using System;
using System.Security.Cryptography;
using System.Text;

namespace ConnectPro.Tools
{
    /// <summary>
    /// Provides encryption and decryption utilities using AES encryption.
    /// </summary>
    public class Cryptography
    {
        #region Fields

        /// <summary>
        /// AES encryption key (16 bytes). Ensure this key remains secure.
        /// </summary>
        private static readonly byte[] _key = new byte[16]
        {
            0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF,
            0xFE, 0xDC, 0xBA, 0x98, 0x76, 0x54, 0x32, 0x10
        };

        #endregion

        #region Encryption

        /// <summary>
        /// Encrypts a string using AES encryption.
        /// </summary>
        /// <param name="value">The plain text to encrypt.</param>
        /// <returns>The encrypted string in Base64 format.</returns>
        /// <remarks>
        /// A random initialization vector (IV) is generated for each encryption operation 
        /// and is stored at the beginning of the encrypted data.
        /// </remarks>
        public static string Encrypt(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";

            byte[] textBytes = Encoding.UTF8.GetBytes(value);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = _key;
                aesAlg.GenerateIV(); // Generate a new IV for every encryption

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                byte[] encryptedBytes = encryptor.TransformFinalBlock(textBytes, 0, textBytes.Length);

                // Combine IV and encrypted data
                byte[] combinedBytes = new byte[aesAlg.IV.Length + encryptedBytes.Length];
                Array.Copy(aesAlg.IV, 0, combinedBytes, 0, aesAlg.IV.Length);
                Array.Copy(encryptedBytes, 0, combinedBytes, aesAlg.IV.Length, encryptedBytes.Length);

                return Convert.ToBase64String(combinedBytes);
            }
        }

        #endregion

        #region Decryption

        /// <summary>
        /// Decrypts a string encrypted using AES encryption.
        /// </summary>
        /// <param name="value">The encrypted string in Base64 format.</param>
        /// <returns>The decrypted plain text, or the original input if decryption fails.</returns>
        /// <remarks>
        /// The function extracts the IV from the stored data before performing decryption.
        /// If decryption fails, it returns the original input string.
        /// </remarks>
        public static string Decrypt(string value)
        {
            try
            {
                if (string.IsNullOrEmpty(value)) return "";

                byte[] combinedBytes = Convert.FromBase64String(value);

                // Ensure input contains at least an IV and some encrypted data
                if (combinedBytes.Length < 16 + 1)
                {
                    return value;
                }

                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = _key;

                    // Extract IV from the stored data
                    byte[] iv = new byte[aesAlg.IV.Length];
                    Array.Copy(combinedBytes, 0, iv, 0, aesAlg.IV.Length);
                    aesAlg.IV = iv;

                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    // Extract encrypted data (excluding IV)
                    byte[] encryptedBytes = new byte[combinedBytes.Length - aesAlg.IV.Length];
                    Array.Copy(combinedBytes, aesAlg.IV.Length, encryptedBytes, 0, encryptedBytes.Length);

                    byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

                    return Encoding.UTF8.GetString(decryptedBytes);
                }
            }
            catch
            {
                return value; // Return original value if decryption fails
            }
        }

        #endregion
    }
}
