using Framework.Misc;
using Framework.Runtime.LogSystem.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Framework.Runtime.LogSystem
{
    [LogStackTraceIgnore]
    public static class Log
    {
        public struct LogData
        {
            public static LogData Default = new LogData();
            public LogLevel logLevel; // 日志级别
            public string logTime; // 日志时间
            public string logPrefix;
            public object logMessageObject; // 日志消息
            public StringBuilder logMessage; // 日志消息
            public string logBasicData; // 日志基础数据
            public string logTrack; // 日志堆栈信息
            private string m_MessageCache;
            private string m_ShortMessageCache;
            private string m_MessageWithoutTraceCache;
            private string m_ShortMessageWithoutTraceCache;
            public string shortMessage;

            public bool hasPrited;
            public bool hasWrited;
            internal bool isUnityMessage;

            public string GetColorMessage()
            {
                if (GameEnv.IsEditor() || Application.isEditor)
                {
                    return LogConfig.GetLevelColorString(logLevel, GetMessage());
                }
                return GetMessage();
            }

            public string GetColorMessageWithoutTrack()
            {
                if (GameEnv.IsEditor() || Application.isEditor)
                {
                    return LogConfig.GetLevelColorString(logLevel, GetMessageWithOutTrack());
                }
                return GetMessageWithOutTrack();
            }

            public string GetMessage(int maxLength)
            {
                string msg = GetMessage();
                return msg.Substring(0, Mathf.Min(maxLength, msg.Length));
            }

            public string GetMessage()
            {
                if (string.IsNullOrEmpty(m_MessageCache))
                    m_MessageCache = String.Concat(GetMessageWithOutTrack(), "\n", logTrack);
                return m_MessageCache;
            }

            public string GetShortMessage()
            {
                if (string.IsNullOrEmpty(m_ShortMessageCache))
                    m_ShortMessageCache = String.Concat(GetShortMessageWithOutTrack(), "\n", logTrack);
                return m_ShortMessageCache;
            }

            public string GetMessageWithOutTrack()
            {
                if (string.IsNullOrEmpty(m_MessageWithoutTraceCache))
                    m_MessageWithoutTraceCache = String.Concat(GameEnv.LogConfig.InfoPrefix, logTime, logPrefix, logMessage);
                return m_MessageWithoutTraceCache;
            }

            public string GetShortMessageWithOutTrack()
            {
                if (string.IsNullOrEmpty(m_ShortMessageWithoutTraceCache))
                    m_ShortMessageWithoutTraceCache = String.Concat(GameEnv.LogConfig.InfoPrefix, logTime, logPrefix, shortMessage);
                return m_ShortMessageWithoutTraceCache;
            }

            public static LogData CreateFromUnityLog(string condition, string stacktrace, LogType type)
            {
                LogLevel logLevel = LogLevel.INFO;
                switch (type)
                {
                    case LogType.Error:
                        logLevel = LogLevel.ERROR;
                        break;

                    case LogType.Exception:
                        logLevel = LogLevel.FATAL;
                        break;

                    case LogType.Log:
                        logLevel = LogLevel.INFO;
                        break;

                    case LogType.Warning:
                        logLevel = LogLevel.WARN;
                        break;

                    case LogType.Assert:
                        logLevel = LogLevel.DEBUG;
                        break;
                }

                if (condition.StartsWith(GameEnv.LogConfig.InfoPrefix) &&
                    condition.Contains(LogLevelPrefix[LogLevel.DEBUG]))
                {
                    logLevel = LogLevel.DEBUG;
                }

                LogData data = new LogData();
                data.logMessageObject = condition;
                data.logMessage = UStringBuilderPool.GetSharedStringBuilder(condition);
                data.logTrack = stacktrace;
                data.logLevel = logLevel;
                data.logPrefix = GetLevelPrefix(logLevel); // 获取前缀

                if (IsLogTime(logLevel))
                    data.logTime = DateTime.Now.ToString("HH:mm:ss");
                return data;
            }

            public void OnPrinted()
            {
                hasPrited = true;
            }

            public void OnWrited()
            {
                hasWrited = true;
            }

            public void CheckRelease()
            {
                if (hasPrited && hasWrited)
                {
                    UStringBuilderPool.Release(logMessage);
                }
            }
        }

        private class LogTraceIgnore
        {
            public string logTypeName = string.Empty;

            public LogTraceIgnore(Type logType)
            {
                this.logTypeName = logType.FullName + ":";
            }
        }

        private static LogTraceIgnore[] m_LogTraceIgnore = new LogTraceIgnore[]
        {
#if UNITY_EDITOR || UNITY_WEBGL
            new LogTraceIgnore(typeof(UnityLogPrintHelper)),
#endif
            new LogTraceIgnore(typeof(Log))
        };

        // 日志输出前缀
        public static Dictionary<LogLevel, string> LogLevelPrefix =
            new Dictionary<LogLevel, string>()
            {
                {LogLevel.DEBUG, " [DEBUG]: "},
                {LogLevel.INFO, " [INFO]: "},
                {LogLevel.WARN, " [WARN]: "},
                {LogLevel.ERROR, " [ERROR]: "},
                {LogLevel.FATAL, " [FATAL]: "},
            };

        private static event Action<LogData> OnLogDataReceived;

        private static event Action<LogData> OnLogDataForgot;

        public static void AddLogReceive(Action<LogData> listener)
        {
            OnLogDataReceived += listener;
        }

        public static void RemoveLogReceive(Action<LogData> listener)
        {
            OnLogDataReceived -= listener;
        }

        public static void AddLogForgot(Action<LogData> listener)
        {
            OnLogDataForgot += listener;
        }

        public static void RemoveLogForgot(Action<LogData> listener)
        {
            OnLogDataForgot -= listener;
        }

        public static string GetLevelPrefix(LogLevel logLevel)
        {
            return LogLevelPrefix[logLevel];
        }

        private static void SetLogWriter(ILogWriteHelper writerHelper, WriteOption writeOption)
        {
            m_WriterHelper = writerHelper;
            writerHelper.SetWriteOption(writeOption);
        }

        private static void SetLogPrinter(ILogPrintHelper printHelper)
        {
            m_PrintHelper = printHelper;
        }

        public static int LogHistoryCount => m_LogHistory.Count;
        private static Queue<LogData> m_LogHistory = new Queue<LogData>(100);
        private static Queue<LogData> m_CachePrintQueue = new Queue<LogData>(100);
        private static Queue<LogData> m_CacheWriteQueue = new Queue<LogData>(100);
        private static ILogWriteHelper m_WriterHelper;
        private static WriteOption m_WriterOption;
        private static ILogPrintHelper m_PrintHelper;
        private static bool m_IsEnable = true;

        static Log()
        {
            m_WriterOption = GetWriteOption();
            // 设置默认日志打印器和日志保存
#if UNITY_EDITOR || UNITY_WEBGL
            SetLogPrinter(new UnityLogPrintHelper());
#endif
            //SetLogWriter(new TxtLogWriteHelper(), m_WriterOption);
        }

        private static WriteOption GetWriteOption()
        {
            return new WriteOption(
                GameEnv.Path.platformDir,
                GameEnv.LogConfig.clearOldLog,
                GameEnv.LogConfig.logWriteDirPath,
                GameEnv.LogConfig.saveLogOnlyCurrent,
                GameEnv.LogConfig.logFileMaxCount,
                GameEnv.LogConfig.WriteEnable);
        }

        private static bool HasWriter()
        {
            return m_WriterHelper != null;
        }

        private static bool HasPrinter()
        {
            return m_PrintHelper != null;
        }

        private static ILogWriteHelper GetLogWriter()
        {
            return m_WriterHelper;
        }

        private static ILogPrintHelper GetLogPrinter()
        {
            return m_PrintHelper;
        }

        public static void Start()
        {
            GameApp.Ins.LoopManager.AddLoop(Update);
        }

        public static void Init()
        {
            m_WriterOption = GetWriteOption();
            // 设置默认日志打印器和日志保存
#if UNITY_EDITOR || UNITY_WEBGL
            SetLogPrinter(new UnityLogPrintHelper());
#endif
            //UpdateWriter();
            // 绑定Unity输出
            ReceiveUnityLog();
            m_IsEnable = true;
        }

        public static void EnableWrite()
        {
            SetLogWriter(new TxtLogWriteHelper(), m_WriterOption);
            UpdateWriter();
        }

        private static void OnFrameUtilityInitOver()
        {
            GameApp.Ins.LoopManager.AddLoop(Update);
        }

        public static void UpdateWriter()
        {
            GetLogWriter().SetWriteOption(GetWriteOption()); // 重新设置日志写入配置
        }

        private static void ReceiveUnityLog()
        {
            if (GameEnv.LogConfig.isReceiveUnityLog) Application.logMessageReceived += UnityInternalLog;
            else
            {
                Application.logMessageReceived -= UnityInternalLog;
            }
        }

        public static void Stop()
        {

            if (GameEnv.LogConfig.isReceiveUnityLog) Application.logMessageReceived -= UnityInternalLog;
            Close();
        }

        public static void ClearHistory()
        {
            m_LogHistory.Clear();
        }

        private static void Close()
        {
            m_IsEnable = false;
            m_LogHistory.Clear();
            m_CachePrintQueue.Clear();
            m_CacheWriteQueue.Clear();
            OnLogDataReceived = null;
            OnLogDataForgot = null;
            m_WriterHelper = null;
            m_WriterOption = default;
            m_PrintHelper = null;
        }

        public static void Update()
        {
            if (!m_IsEnable) return;
            LogHandle();
        }

        public static void AppUpdate(GameAppMessage appMessage)
        {
            Update();
            if (appMessage.MessageCode == GameAppMessage.code_gameConfig_loadSuccess)
            {
                EnableWrite();
            }
        }

        private static void HandlePrintCache()
        {
            lock (m_CachePrintQueue)
            {
                int count = m_CachePrintQueue.Count;
                if (count != 0 && HasPrinter())
                {
                    count = Mathf.Min(count, GameEnv.LogConfig.logCacheQueueCount);
                    for (int i = 0; i < count; i++)
                    {
                        var data = m_CachePrintQueue.Dequeue();
                        GetLogPrinter().PrintLog(data);
                        data.OnPrinted();
                    }
                }
            }
        }

        private static void HandleWriteCache()
        {
            lock (m_CacheWriteQueue)
            {
                int count = m_CacheWriteQueue.Count;

                if (count != 0 && HasWriter())
                {
                    count = Mathf.Min(count, GameEnv.LogConfig.logCacheQueueCount);
                    for (int i = 0; i < count; i++)
                    {
                        var data = m_CacheWriteQueue.Dequeue();
                        // 写入日志
                        GetLogWriter().WriteLog(data);
                        data.OnWrited();
                    }
                }
            }
        }

        private static void LogHandle()
        {
            HandlePrintCache();
            HandleWriteCache();
        }

        private static void UnityInternalLog(string condition, string stacktrace, LogType type)
        {
            if (condition.Contains(GameEnv.LogConfig.InfoPrefix)) return;
            LogLevel logLevel = LogLevel.INFO;
            switch (type)
            {
                case LogType.Error:
                    logLevel = LogLevel.ERROR;
                    break;

                case LogType.Exception:
                    logLevel = LogLevel.FATAL;
                    break;

                case LogType.Log:
                    logLevel = LogLevel.INFO;
                    break;

                case LogType.Warning:
                    logLevel = LogLevel.WARN;
                    break;

                case LogType.Assert:
                    logLevel = LogLevel.DEBUG;
                    break;
            }

            RecordLog(condition, logLevel, stacktrace,false,true);
        }

        private static void PrintLog(LogData data, bool toCache = false)
        {
            if (!m_IsEnable) return;
            if (!HasPrinter() || toCache)
            {
                // 保存到缓存中
                if (m_CachePrintQueue.Count >= GameEnv.LogConfig.logCacheQueueCount)
                {
                    m_CachePrintQueue.Dequeue();
                }

                m_CachePrintQueue.Enqueue(data);
                OnLogDataReceived?.Invoke(data);
                return;
            }
            OnLogDataReceived?.Invoke(data);
            GetLogPrinter()?.PrintLog(data);
        }

        private static void WriteLog(LogData data, bool toCache = false)
        {
            if (!m_IsEnable) return;
            if (!HasWriter() || toCache)
            {
                // 保存到缓存中
                if (m_CacheWriteQueue.Count >= GameEnv.LogConfig.logCacheQueueCount)
                {
                    m_CacheWriteQueue.Dequeue();
                }

                m_CacheWriteQueue.Enqueue(data);
                return;
            }

            GetLogWriter().WriteLog(data);
        }

        /// <summary>
        /// 获取堆栈信息
        /// </summary>
        /// <returns></returns>
        private static string GetTrackInfo()
        {
            //把无关的log去掉
            var st = StackTraceUtility.ExtractStackTrace();
            for (int i = 0; i < 1; i++)
            {
                st = st.Remove(0, st.IndexOf('\n') + 1);
            }

            bool isFound = false;
            do
            {
                int index = st.IndexOf('\n') + 1;
                isFound = false;
                string line = st.Substring(0, index);
                for (int j = 0; j < m_LogTraceIgnore.Length; j++)
                {
                    if (line.Contains(m_LogTraceIgnore[j].logTypeName))
                    {
                        st = st.Remove(0, index);
                        isFound = true;
                        break;
                    }
                }
            } while (isFound);

            return st;
        }

        private static bool IsEnableLogLevel(LogLevel logLevel)
        {
            if ((GameEnv.LogConfig.logEnablePrintLevel & logLevel) != 0)
            {
                return true;
            }

            return false;
        }

        private static bool IsEnableWriteLevel(LogLevel logLevel)
        {
            if ((GameEnv.LogConfig.logEnableWriteLevel & logLevel) != 0)
            {
                return true;
            }

            return false;
        }

        private static bool IsLogTime(LogLevel logLevel)
        {
            if ((GameEnv.LogConfig.logEnablePrintTimeLevel & logLevel) != 0)
            {
                return true;
            }

            return false;
        }

        private static bool IsLogTrack(LogLevel logLevel)
        {
            if ((GameEnv.LogConfig.logEnablePrintTrackLevel & logLevel) != 0)
            {
                return true;
            }

            return false;
        }

        private static bool IsIgnoreLevel(LogLevel logLevel)
        {
            if ((GameEnv.LogConfig.logEnableLevel & logLevel) != 0)
            {
                return false;
            }

            return true;
        }

        private static bool IsIgnoreMessage(object message)
        {
            string content = message.ToString();
            if (string.IsNullOrEmpty(content)) return true;
            int count = GameEnv.LogConfig.logMsgFilter.Length;
            for (int i = 0; i < count; i++)
            {
                if (content.ToLower().Contains(GameEnv.LogConfig.logMsgFilter[i].ToLower())) return true;
            }

            return false;
        }

        private static void RecordLog(object message, 
            LogLevel logLevel = LogLevel.INFO,
            string track = "",
            bool toCache = false,
            bool isUnityMessage = false)
        {
            if (message == null || message == default) return;
            if (IsIgnoreLevel(logLevel) || IsIgnoreMessage(message)) return;
            if (IsLogTrack(logLevel))
            {
#if !UNITY_EDITOR || ENABLE_LOG_TRACE
                if (string.IsNullOrEmpty(track))
                {
                    track = GetTrackInfo();
                }
#endif
            }
            else
            {
                track = String.Empty;
            }

            LogHandle();
            LogData data = new LogData();
            data.isUnityMessage = isUnityMessage;
            data.logMessageObject = message;
            data.logMessage = UStringBuilderPool.GetSharedStringBuilder(message.ToString());
            data.shortMessage = data.logMessage.ToString(0, Mathf.Min(50, data.logMessage.Length)).Replace("\n","");
            data.logTrack = track;
            data.logLevel = logLevel;
            data.logPrefix = GetLevelPrefix(logLevel); // 获取前缀
            if (IsLogTime(logLevel)) data.logTime = DateTime.Now.ToString("HH:mm:ss");
            if (GameEnv.LogConfig.logQueueCapacity <= m_LogHistory.Count)
            {
                var log = m_LogHistory.Dequeue();
                OnLogDataForgot?.Invoke(log);
            }

            m_LogHistory.Enqueue(data);

            if (IsEnableLogLevel(logLevel))
            {
                PrintLog(data, toCache);
            }

            if (IsEnableWriteLevel(logLevel))
            {
                WriteLog(data, toCache);
            }
        }

        public static void Info(object message)
        {
            RecordLog(message, LogLevel.INFO);
        }

        public static void InfoAsync(object message)
        {
            //ThreadMsgDispatcher.Instance.SendMessage(RecordLog,message, LogLevel.INFO, "",false);
            //RecordLog(message, LogLevel.INFO, "", true);
        }

        public static void InfoFormat(string msg, params object[] objs)
        {
            string message = string.Format(msg, objs);
            RecordLog(message, LogLevel.INFO);
        }

        public static void Warning(object message)
        {
            RecordLog(message, LogLevel.WARN);
        }

        public static void WarningAsync(object message)
        {
            //ThreadMsgDispatcher.Instance.SendMessage(RecordLog,message, LogLevel.WARN, "",false);
            //RecordLog(message, LogLevel.WARN, "", true);
        }

        public static void WarnFormat(string msg, params object[] objs)
        {
            string message = string.Format(msg, objs);
            RecordLog(message, LogLevel.WARN);
        }

        public static void Debug(object message)
        {
            RecordLog(message, LogLevel.DEBUG);
        }

        public static void DebugAsync(object message)
        {
            //ThreadMsgDispatcher.Instance.SendMessage(RecordLog,message, LogLevel.DEBUG, "",false);
            //RecordLog(message, LogLevel.DEBUG, "", true);
        }

        public static void DebugFormat(string msg, params object[] objs)
        {
            string message = string.Format(msg, objs);
            RecordLog(message, LogLevel.DEBUG);
        }

        public static void Error(object message)
        {
            RecordLog(message, LogLevel.ERROR);
        }

        public static void ErrorAsync(object message)
        {
            //RecordLog(message, LogLevel.ERROR, "", true);
            //ThreadMsgDispatcher.Instance.SendMessage(RecordLog,message, LogLevel.ERROR, "",false);
        }

        public static void ErrorFormat(string msg, params object[] objs)
        {
            string message = string.Format(msg, objs);
            RecordLog(message, LogLevel.ERROR);
        }

        public static void Fatal(object message)
        {
            RecordLog(message, LogLevel.FATAL);
        }

        public static void FatalAsync(object message)
        {
            RecordLog(message, LogLevel.FATAL, "", true);
        }

        public static void FatalFormat(string msg, params object[] objs)
        {
            string message = string.Format(msg, objs);
            RecordLog(message, LogLevel.FATAL);
        }

        public static LogData GetLogDataAt(int index)
        {
            if (index >= m_LogHistory.Count) return LogData.Default;
            return m_LogHistory.ElementAt(index);
        }

        public static List<LogData> GetAllLogData()
        {
            lock (m_LogHistory)
            {
                return m_LogHistory.ToList();
            }
        }
    }
}