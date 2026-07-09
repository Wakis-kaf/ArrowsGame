using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Serialization;
using UnityEngine;

namespace Framework.Utils
{
    public static partial class Utility
    {
        public static class Convert
        {
            private static List<ICustomByteArrayConvert> m_ConvertByteArrayList = new List<ICustomByteArrayConvert>();

            private static List<ICustomStringConvert> m_ConvertList = new List<ICustomStringConvert>();

            private static Dictionary<Type, IConvert> m_TypeConvertMap = new Dictionary<Type, IConvert>();

            static Convert()
            {
                AddStringConvert<Vector2, Vector2Convert>();
                AddStringConvert<Vector3, Vector3Convert>();
                AddStringConvert<string, StringConvert>();
                AddStringConvert<float, FloatConvert>();
                AddStringConvert<double, DoubleConvert>();
                AddStringConvert<bool, BoolConvert>();
                AddStringConvert<int, IntConvert>();
                AddStringConvert<long, LongConvert>();
                AddStringConvert<int[], IntArrayConvert>();
                AddStringConvert<string[], StringArrayConvert>();
                AddStringConvert<float[], FloatArrayConvert>();
                AddStringConvert<EnumConvert>();
            }

            private interface IConvert
            {
            }

            private interface ICustomByteArrayConvert : IConvert
            {
                public T Convert<T>(byte[] value);

                public bool IsConverable<T>(byte[] value);
            }

            private interface ICustomStringConvert : IConvert
            {
                public T Convert<T>(string value, T defaultValue);

                public bool IsConverable<T>(string value);
            }

            private interface IStringConvert<T> : IConvert
            {
                T Convert(string value, T defaultValue);
            }

            public static T DeepCopyBYXML<T>(T obj)
            {
                object retval;
                using (MemoryStream ms = new MemoryStream())
                {
                    XmlSerializer xml = new XmlSerializer(typeof(T));
                    xml.Serialize(ms, obj);
                    ms.Seek(0, SeekOrigin.Begin);
                    retval = xml.Deserialize(ms);
                    ms.Close();
                }
                return (T)retval;
            }
         
            public static bool TryConvertToObject<T>(string value, out T res, T defaultValue = default)
            {
                var t = typeof(T);
                res = default;
                if (m_TypeConvertMap.TryGetValue(t, out IConvert convert))
                {
                    IStringConvert<T> stringConvert = convert as IStringConvert<T>;
                    res = stringConvert.Convert(value, defaultValue);
                    return true;
                }
                for (int i = 0; i < m_ConvertList.Count; i++)
                {
                    var ctmConverter = m_ConvertList[i];
                    if (ctmConverter.IsConverable<T>(value))
                    {
                        res = ctmConverter.Convert<T>(value, defaultValue);
                        return true;
                    }
                }
                return false;
            }

            private static void AddStringConvert<T, TStringConvert>() where TStringConvert : class, IStringConvert<T>, new()
            {
                var t = typeof(T);
                if (m_TypeConvertMap.ContainsKey(t)) return;
                m_TypeConvertMap.Add(t, new TStringConvert());
            }

            private static void AddStringConvert<T>() where T : class, ICustomStringConvert, new()
            {
                m_ConvertList.Add(new T());
            }

            #region string Convert

            private class BoolConvert : IStringConvert<bool>
            {
                public bool Convert(string value, bool defaultValue)
                {
                    if (bool.TryParse(value.Trim(), out bool result))
                    {
                        return result;
                    }
                    if (value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase))
                        return true;
                    if (value == "0" || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        return false;

                    return defaultValue;
                }
            }

            private class DoubleConvert : IStringConvert<double>
            {
                public double Convert(string value, double defaultValue)
                {
                    if (double.TryParse(value, out double result))
                    {
                        return result;
                    }
                    return defaultValue;
                }
            }

            private class EnumConvert : ICustomStringConvert
            {
                public T Convert<T>(string value, T defaultValue)
                {
                    try
                    {
                        var result = Enum.Parse(typeof(T), value, true);
                        return (T)result;
                    }
                    catch (Exception)
                    {
                        return defaultValue;
                    }
                }

                public bool IsConverable<T>(string value)
                {
                    return typeof(T).IsEnum;
                }
            }

