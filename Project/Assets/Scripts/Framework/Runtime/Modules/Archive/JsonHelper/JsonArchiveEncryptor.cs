using System.IO;

namespace Framework.Runtime.Archives
{
    public class JsonArchiveEncryptor : IArchiveEncryptor
    {
        public byte[] GetEncryptBytes(byte[] bytes)
        {
            return EncryptionTool.AESEncryptBytes(bytes); // 调用 AES 加密
        }

        public byte[] GetDecryptBytes(byte[] bytes)
        {
            return EncryptionTool.AESDecryptBytes(bytes); // 调用 AES 解密
        }

        // 处理流解密：读取加密流 -> AES 解密 -> 返回明文流
        public Stream DecryptStream(Stream stream)
        {
            if (stream == null || stream.Length == 0) return stream;
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, (int)stream.Length);

            byte[] decrypted = GetDecryptBytes(buffer);
            return new MemoryStream(decrypted);
        }
            
        public bool Serialize(Stream stream, byte[] rawBytes)
        {
            if (stream == null || rawBytes == null) return false;

            // 调用工具类进行 AES 加密
            byte[] encryptedBytes = EncryptionTool.AESEncryptBytes(rawBytes);

            // 将加密后的字节写入目标流
            stream.Write(encryptedBytes, 0, encryptedBytes.Length);
            return true;
        }
    }
}