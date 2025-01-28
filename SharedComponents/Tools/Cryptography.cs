using System;
using System.Security.Cryptography;
using System.Text;

namespace ConnectPro.Tools
{
    public class Cryptography
    {
        // Define your new key here
        private static readonly byte[] _key = new byte[16] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0xFE, 0xDC, 0xBA, 0x98, 0x76, 0x54, 0x32, 0x10 };

        public static string Encrypt(string value)
        {
            if (value == null) return "";

            byte[] textBytes = Encoding.UTF8.GetBytes(value);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = _key;
                aesAlg.GenerateIV();

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                byte[] encryptedBytes = encryptor.TransformFinalBlock(textBytes, 0, textBytes.Length);

                byte[] combinedBytes = new byte[aesAlg.IV.Length + encryptedBytes.Length];
                Array.Copy(aesAlg.IV, 0, combinedBytes, 0, aesAlg.IV.Length);
                Array.Copy(encryptedBytes, 0, combinedBytes, aesAlg.IV.Length, encryptedBytes.Length);

                return Convert.ToBase64String(combinedBytes);
            }
        }

        public static string Decrypt(string value)
        {
            try
            {
                if (value == null) return "";

                byte[] combinedBytes = Convert.FromBase64String(value);

                if (combinedBytes.Length < 16 + 1)
                {
                    return value;
                }

                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = _key;

                    byte[] iv = new byte[aesAlg.IV.Length];
                    Array.Copy(combinedBytes, 0, iv, 0, aesAlg.IV.Length);
                    aesAlg.IV = iv;

                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    byte[] encryptedBytes = new byte[combinedBytes.Length - aesAlg.IV.Length];
                    Array.Copy(combinedBytes, aesAlg.IV.Length, encryptedBytes, 0, encryptedBytes.Length);

                    byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

                    return Encoding.UTF8.GetString(decryptedBytes);
                }
            }
            catch
            {
                return value;
            }
        }
    }
}
