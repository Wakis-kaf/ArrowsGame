using Framework.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

// 数据变更回调委托（包含键、旧值、新值）
public delegate void DataChangedHandler<TKey, TValue>(TKey key, TValue oldValue, TValue newValue);

public class DataManager
{
    #region 数据存储字典
    // 专用数据类型存储（string key）
    private Dictionary<string, string> m_StringData = new Dictionary<string, string>();
    private Dictionary<string, int> m_IntData = new Dictionary<string, int>();
    private Dictionary<string, long> m_LongData = new Dictionary<string, long>();
    private Dictionary<string, float> m_FloatData = new Dictionary<string, float>();
    private Dictionary<string, bool> m_BoolData = new Dictionary<string, bool>();

    // 专用数据类型存储（int key）
    private Dictionary<int, string> m_IntKeyStringData = new Dictionary<int, string>();
    private Dictionary<int, int> m_IntKeyIntData = new Dictionary<int, int>();
    private Dictionary<int, long> m_IntKeyLongData = new Dictionary<int, long>();
    private Dictionary<int, float> m_IntKeyFloatData = new Dictionary<int, float>();
    private Dictionary<int, bool> m_IntKeyBoolData = new Dictionary<int, bool>();

    // 泛型存储（用于不常用类型）
    private Dictionary<string, object> m_GenericStringData = new Dictionary<string, object>();
    private Dictionary<int, object> m_GenericIntData = new Dictionary<int, object>();
    #endregion

    #region 数据变更事件
    // String Key 专用类型事件
    public event DataChangedHandler<string, string> StringDataChanged;
    public event DataChangedHandler<string, int> IntDataChanged;
    public event DataChangedHandler<string, long> LongDataChanged;
    public event DataChangedHandler<string, float> FloatDataChanged;
    public event DataChangedHandler<string, bool> BoolDataChanged;

    // Int Key 专用类型事件
    public event DataChangedHandler<int, string> IntKeyStringDataChanged;
    public event DataChangedHandler<int, int> IntKeyIntDataChanged;
    public event DataChangedHandler<int, long> IntKeyLongDataChanged;
    public event DataChangedHandler<int, float> IntKeyFloatDataChanged;
    public event DataChangedHandler<int, bool> IntKeyBoolDataChanged;

    // 泛型类型事件
    public event DataChangedHandler<string, object> GenericStringDataChanged;
    public event DataChangedHandler<int, object> GenericIntDataChanged;
    #endregion

    #region String Key 专用接口
    public string GetString(string key, string defaultValue = "")
    {
        return m_StringData.TryGetValue(key, out string value) ? value : defaultValue;
    }

    public void SetString(string key, string value)
    {
        bool hasOldValue = m_StringData.TryGetValue(key, out string oldValue);
        bool isChanged = !hasOldValue
            ? value != null
            : !string.Equals(oldValue, value, StringComparison.Ordinal);

        if (isChanged)
        {
            m_StringData[key] = value;
            StringDataChanged?.Invoke(key, oldValue, value);
        }
    }

    public int GetInt(string key, int defaultValue = 0)
    {
        return m_IntData.TryGetValue(key, out int value) ? value : defaultValue;
    }

    public void SetInt(string key, int value)
    {
        bool hasOldValue = m_IntData.TryGetValue(key, out int oldValue);
        if (!hasOldValue || oldValue != value)
        {
            m_IntData[key] = value;
            IntDataChanged?.Invoke(key, oldValue, value);
        }
    }

    public long GetLong(string key, long defaultValue = 0)
    {
        return m_LongData.TryGetValue(key, out long value) ? value : defaultValue;
    }

    public void SetLong(string key, long value)
    {
        bool hasOldValue = m_LongData.TryGetValue(key, out long oldValue);
        if (!hasOldValue || oldValue != value)
        {
            m_LongData[key] = value;
            LongDataChanged?.Invoke(key, oldValue, value);
        }
    }

    public float GetFloat(string key, float defaultValue = 0f)
    {
        return m_FloatData.TryGetValue(key, out float value) ? value : defaultValue;
    }

    public void SetFloat(string key, float value)
    {
        bool hasOldValue = m_FloatData.TryGetValue(key, out float oldValue);
        if (!hasOldValue || oldValue != value)
        {
            m_FloatData[key] = value;
            FloatDataChanged?.Invoke(key, oldValue, value);
        }
    }

