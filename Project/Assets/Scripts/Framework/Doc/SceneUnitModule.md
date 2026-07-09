# 场景单位模块使用说明

> 本文介绍 UnitFramework 的场景单位系统，包括单位根节点管理、事件通信与场景对象组织。

---

## 1. 模块入口

场景单位模块入口为 [`SceneUnitManager`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/SceneUnit/Base/SceneUnitManager.cs)。

快捷访问：

```csharp
GameApp.SceneUnitManager
```

---

## 2. 核心概念

场景单位系统为场景中的动态对象提供统一根节点与事件机制。预定义了三种根节点：

| 根节点 | 用途 |
| --- | --- |
| `SceneUnitsRoot` | 通用场景单位 |
| `SceneRoleUnitsRoot` | 角色单位 |
| `SceneAnimalUnitsRoot` | 动物/生物单位 |

---

## 3. 获取根节点

```csharp
Transform sceneRoot = GameApp.SceneUnitManager.SceneUnitsRoot;
Transform roleRoot = GameApp.SceneUnitManager.SceneRoleUnitsRoot;
Transform animalRoot = GameApp.SceneUnitManager.SceneAnimalUnitsRoot;
```

---

## 4. 挂载对象

将动态生成的对象挂到对应根节点下，避免场景切换时被意外销毁：

```csharp
GameObject enemy = GameObject.Instantiate(enemyPrefab);
enemy.transform.SetParent(GameApp.SceneUnitManager.SceneUnitsRoot, false);
```

---

## 5. 场景单位事件

[`SceneUnitEventDispatcher`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/SceneUnit/Base/SceneUnitEventDispatcher.cs) 提供场景内单位之间的事件通信。

### 5.1 注册事件

```csharp
SceneUnitEventDispatcher.RegisterEvent("EnemyDie", OnEnemyDie);
```

### 5.2 派发事件

```csharp
SceneUnitEventDispatcher.DispatchEvent("EnemyDie", enemyId);
```

### 5.3 移除事件

```csharp
SceneUnitEventDispatcher.UnRegisterEvent("EnemyDie", OnEnemyDie);
```

---

## 6. 业务扩展

业务可继承场景单位实体实现角色、道具、特效等：

```csharp
public class GameRoleUnit : SceneUnitBase
{
    public void Init(int roleId)
    {
        // 初始化角色
    }

    protected override void OnUnitUpdate()
    {
        base.OnUnitUpdate();
        // 每帧逻辑
    }
}
```

---

## 7. 完整示例

```csharp
public class GameSceneUnitClientHandler : GameModuleLogicHandler
{
    private Transform m_SceneRoot;

    protected override void OnHandlerStart()
    {
        m_SceneRoot = GameApp.SceneUnitManager.SceneUnitsRoot;
        SceneUnitEventDispatcher.RegisterEvent("SpawnEnemy", OnSpawnEnemy);
    }

    private void OnSpawnEnemy(object data)
    {
        if (data is Vector3 pos)
        {
            GameObject enemy = GameObject.Instantiate(enemyPrefab);
            enemy.transform.SetParent(m_SceneRoot, false);
            enemy.transform.position = pos;
        }
    }
}
```

---

## 8. 最佳实践

- 场景中的动态对象统一挂到 `SceneUnitsRoot` 下。
- 角色对象挂到 `SceneRoleUnitsRoot`，方便相机或管理器统一处理。
- 场景单位之间解耦通信使用 `SceneUnitEventDispatcher`。
