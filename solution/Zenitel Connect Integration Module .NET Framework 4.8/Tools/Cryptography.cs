using System;
using System.Security.Cryptography;
using System.Text;

namespace ConnectPro.Tools
{
    public class Cryptography
    {

        public static string Encrypt(string value)
        {
            byte[] textBytes = Encoding.UTF8.GetBytes(value);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = new byte[16];
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
            byte[] combinedBytes = Convert.FromBase64String(value);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = new byte[16]; ;

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
    }
}