    public bool GetBool(string key, bool defaultValue = false)
    {
        return m_BoolData.TryGetValue(key, out bool value) ? value : defaultValue;
    }

    public void SetBool(string key, bool value)
    {
        bool hasOldValue = m_BoolData.TryGetValue(key, out bool oldValue);
        if (!hasOldValue || oldValue != value)
        {
            m_BoolData[key] = value;
            BoolDataChanged?.Invoke(key, oldValue, value);
        }
    }
    #endregion

    #region Int Key 专用接口
    public string GetString(int key, string defaultValue = "")
    {
        return m_IntKeyStringData.TryGetValue(key, out string value) ? value : defaultValue;
    }

    public void SetString(int key, string value)
    {
        bool hasOldValue = m_IntKeyStringData.TryGetValue(key, out string oldValue);
        bool isChanged = !hasOldValue
            ? value != null
            : !string.Equals(oldValue, value, StringComparison.Ordinal);

        if (isChanged)
        {
            m_IntKeyStringData[key] = value;
            IntKeyStringDataChanged?.Invoke(key, oldValue, value);
        }
    }

    public int GetInt(int key, int defaultValue = 0)
    {
        return m_IntKeyIntData.TryGetValue(key, out int value) ? value : defaultValue;
    }

    public void SetInt(int key, int value)
    {
        bool hasOldValue = m_IntKeyIntData.TryGetValue(key, out int oldValue);
        if (!hasOldValue || oldValue != value)
        {
            m_IntKeyIntData[key] = value;
            IntKeyIntDataChanged?.Invoke(key, oldValue, value);
        }
    }

    public long GetLong(int key, long defaultValue = 0)
    {
        return m_IntKeyLongData.TryGetValue(key, out long value) ? value : defaultValue;
    }

    public void SetLong(int key, long value)
    {
        bool hasOldValue = m_IntKeyLongData.TryGetValue(key, out long oldValue);
        if (!hasOldValue || oldValue != value)
        {
            m_IntKeyLongData[key] = value;
            IntKeyLongDataChanged?.Invoke(key, oldValue, value);
        }
    }

    public float GetFloat(int key, float defaultValue = 0f)
    {
        return m_IntKeyFloatData.TryGetValue(key, out float value) ? value : defaultValue;
    }

    public void SetFloat(int key, float value)
    {
        bool hasOldValue = m_IntKeyFloatData.TryGetValue(key, out float oldValue);
        if (!hasOldValue || oldValue != value)
        {
            m_IntKeyFloatData[key] = value;
            IntKeyFloatDataChanged?.Invoke(key, oldValue, value);
        }
    }

    public bool GetBool(int key, bool defaultValue = false)
    {
        return m_IntKeyBoolData.TryGetValue(key, out bool value) ? value : defaultValue;
    }

    public void SetBool(int key, bool value)
    {
        bool hasOldValue = m_IntKeyBoolData.TryGetValue(key, out bool oldValue);
        if (!hasOldValue || oldValue != value)
        {
            m_IntKeyBoolData[key] = value;
            IntKeyBoolDataChanged?.Invoke(key, oldValue, value);
        }
    }
    #endregion

    #region 泛型接口
    public bool TryGetData<T>(string key, out T result, T defaultValue = default(T))
    {
        if (m_GenericStringData.TryGetValue(key, out object value) && value is T typedValue)
        {
            result = typedValue;
            return true;
        }
        result = defaultValue;
        return false;
    }

    public T GetData<T>(string key, T defaultValue = default(T))
    {
        if (m_GenericStringData.TryGetValue(key, out object value) && value is T typedValue)
        {
            return typedValue;
        }
        return defaultValue;
    }

    public void SetData<T>(string key, T value)
    {
        bool hasOldValue = m_GenericStringData.TryGetValue(key, out object oldValue);
        bool isChanged = !hasOldValue
            ? value != null
            : !object.Equals(oldValue, value);

        if (isChanged)
        {
            m_GenericStringData[key] = value;
            GenericStringDataChanged?.Invoke(key, oldValue, value);
        }
    }

    public T GetData<T>(int key, T defaultValue = default(T))
    {
        if (m_GenericIntData.TryGetValue(key, out object value) && value is T typedValue)
        {
            return typedValue;
        }
        return defaultValue;
    }

    public void SetData<T>(int key, T value)
    {
        bool hasOldValue = m_GenericIntData.TryGetValue(key, out object oldValue);
        bool isChanged = !hasOldValue
            ? value != null
            : !object.Equals(oldValue, value);

        if (isChanged)
        {
            m_GenericIntData[key] = value;
            GenericIntDataChanged?.Invoke(key, oldValue, value);
        }
    }
    #endregion

