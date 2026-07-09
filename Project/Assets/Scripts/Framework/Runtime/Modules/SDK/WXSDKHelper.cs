#if UNITY_WEBGL && UNITY_WXGAME
using System;
using WeChatWASM;

namespace Framework.Runtime.MSDK
{
    public class WXSDKHelper : SuperSDKHelper
    {
        public override void GetSdkUserInfo(Action<int, object> sdkCb)
        {
            GetSettingOption info = new GetSettingOption();
            info.complete = (aa) =>
            {
                /*获取完成*/
                sdkCb.Invoke(SDKCode.wxSdx_getUserInfo_complete, aa.errMsg);
            };
            info.fail = (aa) =>
            {
                /*获取失败*/
                sdkCb.Invoke(SDKCode.wxSdx_getUserInfo_fail, aa.errMsg);
            };
            info.success = (aa) =>
            {
                sdkCb.Invoke(SDKCode.wxSdx_getUserInfo_suc, aa.authSetting);
                //if (!aa.authSetting.ContainsKey("scope.userInfo") || !aa.authSetting["scope.userInfo"])
                //{
                //    //《三、调起授权》
                //}
                //else
                //{
                //    //《四、获取用户信息》
                //}
            };
            WX.GetSetting(info);
        }

        public override void InitSdk(Action<int, string> sdkCheckCb)
        {
            base.InitSdk(sdkCheckCb);
            WX.InitSDK((ret) =>
            {
                sdkCheckCb?.Invoke(SDKCode.wxSdk_init_success, ret.ToString());
            });
        }

        public override void SdkLogin(Action<int, string> sdkCb)
        {
            LoginOption info = new LoginOption();
            // 登录完成回调 ，成功失败都会调用
            info.complete = (aa) =>
            {
                sdkCb?.Invoke(SDKCode.wxSdk_login_complete, aa.errMsg);
            };
            info.fail = (aa) =>
            {
                // 登录失败处理
                sdkCb?.Invoke(SDKCode.wxSdk_login_fail, aa.errMsg);
            };

            info.success = (aa) =>
            {
                // 登录成功处理
                sdkCb?.Invoke(SDKCode.wxSdk_login_suc, aa.code);
            };
            WX.Login(info);
        }
    }
}
#endif