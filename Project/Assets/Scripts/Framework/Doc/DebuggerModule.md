# 调试器模块使用说明

> 本文介绍 UnitFramework 的调试器模块，包括运行时调试面板、日志查看与性能监控。

---

## 1. 模块入口

调试器模块入口为 [`DebuggerModule`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/Debugger/DebuggerModule.cs)。

快捷访问：

```csharp
GameApp.DebuggerModule
```

---

## 2. 核心功能

- 运行时日志查看
- FPS / 内存 / 性能统计
- 快捷按钮与命令
- PC 与移动端两套界面

---

## 3. 开启调试器

在 [`FrameworkSetting`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Core/FrameworkSetting.cs) 中配置：

```csharp
[FoldoutGroup("调试设置")]
public bool enableDebugger = true;
```

开发模式下，`DebuggerModule` 启动后会自动打开调试面板。

---

## 4. 常用 API

### 4.1 显示/隐藏调试器

```csharp
GameApp.DebuggerModule.ShowDebugger();
GameApp.DebuggerModule.HideDebugger();
```

### 4.2 添加自定义调试按钮

```csharp
GameApp.DebuggerModule.AddButton("加金币", () =>
{
    GameManageDataHandler.Ins.AddCoin(1000);
});
```

### 4.3 添加调试信息

```csharp
GameApp.DebuggerModule.AddInfoLine($"当前关卡: {GamePlayDataHandler.Ins.CurrentLevel}");
```

---

## 5. 移动端调试

移动端通常通过手势呼出调试器，例如：

- 三指双击
- 画圈手势

具体触发方式在调试器面板 prefab 上配置。

---

## 6. 完整示例

```csharp
public class GameDebugClientHandler : GameModuleLogicHandler
{
    protected override void OnHandlerStart()
    {
        if (GameApp.DebuggerModule == null) return;

        GameApp.DebuggerModule.AddButton("直接胜利", () =>
        {
            MessageDispatcher.Ins.Dispatch(MessageCode.msg_on_gameSuccess_direcly);
        });
    }
}
```

---

## 7. 最佳实践

- 仅在开发/测试包开启调试器，正式包关闭。
- 调试按钮与命令集中在 `GameDebugClientHandler` 中管理。
- 敏感操作（如加金币、跳关）需要在调试器中做二次确认或仅内部包可用。
