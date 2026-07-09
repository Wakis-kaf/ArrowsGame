# 资源模块使用说明

> 本文介绍 UnitFramework 的资源加载系统，包括路径编码、同步/异步加载、AssetVO 使用、资源更新等。

---

## 1. 模块入口

资源模块入口为 [`AssetManager`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/Asset/AssetManager.cs)。

通过以下方式访问：

```csharp
GameApp.AssetManager
```

---

## 2. 核心概念

### 2.1 AssetLink

框架不直接传递原始路径，而是使用 `AssetLink`。`AssetLink` 由 [`AssetPathEncoder`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/Asset/AssetPathEncoder.cs) 生成，格式为：

```
$env:prefab&Assets/AddressableResources/UI/Panel/MyPanel.prefab
$resources:prefab&firstres/UI/Panel/MyPanel
$editor:prefab&Assets/AddressableResources/UI/Panel/MyPanel.prefab
$hot:any&config
```

前缀含义：

| 前缀 | 加载来源 | 说明 |
| --- | --- | --- |
| `$env:` | 环境优先（Editor/Resources/Hot） | 最常用 |
| `$editor:` | Editor 直接加载 | 仅编辑器可用 |
| `$resources:` | Resources 目录 | 用于首包资源 |
| `$hot:` | Addressables / 热更资源 | 用于远程/热更资源 |

### 2.2 AssetType

加载时通常需要指定资源类型：

```csharp
public enum AssetType
{
    Auto,                 // 自动推断
    PrefabAsset,          // GameObject 预制体
    PngSpriteAsset,       // PNG Sprite
    JpgSpriteAsset,
    TgaSpriteAsset,
    PngTextureAsset,      // PNG Texture2D
    TxtTextAsset,         // TextAsset
    JsonTextAsset,
    BytesAsset,           // 二进制
    HotCodeBytesAsset,    // 热更 DLL
    AddressableGroupAsset,// Addressable Group 资源列表
    SpriteAtlasAsset,     // SpriteAtlas
    SceneAsset,           // Scene
    WavAudioClipAsset,    // AudioClip
    Mp3AudioClipAsset,
    // ...
}
```

---

## 3. 常用 API

### 3.1 异步加载

```csharp
// 使用环境路径
GameApp.AssetManager.LoadEnvAsset(
    "Assets/AddressableResources/UI/Panel/MyPanel.prefab",
    OnLoaded,
    AssetType.PrefabAsset);

void OnLoaded(IAssetVO assetVO)
{
    if (assetVO != null && assetVO.IsLoadSuccess)
    {
        GameObject prefab = assetVO.GetAsset<GameObject>();
        GameObject go = assetVO.GetInstance(); // 实例化
    }
}
```

### 3.2 同步加载

```csharp
IAssetVO assetVO = GameApp.AssetManager.LoadEnvAssetSync(
    "Assets/AddressableResources/UI/Panel/MyPanel.prefab",
    AssetType.PrefabAsset);

if (assetVO != null && assetVO.IsLoadSuccess)
{
    GameObject prefab = assetVO.GetAsset<GameObject>();
}
```

### 3.3 通用 LoadAssetAsync / LoadAssetSync

如果已经有 `AssetLink`：

```csharp
string link = "Assets/AddressableResources/UI/Panel/MyPanel.prefab".EncodeEnvAssetLink(AssetType.PrefabAsset);

GameApp.AssetManager.LoadAssetAsync(link, OnLoaded);
GameApp.AssetManager.LoadAssetSync(link);
```

### 3.4 加载 Resources 资源

```csharp
GameApp.AssetManager.LoadResourcesAsset(
    "firstres/UI/GameLoading/Prefabs/GameLoadingPanel",
    OnLoaded,
    AssetType.PrefabAsset);
```

### 3.5 加载热更资源

```csharp
GameApp.AssetManager.LoadHotAsset("config", OnLoaded, AssetType.AddressableGroupAsset);
```

---

## 4. IAssetVO 使用

[`AssetVO`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/Asset/AssetVO.cs) 是资源加载结果：

| 属性/方法 | 说明 |
| --- | --- |
| `assetPath` | 资源路径 |
| `IsLoaded` | 是否加载完成 |
| `IsLoadSuccess` | 是否加载成功 |
| `GetAsset<T>()` | 获取指定类型资源 |
| `GetAsset()` | 获取 object 类型资源 |
| `GetInstance(parent)` | 如果是 GameObject，实例化一个 |
| `UnLoadAsync()` | 异步释放 |
| `UnLoadSync()` | 同步释放 |

示例：

```csharp
GameApp.AssetManager.LoadEnvAsset("Assets/.../MyTexture.png", (assetVO) =>
{
    Texture2D tex = assetVO.GetAsset<Texture2D>();
    spriteRenderer.sprite = Sprite.Create(tex, ...);
}, AssetType.PngTextureAsset);
```

---

## 5. 资源路径编码

[`AssetPathEncoder`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/Asset/AssetPathEncoder.cs) 提供扩展方法：

```csharp
// 原始路径 → AssetLink
string link1 = "Assets/AddressableResources/UI/MyPanel.prefab".EncodeEnvAssetLink(AssetType.PrefabAsset);
string link2 = AssetPathEncoder.EncodeResourcesAssetLink("firstres/UI/MyPanel", AssetType.PrefabAsset);
string link3 = AssetPathEncoder.EncodeHotAssetLink("config", AssetType.AddressableGroupAsset);
```

如果路径已经是 Link，编码方法会原样返回。

---

## 6. 资源更新

[`AssetUpdater`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/Asset/AssetUpdater.cs) 负责检查并下载远程最新资源。

启动流程中，资源模块在 `code_assetModule_loadSuccess` 后自动调用：

```csharp
AssetUpdater.SetResNewsetCallback(OnResNewestCheckOver);
AssetUpdater.CheckResNewest();
```

完成后发送 `code_assetModule_newestSuccess`。

---

## 7. 最佳实践

- UI 面板优先使用 `Panel.OpenPanel<T>()`，内部会自动调用资源加载。
- 加载配置、音频等尽量使用异步接口，避免阻塞启动。
- 需要手动释放的资源调用 `assetVO.UnLoadAsync()`。
- 使用 `AssetType.Auto` 时确保路径包含明确扩展名，否则建议显式指定类型。
