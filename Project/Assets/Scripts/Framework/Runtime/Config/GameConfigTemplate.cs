using Framework.Runtime.Archives;
using Framework.Runtime.LogSystem;
using Framework.Utils;

using System;
using System.Collections.Generic;

public class DebuggerConfigTemplate
{
    public bool isForceShowDebugger = false;
}

[Serializable]
public class GameConfigTemplate
{
    public List<string> cdnList;
    public DebuggerConfigTemplate debuggerConfig;
    public bool enableSDK = true;
    public bool isDebugMode = true;
    public LogConfigTemplate logConfig;
    public LuaConfigTemplate luaConfig;
    public string plafromtDir;
    public ResourcesConfigTemplate resourcesConfig;

    public static GameConfigTemplate GetInitGameConfigTemplate()
    {
        GameConfigTemplate template = new GameConfigTemplate();
        template.isDebugMode = true;
        template.plafromtDir = Utility.Path.GetPersistentDataPath();
        template.logConfig = new LogConfigTemplate();
        template.luaConfig = new LuaConfigTemplate();
        template.resourcesConfig = new ResourcesConfigTemplate();
        template.debuggerConfig = new DebuggerConfigTemplate();
        return template;
    }
}

public class LogConfigTemplate
{
    // 日志写入配置
    public bool clearOldLog = true;

    // 日志输入
    public bool isReceiveUnityLog; // 是否接受Unity 日志输出

    public int logCacheQueueCount = 100;

    // 日志缓存最大队列容量
    public LogLevel logEnableLevel = (LogLevel)~0;

    //开启打印和写入的日志等级
    public LogLevel logEnablePrintLevel = (LogLevel)~0;

    public LogLevel logEnablePrintTimeLevel = LogLevel.FATAL | LogLevel.ERROR;

    //允许打印时间信息的日志等级
    public LogLevel logEnablePrintTrackLevel = LogLevel.FATAL | LogLevel.ERROR;

    //允许打印日志信息的日志等级
    public LogLevel logEnableWriteLevel = (LogLevel)~0;

    // 是否清除旧日志
    public int logFileMaxCount = 20;

    //允许写入日志文件的日志等级
    // 允许打印堆栈信息的日志等级
    public string[] logMsgFilter = new string[0];

    public int logQueueCapacity = 1000; // 日志队列容量
                                        // 输出过滤

    public string logWriteDirPath = "GameLogs/";

    // 相对路径
    public SaveDirPath saveDirPath = SaveDirPath.PersistencePath;

    // 旧日志文件上限
    public bool saveLogOnlyCurrent = true; // 只保留当前允许的日志文件,删除其他文件

    // 保存相对路径
}

public class LuaConfigTemplate
{
    public bool enableLua = true;
    public string entryLua = "GameMain";
    public string luaResDir = "luascripts";
    public bool readLuaFromAB = false;
}

public class ResourcesConfigTemplate
{
    public bool canReadResFromAB = true;

    // 资源加载
    public bool canReadResFromEditor = true;

    // 是否从编辑器中读取资源,只在 editorMode 和Debug模式 才生效 是否从AB包中加载资源
    public bool canReadResFromResources = true;

    public bool isCopyFirstResToLocal = false; // 跳过首包加载

    public bool isSkipResNewest; // 跳过资源热更

    // 是否从Resources中加载资源
    public string resOutPutPath = "";
}