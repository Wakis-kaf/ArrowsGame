using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Framework.Runtime.MSDK
{
    public class SDKCallbackListener : MonoBehaviour
    {
        /// <summary>
        /// 对接Java层用户系统回调接口
        /// </summary>
        /// <param name="msg">字符串，其中有code、msg两个关键字</param>
        public void OnCallFromSDK(string msg)
        {
            SuperSDKHelper.Instance.OnCallFromSDK(msg);
        }
    }
}