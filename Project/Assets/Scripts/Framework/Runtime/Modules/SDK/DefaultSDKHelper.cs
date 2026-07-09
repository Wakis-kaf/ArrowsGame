using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Framework.Runtime.LogSystem;

namespace Framework.Runtime.MSDK
{
    public class DefaultSDKHelper : SuperSDKHelper
    {
        public override void InitSdk(Action<int, string> sdkCheckCb)
        {
            Log.Info("DefaultSDKHelper 初始化!");
            // sdkCheckCb?.Invoke();
            base.InitSdk(sdkCheckCb);
        }
    }
}