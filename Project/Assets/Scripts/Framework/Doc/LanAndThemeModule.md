# 多语言与主题模块使用说明

> 本文介绍 UnitFramework 的多语言（Lan）与主题（Theme）系统，包括适配器注册与运行时切换。

---

## 1. 模块入口

多语言与主题模块入口为 [`LanAndThemeModule`](file:///d:/UnityGame\UnitFramework\Project\Assets\Scripts\Framework\Runtime\Modules\LanAndTheme\LanAndThemeModule.cs)。

快捷访问：

```csharp
GameApp.LanAndThemeModule
```

内部包含：

- [`Lan2LocalManager`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/LanAndTheme/Lan2LocalManager.cs)
- [`Theme2LocalManager`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/LanAndTheme/Theme2LocalManager.cs)

---

## 2. 多语言系统

### 2.1 语言类型

```csharp
public enum LanguageType
{
    Zh_CN,   // 简体中文
    Zh_TW,   // 繁体中文
    En_US,   // 英文
    Ja_JP,   // 日文
    // ...
}
```

### 2.2 切换语言

```csharp
GameApp.LanAndThemeModule.Lan2LocalManager.SetCurrentLanType(LanguageType.En_US);
```

### 2.3 注册适配器

业务组件实现 `ILanAdapter`：

```csharp
public class MyTextAdapter : MonoBehaviour, ILanAdapter
{
    public bool IsEnableLan { get; set; } = true;
    public LanguageType LanType { get; set; }
    public string LanId { get; set; }

    public void SetCurrentLanType(LanguageType currentLanType)
    {
        // 根据 currentLanType 刷新文本
    }
}
```

注册：

```csharp
GameApp.LanAndThemeModule.Lan2LocalManager.RegisterLanAdapter(myAdapter);
```

---

## 3. 主题系统

### 3.1 主题类型

```csharp
public enum ThemeType
{
    None,
    Default,
    Dark,
    Festival,
    FollowEnv,  // 跟随环境
    // ...
}
```

### 3.2 切换主题

```csharp
GameApp.LanAndThemeModule.Theme2LocalManager.SetCurrentThemeType(ThemeType.Dark);
```

### 3.3 注册主题适配器

```csharp
public class MyThemeAdapter : MonoBehaviour, IThemeAdapter
{
    public bool IsEnableTheme { get; set; } = true;
    public ThemeType UseThemeType { get; set; } = ThemeType.FollowEnv;
    public ThemeType CurrentThemeType { get; set; }
    public string ThemeItemId { get; set; }

    public void SetCurrentThemeType(ThemeType currentThemeType)
    {
        // 根据 currentThemeType 切换图片/颜色
    }
}

GameApp.LanAndThemeModule.Theme2LocalManager.RegisterThemeAdapter(myAdapter);
```

### 3.4 查找主题配置

```csharp
CfgThemeItem item = GameApp.LanAndThemeModule.Theme2LocalManager.FindThemeItem(ThemeType.Dark, "ButtonPrimary");
```

---

## 4. 运行时适配组件

框架提供若干内置适配组件：

- `EndAdapter_TMPText`：TMP 文本多语言适配
- `EndAdapter_Image`：图片主题适配
- `EndAdapter_Text`：普通文本多语言适配

---

## 5. 完整示例

```csharp
public class SettingPanel : Panel
{
    public void OnClickLanguage(LanguageType type)
    {
        GameApp.LanAndThemeModule.Lan2LocalManager.SetCurrentLanType(type);
    }

    public void OnClickTheme(ThemeType type)
    {
        GameApp.LanAndThemeModule.Theme2LocalManager.SetCurrentThemeType(type);
    }
}
```

---

## 6. 最佳实践

- 文本组件优先使用 `UTMPText` 并挂载多语言适配器。
- 主题资源通过 `CfgThemeMap` 配置，避免硬编码。
- 切换语言/主题后，已存在的适配器会自动刷新。
