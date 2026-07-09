using System;
using System.IO;

namespace Framework.Runtime.Archives
{
    public interface IArchiveSerializer
    {
        /// <summary>
        /// 反序列化数据
        /// </summary>
        /// <param name="stream"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T DeSerialize<T>(Stream stream);

        T DeSerialize<T>(byte[] bytes);

        object DeSerialize(Type type, Stream stream);

        /// <summary>
        /// 序列化为字节流
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        byte[] GetSerializeBytes(object data);

        /// <summary>
        /// 序列化数据
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        bool Serialize(Stream stream, object data);
    }
}