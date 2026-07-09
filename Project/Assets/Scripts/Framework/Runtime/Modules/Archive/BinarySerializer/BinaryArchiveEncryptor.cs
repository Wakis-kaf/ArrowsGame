using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Runtime.Archives
{
    public class BinaryArchiveEncryptor : IArchiveEncryptor
    {
        public Stream DecryptStream(Stream stream)
        {
            if (stream == null || stream.Length == 0) return stream;

            // 将流读入内存
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);

            // 调用现有的异或解密
            byte[] decryptedBytes = GetDecryptBytes(buffer);

            // 返回解密后的内存流
            return new MemoryStream(decryptedBytes);
        }

        public byte[] GetDecryptBytes(byte[] bytes)
        {
            return EncryptionTool.GetXorBytes(bytes); 
        }

        public byte[] GetEncryptBytes(byte[] data)
        {
            return EncryptionTool.GetXorBytes(data);
        }
        public bool Serialize(Stream stream, byte[] rawBytes)
        {
            if (stream == null) return false;
            byte[] encryptedBytes = GetEncryptBytes(rawBytes);
            // 写入目标流
            stream.Write(encryptedBytes, 0, encryptedBytes.Length);
            return true;
        }
    }
}
