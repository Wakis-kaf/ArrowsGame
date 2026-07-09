using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Framework.Runtime.Archives
{
    public static partial class ObjectSerializer
    {
        private static Dictionary<Type, ISerializationSurrogate> m_BaseSerializerTable; // 基础类型序列化字典
        private static Dictionary<Type, ISerializationSurrogate> m_CustomSerializerTable; // 自定义类型序列化字典
        private static Dictionary<Type, bool> m_HasSerializeValidated;

        /// <summary>
        /// 静态生命区
        /// </summary>
        static ObjectSerializer()
        {
            m_BaseSerializerTable = new Dictionary<Type, ISerializationSurrogate>(); // 基础类型序列化
            m_CustomSerializerTable = new Dictionary<Type, ISerializationSurrogate>(); // 自定义类型序列化
            m_HasSerializeValidated = new Dictionary<Type, bool>();

            // 注册table
            Register<Vector3, Vector3Serializer>();
            Register<Quaternion, QuaternionSerializer>();
            Register<Color, ColorSerializer>();
            Register<Rect, RectSerializer>();
            Register(typeof(Dictionary<,>), typeof(DictionarySurrogate));
        }

        public static void DeRegister<T>()
        {
            DeRegister(typeof(T));
        }

        public static void DeRegister(Type type)
        {
            if (m_CustomSerializerTable.ContainsKey(type))
            {
                m_CustomSerializerTable.Remove(type);
            }
        }

        public static BinaryFormatter GetBinaryFormatter(object validateData = null)
        {
            DoValidate(validateData);
            var bf = new BinaryFormatter();
            SurrogateSelector ss = new SurrogateSelector();
            var streamingContext = new StreamingContext(StreamingContextStates.All);
            foreach (var item in m_CustomSerializerTable)
            {
                ss.AddSurrogate(item.Key, streamingContext, item.Value);
            }

            bf.SurrogateSelector = ss;
            return bf;
        }

        public static void Register<T, TSerializer>() where TSerializer : ISerializationSurrogate
        {
            Register(typeof(T), typeof(TSerializer));
        }

        public static void Register<T>(ISerializationSurrogate serializer)
        {
            Register(typeof(T), serializer);
        }

        public static void Register(Type type, Type serializerType)
        {
            var serializer = Activator.CreateInstance(serializerType) as ISerializationSurrogate;
            Register(type, serializer);
        }

        public static void Register(Type type, ISerializationSurrogate serializer)
        {
            if (!m_CustomSerializerTable.ContainsKey(type))
            {
                m_CustomSerializerTable.Add(type, serializer);
            }
            else
            {
                m_CustomSerializerTable[type] = serializer;
            }
        }

        private static void DeRegisterBase<T>()
        {
            DeRegisterBase(typeof(T));
        }

        private static void DeRegisterBase(Type type)
        {
            if (m_BaseSerializerTable.ContainsKey(type))
            {
                m_BaseSerializerTable.Remove(type);
            }
        }

        private static void DoSerializeValidate(Type targetType)
        {
            if (HasSerializeValidate(targetType)) return; // 已经检查过了就退出

            var serializeReadyList = new List<Type>();

            // 如果该类是泛型，需要对泛型的参数也进行序列化检查
            if (targetType.IsGenericType)
            {
                var genericTypes = targetType.GetGenericArguments();
                for (int i = 0; i < genericTypes.Length; i++)
                {
                    serializeReadyList.Add(genericTypes[i]);
                    ;
                }
            }

            // 对该类的字段进行检查
            FieldInfo[] fieldInfos =
                targetType.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                                     BindingFlags.NonPublic);
            // 添加序列化检查记录
            m_HasSerializeValidated.Add(targetType, true);
            // 获取需要序列化的字段
            for (int i = 0; i < fieldInfos.Length; i++)
            {
                var field = fieldInfos[i];
                var fieldType = field.FieldType; // 获取字段类型
                if (field.IsPublic || (!field.IsPublic && Attribute.IsDefined(field, typeof(SerializeField))))
                {
                    // 对字段进行序列化检查
                    if (field.IsNotSerialized) continue;
                    //Debug.Log($"{fieldType} Dont has NotSerialized");
                    if ((fieldType.IsSerializable && HasSerializeValidate(fieldType)) ||
                        (targetType == fieldType)) continue;
                    //Debug.Log($"{fieldType} Add to serializeReadyList");
                    serializeReadyList.Add(fieldType);
                }
            }

            // 对需要序列化的字段进行序列化注册
            for (int i = 0; i < serializeReadyList.Count; i++)
            {
                var readySerialize = serializeReadyList[i];
                bool isSerializable = readySerialize.IsSerializable;

                // 进行序列化
                //Debug.Log($"readySerialize {readySerialize}");
                if (HasRegisterBase(readySerialize))
                {
                    isSerializable = true;
                    DoSerializeValidate(readySerialize);
                    continue;
                }

                if (HasRegister(readySerialize))
                {
                    isSerializable = true;
                    DoSerializeValidate(readySerialize);
                    continue;
                }

                // 判断是否是泛型
                if (readySerialize.IsGenericType)
                {
                    // 获取泛型类型
                    var genericType = readySerialize.GetGenericTypeDefinition();
                    // Debug.Log($"GenericType {genericType} originType {readySerialize}");
                    if (HasRegisterBase(genericType))
                    {
                        isSerializable = true;
                        RegisterBase(readySerialize, GetBase(genericType));
                    }

                    if (HasRegister(genericType))
                    {
                        isSerializable = true;
                        Register(readySerialize, Get(genericType));
                    }
                }

                if (!isSerializable)
                {
                    throw new Exception($"type {readySerialize} is not serializable!");
                }

                DoSerializeValidate(readySerialize);
            }
        }

        private static void DoValidate(object validateData)
        {
            if (ReferenceEquals(validateData, null)) return;
            // 检查对象的所有字典对 公共和私有的可序列化字段进行序列化检查
            var type = validateData.GetType();
            DoSerializeValidate(type);
        }

        private static ISerializationSurrogate Get(Type type)
        {
            if (HasRegister(type)) return m_CustomSerializerTable[type];
            return null;
        }

        private static ISerializationSurrogate GetBase(Type type)
        {
            if (HasRegisterBase(type)) return m_BaseSerializerTable[type];
            return null;
        }

        private static bool HasRegister(Type type)
        {
            return m_CustomSerializerTable.ContainsKey(type) && m_CustomSerializerTable[type] != null;
        }

        private static bool HasRegisterBase(Type type)
        {
            return m_BaseSerializerTable.ContainsKey(type) && m_BaseSerializerTable[type] != null;
        }

        private static bool HasSerializeValidate(Type targetType)
        {
            return m_HasSerializeValidated.ContainsKey(targetType) && m_HasSerializeValidated[targetType];
        }

        private static void RegisterBase<T, TSerializer>() where TSerializer : ISerializationSurrogate
        {
            Register(typeof(T), typeof(TSerializer));
        }

        private static void RegisterBase<T>(ISerializationSurrogate serializer)
        {
            RegisterBase(typeof(T), serializer);
        }

        private static void RegisterBase(Type type, Type serializerType)
        {
            var serializer = Activator.CreateInstance(serializerType) as ISerializationSurrogate;
            RegisterBase(type, serializer);
        }

        private static void RegisterBase(Type type, ISerializationSurrogate serializer)
        {
            if (!m_BaseSerializerTable.ContainsKey(type))
            {
                m_BaseSerializerTable.Add(type, serializer);
            }
            else
            {
                m_BaseSerializerTable[type] = serializer;
            }
        }
    }
}