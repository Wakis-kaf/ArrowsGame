using Framework.Runtime.UI;
using Framework.Utils;

using System;
using UnityEngine;

namespace Framework.Runtime.LogSystem
{
    [Flags]
    public enum LogLevel
    {
        DEBUG = 1 << 1, // 开发过程中的一些运行信息
        INFO = 1 << 2, // 一些重要的或者比较感兴趣的信息，用于生产环境中的一些重要信息
        WARN = 1 << 3, // 警告信息用于提供一些提示
        ERROR = 1 << 4, // 显示发生错误信息，但不影响系统的继续运行打印错误和异常信息
        FATAL = 1 << 5, // 指出每个严重的错误事件将会导致应用程序的退出，重大错误，可以直接停止程序；
    }

    public static class LogConfig
    {
        //public static int LogQueueCapacity = 500; // 日志容量， -1 表示无限制
        //public static int LogCacheQueueCount = 100; //日志缓存队列容量
        //public static int LogCacheHandleMaxCount = 100; //日志缓存每帧最大处理数量
        //public static bool WriteEnable = false; //日志写入
        //public static LogLevel EnableLevel = (LogLevel) (~0); //允许输出日志等级
        //public static LogLevel EnablePrintLevel = (LogLevel) (~0); //允许打印日志等级
        //public static LogLevel EnableWriteLevel = (LogLevel) (~0); //允许Write日志等级
        //public static LogLevel EnableTimeLevel = LogLevel.FATAL | LogLevel.ERROR; //允许输出日志时间
        //public static LogLevel EnableTrackLevel = LogLevel.FATAL | LogLevel.ERROR; //堆栈输出允许等级
        //public static List<string> MessageFilterStrs = new List<string>(); //输出过滤

        //// 日志写入配置
        //public static bool clearOldLog = true; // 是否清除旧日志
        //public static int logFileMaxCount = 20; // 旧日志文件上限
        //public static bool saveLogOnlyCurrent = false; // 只保留当前允许的日志文件,删除其他文件
        //public static string logWriteDirPath = "/GameLogs/"; // 相对路径
        //public static string saveDirPath = ""; // 保存相对路径

        //public static string InfoPrefix = "[FRAME LOG] ";

        public static Color GetLevelColor(LogLevel logLevel)
        {
            Color color = Color.black;
            switch (logLevel)
            {
                case LogLevel.DEBUG:
                    //color = UIUtil.Hex2Color("#96c24e");
                    color = UIUtil.Hex2Color("#2775b6");
                    break;

                case LogLevel.INFO:
                    color = Color.white;
                    break;

                case LogLevel.WARN:
                    color = Color.yellow;
                    break;

                case LogLevel.ERROR:
                    color = UIUtil.Hex2Color("#ed5a65");
                    break;

                case LogLevel.FATAL:
                    color = UIUtil.Hex2Color("#cc163a");
                    break;
            }

            return color;
        }

        public static string GetLevelColorString(LogLevel logLevel, string content)
        {
            return Utility.StringUtil.Concat($"<color=#{GetLevelHExColor(logLevel)}>", content, "</color>");
        }

        public static string GetLevelHExColor(LogLevel logLevel)
        {
            return UIUtil.Color2Hex(GetLevelColor(logLevel));
        }
    }
}