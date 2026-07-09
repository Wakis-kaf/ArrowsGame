using Framework.Runtime.LogSystem;

using System;
using System.Reflection;

namespace Framework.Runtime.Archives
{
    /// <summary>
    /// 二进制存档
    /// </summary>
    public class BinaryArchive
    {
        public BinaryArchive(object data)
        {
            Type type = data.GetType(); // 获取类型
            // 获取可序列化的字段
            FieldInfo[] fileInfos = type.GetFields(BindingFlags.Instance | BindingFlags.Public |
                                                   BindingFlags.NonPublic | BindingFlags.Static);
            foreach (var fileInfo in fileInfos)
            {
                Log.Info($"Serializer Target {fileInfo.Name} IsSerializable: {fileInfo.GetType().IsSerializable}");
            }
        }

        /// <summary>
        /// 根据存档创建二进制存档
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static BinaryArchive Create(object data)
        {
            return new BinaryArchive(data);
        }
    }
}