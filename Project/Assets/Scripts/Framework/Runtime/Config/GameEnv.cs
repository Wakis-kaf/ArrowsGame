using Framework.Runtime.Archives;
using Framework.Runtime.LogSystem;
using Framework.Runtime.UI;
using Framework.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

#region

namespace Framework.Runtime
{
    public static class GameEnv
    {
        /// <summary>
        /// 游戏环境
        /// </summary>
        public static AppEnv AppEnv = AppEnv.WindowEditor;

        /// <summary>
        /// 游戏状态
        /// </summary>
        public static AppStatus AppStatus = AppStatus.Devlop;

        /// <summary>
        /// 是否启用开发平台
        /// </summary>
        public static bool EnableDevlopPlatform = false;

        /// <summary>
        /// 代码热更模式
        /// </summary>
        public static HotCodeModel hotCodeModel;

        /// <summary>
        /// 帧率
        /// </summary>
        public static int frameRate = 60;
        public static bool runInBackground = false;

        public static bool IsEditor()
        {
            return AppEnv == AppEnv.WindowEditor;
        }

        /// <summary>
        /// 游戏是否基于安卓
        /// </summary>
        /// <returns></returns>
        public static bool IsGameForAndroid()
        {
            return AppEnv == AppEnv.AndroidGame;
        }

        /// <summary>
        /// 游戏是否基于苹果
        /// </summary>
        /// <returns></returns>
        public static bool IsGameForIphone()
        {
            return AppEnv == AppEnv.IhoneGame;
        }

        /// <summary>
        /// 游戏是否基于PC
        /// </summary>
        /// <returns></returns>
        public static bool IsGameForPc()
        {
            return AppEnv == AppEnv.WindowGame;
        }

        /// <summary>
        /// 游戏是否基于Web
        /// </summary>
        /// <returns></returns>
        public static bool IsGameForWeb()
        {
            return AppEnv == AppEnv.WebGame ||
                AppEnv == AppEnv.WXWebGame ||
                AppEnv == AppEnv.TikTokWebGame ||
                AppEnv == AppEnv.QuickHandWebGame ||
                AppEnv == AppEnv.WXMiniGame ||
                AppEnv == AppEnv.TikTokMiniGame ||
                AppEnv == AppEnv.QuickHandMiniGame;
        }

        public static bool IsInDevlopMode()
        {
            return AppStatus == AppStatus.Devlop;
        }

        /// <summary>
        /// 游戏是否是不需要外部资源的迷你游戏
        /// </summary>
        /// <returns></returns>
        public static bool IsMiniGame()
        {
            return AppEnv == AppEnv.WXMiniGame ||
                AppEnv == AppEnv.TikTokMiniGame ||
                AppEnv == AppEnv.QuickHandMiniGame;
        }

        public static bool IsUnityEditor()
        {
            return Application.isEditor;
        }

        public static bool TryGetGameConfigJsonStoragePath(out string path, out ResLoadWay resLoadWay)
        {
            if (IsEditor())
            {
                path = GameConfig.FieldName_GameConfigJson;
                resLoadWay = ResLoadWay.Storage;
                return true;
            }
            if (IsMiniGame())
            {
                path = string.Empty;
                resLoadWay = ResLoadWay.Disable;
                return false;
            }
            if (IsGameForWeb())
            {
                resLoadWay = ResLoadWay.Web;
                path = new System.Uri(Utility.Path.PathCombine(GameConfig.GetAssetCDNPath(), GameConfig.FieldName_GameConfigJson)).AbsoluteUri;
                return true;
            }
            else
            {
                resLoadWay = ResLoadWay.Storage;
                path = GameConfig.FieldName_GameConfigJson;
                return true;
            }
        }

        public static bool TryGetInPackageGameConfigJsonPath(out string path, out ResLoadWay resLoadWay)
        {
            if (IsEditor())
            {
                path = Utility.Path.PathCombine(Utility.Path.GetStreamingAssetsPath(), GameConfig.FieldName_GameConfigJson);
                resLoadWay = ResLoadWay.IO;
                return true;
            }

            if (IsMiniGame())
            {
                resLoadWay = ResLoadWay.Resources;
                path = GameConfig.FieldName_GameConfigJsonNoExtension;
                return true;
            }

            if (ResConfig.IsUseGameFirstRes)
            {
                if (ResConfig.FirstResMode == FirstResMode.Resources)
                {
                    resLoadWay = ResLoadWay.Resources;
                    path = GameConfig.FieldName_GameConfigJsonNoExtension;
                    return true;
                }
                if (ResConfig.firstResMode == FirstResMode.RemoteCDN)
                {
                    resLoadWay = ResLoadWay.Web;
                    path = new System.Uri(Utility.Path.PathCombine(GameConfig.GetAssetCDNPath(), GameConfig.FieldName_GameConfigJson)).AbsoluteUri;
                    return true;
                }
                if (ResConfig.FirstResMode == FirstResMode.StreamingAssets)
                {
                    if (!IsGameForPc())
                    {
                        resLoadWay = ResLoadWay.Web;
                        path = new System.Uri(Utility.Path.PathCombine(Utility.Path.GetStreamingAssetsPath(), GameConfig.FieldName_GameConfigJson)).AbsoluteUri;
                        return true;
                    }
                    resLoadWay = ResLoadWay.IO;
                    path = Utility.Path.PathCombine(Utility.Path.GetStreamingAssetsPath(), GameConfig.FieldName_GameConfigJson);
                    return true;
                }
                path = "";
                resLoadWay = ResLoadWay.Storage;
                return false;
            }
            if (IsGameForWeb())
            {
                resLoadWay = ResLoadWay.Web;
                path = new System.Uri(Utility.Path.PathCombine(GameConfig.GetAssetCDNPath(), GameConfig.FieldName_GameConfigJson)).AbsoluteUri;
                return true;
            }
            else
            {
                resLoadWay = ResLoadWay.Storage;
                path = GameConfig.FieldName_GameConfigJson;
                return true;
            }
        }

