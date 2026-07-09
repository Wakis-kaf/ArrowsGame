using Framework.Utils;
using System;
using UnityEngine;

namespace Framework.Runtime.MSDK
{
    public class SDKDataManager : Singleton<SDKDataManager>
    {
#if UNITY_ANDROID
        private AndroidJavaObject m_AndDataManager = null;
#endif

        public SDKDataManager()
        {
#if UNITY_ANDROID
            using (AndroidJavaClass tem = new AndroidJavaClass("com.yaoguang.andclient.AndDataManager"))
            {
                m_AndDataManager = tem.CallStatic<AndroidJavaObject>("getInstance");
            }
#endif
        }

        public string GetStringValue(string type)
        {
            string value = string.Empty;
#if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                value = m_AndDataManager.Call<String>("getStringValue", type);
            }
#endif
            return value;
        }

        public void SetStringValue(string type, String value)
        {
#if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                m_AndDataManager.Call("setStringValue", type, value);
            }
#endif
        }

        public bool GetBoolValue(string type)
        {
            bool value = false;
#if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                value = m_AndDataManager.Call<bool>("getBoolValue", type);
            }
#endif
            return value;
        }

        public void SetBoolValue(string type, bool value)
        {
#if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                m_AndDataManager.Call("setBoolValue", type, value);
            }
#endif
        }

        public int GetIntValue(string type)
        {
            int value = 0;
#if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                value = m_AndDataManager.Call<int>("getIntValue", type);
            }
#endif
            return value;
        }

        public void SetIntValue(string type, int value)
        {
#if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                m_AndDataManager.Call("setIntValue", type, value);
            }
#endif
        }

        public float GetFloatValue(string type)
        {
            float value = 0;
#if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                value = m_AndDataManager.Call<float>("getFloatValue", type);
            }
#endif
            return value;
        }

        public void SetFloatValue(string type, float value)
        {
#if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                m_AndDataManager.Call("setFloatValue", type, value);
            }
#endif
        }
    }
}