    #region 数据操作接口
    // 检查数据是否存在
    public bool HasString(string key) => m_StringData.ContainsKey(key);
    public bool HasInt(string key) => m_IntData.ContainsKey(key);
    public bool HasLong(string key) => m_LongData.ContainsKey(key);
    public bool HasFloat(string key) => m_FloatData.ContainsKey(key);
    public bool HasBool(string key) => m_BoolData.ContainsKey(key);

    public bool HasString(int key) => m_IntKeyStringData.ContainsKey(key);
    public bool HasInt(int key) => m_IntKeyIntData.ContainsKey(key);
    public bool HasLong(int key) => m_IntKeyLongData.ContainsKey(key);
    public bool HasFloat(int key) => m_IntKeyFloatData.ContainsKey(key);
    public bool HasBool(int key) => m_IntKeyBoolData.ContainsKey(key);

    // 删除数据
    public bool RemoveString(string key)
    {
        if (m_StringData.TryGetValue(key, out string oldValue) && m_StringData.Remove(key))
        {
            StringDataChanged?.Invoke(key, oldValue, default);
            return true;
        }
        return false;
    }

    public bool RemoveInt(string key)
    {
        if (m_IntData.TryGetValue(key, out int oldValue) && m_IntData.Remove(key))
        {
            IntDataChanged?.Invoke(key, oldValue, default);
            return true;
        }
        return false;
    }

    public bool RemoveLong(string key)
    {
        if (m_LongData.TryGetValue(key, out long oldValue) && m_LongData.Remove(key))
        {
            LongDataChanged?.Invoke(key, oldValue, default);
            return true;
        }
        return false;
    }

    public bool RemoveFloat(string key)
    {
        if (m_FloatData.TryGetValue(key, out float oldValue) && m_FloatData.Remove(key))
        {
            FloatDataChanged?.Invoke(key, oldValue, default);
            return true;
        }
        return false;
    }

    public bool RemoveBool(string key)
    {
        if (m_BoolData.TryGetValue(key, out bool oldValue) && m_BoolData.Remove(key))
        {
            BoolDataChanged?.Invoke(key, oldValue, default);
            return true;
        }
        return false;
    }

    public bool RemoveString(int key)
    {
        if (m_IntKeyStringData.TryGetValue(key, out string oldValue) && m_IntKeyStringData.Remove(key))
        {
            IntKeyStringDataChanged?.Invoke(key, oldValue, default);
            return true;
        }
        return false;
    }

    public bool RemoveInt(int key)
    {
        if (m_IntKeyIntData.TryGetValue(key, out int oldValue) && m_IntKeyIntData.Remove(key))
        {
            IntKeyIntDataChanged?.Invoke(key, oldValue, default);
            return true;
        }
        return false;
    }

    public bool RemoveLong(int key)
    {
        if (m_IntKeyLongData.TryGetValue(key, out long oldValue) && m_IntKeyLongData.Remove(key))
        {
            IntKeyLongDataChanged?.Invoke(key, oldValue, default);
            return true;
        }
        return false;
    }

    public bool RemoveFloat(int key)
    {
        if (m_IntKeyFloatData.TryGetValue(key, out float oldValue) && m_IntKeyFloatData.Remove(key))
        {
            IntKeyFloatDataChanged?.Invoke(key, oldValue, default);
            return true;
        }
        return false;
    }

    public bool RemoveBool(int key)
    {
        if (m_IntKeyBoolData.TryGetValue(key, out bool oldValue) && m_IntKeyBoolData.Remove(key))
        {
            IntKeyBoolDataChanged?.Invoke(key, oldValue, default);
            return true;
        }
        return false;
    }

