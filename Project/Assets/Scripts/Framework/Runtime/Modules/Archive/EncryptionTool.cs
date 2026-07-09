using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Framework.Runtime.Archives
{
    public static class EncryptionTool
    {
        private static readonly string Key = "com.waterbear666.company12345678"; // AES-256 需要 32 字节
        private static readonly string IV = "1234567890123456"; // 16 字节

        // ================== XOR 逻辑 (供 Binary 使用) ==================

        public static byte[] GetXorBytes(byte[] bytes)
        {
            try
            {
                if (bytes == null || bytes.Length == 0) return bytes;
                byte[] keyBytes = Encoding.UTF8.GetBytes(Key);
                for (int i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = (byte)(bytes[i] ^ keyBytes[i % keyBytes.Length]);
                }
                return bytes;
            }
            catch (Exception)
            {
                return bytes;
            }
          
        }

        // ================== AES 逻辑 (供 JSON 使用) ==================

        /// <summary>
        /// AES 加密字节数组
        /// </summary>
        public static byte[] AESEncryptBytes(byte[] rawBytes)
        {
            try
            {
                if (rawBytes == null || rawBytes.Length == 0) return rawBytes;
                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(Key);
                    aes.IV = Encoding.UTF8.GetBytes(IV);
                    using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                    {
                        return encryptor.TransformFinalBlock(rawBytes, 0, rawBytes.Length);
                    }
                }
            }
            catch (Exception)
            {
                return rawBytes;
            }
          
        }

        /// <summary>
        /// AES 解密字节数组
        /// </summary>
        public static byte[] AESDecryptBytes(byte[] encryptedBytes)
        {
            try
            {
                if (encryptedBytes == null || encryptedBytes.Length == 0) return encryptedBytes;
                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(Key);
                    aes.IV = Encoding.UTF8.GetBytes(IV);
                    using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                    {
                        return decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                    }
                }
            }
            catch(Exception e)
            {
                return encryptedBytes;
            }
            
        }
    }
}