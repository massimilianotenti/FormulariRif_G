// File: Utils/EncryptionHelper.cs
// Questa classe fornisce metodi per la criptazione e decriptazione di stringhe usando AES.
// La chiave di criptazione è hardcoded per questo esempio, ma in un'applicazione reale
// dovrebbe essere gestita in modo più sicuro (es. Azure Key Vault, DPAPI).
using System.Security.Cryptography;
using System.Text;

namespace FormulariRif_G.Utils
{
    public static class EncryptionHelper
    {
        // La chiave di criptazione deve essere esattamente di 16, 24 o 32 byte per AES.
        // PERICOLO: Non usare una chiave hardcoded in un'applicazione di produzione.
        // Questo è solo a scopo dimostrativo.
        private static byte[] _key = Encoding.UTF8.GetBytes("SixteenByteKey123!"); // 16 bytes

        /// <summary>
        /// Imposta la chiave di criptazione.
        /// </summary>
        /// <param name="keyString">La stringa della chiave (deve essere di 16, 24 o 32 caratteri UTF-8).</param>
        public static void SetKey(string keyString)
        {
            _key = Encoding.UTF8.GetBytes(keyString);
            if (_key.Length != 16 && _key.Length != 24 && _key.Length != 32)
            {
                throw new ArgumentException("La chiave di criptazione deve essere di 16, 24 o 32 byte.");
            }
        }

        /// <summary>
        /// Cripta una stringa usando l'algoritmo AES.
        /// </summary>
        /// <param name="plainText">La stringa in chiaro da criptare.</param>
        /// <returns>La stringa criptata in formato Base64.</returns>
        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = _key;
                aesAlg.GenerateIV(); // Genera un IV casuale per ogni operazione di criptazione
                byte[] iv = aesAlg.IV;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        byte[] encryptedContent = msEncrypt.ToArray();
                        // Prepend IV to the encrypted content
                        byte[] result = new byte[iv.Length + encryptedContent.Length];
                        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                        Buffer.BlockCopy(encryptedContent, 0, result, iv.Length, encryptedContent.Length);
                        return Convert.ToBase64String(result);
                    }
                }
            }
        }

        /// <summary>
        /// Decripta una stringa criptata in formato Base64 usando l'algoritmo AES.
        /// </summary>
        /// <param name="cipherText">La stringa criptata in formato Base64.</param>
        /// <returns>La stringa decriptata in chiaro.</returns>
        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;

            byte[] fullCipher = Convert.FromBase64String(cipherText);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = _key;

                // Estrae l'IV dalla parte iniziale della stringa criptata
                byte[] iv = new byte[aesAlg.BlockSize / 8];
                Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
                aesAlg.IV = iv;

                byte[] encryptedContent = new byte[fullCipher.Length - iv.Length];
                Buffer.BlockCopy(fullCipher, iv.Length, encryptedContent, 0, encryptedContent.Length);

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(encryptedContent))
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