        public static class LogConfig
        {
            // 日志写入配置
            public static bool clearOldLog = true;

            public static string InfoPrefix = "[FRAME LOG] ";
            public static bool isReceiveUnityLog = true; // 是否接受Unity 日志输出
            public static int logCacheQueueCount = 100;

            // 日志缓存最大队列容量
            public static LogLevel logEnableLevel = (LogLevel)~0;

            //开启打印和写入的日志等级
            public static LogLevel logEnablePrintLevel = (LogLevel)~0;

            public static LogLevel logEnablePrintTimeLevel = LogLevel.FATAL | LogLevel.ERROR;

            //允许打印时间信息的日志等级
            public static LogLevel logEnablePrintTrackLevel = LogLevel.FATAL | LogLevel.ERROR;

            //允许打印日志信息的日志等级
            public static LogLevel logEnableWriteLevel = (LogLevel)~0;

            // 是否清除旧日志
            public static int logFileMaxCount = 10;

            //允许写入日志文件的日志等级
            // 允许打印堆栈信息的日志等级
            public static string[] logMsgFilter = new string[0];

            public static int logQueueCapacity = 200; // 日志队列容量
                                                      // 输出过滤

            public static string logWriteDirPath = "GameLogs/";

            // 相对路径
            public static SaveDirPath saveDirPath = SaveDirPath.PersistencePath;

            // 旧日志文件上限
            public static bool saveLogOnlyCurrent = false; // 只保留当前允许的日志文件,删除其他文件

            // 保存相对路径
            public static bool WriteEnable = true;
        }

        public static class Path
        {
            public static string firstResDirPath = "";
            public static string firstResOutputPath = "";
            public static string platformDir = "";
        }

        public static class ResConfig
        {
            /// <summary>
            /// 是否允许Editor资源加载
            /// </summary>
            public static bool EnableEditorResLoad;

            /// <summary>
            /// 是否加载热更新资源
            /// </summary>
            public static bool EnableHotResLoad;

            /// <summary>
            /// 是否允许Resources资源加载
            /// </summary>
            public static bool EnableResourcesResLoad;

            /// <summary>
            /// 首包资源模式
            /// </summary>
            public static FirstResMode firstResMode = FirstResMode.StreamingAssets;

            /// <summary>
            /// 首包资源加载策略
            /// </summary>
            public static FirstResMode FirstResMode = FirstResMode.StreamingAssets;

            /// <summary>
            /// 是否首包资源本地化
            /// </summary>
            public static bool IsStorageFirstRes = false;

            /// <summary>
            /// 游戏资源更新检测
            /// </summary>
            public static bool IsUpdateResNewset = false;

            /// <summary>
            /// 是否使用首次资源加载游戏
            /// </summary>
            public static bool IsUseGameFirstRes = true;

            /// <summary>
            /// 资源加载策略
            /// </summary>
            public static ResLoadWay resLoadWay = ResLoadWay.Editor;
        }

        public static class SdkConfig
        {
            /// <summary>
            /// 是否启用SDK
            /// </summary>
            public static bool IsUseSdk = true;
        }

        public static class WebConfig
        {
            public static bool EnableUrlStamptime = false;
        }
        public static class ArchiveConfig
        {
            public static bool enableArchiveEncrypt = false;

        }
    }
}

/// <summary>
/// 游戏环境
/// </summary>
public enum AppEnv
{
    [LabelText("编辑器环境")]
    WindowEditor,

    [LabelText("PC游戏")]
    WindowGame,

    [LabelText("网页游戏")]
    WebGame,

    [LabelText("安卓游戏")]
    AndroidGame,

    [LabelText("苹果游戏")]
    IhoneGame,

    [LabelText("微信mini游戏")]
    WXMiniGame,

    [LabelText("抖音游戏")]
    TikTokMiniGame,

    [LabelText("快手迷你游戏")]
    QuickHandMiniGame,

    [LabelText("微信Web游戏")]
    WXWebGame,

    [LabelText("抖音Web游戏")]
    TikTokWebGame,

    [LabelText("快手Web游戏")]
    QuickHandWebGame
}

public enum AppStatus
{
    [LabelText("开发")]
    Devlop,

    [LabelText("提审")]
    Interrogation,

    [LabelText("正式")]
    Release
}

/// <summary>
/// 首包资源模式
/// </summary>
public enum FirstResMode
{
    Resources,
    StreamingAssets,
    RemoteCDN,
}

public enum HotCodeModel
{
    None,
    ToLua,
    HybirdCLR
}

/// <summary>
/// 资源加载模式
/// </summary>
public enum ResLoadWay
{
    Disable,

    [LabelText("编辑器")]
    Editor,

    [LabelText("Resources加载")]
    Resources,

    [LabelText("Web下载")]
    Web,

    [LabelText("平台Storage策略读取")]
    Storage,

    [LabelText("IO读取")]
    IO,

    [LabelText("Addressable加载")]
    Addressable,
}

#endregion