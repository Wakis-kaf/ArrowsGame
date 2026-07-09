using UnityEngine;

namespace Framework.Runtime.LogSystem.Helper
{
#if UNITY_EDITOR || UNITY_WEBGL

    [LogStackTraceIgnore]
    public class UnityLogPrintHelper : ILogPrintHelper
    {
        public void PrintLog(Log.LogData logData)
        {
            if (logData.isUnityMessage) return;
            switch (logData.logLevel)
            {
                case LogLevel.WARN:
                    Debug.LogWarning(logData.GetColorMessage());
                    break;

                case LogLevel.FATAL:
                    Debug.LogError(logData.GetColorMessage());
                    break;

                case LogLevel.ERROR:
                    Debug.LogError(logData.GetColorMessage());
                    break;

                default:
                    Debug.Log(logData.GetColorMessage());
                    break;
            }
        }
    }

#endif
}