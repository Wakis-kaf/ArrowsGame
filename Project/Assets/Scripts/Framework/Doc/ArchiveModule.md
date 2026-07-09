# 存档模块使用说明

> 本文介绍 UnitFramework 的存档系统，包括存档创建、读取、保存、加密与自动保存。

---

## 1. 模块入口

存档模块入口为 [`ArchiveModule`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/Archive/ArchiveModule.cs)。

快捷访问：

```csharp
GameApp.ArchiveModule
```

---

## 2. 核心概念

### 2.1 Archive

[`Archive`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/Archive/Archive.cs) 是单个存档对象，业务数据通过继承 `Archive` 实现持久化。

```csharp
[Serializable]
public class PlayerArchive : Archive
{
    public int level;
    public int coin;
    public List<string> items = new List<string>();
}
```

### 2.2 存档类型

```csharp
public static class ArchiveTypeCode
{
    public const int SystemData = 1;  // 系统数据
    public const int GameArchive = 0; // 游戏存档
}
```

### 2.3 序列化与加密

[`ArchiveManager`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/Archive/ArchiveModule.ArchiveManager.cs) 内部组合：

- `IArchiveSerializer`：JSON / Binary 序列化器
- `IArchiveEncryptor`：可选加密器

---

## 3. 创建存档

```csharp
PlayerArchive archive = GameApp.ArchiveModule.CreateArchive<PlayerArchive>("player_save");
```

---

## 4. 保存存档

### 4.1 立即保存

```csharp
archive.Save();                              // 覆盖保存
archive.Save(false);                         // 不覆盖（若已存在则不保存）
archive.Save(true, OnSuccess, OnFail);       // 带回调
```

### 4.2 标记脏数据

```csharp
archive.MarkDirty(); // 由 ArchiveSaveCotoutine 异步写入
```

### 4.3 自动保存

```csharp
GameApp.ArchiveModule.SetAutoSave(archive, true);
```

启用自动保存后，模块会定期检查 `IsDirty` 并写入。

---

## 5. 读取存档

### 5.1 注册读取回调

```csharp
GameApp.ArchiveModule.RegisterLoad<PlayerArchive>((archive) =>
{
    // 读取到存档，初始化业务数据
});
```

### 5.2 注册保存回调

```csharp
GameApp.ArchiveModule.RegisterSave<PlayerArchive>((archive) =>
{
    // 存档即将保存，可在此同步最新业务数据
});
```

---

## 6. 存档管理

### 6.1 获取所有存档信息

```csharp
List<ArchiveModule.ArchiveInfo> infos = GameApp.ArchiveModule.GetAllArchiveInfo(ArchiveTypeCode.GameArchive);
```

### 6.2 删除存档

```csharp
GameApp.ArchiveModule.DeleteArchive(archive);
```

### 6.3 清空某类存档

```csharp
GameApp.ArchiveModule.ClearAllArchive(ArchiveTypeCode.GameArchive);
```

---

## 7. 存档路径

默认相对路径为 `Archives`，保存在持久化目录：

```csharp
public string relativePath = "Archives";
public SaveDirPath savePath = SaveDirPath.PersistencePath;
```

---

## 8. 加密

在 `FrameworkSetting` 中开启 `enableArchiveEncrypt`：

```csharp
[FoldoutGroup("存档设置")]
[LabelText("是否开启存档加密")]
public bool enableArchiveEncrypt = false;
```

开启后，[`ArchiveModule.OnModuleConstructed()`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/Archive/ArchiveModule.cs#L333-L342) 会自动为 `ArchiveManager` 设置加密器。

---

## 9. 完整示例

```csharp
[Serializable]
public class GameProgressArchive : Archive
{
    public int currentLevel;
    public int score;
}

public class GameManageDataHandler : GameConfigDataHandler
{
    private GameProgressArchive m_Archive;

    protected override void OnHandlerStart()
    {
        // 读取或创建存档
        GameApp.ArchiveModule.RegisterLoad<GameProgressArchive>(OnArchiveLoaded);
        m_Archive = GameApp.ArchiveModule.CreateArchive<GameProgressArchive>("progress");
    }

    private void OnArchiveLoaded(GameProgressArchive archive)
    {
        m_Archive = archive;
    }

    public void AddScore(int add)
    {
        m_Archive.score += add;
        m_Archive.MarkDirty(); // 触发自动保存
    }
}
```

---

## 10. 最佳实践

- 将需要持久化的数据集中到 `Archive` 子类中。
- 避免在 `Update` 中频繁 `MarkDirty`，应在数据真正变化时标记。
- 多个模块共享同一份存档时，通过 `RegisterLoad` / `RegisterSave` 同步。
