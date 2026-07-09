using CustomLitJson;
using System;
using System.Diagnostics;
using System.Reflection;

namespace Framework.Utils
{
    public static partial class Utility
    {
        public static class Json
        {
            private static Utility.Json.IJsonHelper m_JsonHelper = new DefaultJsonHelper();

            /// <summary>JSON 辅助器接口。</summary>
            public interface IJsonHelper
            {
                JsonData ReadJson(string jsonStr);

                JsonData ReadJsonFromPath(string path, string jsonName);

                JsonData ReadJsonFromPath(string fullPath);

                T ReadJsonObject<T>(string path, string jsonName);

                T ReadJsonObject<T>(string fullPath);

                void SaveObjectToJsonFile(object obj, string path, string jsonName);

                /// <summary>将对象序列化为 JSON 字符串。</summary>
                /// <param name="obj">要序列化的对象。</param>
                /// <returns>序列化后的 JSON 字符串。</returns>
                string ToJson(object obj);

                /// <summary>将 JSON 字符串反序列化为对象。</summary>
                /// <typeparam name="T">对象类型。</typeparam>
                /// <param name="json">要反序列化的 JSON 字符串。</param>
                /// <returns>反序列化后的对象。</returns>
                T ToObject<T>(string json);

                /// <summary>将 JSON 字符串反序列化为对象。</summary>
                /// <param name="objectType">对象类型。</param>
                /// <param name="json">要反序列化的 JSON 字符串。</param>
                /// <returns>反序列化后的对象。</returns>
                object ToObject(Type objectType, string json);
            }

            public static JsonData ReadJson(string jsonStr)
            {
                if (Utility.Json.m_JsonHelper == null)
                    throw new FrameworkException("JSON helper is invalid.");
                return Utility.Json.m_JsonHelper.ReadJson(jsonStr);
            }

            public static JsonData ReadJsonFromPath(string fullPath)
            {
                if (Utility.Json.m_JsonHelper == null)
                    throw new FrameworkException("JSON helper is invalid.");
                return Utility.Json.m_JsonHelper.ReadJsonFromPath(fullPath);
            }

            public static JsonData ReadJsonFromPath(string path, string jsonName)
            {
                if (Utility.Json.m_JsonHelper == null)
                    throw new FrameworkException("JSON helper is invalid.");
                return Utility.Json.m_JsonHelper.ReadJsonFromPath(path, jsonName);
            }

            public static T ReadJsonObject<T>(string path, string jsonName)
            {
                if (Utility.Json.m_JsonHelper == null)
                    throw new FrameworkException("JSON helper is invalid.");
                return Utility.Json.m_JsonHelper.ReadJsonObject<T>(path, jsonName);
            }

            public static T ReadJsonObject<T>(string fullPath)
            {
                if (Utility.Json.m_JsonHelper == null)
                    throw new FrameworkException("JSON helper is invalid.");
                return Utility.Json.m_JsonHelper.ReadJsonObject<T>(fullPath);
            }

            public static void SaveObjectToJsonFile(object obj, string path, string jsonName)
            {
                if (Utility.Json.m_JsonHelper == null)
                    throw new FrameworkException("JSON helper is invalid.");
                Utility.Json.m_JsonHelper.SaveObjectToJsonFile(obj, path, jsonName);
            }

            /// <summary>设置 JSON 辅助器。</summary>
            /// <param name="jsonHelper">要设置的 JSON 辅助器。</param>
            public static void SetJsonHelper(Utility.Json.IJsonHelper jsonHelper)
            {
                Utility.Json.m_JsonHelper = jsonHelper;
            }

            /// <summary>将对象序列化为 JSON 字符串。</summary>
            /// <param name="obj">要序列化的对象。</param>
            /// <returns>序列化后的 JSON 字符串。</returns>
            public static string ToJson(object obj)
            {
                if (Utility.Json.m_JsonHelper == null)
                    throw new FrameworkException("JSON helper is invalid.");
                try
                {
                    return Utility.Json.m_JsonHelper.ToJson(obj);
                }
                catch (Exception ex)
                {
                    if (!(ex is FrameworkException))
                        throw new FrameworkException(string.Format("Can not convert to JSON with exception '{0}'.",
                                (object)ex.ToString()), ex);
                    throw;
                }
            }

            /// <summary>将 JSON 字符串反序列化为对象。</summary>
            /// <typeparam name="T">对象类型。</typeparam>
            /// <param name="json">要反序列化的 JSON 字符串。</param>
            /// <returns>反序列化后的对象。</returns>
            public static T ToObject<T>(string json)
            {
                if (Utility.Json.m_JsonHelper == null)
                {
                    UnityEngine.Debug.LogError("JSON helper is invalid.");
                    return default;
                }
                try
                {
                    return Utility.Json.m_JsonHelper.ToObject<T>(json);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"Can not convert to object with exception {ex}");
                    return default;
                }
            }

