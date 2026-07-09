# UnitFramework 框架使用说明文档

> 文档生成日期：2026-07-08  
> 基于项目目录：`Assets/Scripts/Framework` 与 `Assets/Scripts/Game` 的源码分析整理。

---

## 1. 概述

本项目采用 **Framework（框架层）+ Game（业务层）** 的双层架构：

- **Framework 层**：提供游戏运行所需的基础设施，包括生命周期、模块管理、资源、音频、UI、存档、网络、日志、输入、战斗等通用能力。该层代码位于 `Assets/Scripts/Framework`。
- **Game 层**：基于 Framework 层实现具体业务逻辑，例如登录、主界面、关卡、战斗、任务等。该层代码位于 `Assets/Scripts/Game`，并通过 `GameAppEntry/GameAppStarter.cs` 接入框架启动流程。

框架整体遵循 **Manager-Module-Handler** 的纵向分层思路：

```
GameApp（全局入口）
  ├── Core 系统层：LoopManager / UnitManager / PlatformStorage / MessageDispatcher / CameraStackManager
  ├── Module 层：Asset / UI / Audio / Archive / Input / Combat / SceneUnit / Pool 等
  └── Game 业务层：GameModuleManager → GameModuleBase → Client/Data/View/Server Handler
```

---

## 2. 目录结构

```
Assets/Scripts/
├── Framework/
│   ├── Assets/                 # 框架静态资源（字库等）
│   ├── Common/                 # 通用工具、集合、扩展、异常
│   │   ├── Collections/        # 字典、树、序列化集合等
│   │   ├── Utils/              # Utility 工具箱与 Extension 扩展
│   │   └── FrameworkCommon.asmdef
│   ├── Doc/                    # 框架说明文档（本文件）
│   ├── Editor/                 # 编辑器工具（资源导入、UI 编辑器、模块代码生成等）
│   │   ├── ModuleHelper/       # 游戏模块代码生成模板
│   │   ├── Modules/            # UI、TexturePacker、AssetAutoImporter 等编辑器扩展
│   │   ├── ResEditor/          # 资源构建工具
│   │   └── FrameworkEditor.asmdef
│   ├── Library/                # 运行时小库（LitJson、Coroutine、特效组件等）
│   └── Runtime/                # 运行时核心与模块
│       ├── Config/             # 全局配置、环境变量、常量
│       ├── Core/               # GameApp / MainGame / GameAppShell / LoopManager
│       ├── Modules/            # 框架内置功能模块
│       └── FrameworkRuntime.asmdef
│
├── Game/
│   ├── Editor/                 # 业务编辑器工具（如 GridTile 编辑窗口）
│   └── Runtime/                # 业务运行时
│       ├── Modules/            # 各业务模块（Play/Scene/Player/Audio/...）
│       ├── Library/            # 业务公共库
│       ├── Utils/              # 业务工具
│       └── GameRuntime.asmdef
│
└── GameAppEntry/
    └── GameAppStarter.cs       # 框架启动入口
```

---

## 3. 框架层（Framework Layer）

### 3.1 核心入口与生命周期

框架全局入口为 [`GameApp`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Core/GameApp.cs)。

#### 3.1.1 启动流程

