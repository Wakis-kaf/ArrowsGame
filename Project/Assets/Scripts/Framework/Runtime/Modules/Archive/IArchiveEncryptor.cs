using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Runtime.Archives
{
    public interface IArchiveEncryptor
    {
        Stream DecryptStream(Stream stream);
        byte[] GetDecryptBytes(byte[] bytes);
        byte[] GetEncryptBytes(byte[] bytes);
        public bool Serialize(Stream stream, byte[] rawBytes);
    }
}
