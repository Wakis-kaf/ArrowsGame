# Unit 生命周期使用说明

> 本文介绍 UnitFramework 的 Unit 对象系统，包括生命周期接口、BehaviourUnit 与 MonoBehaviourUnit 的使用。

---

## 1. 核心概念

框架将运行时对象抽象为 **Unit**，统一由 [`UnitManager`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/System/Unit/UnitManager.cs) 管理生命周期与 Update 驱动。

核心接口：

| 接口 | 说明 | 文件 |
| --- | --- | --- |
| `IUnitObject` | 对象基础接口 | [`IUnitObject.cs`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/System/Unit/IUnitObject.cs) |
| `IBehaviourUnit` | 可挂载行为的 Unit 接口 | [`IBehaviourUnit.cs`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/System/Unit/Base/IBehaviourUnit.cs) |
| `IUnitEntity` | 组合生命周期接口 | [`IUnitEntity.cs`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/System/Unit/BuiltInImplementation/IUnitEntity.cs) |

---

## 2. 生命周期接口

| 接口 | 触发时机 |
| --- | --- |
| `IUnitAwake` | 注册到 UnitManager 时 |
| `IUnitStart` | Awake 后下一帧 |
| `IUnitEnable` | 启用时 |
| `IUnitUpdate` | 每帧 Update |
| `IUnitFixedUpdate` | 每 FixedUpdate |
| `IUnitLateUpdate` | 每 LateUpdate |
| `IUnitDisable` | 禁用时 |
| `IUnitDestroy` | 销毁时 |

---

## 3. BehaviourUnit

[`BehaviourUnit`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/System/Unit/Base/BehaviourUnit.cs) 是不依赖 MonoBehaviour 的 Unit，适合纯逻辑对象。

```csharp
using Framework.Runtime.UnitSystem.Base;

public class MyLogicUnit : BehaviourUnit, IUnitUpdate, IUnitDestroy
{
    public void OnUnitUpdate()
    {
        // 每帧逻辑
    }

    protected override void DisposeManagedResources()
    {
        base.DisposeManagedResources();
        // 释放托管资源
    }
}
```

创建并注册：

```csharp
MyLogicUnit unit = new MyLogicUnit();
GameApp.Ins.UnitManager.AddBehaviourUnit(unit, "MyLogic");
```

---

## 4. MonoBehaviourUnit

[`MonoBehaviourUnit`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/System/Unit/Base/Mono/MonoBehaviourUnit.cs) 继承 MonoBehaviour，适合需要挂载在 GameObject 上的脚本。

```csharp
using Framework.Runtime.UnitSystem.Base;

public class MyMonoUnit : MonoBehaviourUnit, IUnitAwake, IUnitUpdate, IUnitDestroy
{
    public void OnUnitAwake()
    {
        Debug.Log("Awake");
    }

    public void OnUnitUpdate()
    {
        // 每帧逻辑
    }

    public void OnUnitDestroy()
    {
        Debug.Log("Destroy");
    }
}
```

---

## 5. 父子 Unit

`BehaviourUnit` 支持父子关系，父 Unit 销毁时子 Unit 会自动销毁。

```csharp
IBehaviourUnit parent = new MyLogicUnit();
IBehaviourUnit child = new MyLogicUnit();
parent.AddChildUnit(child);
```

---

## 6. 优先级

`IBehaviourUnit.UnitPriority` 决定同一父 Unit 下子 Unit 的执行顺序：

- 优先级越高，`Awake/Start/Update` 越早执行。
- 销毁时越晚释放资源。

---

## 7. 完整示例

```csharp
public class GameTimer : BehaviourUnit, IUnitUpdate
{
    private float m_Elapsed;
    private Action m_OnComplete;

    public void Init(float duration, Action onComplete)
    {
        m_Elapsed = duration;
        m_OnComplete = onComplete;
    }

    public void OnUnitUpdate()
    {
        m_Elapsed -= Time.deltaTime;
        if (m_Elapsed <= 0)
        {
            m_OnComplete?.Invoke();
            Dispose();
        }
    }
}

// 使用
GameTimer timer = new GameTimer();
timer.Init(5f, () => Log.Info("计时结束"));
GameApp.Ins.UnitManager.AddBehaviourUnit(timer, "GameTimer");
```

---

## 8. 最佳实践

- 优先使用 `BehaviourUnit` 而非 MonoBehaviour，减少引擎开销。
- 需要每帧执行的逻辑实现 `IUnitUpdate`，避免自己写 MonoBehaviour 的 Update。
- 销毁时释放资源，避免内存泄漏。
- 父子关系用于组织复杂的对象树。
