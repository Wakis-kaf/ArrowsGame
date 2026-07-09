# 业务模块开发指南

> 本文介绍如何在 UnitFramework 中创建新的游戏业务模块，包括 Module、Handler、注册流程与代码模板。

---

## 1. 业务模块结构

每个业务模块通常包含以下文件：

```
Assets/Scripts/Game/Runtime/Modules/MyFeature/
├── GameMyFeatureModule.cs          // 模块入口
├── GameMyFeatureClientHandler.cs   // 客户端逻辑
├── GameMyFeatureDataHandler.cs     // 数据/配置
├── GameMyFeatureViewHandler.cs     // UI 视图
├── GameMyFeatureServerHandler.cs   // 服务端/网络（可选）
├── GameMyFeatureConstant.cs        // 常量
└── GameMyFeatureUtils.cs           // 工具方法
```

---

## 2. 模块入口

继承 [`GameModuleBaseInstance<T>`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/GameModule/GameModuleBaseInstance.cs)：

```csharp
namespace Game.Modules.GModuleMyFeature
{
    public class GameMyFeatureModule : GameModuleBaseInstance<GameMyFeatureModule>
    {
        protected override void GenerateHandlers()
        {
            RegisterHandler<GameMyFeatureClientHandler>();
            RegisterHandler<GameMyFeatureDataHandler>();
            RegisterHandler<GameMyFeatureViewHandler>();
        }

        protected override void OnModuleAwake() { }
        protected override void OnModuleStart() { }
        protected override void OnModuleDestroy() { }
    }
}
```

---

## 3. Handler 基类

### 3.1 ClientHandler

```csharp
public class GameMyFeatureClientHandler : GameModuleLogicHandler
{
    public static GameMyFeatureClientHandler Ins => GetModuleHandlerIns<GameMyFeatureClientHandler>();

    protected override void OnHandlerStart()
    {
        // 注册消息、初始化流程
    }
}
```

### 3.2 DataHandler

```csharp
public class GameMyFeatureDataHandler : GameConfigDataHandler
{
    public static GameMyFeatureDataHandler Ins => GetModuleHandlerIns<GameMyFeatureDataHandler>();

    protected override void OnHandlerStart()
    {
        // 读取配置、初始化存档
    }

    public bool TryReadMyConfig(out MyConfig config)
    {
        return TryReadConfig("MyConfig", out config);
    }
}
```

### 3.3 ViewHandler

```csharp
public class GameMyFeatureViewHandler : GameModuleViewHandler
{
    public static GameMyFeatureViewHandler Ins => GetModuleHandlerIns<GameMyFeatureViewHandler>();

    public void OpenMyPanel()
    {
        Panel.OpenPanel<MyPanel>();
    }
}
```

---

## 4. 注册模块

在 [`GameModuleFactory.GameModuleList`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Game/Runtime/Modules/GameModuleFactory.cs) 中添加：

```csharp
public static List<Type> GameModuleList = new List<Type>()
{
    // ...
    typeof(GameMyFeatureModule),
};
```

---

## 5. 模块生命周期

```
Construct → Awake → CheckLoad → Start → Enable → Disable → Destroy
```

每个 Handler 也遵循相同生命周期，对应方法：

- `OnHandlerConstructed()`
- `OnHandlerAwake()`
- `OnHandlerStart()`
- `OnHandlerEnable()`
- `OnHandlerDisable()`
- `OnHandlerDestroy()`

---

## 6. 代码生成工具

框架提供编辑器工具批量生成模块代码：

- 路径：[Framework/Editor/ModuleHelper/](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Editor/ModuleHelper)
- 可通过菜单 `Tools/Framework/Module Helper` 打开。

---

## 7. 完整示例

```csharp
// DataHandler
public class GameTaskDataHandler : GameConfigDataHandler
{
    public static GameTaskDataHandler Ins => GetModuleHandlerIns<GameTaskDataHandler>();

    public CfgTaskMap TaskMap { get; private set; }

    protected override void OnHandlerStart()
    {
        if (TryReadConfig("CfgTask", out TaskMap))
        {
            Log.Info($"任务配置加载完成，共 {TaskMap.Items.Count} 条");
        }
    }
}

// ClientHandler
public class GameTaskClientHandler : GameModuleLogicHandler
{
    public static GameTaskClientHandler Ins => GetModuleHandlerIns<GameTaskClientHandler>();

    protected override void OnHandlerStart()
    {
        MessageDispatcher.Ins.Subscribe(MessageCode.msg_on_game_start, OnGameStart);
    }

    private void OnGameStart()
    {
        var map = GameTaskDataHandler.Ins.TaskMap;
        // 初始化任务状态
    }
}

// ViewHandler
public class GameTaskViewHandler : GameModuleViewHandler
{
    public static GameTaskViewHandler Ins => GetModuleHandlerIns<GameTaskViewHandler>();

    public void OpenTaskPanel()
    {
        Panel.OpenPanel<TaskPanel>();
    }
}
```

---

## 8. 最佳实践

- 一个模块只负责一个业务领域。
- ClientHandler 处理流程与消息，DataHandler 处理数据与配置，ViewHandler 处理 UI。
- 不要在 Handler 中直接持有其他模块 Handler 的引用，通过消息或数据查询解耦。
- 配置读取统一在 DataHandler 中完成，避免多处读取同一份配置。
