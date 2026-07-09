using Framework.Runtime.LogSystem;
using Framework.Utils;

using System;
using System.IO;
using System.Text;

namespace Framework.Runtime.Archives
{
    public class JsonArchiveSerializer : IArchiveSerializer
    {
        public T DeSerialize<T>(Stream stream)
        {
            try
            {
                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
                stream.Close();
                string content = UTF8Encoding.UTF8.GetString(bytes);
                return Utility.Json.ToObject<T>(content);
            }
            catch (Exception e)
            {
                Log.Fatal($"Archive Json DeSerialize Fail! \n {e}");
                throw;
            }
        }

        public object DeSerialize(Type type, Stream stream)
        {
            try
            {
                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
                stream.Close();
                string content = UTF8Encoding.UTF8.GetString(bytes);
                return Utility.Json.ToObject(type, content);
            }
            catch (Exception e)
            {
                Log.Fatal($"Archive Json DeSerialize Fail! \n {e}");
                throw;
            }
        }

        public T DeSerialize<T>(byte[] bytes)
        {
            string content = UTF8Encoding.UTF8.GetString(bytes);
            return Utility.Json.ToObject<T>(content);
        }

        public byte[] GetSerializeBytes(object data)
        {
            string json = Utility.Json.ToJson(data);
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            byte[] bts = Encoding.UTF8.GetBytes(json);
            return bts;
        }

        public bool Serialize(Stream stream, object data)
        {
            // 将对象写入对json 对象并保存
            try
            {
                string json = Utility.Json.ToJson(data);
                if (string.IsNullOrEmpty(json))
                {
                    return false;
                }

                byte[] bts = Encoding.UTF8.GetBytes(json);
                stream.Write(bts, 0, bts.Length);

                stream.Flush(); // 清空缓存
                stream.Close(); // 关闭流
                stream.Dispose(); // 清空缓存
            }
            catch (Exception e)
            {
                Log.Fatal($"Archive Json Serialize Fail! \n {e}");
                throw;
            }

            return true;
        }
    }
}