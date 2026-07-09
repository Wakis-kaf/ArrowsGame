using Framework.Runtime.LogSystem;

using System;
using System.Collections.Generic;

namespace Framework.Runtime.MDebugger
{
    public static class GameConsole
    {
        private static Action<string, Action<string, string[]>> m_CMDAddAgent;

        private static Action<string, Action<string, string[]>> m_CMDRemoveAgent;

        private static Action<ConsoleLogVO> m_ConsolePrintAgent;

        private static Action<ConsoleLogVO> m_ConsolePrintCaches;

        private static List<ConsoleLogVO> m_ConsolePrintHistoryCache;

        private static Dictionary<string, Action<string, string[]>> m_Prefix2HandlerCache;

        static GameConsole()
        {
            m_ConsolePrintHistoryCache = new List<ConsoleLogVO>();
            m_Prefix2HandlerCache = new Dictionary<string, Action<string, string[]>>();
        }

        public static void ClearCMDCache()
        {
            m_Prefix2HandlerCache.Clear();
        }

        public static void ClearPrintCache()
        {
            m_ConsolePrintHistoryCache.Clear();
        }

        public static void Debug(string content)
        {
            ConsoleLog(content, LogLevel.DEBUG);
        }

        public static void Error(string content)
        {
            ConsoleLog(content, LogLevel.ERROR);
        }

        public static void Fatal(string content)
        {
            ConsoleLog(content, LogLevel.FATAL);
        }

        public static void Info(string content)
        {
            ConsoleLog(content, LogLevel.INFO);
        }

        public static void RegisterCMD(string prefix, Action<string, string[]> func = null)
        {
            if (m_CMDAddAgent != null)
                m_CMDAddAgent?.Invoke(prefix, func);
            else
            {
                if (!m_Prefix2HandlerCache.ContainsKey(prefix))
                {
                    m_Prefix2HandlerCache.Add(prefix, func);
                }
                else if (func != null)
                {
                    m_Prefix2HandlerCache[prefix] += func;
                }
            }
        }

        public static void RemoveCMD(string prefix, Action<string, string[]> func = null)
        {
            if (m_CMDAddAgent != null)
                m_CMDRemoveAgent?.Invoke(prefix, func);
            else if (func != null && m_Prefix2HandlerCache.ContainsKey(prefix))
            {
                m_Prefix2HandlerCache[prefix] -= func;
            }
        }

        public static void RemoveCMDAddAgent()
        {
            m_CMDAddAgent = null;
        }

        public static void RemoveCMDRemoveAgent()
        {
            m_CMDRemoveAgent = null;
        }

        public static void RemoveConsolePrintAgent()
        {
            m_ConsolePrintAgent = null;
        }

        public static void SetCMDAddAgent(Action<string, Action<string, string[]>> agent)
        {
            foreach (var kvp in m_Prefix2HandlerCache)
            {
                agent?.Invoke(kvp.Key, kvp.Value);
            }

            m_CMDAddAgent = agent;
        }

        public static void SetCMDRemoveAgent(Action<string, Action<string, string[]>> agent)
        {
            m_CMDRemoveAgent = agent;
        }

        public static void SetConsolePrintAgent(Action<ConsoleLogVO> agent)
        {
            if (m_ConsolePrintHistoryCache.Count != 0)
            {
                for (int i = 0; i < m_ConsolePrintHistoryCache.Count; i++)
                {
                    agent?.Invoke(m_ConsolePrintHistoryCache[i]);
                }
            }

            m_ConsolePrintAgent = agent;
        }

        public static void Warning(string content)
        {
            ConsoleLog(content, LogLevel.WARN);
        }

        private static void ConsoleLog(string content, LogLevel logLevel)
        {
            var logItem = new ConsoleLogVO(content, logLevel);
            if (m_ConsolePrintAgent != null)
                m_ConsolePrintAgent?.Invoke(logItem);
            else
            {
                m_ConsolePrintHistoryCache.Add(logItem);
            }
        }

        public struct ConsoleLogVO
        {
            public string content;
            public LogLevel logLevel;

            public ConsoleLogVO(string content, LogLevel logLevel)
            {
                this.content = content;
                this.logLevel = logLevel;
            }
        }
    }
}