            private class FloatConvert : IStringConvert<float>
            {
                public float Convert(string value, float defaultValue)
                {
                    if (float.TryParse(value, out float result))
                    {
                        return result;
                    }
                    return defaultValue;
                }
            }
            private class Vector2Convert : IStringConvert<Vector2>
            {
                public Vector2 Convert(string value, Vector2 defaultValue)
                {
                    float[] array = FloatArrayConvert.ConvertToArray(value, new float[2] { 0,0});
                    if (array.Length >= 2)
                    {
                        return new Vector2(array[0], array[1]);
                    }
                    return defaultValue;
                }
            }
            private class Vector3Convert : IStringConvert<Vector3>
            {
                public Vector3 Convert(string value, Vector3 defaultValue)
                {
                    float[] array = FloatArrayConvert.ConvertToArray(value, new float[3] { 0, 0,0 });
                    if (array.Length >= 3)
                    {
                        return new Vector3(array[0], array[1], array[2]);
                    }
                    return defaultValue;
                }
            }
            private class StringArrayConvert: IStringConvert<string[]>
            {
                public string[] Convert(string stringValue, string[] defaultValue)
                {
                    return ConvertToArray(stringValue, defaultValue);
                }
                public static List<string> ConvertToList(string stringValue, string[] defaultValue)
                {
                    return ConvertToArray(stringValue, defaultValue).ToList();
                }
                public static string[] ConvertToArray(string stringValue, string[] defaultValue)
                {
                    string[] elements = Array.Empty<string>();
                    // 处理 "[1,2,3]" 格式
                    if (stringValue.StartsWith("[") && stringValue.EndsWith("]"))
                    {
                        string trimmedValue = stringValue.Substring(1, stringValue.Length - 2);
                        elements = trimmedValue.Split(',');
                    }
                    // 处理 "1,2,3" 格式
                    else if (stringValue.Contains(","))
                    {
                        elements = stringValue.Split(',');
                    }
                    // 处理单个值 "123" 格式
                    else
                    {
                        elements = new string[] { stringValue };
                    }

                    return elements;
                }
            }
            private class IntArrayConvert : IStringConvert<int[]>
            {
                public  int[] Convert(string stringValue, int[] defaultValue)
                {
                    return ConvertToArray(stringValue, defaultValue);
                }
                public static int[] ConvertToArray(string stringValue, int[] defaultValue)
                {
                    string[] elements;
                    List<int> ints = new List<int>();
                    // 处理 "[1,2,3]" 格式
                    if (stringValue.StartsWith("[") && stringValue.EndsWith("]"))
                    {
                        string trimmedValue = stringValue.Substring(1, stringValue.Length - 2);
                        elements = trimmedValue.Split(',');
                    }
                    // 处理 "1,2,3" 格式
                    else if (stringValue.Contains(","))
                    {
                        elements = stringValue.Split(',');
                    }
                    // 处理单个值 "123" 格式
                    else
                    {
                        elements = new string[] { stringValue };
                    }

                    foreach (string element in elements)
                    {
                        string trimmedElement = element.Trim();
                        if (int.TryParse(trimmedElement, out int intValue))
                        {
                            ints.Add(intValue);
                        }
                    }
                    return ints.ToArray();
                }
            }
            private class FloatArrayConvert : IStringConvert<float[]>
            {
                public float[] Convert(string stringValue, float[] defaultValue)
                {
                    return ConvertToArray(stringValue, defaultValue);
                }
                public static float[] ConvertToArray(string stringValue, float[] defaultValue)
                {
                    string[] elements;
                    List<float> nums = new List<float>();
                    if (stringValue.StartsWith("[") && stringValue.EndsWith("]"))
                    {
                        string trimmedValue = stringValue.Substring(1, stringValue.Length - 2);
                        elements = trimmedValue.Split(',');
                    }
                    // 处理 "1,2,3" 格式
                    else if (stringValue.Contains(","))
                    {
                        elements = stringValue.Split(',');
                    }
                    // 处理单个值 "123" 格式
                    else
                    {
                        elements = new string[] { stringValue };
                    }

                    foreach (string element in elements)
                    {
                        string trimmedElement = element.Trim();
                        if (float.TryParse(trimmedElement, out float intValue))
                        {
                            nums.Add(intValue);
                        }
                    }
                    return nums.ToArray();
                }
            }

            private class IntConvert : IStringConvert<int>
            {
                public int Convert(string value, int defaultValue)
                {
                    if (int.TryParse(value, out int result))
                    {
                        return result;
                    }
                    return defaultValue;
                    
                }
            }

            private class LongConvert : IStringConvert<long>
            {
                public long Convert(string value,long defaultValue)
                {
                    if (long.TryParse(value, out long result))
                    {
                        return result;
                    }
                    return defaultValue;
                 
                }
            }

            private class StringConvert : IStringConvert<string>
            {
                public string Convert(string value, string defaultValue)
                {
                   
                    return value;
                }
            }

            #endregion string Convert

            #region byte convert

            public static object ConvertByteToType(byte[] data, Type targetType)
            {
                if (data == null)
                {
                    Debug.Log($"转换失败，数据为空");
                    return null;
                }
                if (targetType == null)
                {
                    Debug.Log($"转换失败，类型为空");
                    return null;
                }

                try
                {
                    // 处理基本类型
                    if (targetType == typeof(int))
                        return BitConverter.ToInt32(data, 0);

                    if (targetType == typeof(uint))
                        return BitConverter.ToUInt32(data, 0);

                    if (targetType == typeof(short))
                        return BitConverter.ToInt16(data, 0);

                    if (targetType == typeof(ushort))
                        return BitConverter.ToUInt16(data, 0);

                    if (targetType == typeof(long))
                        return BitConverter.ToInt64(data, 0);

                    if (targetType == typeof(ulong))
                        return BitConverter.ToUInt64(data, 0);

                    if (targetType == typeof(float))
                        return BitConverter.ToSingle(data, 0);

                    if (targetType == typeof(double))
                        return BitConverter.ToDouble(data, 0);

                    if (targetType == typeof(bool))
                        return BitConverter.ToBoolean(data, 0);

                    if (targetType == typeof(char))
                        return BitConverter.ToChar(data, 0);

                    if (targetType == typeof(string))
                        return System.Text.Encoding.UTF8.GetString(data);

                    if (targetType == typeof(byte[]))
                        return data;

                    if (targetType == typeof(byte))
                        return data.Length > 0 ? data[0] : (byte)0;
                    //if(targetType == typeof(AssetBundle))
                    //{
                    //    return data.Length>0?AssetBundle.LoadFromMemory(data):null;
                    //}

                    return null;
                }
                catch (Exception ex)
                {
                    Debug.Log($"转换失败: {ex.Message}");
                    return null;
                }
            }

            #endregion byte convert
        }
    }
}