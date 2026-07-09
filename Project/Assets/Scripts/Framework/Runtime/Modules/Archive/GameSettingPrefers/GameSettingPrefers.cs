using Framework.Runtime.Module;

using System;
using System.Collections.Generic;

namespace Framework.Runtime.Archives
{
    public static class GameSettingPrefers
    {
        private static readonly string defaultSaveName = "GameSettingPrefers";
        private static GameSettingData m_Prefers;

        public static string[] GetAllSettingNames()
        {
            return m_Prefers.GetAllSettingNames();
        }

        public static void GetAllSettingNames(List<string> results)
        {
            m_Prefers.GetAllSettingNames(results);
        }

        public static bool GetBool(string settingName)
        {
            return m_Prefers.GetBool(settingName);
            ;
        }

        public static bool GetBool(string settingName, bool defaultValue)
        {
            return m_Prefers.GetBool(settingName, defaultValue);
            ;
        }

        public static float GetFloat(string settingName)
        {
            return m_Prefers.GetFloat(settingName);
        }

        public static float GetFloat(string settingName, float defaultValue)
        {
            return m_Prefers.GetFloat(settingName, defaultValue);
        }

        public static int GetInt(string settingName)
        {
            return m_Prefers.GetInt(settingName);
            ;
        }

        public static int GetInt(string settingName, int defaultValue)
        {
            return m_Prefers.GetInt(settingName, defaultValue);
        }

        public static T GetObject<T>(string settingName)
        {
            return m_Prefers.GetObject<T>(settingName);
        }

        public static object GetObject(Type objectType, string settingName)
        {
            return m_Prefers.GetObject(objectType, settingName);
        }

        public static T GetObject<T>(string settingName, T defaultObj)
        {
            return m_Prefers.GetObject<T>(settingName, defaultObj);
        }

        public static object GetObject(Type objectType, string settingName, object defaultObj)
        {
            return m_Prefers.GetObject(objectType, settingName, defaultObj);
        }

        public static string GetString(string settingName)
        {
            return m_Prefers.GetString(settingName);
        }

        public static string GetString(string settingName, string defaultValue)
        {
            return m_Prefers.GetString(settingName);
        }

        public static bool HasSetting(string settingName)
        {
            return m_Prefers.HasSetting(settingName);
            ;
        }

        public static void Init()
        {
            InitSettings();
        }

        public static void RemoveAllSettings()
        {
            m_Prefers.RemoveAllSettings();
            ;
        }

        public static bool RemoveSetting(string settingName)
        {
            return m_Prefers.RemoveSetting(settingName);
            ;
        }

        public static void Save()
        {
            m_Prefers.Save();
        }

        public static void SetBool(string settingName, bool value)
        {
            m_Prefers.SetBool(settingName, value);
            Save();
        }

        public static void SetFloat(string settingName, float value)
        {
            m_Prefers.SetFloat(settingName, value);
            Save();
        }

        public static void SetInt(string settingName, int value)
        {
            m_Prefers.SetInt(settingName, value);
            Save();
        }

        public static void SetObject<T>(string settingName, T obj)
        {
            m_Prefers.SetObject<T>(settingName, obj);
            Save();
        }

        public static void SetObject(string settingName, object obj)
        {
            m_Prefers.SetObject(settingName, obj);
            Save();
        }

        public static void SetString(string settingName, string value)
        {
            m_Prefers.SetString(settingName, value);
            Save();
        }

        private static void InitSettings()
        {
            var archiveModule = GameApp.ArchiveModule;
            var setting = archiveModule.LoadArchiveSync<GameSettingData>(defaultSaveName, true);
            if (setting == null)
            {
                setting = archiveModule.CreateArchive<GameSettingData>(defaultSaveName);
            }

            m_Prefers = setting;
            if (m_Prefers == null)
            {
                m_Prefers = new GameSettingData();
            }
        }
    }
}