[`GameApp.StartApplication()`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Core/GameApp.cs#L80-L101) 按以下顺序初始化：

1. **BaseInit** — 创建 `LoopManager`、`ModuleManager`、`PlatformStorage`、`UnitManager`、`MessageDispatcher`、`CameraStackManager`。
2. **SystemInit / SystemStart** — 初始化并启动系统层：`GameConfig`、`PlatformStorage`、`Log`、`LoopManager`、`UnitManager`、`GameLoading`。
3. **ModuleInit / ModuleStart** — 通过 `ModuleManager` 注册并构造所有框架模块。
4. **MainGameInit / MainGameAwake** — 创建 `MainGame` 单例，进入业务准备阶段。

启动触发点：

- [`GameAppStarter.Awake()`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/GameAppEntry/GameAppStarter.cs#L9-L15) 调用 `GameApp.CreateInstance()` 与 `StartApplication()`。
- [`GameAppShell`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Core/GameAppShell.cs) 为常驻 MonoBehaviour 壳，承载协程与生命周期。

#### 3.1.2 状态定义

```csharp
public enum GameAppMainState
{
    Sleepy,     // 未启动
    Playing,    // 运行中
    Destroyed,  // 已销毁
}
```

#### 3.1.3 核心消息码

在 [`GameAppMessage`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Core/GameApp.cs#L289-L320) 中定义，常用：

| 消息码 | 含义 |
| --- | --- |
| `code_gameConfig_loadSuccess` | `gameconfig.json` 读取完成 |
| `code_assetModule_loadSuccess` | 资源模块初始化完成 |
| `code_assetModule_newestSuccess` | 资源更新检测完成 |
| `code_gameModule_start` | 主游戏开始，准备加载业务模块 |
| `code_gameSytem_shutdown` | 应用关闭 |

### 3.2 Unit 对象系统

框架将大部分运行时对象抽象为 **Unit**，统一生命周期与资源释放。

| 类型 | 说明 | 路径 |
| --- | --- | --- |
| `IUnitObject` | 对象接口：是否已释放、类型 | [`IUnitObject.cs`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/System/Unit/IUnitObject.cs) |
| `UnitObject` | 基础对象，提供 `Dispose` 与虚方法 | [`UnitObject.cs`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/System/Unit/UnitObject.cs) |
| `BehaviourUnit` | 可挂载到框架生命周期的单元，支持父子关系 | [`BehaviourUnit.cs`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/System/Unit/Base/BehaviourUnit.cs) |
| `MonoBehaviourUnit` | 继承 MonoBehaviour 的单元 | [`MonoBehaviourUnit.cs`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/System/Unit/Base/Mono/MonoBehaviourUnit.cs) |
| `UnitManager` | 管理所有 `IBehaviourUnit` 的注册、启用、禁用、销毁 | [`UnitManager.cs`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/System/Unit/UnitManager.cs) |

`UnitManager` 内置了 Awake / Start / Update / FixedUpdate / LateUpdate / Enable / Disable / Destroy 等 Handler，业务脚本只需实现对应接口（如 `IUnitUpdate`）即可被框架统一驱动。

### 3.3 消息与事件系统

全局消息总线为 [`MessageDispatcher`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/System/Event/MessageDispatcher.cs)，单例访问：

```csharp
MessageDispatcher.Ins.Dispatch("msg_id");
MessageDispatcher.Ins.Dispatch<int>("msg_id", 100);
MessageDispatcher.Ins.Subscribe("msg_id", OnCallback);
MessageDispatcher.Ins.Unsubscribe("msg_id", OnCallback);
```

支持 0~4 个泛型参数，订阅时可传入 `IMessageSubscriber` 实现按对象自动清理。

> 注意：这里的事件是全局字符串消息；战斗系统内部另有 [`CombatEvent`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/Combat/CombatEvent.cs) 事件池。

### 3.4 日志系统

[`Log`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/System/Log/Log.cs) 提供统一日志输出：

```csharp
Log.Debug("...");
Log.Info("...");
Log.Warn("...");
Log.Error("...");
Log.Fatal("...");
```

- 支持颜色高亮、时间戳、堆栈、日志级别过滤。
- 可通过 `Log.AddLogReceive` / `AddLogForgot` 接入自定义打印与文件写入。
- 编辑器下可通过 [`LogRedirect`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Editor/Log/LogRedirect.cs) 捕获 Unity 原生日志。

### 3.5 平台存储

[`PlatformStorage`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/System/PlatformStorage/PlatformStorage.cs) 统一不同平台的文件读写：

- Editor / Standalone / Android：`IOStorageHelper`
- 微信小游戏：`WXPlaftormStorageHelper`

```csharp
PlatformStorage.Instance.TrySaveStorageSync(path, bytes);
PlatformStorage.Instance.TryGetStorageSync(path, out byte[] bytes);
```

### 3.6 循环管理器

[`LoopManager`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/LoopManager.cs) 以协程方式驱动 Update / FixedUpdate / LateUpdate，避免大量 MonoBehaviour 带来的开销。支持：

```csharp
GameApp.Ins.LoopManager.AddLoop(callback, priority);      // 每帧
GameApp.Ins.LoopManager.AddFixedLoop(callback);           // FixedUpdate
GameApp.Ins.LoopManager.AddLateLoop(callback);            // LateUpdate
GameApp.Ins.LoopManager.AddTimer(callback, interval);     // 定时器
GameApp.Ins.LoopManager.AddTimeout(callback, delay);      // 延时回调
GameApp.Ins.LoopManager.AddSecond(callback, second);      // 按秒循环
```

### 3.7 模块系统

#### 3.7.1 ModuleUnit

所有框架模块继承 [`ModuleUnit`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/ModuleUnit.cs)，生命周期：

1. 构造函数触发 `OnInit()`
2. `ModuleManager.UpdateModule()` 调用 `DoConstruct()` 触发 `OnModuleConstructed()`
3. 运行期每帧接收 `OnAppUpdate(GameAppMessage)` 与 `OnAppPopupUpdate(GameAppMessage)`
4. 应用关闭时调用 `Dispose()`

#### 3.7.2 模块注册表

[`ModuleGenerateTable`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/ModuleGenerateTable.cs) 定义了内置模块与加载顺序：

```csharp
{typeof(PoolModule), 1000},
{typeof(ArchiveModule), 3000},
{typeof(AssetManager), 3500},
{typeof(WebRequestModule), 6000},
{typeof(InputModule), 7000},
{typeof(AudioModule), 8000},
{typeof(UIModule), 9000},
{typeof(DebuggerModule), 10000},
{typeof(GameModuleManager), 2000},
{typeof(SceneUnitManager), 12000},
{typeof(CombatSystem), 130000},
{typeof(LanAndThemeModule), 140000},
```

可通过 `BuiltInPluginModule` / `BuiltInGameThemeModule` 扩展第三方或主题模块。

#### 3.7.3 快捷访问

[`GameApp`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Core/GameApp.cs#L270-L287) 提供静态属性访问常用模块：

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

## 4. 框架内置功能模块

### 4.1 资源模块（Asset）

路径：[`Runtime/Modules/Asset/`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/Asset)

核心类：

| 类 | 职责 |
| --- | --- |
| `AssetManager` | 模块入口，统一对外加载接口 | [`AssetManager.cs`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/Asset/AssetManager.cs) |
| `AssetLoader` | 加载器中枢，按 `AssetLink` 分派具体加载器 | [`AssetLoader.cs`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/Asset/AssetLoader.cs) |
| `AddressablesAssetLoader` | Addressables 加载 | [`AddressablesAssetLoader.cs`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/Asset/AddressablesAssetLoader.cs) |
| `ResourcesAssetLoader` | Resources 加载 | [`ResourcesAssetLoader.cs`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/Asset/ResourcesAssetLoader.cs) |
| `EditorAssetLoader` | Editor 直接加载 | [`EditorAssetLoader.cs`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/Asset/EditorAssetLoader.cs) |
| `AssetPathEncoder` | 将原始路径编码为 `AssetLink`，决定加载方式 | [`AssetPathEncoder.cs`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/Asset/AssetPathEncoder.cs) |
| `AssetUpdater` | 资源更新检测 | [`AssetUpdater.cs`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/Asset/AssetUpdater.cs) |
| `AssetPool` | 资源缓存池 | [`AssetPool.cs`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/Asset/AssetPool.cs) |

常用 API：

```csharp
// 异步加载
GameApp.AssetManager.LoadAssetAsync(assetLink, OnLoaded);
GameApp.AssetManager.LoadEnvAsset(path, OnLoaded, AssetType.Prefab);
GameApp.AssetManager.LoadResourcesAsset(path, OnLoaded);

// 同步加载
var assetVO = GameApp.AssetManager.LoadAssetSync(assetLink);
var tex = assetVO.GetAsset<Texture2D>();
```

资源加载结果统一返回 `IAssetVO`，加载完成后通过回调或同步取值。

### 4.2 UI 模块

路径：[`Runtime/Modules/UI/`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/UI)

核心类：

| 类 | 职责 |
| --- | --- |
| `UIModule` | 模块入口，绑定资源加载代理 | [`UIModule.cs`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/UI/UIModule.cs) |
| `UIWindow` | DisplayUnit 创建、打开、关闭、缓存 | [`UIWindow.cs`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/UI/UIWindow.cs) |
| `PanelManager` | Panel 管理、层级、缓存 | [`PanelManager.cs`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/UI/PanelManager.cs) |
| `WindowLayerManager` | 窗口层级排序与显隐 | [`WindowLayerManager.cs`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/UI/WindowLayerManager.cs) |
| `DisplayUnit` | UI 视图基类（Panel/View 均继承） | [`DisplayUnit.cs`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/UI/ViewMediator/DisplayUnit.cs) |
| `Panel` | 面板基类，提供通用显隐动画 | [`Panel.cs`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/UI/ViewMediator/Panel.cs) |
| `UIAgent` | UI 组件代理，对接资源与日志 | [`UIAgent.cs`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/UI/UIAgent.cs) |

层级常量（[`GlobalConstant`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Config/GlobalConstant.cs)）：

```csharp
LAYER_PANEL = 5;       // 普通面板
LAYER_ALERT = 6;       // 弹窗
LAYER_HIGH_PANEL = 8;  // 高级面板
LAYER_TIP = 10;        // 提示
LAYER_LOADING = 11;    // Loading
LAYER_DEBUGGER = 12;   // 调试器
```

打开面板：

```csharp
Panel.OpenPanel<MyPanel>();
PanelManager.Ins.OpenPanel<MyPanel>(prefabPath, GlobalConstant.LAYER_PANEL);
```

### 4.3 音频模块（Audio）

路径：[`Runtime/Modules/Audio/`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/Audio)

核心类：

- `AudioModule`：模块入口
- `AudioSound`：单个声音实例，挂载在 `AudioModule` 根节点下
- `AudioAgent`：音频配置/播放代理

常用 API：

```csharp
GameApp.AudioModule.PlayBgm("AUDIO_BGM_Link", volume);
GameApp.AudioModule.PlayEffect("effect_link");
GameApp.AudioModule.StopBgm();
GameApp.AudioModule.SetBgmVolume(0.5f);
```

### 4.4 存档模块（Archive）

路径：[`Runtime/Modules/Archive/`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/Archive)

核心类：

- `ArchiveModule`：管理存档目录、存档元数据、读写队列
- `Archive`：单个存档对象
- `BinaryArchive` / `JsonArchive`：二进制/JSON 序列化
- `EncryptionTool` / `BinaryArchiveEncryptor`：加密支持

存档支持自动保存、多存档槽位、加密、异步保存队列。

### 4.5 输入模块（Input）

路径：[`Runtime/Modules/Input/`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/Input)

- `InputModule`：按层管理输入，每层由 `InputLayer` + `LayerBlocker` 组成。
- 支持键盘、鼠标、按钮、轴、鼠标位置等多种输入类型。
- 支持注册快捷键：`RegisterShortcut(layerName, inputName, ShortcutType, callback, data)`。

### 4.6 对象池模块（Pool）

路径：[`Runtime/Modules/Pool/`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/Pool)

- `PoolModule` 管理多个 `Pool`。
- `GameObjectPool` 提供 GameObject 的对象池能力。

```csharp
GameApp.PoolModule.CreateGameObjectPool("MyPool");
var pool = GameApp.PoolModule.GetPool<GameObjectPool>();
```

### 4.7 场景单位模块（SceneUnit）

路径：[`Runtime/Modules/SceneUnit/`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/SceneUnit)

- `SceneUnitManager`：管理场景单位根节点（`SceneUnitsRoot`、`SceneRoleUnitsRoot`、`SceneAnimalUnitsRoot`）。
- 提供 `SceneUnitEventDispatcher` 用于场景单位事件通信。
- 业务可继承场景单位实体扩展角色、特效、模型等。

### 4.8 网络/下载模块（WebRequest）

路径：[`Runtime/Modules/WebquesetModule/`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/WebquesetModule)

- `WebRequestModule`：封装 `UnityWebRequestMgr`。
- `WebDownloader`：提供文件下载能力。

### 4.9 战斗模块（Combat）

路径：[`Runtime/Modules/Combat/`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/Combat)

- `CombatSystem`：战斗事件分发、Combator 管理、标签与效果处理。
- `Combator`：战斗参与者。
- `CombatEvent` / `CombatEventPool`：战斗事件与对象池。
- `CombatEffectManager`：根据 EffectTag 触发效果。
- `AttributeBox` / `NumberAttribute` / `NumericFormula`：属性系统。

### 4.10 多语言与主题（LanAndTheme）

路径：[`Runtime/Modules/LanAndTheme/`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/LanAndTheme)

- `LanAndThemeModule` 包含 `Lan2LocalManager` 与 `Theme2LocalManager`。
- 提供 `EndAdapter_TMPText`、`EndAdapter_Image` 等运行时适配组件。

### 4.11 调试器模块（Debugger）

路径：[`Runtime/Modules/Debugger/`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/Debugger)

- `DebuggerModule`：开发模式下自动打开调试面板。
- 支持 PC / 手机两套调试界面。

---

## 5. 游戏层（Game Layer）

### 5.1 入口与启动

[`GameAppStarter`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/GameAppEntry/GameAppStarter.cs) 是业务接入点：

1. `Awake` 中创建 `GameApp` 并启动。
2. 监听 `msg_gamemodules_loaded`。
3. 根据 `GameEnv.hotCodeModel` 决定如何获取 `GameModuleFactory.GameModuleList`：
   - `None`：直接引用当前程序集中的 `GameModuleFactory`。
   - `HybirdCLR`：通过反射从热更 DLL `GameRuntime` 中读取。

### 5.2 业务模块结构

每个业务模块通常由 **Module + 4 类 Handler** 组成：

```
GameXXXModule.cs          // 模块入口，继承 GameModuleBaseInstance<T>
GameXXXClientHandler.cs   // 客户端逻辑/流程控制
GameXXXDataHandler.cs     // 数据、配置读取，继承 GameConfigDataHandler
GameXXXViewHandler.cs     // UI 打开/关闭、视图表现
GameXXXServerHandler.cs   // 服务端模拟或网络协议处理（可选）
GameXXXConstant.cs        // 常量
GameXXXUtils.cs           // 工具方法
```

例如 [`GamePlayModule`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Game/Runtime/Modules/Play/GamePlayModule.cs) 注册了：

```csharp
RegisterHandler<GamePlayClientHandler>();
RegisterHandler<GamePlayDataHandler>();
RegisterHandler<GamePlayViewHandler>();
```

### 5.3 GameModuleBase 生命周期

[`GameModuleBase`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/GameModule/GameModuleBase.cs)：

1. 构造函数 → `OnConstructed()`
2. `AwakeModule()` → `OnModuleAwake()` → `GenerateHandlers()` → Handler 创建
3. `CheckModuleLoad()` → `OnCheckModuleLoad()` → `OnModuleLoaded()`
4. `CheckModuleHandlerLoad()` → 每个 Handler `CheckLoad()` → `OnModuleHandlerLoaded()`
5. `StartModule()` → `OnModuleStart()` → Handler `OnHandlerStart()`
6. `EnableModule()` → `OnModuleEnable()` → Handler `OnHandlerEnable()`
7. `DestroyModule()` → Handler `OnHandlerDestroy()` → `OnModuleDestroy()`

### 5.4 Handler 基类

| 基类 | 职责 |
| --- | --- |
| `GameModuleHandler` | 通用 Handler 基类，提供生命周期与所属模块访问 | [`GameModuleHandler.cs`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/GameModule/GameModuleHandler.cs) |
| `GameModuleLogicHandler` | 逻辑 Handler 标识 | [`GameModuleLogicHandler.cs`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/GameModule/GameModuleLogicHandler.cs) |
| `GameModuleDataHandler` | 数据 Handler 标识 | [`GameModuleDataHandler.cs`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/GameModule/GameModuleDataHandler.cs) |
| `GameModuleViewHandler` | 视图 Handler，提供 `OpenPanel<T>` 等静态辅助 | [`GameModuleViewHandler.cs`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/GameModule/GameModuleViewHandler.cs) |
| `GameConfigDataHandler` | 配置读取辅助，提供 `TryReadConfig<T>` | [`GameConfigDataHandler.cs`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Game/Runtime/Modules/Config/GameConfigDataHandler.cs) |

Handler 单例访问：

```csharp
GamePlayClientHandler.Ins
GamePlayDataHandler.Ins
GamePlayViewHandler.Ins
```

### 5.5 业务模块注册

所有业务模块在 [`GameModuleFactory.GameModuleList`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Game/Runtime/Modules/GameModuleFactory.cs) 中注册：

```csharp
public static List<Type> GameModuleList = new List<Type>()
{
    typeof(GameConfigModule),
    typeof(GameRedPointModule),
    typeof(GameGuidModule),
    typeof(GameInventoryModule),
    typeof(GameSceneUnitModule),
    typeof(GameInputModule),
    typeof(GameTip),
    typeof(GameBar),
    typeof(GameFX),
    typeof(GameAudioModule),
    typeof(GameModelCapture),
    typeof(GameManageModule),
    typeof(GamePlayModule),
    typeof(GameStageModule),
    typeof(GameSceneModule),
    typeof(GamePlayerModule),
    typeof(GameArrowsModule),
    typeof(GameArrowGenerateEditModule),
};
```

### 5.6 配置模块（GameConfigModule）

[`GameConfigModule`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Game/Runtime/Modules/Config/GameConfigModule.cs) 负责加载 `config` Addressable Group 下的所有 TextAsset，并通过 `GameConfigDataHandler.TryReadConfig<T>(configName, out config)` 反序列化为业务配置类。

### 5.7 热更新支持

[`GameModuleManager`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/GameModule/GameModuleManager.cs) 支持 HybridCLR 热更新：

1. `LoadHotUpdateAssemply()` 异步加载热更 DLL。
2. `LoadMetadataForAOTAssemblies()` 加载 AOT 元数据。
3. 加载完成后通过反射创建业务模块实例。

---

## 6. 使用指南

### 6.1 创建新的业务模块

推荐通过编辑器工具生成（[`Editor/ModuleHelper/`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Editor/ModuleHelper)），或手动创建：

1. 在 `Assets/Scripts/Game/Runtime/Modules/` 下新建目录 `MyFeature/`。
2. 创建 `GameMyFeatureModule.cs`：

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

3. 在 `GameModuleFactory.GameModuleList` 中注册该模块。

### 6.2 创建新的 UI 面板

1. 创建 `MyPanel.cs` 继承 `Panel`。
2. 实现 `OnGUI(object data)` 渲染数据。
3. 在 ViewHandler 中打开：

```csharp
Panel.OpenPanel<MyPanel>();
```

如需自定义预制体路径：

```csharp
PanelManager.Ins.OpenPanel<MyPanel>(prefabPath, GlobalConstant.LAYER_PANEL);
```

### 6.3 加载资源

```csharp
GameApp.AssetManager.LoadAssetAsync(assetLink, (assetVO) =>
{
    if (assetVO.IsLoadSuccess)
    {
        var prefab = assetVO.GetAsset<GameObject>();
    }
});
```

### 6.4 播放音频

```csharp
GameAudioClientHandler.Ins.PlayBgm(GameAudioConstant.Bgm_Main);
// 或直接
GameApp.AudioModule.PlayEffect("audio_link");
```

### 6.5 发送/接收消息

```csharp
// 发送
MessageDispatcher.Ins.Dispatch(MessageCode.msg_open_gameplay_panel);

// 订阅
MessageDispatcher.Ins.Subscribe(MessageCode.msg_open_gameplay_panel, OnOpen);
```

### 6.6 使用 Unit 生命周期

让脚本继承 `MonoBehaviourUnit` 或 `BehaviourUnit`，实现 `IUnitAwake`、`IUnitUpdate`、`IUnitDestroy` 等接口，即可被 `UnitManager` 自动驱动。

---

## 7. 配置说明

### 7.1 FrameworkSetting

[`FrameworkSetting`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Core/FrameworkSetting.cs) 是场景中的 ScriptableObject/MonoBehaviour 配置，包含：

- 项目版本、环境、阶段
- 热更新模式（None / ToLua / HybirdCLR）
- 资源加载开关（Editor / Resources / Hot）
- 首包与远程资源模式
- SDK 启用
- 日志、存档加密、帧率、Loading 结束消息等

运行时通过 `UpdateConfig()` 将值同步到 [`GameEnv`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Config/GameEnv.cs)。

### 7.2 gameconfig.json

[`GameConfig`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Config/GameConfig.cs) 负责读取 `gameconfig.json`，支持 Resources / StreamingAssets / Persistent / Web 等多种读取方式。

模板结构见 [`GameConfigTemplate`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Config/GameConfigTemplate.cs)。

---

## 8. 程序集定义（Assembly Definition）

| 程序集 | 路径 | 说明 |
| --- | --- | --- |
| `FrameworkCommon` | [`Common/FrameworkCommon.asmdef`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Common/FrameworkCommon.asmdef) | 纯工具与集合，不依赖 Runtime |
| `FrameworkRuntime` | [`Runtime/FrameworkRuntime.asmdef`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/FrameworkRuntime.asmdef) | 框架运行时核心与模块 |
| `FrameworkEditor` | [`Editor/FrameworkEditor.asmdef`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Editor/FrameworkEditor.asmdef) | 编辑器工具 |
| `GameRuntime` | [`Game/Runtime/GameRuntime.asmdef`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Game/Runtime/GameRuntime.asmdef) | 业务运行时，引用 FrameworkRuntime |

---

## 9. 编辑器工具速览

| 工具 | 路径 | 用途 |
| --- | --- | --- |
| ModuleHelper | [`Editor/ModuleHelper/`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Editor/ModuleHelper) | 生成业务模块代码模板 |
| UI 组件编辑器 | [`Editor/Modules/UI/Component/`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Editor/Modules/UI/Component) | UButton、UList、UText 等自定义 Inspector |
| TexturePacker | [`Editor/Modules/UI/TexturePacker/`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Editor/Modules/UI/TexturePacker) | 图集拆分与压缩 |
| AssetAutoImporter | [`Editor/Modules/AssetAutoImporter/`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Editor/Modules/AssetAutoImporter) | 资源导入规则自动化 |
| ResourcesBuild | [`Editor/ResEditor/`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Editor/ResEditor) | 资源打包与构建 |
| GridTileTool | [`Game/Editor/GridTileTool/`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Game/Editor/GridTileTool) | 场景网格编辑工具 |

---

## 10. 总结

- **Framework 层** 是项目的通用基础设施，提供统一的生命周期、模块管理、资源、UI、音频、存档、日志、输入、战斗等能力。
- **Game 层** 通过 `GameModuleBaseInstance<T>` + Handler 模式组织业务，所有模块在 `GameModuleFactory` 中注册。
- 新增业务功能时，优先使用 `ModuleHelper` 生成标准模板，避免重复造轮子。
- 资源、UI、消息等高频操作均通过 `GameApp` 单例或各模块 `Ins` 静态属性访问，保持调用统一。
