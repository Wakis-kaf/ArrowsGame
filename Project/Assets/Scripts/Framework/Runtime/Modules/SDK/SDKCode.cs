using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Runtime.MSDK
{
    public static class SDKCode
    {
        public const int wxSdk_init_success = 1;
        public const int wxSdk_login_complete = 2;
        public static int wxSdk_login_fail = 3;
        public static int wxSdk_login_suc = 4;
        public static int wxSdx_getUserInfo_complete = 4;
        public static int wxSdx_getUserInfo_fail = 5;
        public static int wxSdx_getUserInfo_suc = 6;
    }
}