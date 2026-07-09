using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Framework.Runtime.Archives
{
    public class ArchiveSerializerException : Exception
    {
        public ArchiveSerializerException(string s) : base(s)
        {
        }
    }

    /// <summary>
    /// 二进制存档序列化
    /// </summary>
    public class BinaryArchiveSerializer : IArchiveSerializer
    {
        public T DeSerialize<T>(Stream stream)
        {
            if (stream == Stream.Null || stream.Length == 0) return default;
            var bf = ObjectSerializer.GetBinaryFormatter();
            return (T)bf.Deserialize(stream);
        }

        public object DeSerialize(Type type, Stream stream)
        {
            if (stream == Stream.Null || stream.Length == 0) return default;
            var bf = ObjectSerializer.GetBinaryFormatter();
            return bf.Deserialize(stream);
        }

        public T DeSerialize<T>(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return default(T);

            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                return (T)formatter.Deserialize(stream);
            }
        }

        public byte[] GetSerializeBytes(object data)
        {
            var bf = ObjectSerializer.GetBinaryFormatter(data);
            if (bf == null) return null;
            byte[] byteArray;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // 将对象序列化到内存流中
                bf.Serialize(memoryStream, data);

                // 将内存流中的内容转换为字节数组
                byteArray = memoryStream.ToArray();

                // 此时 memoryStream 的 Position 在末尾，如果你需要重置以便读取，可以： memoryStream.Seek(0, SeekOrigin.Begin);
            }
            return byteArray;
        }

        public bool Serialize(Stream stream, object data)
        {
            // 使用二进制进行序列化 将对象进行 序列化封装之后进行保存
            var bf = ObjectSerializer.GetBinaryFormatter(data);
            if (stream == Stream.Null) return false;
            bf.Serialize(stream, data);
            return true;
        }
    }
}