    // 清空所有数据
    public void ClearAll(bool sendEvent = true)
    {
        // 缓存 String Key 数据
        var stringDataCopy = new Dictionary<string, string>(m_StringData);
        var intDataCopy = new Dictionary<string, int>(m_IntData);
        var longDataCopy = new Dictionary<string, long>(m_LongData);
        var floatDataCopy = new Dictionary<string, float>(m_FloatData);
        var boolDataCopy = new Dictionary<string, bool>(m_BoolData);

        // 缓存 Int Key 数据
        var intKeyStringCopy = new Dictionary<int, string>(m_IntKeyStringData);
        var intKeyIntCopy = new Dictionary<int, int>(m_IntKeyIntData);
        var intKeyLongCopy = new Dictionary<int, long>(m_IntKeyLongData);
        var intKeyFloatCopy = new Dictionary<int, float>(m_IntKeyFloatData);
        var intKeyBoolCopy = new Dictionary<int, bool>(m_IntKeyBoolData);

        // 缓存泛型数据
        var genericStringCopy = new Dictionary<string, object>(m_GenericStringData);
        var genericIntCopy = new Dictionary<int, object>(m_GenericIntData);

        // 清空所有字典
        m_StringData.Clear();
        m_IntData.Clear();
        m_LongData.Clear();
        m_FloatData.Clear();
        m_BoolData.Clear();

        m_IntKeyStringData.Clear();
        m_IntKeyIntData.Clear();
        m_IntKeyLongData.Clear();
        m_IntKeyFloatData.Clear();
        m_IntKeyBoolData.Clear();

        m_GenericStringData.Clear();
        m_GenericIntData.Clear();
        if (!sendEvent) return;
        // 触发清空回调（新值为默认值）
        foreach (var kvp in stringDataCopy)
            StringDataChanged?.Invoke(kvp.Key, kvp.Value, default);
        foreach (var kvp in intDataCopy)
            IntDataChanged?.Invoke(kvp.Key, kvp.Value, default);
        foreach (var kvp in longDataCopy)
            LongDataChanged?.Invoke(kvp.Key, kvp.Value, default);
        foreach (var kvp in floatDataCopy)
            FloatDataChanged?.Invoke(kvp.Key, kvp.Value, default);
        foreach (var kvp in boolDataCopy)
            BoolDataChanged?.Invoke(kvp.Key, kvp.Value, default);

        foreach (var kvp in intKeyStringCopy)
            IntKeyStringDataChanged?.Invoke(kvp.Key, kvp.Value, default);
        foreach (var kvp in intKeyIntCopy)
            IntKeyIntDataChanged?.Invoke(kvp.Key, kvp.Value, default);
        foreach (var kvp in intKeyLongCopy)
            IntKeyLongDataChanged?.Invoke(kvp.Key, kvp.Value, default);
        foreach (var kvp in intKeyFloatCopy)
            IntKeyFloatDataChanged?.Invoke(kvp.Key, kvp.Value, default);
        foreach (var kvp in intKeyBoolCopy)
            IntKeyBoolDataChanged?.Invoke(kvp.Key, kvp.Value, default);

        foreach (var kvp in genericStringCopy)
            GenericStringDataChanged?.Invoke(kvp.Key, kvp.Value, default);
        foreach (var kvp in genericIntCopy)
            GenericIntDataChanged?.Invoke(kvp.Key, kvp.Value, default);
    }

    // 数值累加操作
    public int AddInt(string key, int addValue)
    {
        int current = GetInt(key, 0);
        int newValue = current + addValue;
        SetInt(key, newValue); // 复用 SetInt 的回调逻辑
        return newValue;
    }

    public long AddLong(string key, long addValue)
    {
        long current = GetLong(key, 0);
        long newValue = current + addValue;
        SetLong(key, newValue); // 复用 SetLong 的回调逻辑
        return newValue;
    }

    public float AddFloat(string key, float addValue)
    {
        float current = GetFloat(key, 0f);
        float newValue = current + addValue;
        SetFloat(key, newValue); // 复用 SetFloat 的回调逻辑
        return newValue;
    }
    #endregion

    #region 字符串解析的泛型接口

    public bool TryGetDataFromString<T>(string key, out T res, T defaultValue = default(T))
    {
        if (TryGetFromTypedStorages(key, out  res))
            return true;

        if (m_GenericStringData.TryGetValue(key, out object genericValue))
        {
            if (genericValue is T typedValue)
            {
                res = typedValue;
                return true;
            }

            res = ConvertValue(genericValue, defaultValue);
            return true;
        }

        if (m_StringData.TryGetValue(key, out string stringValue))
        {
            res= ParseStringToType(stringValue, defaultValue);
            return true;
        }
        res = defaultValue;
        return false;
    }

    public T GetDataFromStr<T>(string key, T defaultValue = default(T))
    {
        if (TryGetFromTypedStorages(key, out T result))
            return result;

        if (m_GenericStringData.TryGetValue(key, out object genericValue))
        {
            if (genericValue is T typedValue)
                return typedValue;
           return  ConvertValue(genericValue, defaultValue);
        }

        if (m_StringData.TryGetValue(key, out string stringValue))
        {
            result = ParseStringToType(stringValue, defaultValue);
            m_GenericStringData.Add(key, result);
            return result;
        }
        return defaultValue;
    }

