using CustomLitJson.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Runtime.Archives
{
    [Serializable]
    public class GameSettingData : Archive
    {
        [SerializeField, JsonSerializer]
        private Dictionary<string, bool> m_BoolPrefers = new Dictionary<string, bool>();

        [SerializeField, JsonSerializer]
        private Dictionary<string, float> m_FloatPrefers = new Dictionary<string, float>();

        [SerializeField, JsonSerializer] private Dictionary<string, int> m_IntPrers = new Dictionary<string, int>();

        [SerializeField, JsonSerializer]
        private Dictionary<string, object> m_ObjectPrefers = new Dictionary<string, object>();

        [SerializeField, JsonSerializer]
        private Dictionary<string, string> m_StringPrefers = new Dictionary<string, string>();

        public GameSettingData():base()
        {
        }


        public override int ArchiveType
        {
            get => 1;
        }

        public string[] GetAllSettingNames()
        {
            var results = new List<string>();
            GetAllSettingNames(results);
            return results.ToArray();
        }

        public void GetAllSettingNames(List<string> results)
        {
            results.AddRange(m_IntPrers.Keys);
            results.AddRange(m_BoolPrefers.Keys);
            results.AddRange(m_StringPrefers.Keys);
            results.AddRange(m_FloatPrefers.Keys);
            results.AddRange(m_ObjectPrefers.Keys);
        }

        public bool GetBool(string settingName)
        {
            if (m_BoolPrefers.ContainsKey(settingName))
                return m_BoolPrefers[settingName];
            return false;
        }

        public bool GetBool(string settingName, bool defaultValue)
        {
            if (m_BoolPrefers.ContainsKey(settingName))
                return m_BoolPrefers[settingName];
            return defaultValue;
        }

        public float GetFloat(string settingName)
        {
            if (m_FloatPrefers.ContainsKey(settingName))
                return m_FloatPrefers[settingName];
            return 0f;
        }

        public float GetFloat(string settingName, float defaultValue)
        {
            if (m_FloatPrefers.ContainsKey(settingName))
                return m_FloatPrefers[settingName];
            return defaultValue;
        }

        public int GetInt(string settingName)
        {
            if (m_IntPrers.ContainsKey(settingName))
                return m_IntPrers[settingName];
            return 0;
        }

        public int GetInt(string settingName, int defaultValue)
        {
            if (m_IntPrers.ContainsKey(settingName))
                return m_IntPrers[settingName];
            return defaultValue;
        }

        public T GetObject<T>(string settingName)
        {
            if (m_ObjectPrefers.ContainsKey(settingName) && m_ObjectPrefers[settingName] is T res)
                return res;
            return default;
        }

        public object GetObject(Type objectType, string settingName)
        {
            if (m_ObjectPrefers.ContainsKey(settingName))
                return Convert.ChangeType(m_ObjectPrefers[settingName], objectType);
            return null;
        }

        public T GetObject<T>(string settingName, T defaultObj)
        {
            if (m_ObjectPrefers.ContainsKey(settingName) && m_ObjectPrefers[settingName] is T res)
                return res;
            return defaultObj;
        }

        public object GetObject(Type objectType, string settingName, object defaultObj)
        {
            if (m_ObjectPrefers.ContainsKey(settingName))
            {
                var t = m_ObjectPrefers[settingName].GetType();
                if (t.IsSubclassOf(objectType) || t == objectType)
                {
                    return Convert.ChangeType(m_ObjectPrefers[settingName], objectType);
                }
            }

            return defaultObj;
        }

        public string GetString(string settingName)
        {
            if (m_StringPrefers.ContainsKey(settingName))
                return m_StringPrefers[settingName];
            return string.Empty;
        }

        public string GetString(string settingName, string defaultValue)
        {
            if (m_StringPrefers.ContainsKey(settingName))
                return m_StringPrefers[settingName];
            return defaultValue;
        }

        public bool HasSetting(string settingName)
        {
            if (m_IntPrers.ContainsKey(settingName)) return true;
            if (m_BoolPrefers.ContainsKey(settingName)) return true;
            if (m_StringPrefers.ContainsKey(settingName)) return true;
            if (m_FloatPrefers.ContainsKey(settingName)) return true;
            if (m_ObjectPrefers.ContainsKey(settingName)) return true;
            return false;
        }

        public void RemoveAllSettings()
        {
            m_IntPrers.Clear();
            m_BoolPrefers.Clear();
            m_FloatPrefers.Clear();
            m_StringPrefers.Clear();
            m_ObjectPrefers.Clear();
        }

        public bool RemoveSetting(string settingName)
        {
            bool isRemoved = false;
            if (m_IntPrers.ContainsKey(settingName))
            {
                isRemoved = true;
                m_IntPrers.Remove(settingName);
            }

            if (m_BoolPrefers.ContainsKey(settingName))
            {
                isRemoved = true;
                m_IntPrers.Remove(settingName);
            }

            if (m_StringPrefers.ContainsKey(settingName))
            {
                isRemoved = true;
                m_IntPrers.Remove(settingName);
            }

            if (m_FloatPrefers.ContainsKey(settingName))
            {
                isRemoved = true;
                m_IntPrers.Remove(settingName);
            }

            if (m_ObjectPrefers.ContainsKey(settingName))
            {
                isRemoved = true;
                m_IntPrers.Remove(settingName);
            }

            return isRemoved;
        }

        public void SetBool(string settingName, bool value)
        {
            if (!m_BoolPrefers.ContainsKey(settingName))
                m_BoolPrefers.Add(settingName, value);
            m_BoolPrefers[settingName] = value;
        }

        public void SetFloat(string settingName, float value)
        {
            if (!m_FloatPrefers.ContainsKey(settingName))
                m_FloatPrefers.Add(settingName, value);
            m_FloatPrefers[settingName] = value;
        }

        public void SetInt(string settingName, int value)
        {
            if (!m_IntPrers.ContainsKey(settingName))
                m_IntPrers.Add(settingName, value);
            m_IntPrers[settingName] = value;
        }

        public void SetObject<T>(string settingName, T obj)
        {
            if (!m_ObjectPrefers.ContainsKey(settingName))
                m_ObjectPrefers.Add(settingName, obj);
            m_ObjectPrefers[settingName] = obj;
        }

        public void SetObject(string settingName, object obj)
        {
            if (!m_ObjectPrefers.ContainsKey(settingName))
                m_ObjectPrefers.Add(settingName, obj);
            m_ObjectPrefers[settingName] = obj;
        }

        public void SetString(string settingName, string value)
        {
            if (!m_StringPrefers.ContainsKey(settingName))
                m_StringPrefers.Add(settingName, value);
            m_StringPrefers[settingName] = value;
        }
    }
}