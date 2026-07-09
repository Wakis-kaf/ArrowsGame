# 输入模块使用说明

> 本文介绍 UnitFramework 的输入系统，包括分层输入、按键检测、轴值读取、快捷键注册。

---

## 1. 模块入口

输入模块入口为 [`InputModule`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/Input/InputModule.cs)。

快捷访问：

```csharp
GameApp.InputModule
```

---

## 2. 核心概念

### 2.1 输入层（InputLayer）

框架按层管理输入，每个 [`InputLayer`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/Input/InputController.cs) 包含一组输入项，可独立启用/禁用。

```csharp
InputLayer layer = GameApp.InputModule.GetController("Gameplay");
layer.SetEnable(true);
```

### 2.2 输入类型

```csharp
public enum InputType
{
    Keyboard,        // 键盘按键
    MouseButton,     // 鼠标按键
    Button,          // UGUI Button
    ScrollXValue,    // 滚轮 X
    ScrollYValue,    // 滚轮 Y
    AxisValue,       // Input Axis
    MousePosition,   // 鼠标位置
}
```

### 2.3 LayerBlocker

[`LayerBlocker`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/Input/LayerBlocker.cs) 封装底层输入源，支持按键状态查询与事件绑定。

---

## 3. 常用 API

### 3.1 获取输入层

```csharp
InputLayer gameplayLayer = GameApp.InputModule["Gameplay"];
InputLayer uiLayer = GameApp.InputModule.GetController("UI");
```

### 3.2 按键检测

```csharp
// 是否刚按下
bool isDown = gameplayLayer.IsKeyDown("Jump");

// 是否按住
bool isPushing = gameplayLayer.IsKeyPushing("Jump");

// 是否刚释放
bool isUp = gameplayLayer.IsKeyUp("Jump");
```

### 3.3 轴值读取

```csharp
float value = gameplayLayer.Value("Horizontal");
float rawValue = gameplayLayer.ValueRaw("Horizontal");
```

### 3.4 鼠标位置

```csharp
float mouseX = GameApp.InputModule.GetMousePositionX();
float mouseY = GameApp.InputModule.GetMousePositionY();
Vector2 pos = gameplayLayer.Pos("MousePos");
```

### 3.5 是否点击在 UI 上

```csharp
bool isOverUI = GameApp.InputModule.IsPointOverGameObject();
```

---

## 4. 注册输入层

通过配置数组批量注册：

```csharp
InputLayerOption[] options = new InputLayerOption[]
{
    new InputLayerOption("Gameplay")
    {
        Enable = true,
        Inputs = new InputModule.InputData[]
        {
            new InputModule.InputData { inputName = "Jump", inputType = InputModule.InputType.Keyboard, keyCode = KeyCode.Space },
            new InputModule.InputData { inputName = "Fire", inputType = InputModule.InputType.MouseButton, mouseKey = 0 },
            new InputModule.InputData { inputName = "Horizontal", inputType = InputModule.InputType.AxisValue, axisName = "Horizontal" }
        }
    }
};

GameApp.InputModule.RegisterLayerMap(options);
```

---

## 5. 快捷键

### 5.1 注册快捷键

```csharp
GameApp.InputModule.RegisterShortcut(
    "Gameplay",       // 层名
    "OpenMenu",       // 输入名
    InputModule.ShortcutType.Down, // 触发类型
    (data) => { Panel.OpenPanel<GameMenuPanel>(); },
    null);
```

触发类型：

- `Down`：刚按下触发一次
- `Press`：按住持续触发
- `Up`：释放时触发一次

### 5.2 取消注册

```csharp
GameApp.InputModule.UnRegisterShortcut("Gameplay", "OpenMenu", InputModule.ShortcutType.Down, callback);
GameApp.InputModule.UnRegisterAllShortcuts("Gameplay");
```

---

## 6. 完整示例

```csharp
public class GameInputClientHandler : GameModuleLogicHandler
{
    private InputLayer m_GameplayLayer;

    protected override void OnHandlerStart()
    {
        m_GameplayLayer = GameApp.InputModule.GetController("Gameplay");

        GameApp.InputModule.RegisterShortcut(
            "Gameplay",
            "Pause",
            InputModule.ShortcutType.Down,
            OnPause);
    }

    protected override void OnHandlerDestroy()
    {
        GameApp.InputModule.UnRegisterShortcut(OnPause);
    }

    private void OnPause(object data)
    {
        Panel.OpenPanel<PausePanel>();
    }

    public void Tick()
    {
        if (m_GameplayLayer == null) return;

        float h = m_GameplayLayer.Value("Horizontal");
        float v = m_GameplayLayer.Value("Vertical");
        bool jump = m_GameplayLayer.IsKeyDown("Jump");

        // 驱动角色移动
    }
}
```

---

## 7. 最佳实践

- 不同游戏状态使用不同输入层（Gameplay / UI / Menu），通过 `SetEnable` 切换。
- 移动、攻击等持续检测放在 `OnUnitUpdate` 或 `Update` 中每帧读取。
- 菜单、技能等一次性触发使用 `RegisterShortcut`。
- UI 拦截点击时使用 `IsPointOverGameObject()`。