    public T GetDataFromString<T>(int key, T defaultValue = default(T))
    {
        if (TryGetFromTypedStorages(key, out T result))
            return result;

        if (m_GenericIntData.TryGetValue(key, out object genericValue))
        {
            if (genericValue is T typedValue)
                return typedValue;
            return ConvertValue(genericValue, defaultValue);
        }

        if (m_IntKeyStringData.TryGetValue(key, out string stringValue))
        {
            result = ParseStringToType(stringValue, defaultValue);
            m_GenericIntData.Add(key, result);
            return result;
        }

        return defaultValue;
    }

    public void SetDataAsString<T>(string key, T value)
    {
        string stringValue = ConvertToString(value);
        SetString(key, stringValue); // 复用 SetString 的回调逻辑
    }

    public void SetDataAsString<T>(int key, T value)
    {
        string stringValue = ConvertToString(value);
        SetString(key, stringValue); // 复用 SetString 的回调逻辑
    }
    #endregion

    #region 批量操作接口
    public Dictionary<string, T> BatchGetDataFromString<T>(IEnumerable<string> keys, T defaultValue = default(T))
    {
        var result = new Dictionary<string, T>();
        foreach (string key in keys)
            result[key] = GetDataFromStr(key, defaultValue);
        return result;
    }

    public void BatchSetDataAsString<T>(Dictionary<string, T> dataMap)
    {
        foreach (var kvp in dataMap)
            SetDataAsString(kvp.Key, kvp.Value); // 复用 SetDataAsString 的回调逻辑
    }
    #endregion

    #region 私有辅助方法
    private bool TryGetFromTypedStorages<T>(string key, out T result)
    {
        result = default;
        Type targetType = typeof(T);

        if (targetType == typeof(int) && m_IntData.TryGetValue(key, out int intValue))
        {
            result = (T)(object)intValue;
            return true;
        }
        else if (targetType == typeof(long) && m_LongData.TryGetValue(key, out long longValue))
        {
            result = (T)(object)longValue;
            return true;
        }
        else if (targetType == typeof(float) && m_FloatData.TryGetValue(key, out float floatValue))
        {
            result = (T)(object)floatValue;
            return true;
        }
        else if (targetType == typeof(bool) && m_BoolData.TryGetValue(key, out bool boolValue))
        {
            result = (T)(object)boolValue;
            return true;
        }
        else if (targetType == typeof(string) && m_StringData.TryGetValue(key, out string stringValue))
        {
            result = (T)(object)stringValue;
            return true;
        }

        return false;
    }

    private bool TryGetFromTypedStorages<T>(int key, out T result)
    {
        result = default;
        Type targetType = typeof(T);

        if (targetType == typeof(int) && m_IntKeyIntData.TryGetValue(key, out int intValue))
        {
            result = (T)(object)intValue;
            return true;
        }
        else if (targetType == typeof(long) && m_IntKeyLongData.TryGetValue(key, out long longValue))
        {
            result = (T)(object)longValue;
            return true;
        }
        else if (targetType == typeof(float) && m_IntKeyFloatData.TryGetValue(key, out float floatValue))
        {
            result = (T)(object)floatValue;
            return true;
        }
        else if (targetType == typeof(bool) && m_IntKeyBoolData.TryGetValue(key, out bool boolValue))
        {
            result = (T)(object)boolValue;
            return true;
        }
        else if (targetType == typeof(string) && m_IntKeyStringData.TryGetValue(key, out string stringValue))
        {
            result = (T)(object)stringValue;
            return true;
        }

        return false;
    }

    public static T ParseStringToType<T>(string stringValue, T defaultValue = default)
    {
        Utility.Convert.TryConvertToObject<T>(stringValue, out T res, defaultValue);
        return res;
    }

    private string ConvertToString<T>(T value)
    {
        if (value == null)
            return string.Empty;

        Type valueType = typeof(T);
        return valueType.IsEnum ? value.ToString() : value.ToString();
    }

    private T ConvertValue<T>(object value, T defaultValue)
    {
        if (value == null)
            return defaultValue;

        try
        {
            if (value is T typedValue)
                return typedValue;
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return defaultValue;
        }
    }
    #endregion
}