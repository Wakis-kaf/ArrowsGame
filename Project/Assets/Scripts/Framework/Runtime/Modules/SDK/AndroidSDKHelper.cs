using Framework.Runtime.LogSystem;
using System;
using UnityEngine;

namespace Framework.Runtime.MSDK
{
    public class AndroidSDKHelper : SuperSDKHelper
    {
#if UNITY_ANDROID
        protected AndroidJavaObject m_AndMsgManager;
#endif

        private void checkAndCreateAndroObject()
        {
#if UNITY_ANDROID
            if (null == m_AndMsgManager)
            {
                using (AndroidJavaClass tempClass = new AndroidJavaClass("com.yaoguang.andclient.AndMsgManager"))
                {
                    m_AndMsgManager = tempClass.CallStatic<AndroidJavaObject>("getInstance");
                }
            }
#endif
        }

        public override string CallFun(string type, string args = "")
        {
            string val = string.Empty;
#if UNITY_ANDROID
            checkAndCreateAndroObject();
            try
            {
                if (string.IsNullOrEmpty(args))
                {
                    args = "";
                }
                val = m_AndMsgManager.Call<string>("andCall", type, args);
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }
#endif
            return val;
        }
    }
}