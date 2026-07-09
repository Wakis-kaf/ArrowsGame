using Framework.Runtime.Module.Core;
using UnityEngine;

namespace Framework.Runtime
{
    public class UnityWebRequestMgr : BaseWebRequest
    {

        public void CopyFile(string inPath, string outPath, DelWebRequestCallback callback = null)
        {
            outPath = outPath.Replace("file://", null);
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.IPhonePlayer:

                    inPath = @"file://" + inPath;
                    break;
            }

            Download(inPath, outPath, callback);
        }
    }
}