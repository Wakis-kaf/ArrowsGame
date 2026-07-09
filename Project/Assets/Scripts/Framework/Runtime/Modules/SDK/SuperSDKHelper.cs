using System;
using Framework.Runtime.LogSystem;

namespace Framework.Runtime.MSDK
{
    public abstract class SuperSDKHelper
    {
        private static SuperSDKHelper m_Instance;

        public static SuperSDKHelper Instance
        {
            get
            {
                if (m_Instance == null)
                {
#if  UNITY_STANDALONE_WIN
                    m_Instance = new DefaultSDKHelper();
#elif UNITY_ANDROID && !UNITY_TIKTOKGAME
                    m_Instance = new AndroidSDKHelper();
#elif UNITY_TIKTOKGAME
                    m_Instance = new TikTokGameSDKHelper();
#elif UNITY_IOS || UNITY_IPHONE
                    m_Instance = new ApiSDKHelper();
#elif UNITY_WEBGL && UNITY_WXGAME
                    m_Instance = new WXSDKHelper();
#endif
                    if (m_Instance == null)
                    {
                        Log.Error("无法找到对应平台的SDK 处理器,请检查是否是正确平台!");
                    }

                }
                return m_Instance;
            }
        }

        private Action<string, object> m_Listeners;

        public void AddSDKMsgListener(Action<string, object> cb)
        {
            m_Listeners += cb;
        }

        public void RemoveSDKMsgListener(Action<string, object> cb)
        {
            m_Listeners -= cb;
        }

        public virtual string CallFun(string type, string args = "")
        {
            return "";
        }

        public virtual void OnCallFromSDK(string msg, object data = null)
        {
            m_Listeners?.Invoke(msg, data);
        }

        public virtual void InitSdk(Action<int, string> sdkCheckCb)
        {
            sdkCheckCb?.Invoke(0, "");
        }

        public virtual void SdkLogin(Action<int, string> sdkCb)
        {
            sdkCb?.Invoke(0, "");
        }

        public virtual void GetSdkUserInfo(Action<int, object> sdkCb)
        {
            sdkCb?.Invoke(0, null);
        }
    }
}