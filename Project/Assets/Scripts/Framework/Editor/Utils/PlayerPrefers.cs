using Framework.Utils;
using UnityEngine;

namespace Framework.Editor.Utils
{
    public static class PlayerPrefers
    {
        public static string GetString(string key, string defaultValue = "")
        {
            if(PlayerPrefs.HasKey(key))
            {
                string value = PlayerPrefs.GetString(key);
                return value;
            }
            return defaultValue;
        }
        public static float GetFloat(string key, float defaultValue = 0)
        {
            if(PlayerPrefs.HasKey(key))
            {
                float value = PlayerPrefs.GetFloat(key);
                return value;
            }
            return defaultValue;
        }
        public static int GetInt(string key, int defaultValue = 0)
        {
            if(PlayerPrefs.HasKey(key))
            {
                int value = PlayerPrefs.GetInt(key);
                return value;
            }
            return defaultValue;
        }

        public static void SaveString(string key, string value)
        {
            PlayerPrefs.SetString(key,value);
            
        }
        public static void SaveInt(string key, int value)
        {
            PlayerPrefs.SetInt(key,value);
        }
        public static void SaveFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(key,value);
        }

        public static T GetData<T>(string key) where T : class
        {
            if(PlayerPrefs.HasKey(key))
            {
                string str = PlayerPrefs.GetString(key);
                T data = Utility.Json.ToObject<T>(str);
                if (data!=null)
                {
                    return data;
                }
            }
            return Utility.ReflectionUtil.CreateInstance<T>();
        }
        public static void  SaveData(string key,object data) 
        {
            string str = Utility.Json.ToJson(data);
            PlayerPrefs.SetString(key,str);
        }
    }
}