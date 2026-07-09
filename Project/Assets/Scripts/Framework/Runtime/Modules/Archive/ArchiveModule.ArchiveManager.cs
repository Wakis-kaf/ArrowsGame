using Framework.Runtime.UnitSystem.Base;
using System;
using System.IO;

namespace Framework.Runtime.Archives
{
    public sealed partial class ArchiveModule
    {
        private class ArchiveManager : BehaviourUnit
        {
            private IArchiveSerializer m_Helper;
            private IArchiveEncryptor m_Entryptor;

            /// <summary>
            /// 反序列化数据
            /// </summary>
            /// <param name="stream"></param>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            public T DeSerialize<T>(Stream stream)
            {
                Stream targetStream = stream;
                if (m_Entryptor != null)
                {
                    targetStream = m_Entryptor.DecryptStream(stream);
                }

                T result = m_Helper != null ? m_Helper.DeSerialize<T>(targetStream) : default;

                // 如果创建了临时内存流，手动释放
                if (targetStream != stream)
                {
                    targetStream.Dispose();
                }
                return result;
            }

            public T DeSerialize<T>(byte[] bytes)
            {
                if (m_Entryptor != null)
                {
                    bytes = m_Entryptor.GetDecryptBytes(bytes);
                }
                if (m_Helper != null)
                {
                    return m_Helper.DeSerialize<T>(bytes);
                }

                return default;
            }

            public object DeSerialize(Type type, Stream stream)
            {
                if (stream == null) return default;

                Stream targetStream = stream;
                if (m_Entryptor != null)
                {
                    targetStream = m_Entryptor.DecryptStream(stream);
                }
                object result = m_Helper != null ? m_Helper.DeSerialize(type,targetStream) : default;
                if (targetStream != stream)
                {
                    targetStream.Dispose();
                }
                return result;
            }

            /// <summary>
            /// 或获取字节流
            /// </summary>
            /// <param name="data"></param>
            /// <returns></returns>
            public byte[] GetSerializeBytes(object data)
            {
                byte[] bytes = null;
                if (m_Helper != null)
                {
                    bytes= m_Helper.GetSerializeBytes(data);
                }
                if (m_Entryptor != null )
                {
                    bytes= m_Entryptor.GetEncryptBytes(bytes);
                }
                return bytes;
            }

            /// <summary>
            /// 序列化数据到目标流中
            /// </summary>
            /// <param name="stream"></param>
            /// <param name="data"></param>
            public bool Serialize(Stream stream, object data)
            {
                if (m_Helper == null || stream == null) return false;

                // 1. 如果有加密器，需要中间层处理
                if (m_Entryptor != null)
                {
                    byte[] rawBytes = m_Helper.GetSerializeBytes(data);
                    return m_Entryptor.Serialize(stream, rawBytes);
                }

                // 2. 没有加密器则直接序列化
                return m_Helper.Serialize(stream, data);
            }

            public IArchiveSerializer SetHelper(IArchiveSerializer helper)
            {
                m_Helper = helper;
                return m_Helper;
            }
            public IArchiveEncryptor SetEntryptor(IArchiveEncryptor encryptor)
            {
                m_Entryptor = encryptor;
                return m_Entryptor;
            }
        }
    }
}