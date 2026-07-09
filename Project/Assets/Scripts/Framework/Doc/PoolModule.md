# 对象池模块使用说明

> 本文介绍 UnitFramework 的对象池系统，包括 GameObject 池的创建、获取、回收与预热。

---

## 1. 模块入口

对象池模块入口为 [`PoolModule`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/Pool/PoolModule.cs)。

快捷访问：

```csharp
GameApp.PoolModule
```

---

## 2. 核心概念

### 2.1 Pool

[`Pool`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/Pool/Core/Pool.cs) 是所有池子的基类，包含：

- `poolInitSize`：初始容量
- `poolLimitCount`：最大容量
- `tagLimitCount`：最大种类数
- `poolName`：池子名称

### 2.2 GameObjectPool

[`GameObjectPool`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/Pool/GameObjectPool/GameObjectPool.cs) 是 GameObject 专用池，支持：

- 按 tag 分类缓存
- 预热
- 延迟回收
- 自动挂载到 `PoolRoot` 下

---

## 3. 创建对象池

```csharp
GameObjectPool pool = GameApp.PoolModule.CreateGameObjectPool("EnemyPool");
```

或使用默认池：

```csharp
GameObjectPool pool = GameApp.PoolModule.GameObjectPool;
```

---

## 4. 获取对象

### 4.1 使用默认池

```csharp
GameObject go = GameApp.PoolModule.GameObjectPool.GetObject(
    "Enemy",
    enemyPrefab,
    position,
    rotation,
    parent);
```

### 4.2 使用自定义池

```csharp
GameObjectPool pool = GameApp.PoolModule.GetPool("EnemyPool") as GameObjectPool;
GameObject go = pool.GetObject("Enemy", enemyPrefab);
```

---

## 5. 回收对象

```csharp
// 立即回收
GameApp.PoolModule.GameObjectPool.PutObject("Enemy", go);

// 延迟回收
GameApp.PoolModule.GameObjectPool.PutObject("Enemy", go, 2f); // 2 秒后回收
```

---

## 6. 预热对象池

```csharp
GameApp.PoolModule.GameObjectPool.PrewarmObject(
    "Enemy",
    enemyPrefab,
    20); // 预先创建 20 个
```

---

## 7. 完整示例

```csharp
public class EnemySpawner
{
    private GameObjectPool m_Pool;
    private GameObject m_EnemyPrefab;

    public void Init()
    {
        m_Pool = GameApp.PoolModule.CreateGameObjectPool("EnemyPool");
        m_Pool.PrewarmObject("Enemy", m_EnemyPrefab, 10);
    }

    public GameObject Spawn(Vector3 pos)
    {
        return m_Pool.GetObject("Enemy", m_EnemyPrefab, pos, Quaternion.identity, null);
    }

    public void Despawn(GameObject enemy)
    {
        m_Pool.PutObject("Enemy", enemy);
    }
}
```

---

## 8. 最佳实践

- 对象池根节点挂载在 `GameAppShell` 下，不随场景切换销毁。
- 业务对象回收前重置状态（如血量、位置、特效）。
- 大量重复生成的对象（敌人、子弹、特效）优先使用对象池。
- 通过 tag 区分不同对象类型，避免池子过于庞大。
