# 图集使用说明

> 本文介绍 UnitFramework 中 UAtlas / USprite 的使用，以及编辑器下图集的管理与运行时加载。

---

## 1. 核心组件

| 组件 | 说明 | 文件 |
| --- | --- | --- |
| `UAtlas` | 图集容器，管理一组 Sprite | [`UAtlas.cs`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/UI/Component/UAtlas.cs) |
| `USprite` | 继承 Image，支持按名称从 UAtlas 取图 | [`USprite.cs`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/UI/Component/USprite.cs) |
| `SpriteAtlas` | Unity 原生 SpriteAtlas | - |

---

## 2. 编辑器下创建 UAtlas

### 2.1 创建图集预制体

1. 在场景中创建一个空 GameObject。
2. 挂载 `UAtlas` 脚本。
3. 指定 `mainTexture` 为大图纹理。
4. `UAtlas` 会在编辑器模式下自动从纹理中收集所有 Sprite 到 `spriteList`。

### 2.2 使用 USprite 显示图集图片

1. 在 UI 上创建 `USprite`（代替 Image）。
2. 在 Inspector 中指定 `Atlas` 为刚才创建的 `UAtlas`。
3. 指定 `SpriteName`，`USprite` 会自动从图集中取出对应 Sprite。

```csharp
// 运行时设置
USprite sprite = GetComponent<USprite>();
sprite.Atlas = myAtlas;
sprite.SpriteName = "icon_coin";
```

---

## 3. UAtlas API

```csharp
// 添加 Sprite
myAtlas.AddSprite(sprite);

// 按名称获取 Sprite
Sprite s = myAtlas.GetSprite("icon_coin");

// 清理空引用
myAtlas.CleanNullSprites();
```

---

## 4. USprite 特性

### 4.1 灰度/变暗

```csharp
USprite sprite = ...;
sprite.Gray = true;   // 置灰
sprite.Dimmed = true; // 变暗
```

### 4.2 透明度

```csharp
sprite.Alpha = 0.5f;
```

### 4.3 自动适配原尺寸

```csharp
sprite.IsAutoSnap = true; // 设置 SpriteName 后自动 SetNativeSize
```

### 4.4 清空

```csharp
sprite.ClearSprite();
```

---

## 5. 运行时加载图集

### 5.1 从 Addressables 加载 SpriteAtlas

```csharp
GameApp.AssetManager.LoadEnvAsset(
    "Assets/AddressableResources/UI/Atlas/MainAtlas.spriteatlas",
    (assetVO) =>
    {
        SpriteAtlas atlas = assetVO.GetAsset<SpriteAtlas>();
        Sprite s = atlas.GetSprite("icon_coin");
        image.sprite = s;
    },
    AssetType.SpriteAtlasAsset);
```

### 5.2 通过 UAtlas 引用图集

如果 UI 预制体上已经挂了 `UAtlas`，直接赋值给 `USprite.Atlas` 即可。

---

## 6. 编辑器工具

### 6.1 TexturePacker

[`TexturePacker`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Editor/Modules/UI/TexturePacker) 工具用于拆分与压缩图集。

### 6.2 UI 组件编辑器

[`Editor/Modules/UI/Component/`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Editor/Modules/UI/Component) 提供 `USprite` 的 Inspector 扩展。

---

## 7. 完整示例

```csharp
public class IconHelper
{
    public static void SetIcon(USprite usprite, string atlasPath, string spriteName)
    {
        GameApp.AssetManager.LoadEnvAsset(
            atlasPath,
            (assetVO) =>
            {
                SpriteAtlas atlas = assetVO.GetAsset<SpriteAtlas>();
                if (usprite == null) return;

                // 如果有 UAtlas 容器
                UAtlas uAtlas = usprite.gameObject.GetOrAddComponent<UAtlas>();
                uAtlas.mainTexture = atlas.GetSprite(spriteName).texture;

                usprite.Atlas = uAtlas;
                usprite.SpriteName = spriteName;
            },
            AssetType.SpriteAtlasAsset);
    }
}
```

---

## 8. 最佳实践

- 同一界面的图标、按钮图片尽量放入同一图集，减少 DrawCall。
- 大图背景、全屏图片单独一个图集或不合图集。
- 使用 `USprite` 代替 `Image` 显示图集中的 Sprite，方便统一管理与换皮。
- 图集路径统一使用 `$env:spriteatlas&...` 加载。