            /// <summary>将 JSON 字符串反序列化为对象。</summary>
            /// <param name="objectType">对象类型。</param>
            /// <param name="json">要反序列化的 JSON 字符串。</param>
            /// <returns>反序列化后的对象。</returns>
            public static object ToObject(Type objectType, string json)
            {
                if (Utility.Json.m_JsonHelper == null)
                    throw new FrameworkException("JSON helper is invalid.");
                if (objectType == null)
                    throw new FrameworkException("Object type is invalid.");
                try
                {
                    return Utility.Json.m_JsonHelper.ToObject(objectType, json);
                }
                catch (Exception ex)
                {
                    if (!(ex is FrameworkException))
                        throw new FrameworkException(
                            string.Format("Can not convert to object with exception '{0}'.",
                                (object)ex.ToString()), ex);
                    throw;
                }
            }

            public static bool TryGetValue<T>(JsonData jsonData, string name, out T res)
            {
                res = default;
                if (string.IsNullOrEmpty(name)) return false;
                if (jsonData.ContainsKey(name))
                {
                    if (!Utility.Convert.TryConvertToObject<T>(jsonData[name].ToString(), out res))
                    {
                        res = Utility.Json.ToObject<T>(jsonData[name].ToJson());
                    }

                    return true;
                }
                return false;
            }

            public static bool TrySetValue<T>(JsonData jsonData, string name, T value)
            {
                if (string.IsNullOrEmpty(name)) return false;
                Type valueType = typeof(T);

                // 处理基本类型
                if (valueType == typeof(string))
                {
                    jsonData[name] = value as string;
                }
                else if (valueType == typeof(int) || valueType == typeof(int?))
                {
                    jsonData[name] = (int)(object)value;
                }
                else if (valueType == typeof(long) || valueType == typeof(long?))
                {
                    jsonData[name] = (long)(object)value;
                }
                else if (valueType == typeof(double) || valueType == typeof(double?))
                {
                    jsonData[name] = (double)(object)value;
                }
                else if (valueType == typeof(float) || valueType == typeof(float?))
                {
                    jsonData[name] = (float)(object)value;
                }
                else if (valueType == typeof(bool) || valueType == typeof(bool?))
                {
                    jsonData[name] = (bool)(object)value;
                }
                else if (valueType == typeof(JsonData))
                {
                    jsonData[name] = value as JsonData;
                }
                else
                {
                    // 对于其他复杂类型，使用 JsonMapper 进行序列化
                    jsonData[name] = JsonMapper.ToObject(JsonMapper.ToJson(value));
                }

                return true;
            }
        }

        public class DefaultJsonHelper : Utility.Json.IJsonHelper
        {
            public string ObjectToJson(object obj)
            {
                UnityTypeBindings.Register();
                return JsonMapper.ToJson(obj);
            }

            public JsonData ReadJson(string jsonStr)
            {
                UnityTypeBindings.Register();
                return JsonMapper.ToObject(jsonStr);
            }

            public JsonData ReadJsonFromPath(string path, string jsonName)
            {
                string jsonStr = ReadJsonStrFromPath(path, jsonName);
                if (string.IsNullOrEmpty(jsonStr)) return null;
                JsonData jsonData = JsonMapper.ToObject(jsonStr);
                return jsonData;
            }

            public JsonData ReadJsonFromPath(string fullPath)
            {
                string jsonStr = Utility.FileUtil.ReadFile(fullPath);
                if (string.IsNullOrEmpty(jsonStr)) return null;
                JsonData jsonData = JsonMapper.ToObject(jsonStr);
                return jsonData;
            }

            public T ReadJsonObject<T>(string path, string jsonName)
            {
                string jsonStr = ReadJsonStrFromPath(path, jsonName);
                return JsonMapper.ToObject<T>(jsonStr);
            }

            public T ReadJsonObject<T>(string fullPath)
            {
                string jsonStr = ReadJsonStrFromPath(fullPath);
                if (string.IsNullOrEmpty(jsonStr)) return default;
                return JsonMapper.ToObject<T>(jsonStr);
            }

            public void SaveObjectToJsonFile(object obj, string path, string jsonName)
            {
                string content = ObjectToJson(obj);
                if (!jsonName.EndsWith(".json"))
                {
                    jsonName += ".json";
                }
                Utility.FileUtil.SaveFile(path, jsonName, content);
            }

            public string ToJson(object obj)
            {
                return ObjectToJson(obj);
            }

            public T ToObject<T>(string json)
            {
                return JsonToObject<T>(json);
            }

            public object ToObject(Type objectType, string json)
            {
                return JsonToObject(json, objectType);
            }

            private T JsonToObject<T>(string jsonStr)
            {
                return JsonMapper.ToObject<T>(jsonStr);
            }

            private object JsonToObject(string jsonStr, Type type)
            {
                return JsonMapper.ReadValue(type, new JsonReader(jsonStr));
            }

            private string ReadJsonStrFromPath(string path, string jsonName)
            {
                // json name validate
                if (!jsonName.EndsWith(".json"))
                {
                    jsonName += ".json";
                }

                return Utility.FileUtil.ReadFile(path, jsonName);
            }

            private string ReadJsonStrFromPath(string fullPath)
            {
                if (!fullPath.EndsWith(".json"))
                {
                    fullPath += ".json";
                }

                return Utility.FileUtil.ReadFile(fullPath);
            }
        }
    }
}