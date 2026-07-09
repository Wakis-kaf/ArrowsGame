# 框架启动与生命周期

> 本文介绍 UnitFramework 的启动流程、核心对象以及生命周期钩子。

---

## 1. 启动入口

框架的启动入口是 [`GameAppStarter`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/GameAppEntry/GameAppStarter.cs)，它通常挂载在一个场景中的 GameObject 上，作为整个游戏的启动器。

```csharp
public class GameAppStarter : MonoBehaviour
{
    private void Awake()
    {
        GameApp.CreateInstance(gameObject);
        GameApp.Ins.StartApplication();
        GameApp.Ins.MessageDispatcher.Subscribe(MessageCode.msg_gamemodules_loaded, OnGameModuleLoaded);
    }
}
```

- `GameApp.CreateInstance()` 创建全局唯一的 [`GameApp`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Core/GameApp.cs) 实例。
- `StartApplication()` 按顺序初始化系统层、模块层、业务层。

---

## 2. GameApp 核心对象

[`GameApp`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Core/GameApp.cs) 是框架全局入口，内部持有以下核心管理器：

| 成员 | 类型 | 职责 |
| --- | --- | --- |
| `LoopManager` | [`LoopManager`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/LoopManager.cs) | Update / FixedUpdate / LateUpdate 协程驱动 |
| `ModuleManager` | [`ModuleManager`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/ModuleManager.cs) | 模块注册、排序、生命周期 |
| `PlatformStorage` | [`PlatformStorage`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/System/PlatformStorage/PlatformStorage.cs) | 跨平台文件读写 |
| `UnitManager` | [`UnitManager`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/System/Unit/UnitManager.cs) | Unit 生命周期管理 |
| `MessageDispatcher` | [`MessageDispatcher`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/System/Event/MessageDispatcher.cs) | 全局消息总线 |
| `CameraStackManager` | [`CameraStackManager`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/Camera/CameraStackManager.cs) | 相机栈管理 |

快捷访问：

```csharp
GameApp.AssetManager
GameApp.UIModule
GameApp.AudioModule
GameApp.ArchiveModule
GameApp.InputModule
GameApp.PoolModule
GameApp.CombatSystem
GameApp.GameModuleManager
...
```

---

## 3. 启动流程

[`GameApp.StartApplication()`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Core/GameApp.cs#L80-L101) 的调用顺序：

```
BaseInit()
  → 创建 LoopManager / ModuleManager / PlatformStorage / UnitManager / MessageDispatcher / CameraStackManager

SystemInit()
  → GameConfig.Init / PlatformStorage.Init / Log.Init / LoopManager.Init / UnitManager.Init / GameLoading.Init

SystemStart()
  → PlatformStorage.Start / GameConfig.Start / Log.Start / LoopManager.Start / UnitManager.Start / GameLoading.Start

ModuleInit()
  → ModuleManager.Init

ModuleStart()
  → ModuleManager.Start（注册并构造所有模块）

MainGameInit()
  → 创建 MainGame GameObject 并初始化

MainGameAwake()
  → 调用 MainGame.AwakeMainGame()

UpdateFromSystem(code_gameSytem_start)
  → 向所有层广播启动完成消息
```

---

## 4. 应用状态

```csharp
public enum GameAppMainState
{
    Sleepy,     // 未启动
    Playing,    // 运行中
    Destroyed,  // 已销毁
}
```

判断运行状态：

```csharp
if (GameApp.IsAppRunning())
{
    // 框架已启动且处于 Playing 状态
}
```

---

## 5. 核心消息码

[`GameAppMessage`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Core/GameApp.cs#L289-L320) 定义了框架级事件：

| 消息码 | 触发时机 |
| --- | --- |
| `code_gameConfig_loadSuccess` | `gameconfig.json` 读取完成 |
| `code_assetModule_loadSuccess` | 资源模块初始化完成 |
| `code_assetModule_newestSuccess` | 资源更新检测完成 |
| `code_gameModule_start` | 主游戏开始，准备加载业务模块 |
| `code_mainGameState_changed` | MainGame 任一准备状态变化 |
| `code_gameSytem_shutdown` | 应用关闭 |

---

## 6. 关闭流程

调用 [`GameApp.QuitApplication()`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Core/GameApp.cs#L48-L64)：

```
StopApplication()
  → UpdateFromSystem(code_gameSytem_shutdown)
  → MainGameStop()
  → ModuleStop()（逆序 Dispose 所有模块）
  → SystemStop()
     → MessageDispatcher.ClearAllMessage
     → UnitManager.Close
     → LoopManager.Stop
     → GameConfig.Stop
     → Log.Stop
     → GameLoading.Close
  → 切换状态为 Destroyed
  → 销毁 GameAppShell
```

---

## 7. MainGame 启动 Gate

[`MainGame`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Core/MainGame.cs) 负责等待多个前置条件完成后才真正启动业务模块：

```csharp
public void AppUpdate(GameAppMessage appMessage)
{
    if (appMessage.MessageCode == GameAppMessage.code_gameConfig_loadSuccess)
        isGameConfigLoadedSuc = true;

    if (appMessage.MessageCode == GameAppMessage.code_assetModule_loadSuccess)
        isAssetModuleLoadSuc = true;

    if (appMessage.MessageCode == GameAppMessage.code_assetModule_newestSuccess)
        isAssetModuleNewestSuc = true;

    CheckMainGameStart();
}
```

当以下三个条件全部满足后，进入 `StartMainGame()`：

1. `isGameConfigLoadedSuc`
2. `isAssetModuleLoadSuc`
3. `isAssetModuleNewestSuc`

之后 `StartMainGame()` 会发送 `code_gameModule_start`，随后 `GameModuleManager.LoadGameModule()` 加载业务模块。

---

## 8. 常用启动期监听

业务代码可以在 `GameAppStarter.OnGameModuleLoaded` 中注册模块列表，或在任意 Handler 中订阅：

```csharp
MessageDispatcher.Ins.Subscribe(MessageCode.msg_mainGame_start, OnMainGameStart);
MessageDispatcher.Ins.Subscribe(MessageCode.msg_gamemodules_loaded, OnGameModulesLoaded);
```

---

## 9. 最佳实践

- 不要在 `Awake` 中直接访问 `GameApp.Ins`，除非确认 `GameAppStarter` 已先执行。
- 业务初始化逻辑放在 `GameModuleHandler.OnHandlerStart` 或 `GameModuleBase.OnModuleStart` 中。
- 需要等待框架消息时，使用 `MessageDispatcher` 订阅而不是轮询。
