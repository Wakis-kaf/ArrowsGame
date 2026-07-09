# 日志系统使用说明

> 本文介绍 UnitFramework 的日志系统，包括日志级别、颜色输出、文件写入与自定义接收器。

---

## 1. 模块入口

日志系统入口为 [`Log`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/System/Log/Log.cs)。

快捷访问：

```csharp
Log.Debug("...");
Log.Info("...");
Log.Warn("...");
Log.Error("...");
Log.Fatal("...");
```

---

## 2. 日志级别

```csharp
[Flags]
public enum LogLevel
{
    DEBUG = 1 << 1,
    INFO  = 1 << 2,
    WARN  = 1 << 3,
    ERROR = 1 << 4,
    FATAL = 1 << 5,
}
```

---

## 3. 常用 API

### 3.1 基础输出

```csharp
Log.Debug("调试信息");
Log.Info("普通信息");
Log.Warn("警告信息");
Log.Error("错误信息");
Log.Fatal("致命错误");
```

### 3.2 带格式输出

```csharp
Log.Info($"当前金币: {coin}");
Log.Error($"加载失败: {assetPath}");
```

### 3.3 条件输出

```csharp
Log.Assert(condition, "条件不满足");
```

---

## 4. 配置

日志配置在 [`GameEnv.LogConfig`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Config/GameEnv.cs) 中，通过 [`FrameworkSetting`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Core/FrameworkSetting.cs) 设置：

```csharp
public class LogConfig
{
    public bool WriteEnable;              // 是否写入文件
    public bool clearOldLog;              // 是否清理旧日志
    public string logWriteDirPath;        // 日志目录
    public bool saveLogOnlyCurrent;       // 只保留当前日志
    public int logFileMaxCount;           // 最大日志文件数
    public bool isReceiveUnityLog;        // 是否接收 Unity 原生日志
    public string InfoPrefix;             // 日志前缀
}
```

---

## 5. 自定义打印器与写入器

### 5.1 自定义打印器

实现 `ILogPrintHelper`：

```csharp
public class MyLogPrintHelper : ILogPrintHelper
{
    public void PrintLog(LogData data)
    {
        Debug.Log(data.Content);
    }
}
```

### 5.2 自定义写入器

实现 `ILogWriteHelper`：

```csharp
public class MyLogWriteHelper : ILogWriteHelper
{
    public void SetWriteOption(WriteOption option) { }
    public void WriteLog(LogData data)
    {
        // 写入文件或上传到服务器
    }
}
```

### 5.3 注册接收器

```csharp
Log.OnLogDataReceived += (data) =>
{
    // 自定义处理，如显示在调试面板上
};
```

---

## 6. 颜色输出

[`LogConfig`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/System/Log/LogConfig.cs) 为不同级别提供颜色：

| 级别 | 颜色 |
| --- | --- |
| DEBUG | 蓝色 |
| INFO | 白色 |
| WARN | 黄色 |
| ERROR | 红色 |
| FATAL | 深红 |

---

## 7. 完整示例

```csharp
public class GamePlayClientHandler : GameModuleLogicHandler
{
    protected override void OnHandlerStart()
    {
        Log.Info("GamePlayClientHandler started");
    }

    public void StartLevel(int levelId)
    {
        Log.Debug($"开始关卡: {levelId}");
        if (levelId <= 0)
        {
            Log.Error($"关卡 ID 非法: {levelId}");
            return;
        }
    }
}
```

---

## 8. 最佳实践

- 开发期使用 `Log.Debug`，发布时通过配置关闭。
- 错误与异常使用 `Log.Error`，严重问题使用 `Log.Fatal`。
- 需要保留运行时日志时开启 `WriteEnable`。
