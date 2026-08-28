---
type: code-knowledge-graph
status: active
updated: 2026-08-28
source: Project/Assets/Scripts
scope: module-and-entrypoint
---

# ArrowsGame 代码知识图谱

## 使用规则

这是项目代码关系的首要导航文档。修改代码前，先阅读本图谱，再按任务读取对应源码；图谱与源码不一致时以源码为准，并在同一任务结束前更新图谱。该文件描述模块级关系和关键入口，不替代源码、测试或详细设计文档。

## 总体分层

```text
GameAppStarter (GameAppEntry)
        |
        v
FrameworkRuntime: GameApp -> MainGame -> GameLoading -> GameModuleManager
        |
        v
GameRuntime: GameModuleFactory -> Game Modules -> Client/Data/View/Server handlers
        |
        +--> Scenes / Prefabs / Addressables / Cehua configs
```

## 程序集节点

| 节点 | 位置 | 责任 | 依赖方向 |
|---|---|---|---|
| `FrameworkCommon` | `Project/Assets/Scripts/Framework/Common` | 集合、工具、扩展和通用异常 | Unity/.NET |
| `FrameworkLibrary` | `Project/Assets/Scripts/Framework/Library` | LitJson、协程、通用组件 | FrameworkCommon |
| `FrameworkRuntime` | `Project/Assets/Scripts/Framework/Runtime` | 生命周期、模块管理、资源、UI、输入、战斗等基础设施 | Common/Library/第三方 |
| `FrameworkEditor` | `Project/Assets/Scripts/Framework/Editor` | 编辑器工具和资源导入 | FrameworkRuntime |
| `GameRuntime` | `Project/Assets/Scripts/Game/Runtime` | 箭头游戏业务模块 | FrameworkRuntime + 第三方 |

## 启动与模块注册

```text
Scenes/Entry.unity
  -> GameAppStarter.Awake()
  -> GameApp.CreateInstance()
  -> GameApp.StartApplication()
  -> MainGame / GameLoading
  -> msg_gamemodules_loaded
  -> GameModuleFactory.GameModuleList
  -> GameModuleManager.LoadGameModule / StartGameModule
```

关键入口：

- `Project/Assets/Scripts/GameAppEntry/GameAppStarter.cs`
- `Project/Assets/Scripts/Framework/Runtime/Core/GameApp.cs`
- `Project/Assets/Scripts/Framework/Runtime/Core/MainGame.cs`
- `Project/Assets/Scripts/Game/Runtime/Modules/GameModuleFactory.cs`

## Game 模块节点

所有模块通常由 `Game<Feature>Module`、`ClientHandler`、`DataHandler`、`ViewHandler`、`ServerHandler`、`Utils/Constant/Model/View` 组成；实际文件以目录为准。

| 模块 | 文件数 | 主要职责 | 关键关系 |
|---|---:|---|---|
| `Arrows` | 23 | 箭头棋盘、节点、线段、关卡状态、镜头交互 | `Play`、`Stage`、`SceneUnit`、`Input` |
| `Play` | 14 | 对局 UI、开始/成功/失败/设置流程 | `Arrows`、`MessageDispatcher` |
| `Stage` | 7 | 局内阶段和场景实体承载 | `Arrows`、`SceneUnit` |
| `SceneUnit` | 11 | 场景单位生命周期、对象池、角色/模型实体 | `FrameworkRuntime`、`Stage` |
| `Input` | 9 | 输入模块、摇杆、输入事件 | `Arrows`、`Player` |
| `Player` | 9 | 玩家实体、移动和相机控制 | `Input`、`SceneUnit` |
| `AI` | 9 | A* 节点和 AI 逻辑 | `SceneUnit`、玩法模块 |
| `ArrowGenerateEdit` | 9 | 箭头关卡/点位编辑器运行时支持 | `Arrows`、配置 |
| `Scene` | 10 | 网格、地图和场景配置 | `Stage`、`SceneUnit` |
| `Config` | 7 | 游戏配置读取和处理 | Framework 配置/导表 |
| `Manage` | 12 | 存档、局外管理和归档模型 | `Config`、`Inventory` |
| `Inventory` | 16 | 道具和背包数据 | `Manage`、配置 |
| `Guide` | 14 | 新手引导状态机和触发器 | `MessageDispatcher`、UI |
| `Task` | 9 | 任务模块（当前工厂中注释注册） | `Config`、`Manage` |
| `Audio` | 7 | 游戏音频业务封装 | Framework Audio |
| `FX` | 7 | 特效业务封装 | `SceneUnit`、Pool |
| `Bar` | 6 | 血条/单位条 UI | `SceneUnit`、UI |
| `Tip` | 10 | 飘字和提示对象池 | UI、Pool |
| `RedPoint` | 9 | 红点状态和 UI 扩展 | UI、MessageDispatcher |
| `ModelCapture` | 7 | 模型展示/截图流程 | Scene、UI |
| `GameLoading` | 1 | 游戏业务加载面板 | Framework GameLoading |
| `Common` | 2 | Game 层公共类型和参数 | 各 Game 模块 |

## 箭头玩法主链

```text
GamePlayModule / GamePlayViewHandler
  -> GameArrowsModule
  -> LevelVO / LevelArrowsBoard / LevelArrowsBoardGeneator
  -> ArrowsGameStage
  -> ArrowPointSceneUnit / ArrowLineSceneUnit
  -> MessageDispatcher (level status, line change, success/fail)
  -> PlaySuccessPanel / PlayFailPanel
```

关键文件：

- `Project/Assets/Scripts/Game/Runtime/Modules/Arrows/GameArrowsModule.cs`
- `Project/Assets/Scripts/Game/Runtime/Modules/Arrows/Model/LevelVO.cs`
- `Project/Assets/Scripts/Game/Runtime/Modules/Arrows/Model/LevelArrowsBoard.cs`
- `Project/Assets/Scripts/Game/Runtime/Modules/Arrows/Model/LevelArrowsBoardGeneator.cs`
- `Project/Assets/Scripts/Game/Runtime/Modules/Arrows/Entity/ArrowsGameStage.cs`
- `Project/Assets/Scripts/Game/Runtime/Modules/Play/GamePlayViewHandler.cs`

## 框架关键节点

```text
GameApp
  -> ModuleManager / GameModuleManager
  -> MessageDispatcher
  -> UnitManager / LoopManager
  -> Asset / UI / Audio / Archive / Input / Combat / SceneUnit / Pool
```

## 外部资源关系

- `Project/Assets/Scenes/Entry.unity`：启动场景。
- `Project/Assets/Scenes/Dev.unity`、`LevelEdit.unity`：开发和编辑场景。
- `Project/Assets/AddressableResources/Configs/`：配置资源。
- `Cehua/Excel/`：策划表；`Cehua/导表参考/Output/cfg_export.ts`：导表输出参考。
- `Project/Assets/Plugins/UniTask/`：UniTask 实现和 Unity 扩展。

## 维护规范

1. 新增/删除模块、改变模块注册顺序或改变程序集依赖时，必须更新对应节点和关系。
2. 修改启动链、消息事件、箭头玩法主链或外部资源入口时，必须更新对应流程图和关键文件。
3. 普通实现细节不必逐行登记；只有影响导航、依赖或行为边界的变化才写入图谱。
4. 更新时记录 `updated` 日期，并在交接卡的当前状态中注明图谱是否同步。
