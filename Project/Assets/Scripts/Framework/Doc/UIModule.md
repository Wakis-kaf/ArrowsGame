# UI 模块使用说明

> 本文介绍 UnitFramework 的 UI 系统，包括层级、面板、视图、打开/关闭、数据绑定与动画。

---

## 1. 模块入口

UI 模块入口为 [`UIModule`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/UI/UIModule.cs)。

快捷访问：

```csharp
GameApp.UIModule
GameApp.UIModule.UIWindow          // UI 窗口管理器
GameApp.UIModule.UIWindow.PanelManager  // 面板管理器
```

---

## 2. UI 层级

### 2.1 层级常量

在 [`GlobalConstant`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Config/GlobalConstant.cs) 中定义：

```csharp
LAYER_SLIENCE = 0;              // 沉默层，适合放对象池
LAYER_NAVIGATION = 1;           // 导航层
LAYER_INSTRUCTION = 2;          // 跟随世界物体的指示层
LAYER_SCENE = 4;                // 场景层
LAYER_PANEL = 5;                // 普通面板层
LAYER_HIGH_INSTRUCTION = 7;     // 高级指示层
LAYER_ALERT = 6;                // 弹窗层
LAYER_HIGH_PANEL = 8;           // 高级面板层
LAYER_BROADCAST = 9;            // 广播层
LAYER_TIP = 10;                 // 提示层
LAYER_LOADING = 11;             // Loading 层
LAYER_DEBUGGER = 12;            // 调试器层
```

### 2.2 层级管理

[`WindowLayerManager`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/UI/WindowLayerManager.cs) 负责每个层级的窗口排序、打开、关闭。

---

## 3. 核心概念

### 3.1 DisplayUnit

[`DisplayUnit`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/UI/ViewMediator/DisplayUnit.cs) 是 UI 视图基类，提供：

- `DisplayGO`：关联的 GameObject
- `CanvasGroup`：透明/交互控制
- `RectTransform`：矩形变换
- `Data`：视图数据，设置后触发 `OnGUI`
- `Show()` / `Hide()` / `Destroy()`
- `IsShow` / `IsRealShow`

### 3.2 Panel

[`Panel`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/UI/ViewMediator/Panel.cs) 继承 `DisplayUnit`，是游戏中最常用的面板，内置：

- 通用显隐动画（CanvasGroup 淡入淡出 + 缩放）
- 背景遮罩控制
- `OpenPanel<T>()` / `ClosePanel<T>()` 静态方法

### 3.3 UIRoot

[`UIRoot`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/UI/UIRoot.cs) 是 UI 的根节点，所有层级都挂载在其下。

---

## 4. 创建面板

### 4.1 面板脚本

创建 `MyPanel.cs` 继承 `Panel`：

```csharp
using Framework.Runtime.UI;
using UnityEngine;

public class MyPanel : Panel
{
    // 数据变更时调用，渲染 UI
    protected override void OnGUI(object data)
    {
        if (data is MyPanelData panelData)
        {
            // 刷新界面
        }
    }

    protected override void OnShow()
    {
        base.OnShow();
    }

    protected override void OnHide()
    {
        base.OnHide();
    }
}
```

### 4.2 打开面板

```csharp
// 通过 Panel 静态方法打开（需要预制体路径已配置）
Panel.OpenPanel<MyPanel>();

// 指定预制体路径与层级
PanelManager.Ins.OpenPanel<MyPanel>(
    "Assets/AddressableResources/UI/MyPanel/Prefabs/MyPanel.prefab",
    GlobalConstant.LAYER_PANEL);
```

### 4.3 关闭面板

```csharp
Panel.ClosePanel<MyPanel>();

// 或关闭指定实例
PanelManager.Ins.ClosePanel(myPanel);
```

### 4.4 查找面板

```csharp
MyPanel panel = PanelManager.Ins.FindPanel<MyPanel>();
```

### 4.5 传递数据

```csharp
MyPanel panel = Panel.OpenPanel<MyPanel>();
panel.Data = new MyPanelData { Title = "Hello" };
```

---

## 5. 非面板视图（Window / View）

[`UIWindow`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/UI/UIWindow.cs) 提供更底层的视图打开接口：

```csharp
UIWindow.Ins.OpenWindow<MyWindow>(
    "Assets/AddressableResources/UI/MyWindow.prefab",
    GlobalConstant.LAYER_ALERT);
```

关闭：

```csharp
UIWindow.Ins.Close(myWindow);
UIWindow.Ins.DestroyWindow(myWindow);
```

---

## 6. PrefabBind（预制体绑定）

UI 预制体上通常挂载 [`PrefabBinder`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/UI/PrefabBind/PrefabBinder.cs)，用于将子节点按名称绑定到脚本：

```csharp
public class MyPanel : Panel
{
    private UText m_TitleText;
    private UButton m_ConfirmBtn;

    protected override void OnModelLoaded()
    {
        base.OnModelLoaded();
        var binder = PrefabBinder;
        m_TitleText = binder.GetObj<UText>("txt_title");
        m_ConfirmBtn = binder.GetObj<UButton>("btn_confirm");
    }
}
```

命名前缀约定：

| 前缀 | 组件类型 |
| --- | --- |
| `ubtn` | UButton |
| `utxt` | UText |
| `utmpTxt` | UTMPText |
| `uimg` | UImage |
| `usp` | USprite |
| `ulist` | UList |
| `upb` | UProgressBar |
| `rt` | RectTransform |
| `go` | GameObject |

---

## 7. 动画

`Panel` 内置通用动画，可通过 `UIPanel` 组件配置：

- `UseCommonVisibleEffect`：是否使用通用显隐动画
- `UseCommonScaleEffect`：是否使用缩放动画
- `showAnimCaller` / `hideAnimCaller`：自定义动画调用器

---

## 8. 最佳实践

- 面板脚本放在 `Game/Runtime/Modules/XXX/View/` 下。
- 通过 `GameModuleViewHandler` 集中管理面板的打开/关闭。
- 不要在 `OnGUI` 中做复杂逻辑，只负责刷新显示。
- 使用 `MessageDispatcher` 解耦面板之间的调用。
