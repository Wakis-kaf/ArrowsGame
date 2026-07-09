# 网络请求模块使用说明

> 本文介绍 UnitFramework 的网络/下载系统，包括 HTTP Get/Post、文件下载、上传等。

---

## 1. 模块入口

网络模块入口为 [`WebRequestModule`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/WebquesetModule/WebRequestModule.cs)。

快捷访问：

```csharp
GameApp.WebRequestModule
```

底层封装：

- [`BaseWebRequest`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/WebquesetModule/BaseWebRequest.cs)
- [`UnityWebRequestMgr`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/WebquesetModule/UnityWebRequestMgr.cs)

---

## 2. 常用 API

### 2.1 Get 请求

```csharp
GameApp.WebRequestModule.UnityWebRequestMgr.Get(
    "https://example.com/api/config",
    (uwr) =>
    {
        if (uwr.result == UnityWebRequest.Result.Success)
        {
            string json = uwr.downloadHandler.text;
        }
    });
```

### 2.2 Post 请求

```csharp
WWWForm form = new WWWForm();
form.AddField("userId", "12345");

GameApp.WebRequestModule.UnityWebRequestMgr.Post(
    "https://example.com/api/login",
    form,
    (uwr) =>
    {
        Debug.Log(uwr.downloadHandler.text);
    });
```

### 2.3 下载文件

```csharp
GameApp.WebRequestModule.UnityWebRequestMgr.Download(
    "https://example.com/res/bundle.assetbundle",
    Application.persistentDataPath + "/bundle.assetbundle",
    (error, progress, uwr) =>
    {
        if (!string.IsNullOrEmpty(error))
        {
            Debug.LogError(error);
            return;
        }
        Debug.Log($"下载进度: {progress}");
    },
    timeout: 60);
```

### 2.4 下载文本

```csharp
GameApp.WebRequestModule.UnityWebRequestMgr.GetText(
    "https://example.com/notice.txt",
    (progress) => { },
    (error, text) =>
    {
        if (string.IsNullOrEmpty(error))
        {
            Debug.Log(text);
        }
    });
```

### 2.5 下载 AssetBundle

```csharp
GameApp.WebRequestModule.UnityWebRequestMgr.GetAssetBundle(
    url,
    (progress) => Debug.Log(progress),
    (error, ab) =>
    {
        if (string.IsNullOrEmpty(error))
        {
            // 使用 AssetBundle
        }
    });
```

### 2.6 上传字节

```csharp
byte[] data = System.Text.Encoding.UTF8.GetBytes("hello");
GameApp.WebRequestModule.UnityWebRequestMgr.Upload(
    data,
    (error, progress, uwr) =>
    {
        Debug.Log($"上传进度: {progress}");
    });
```

---

## 3. 超时设置

默认超时 30 秒，可在调用时指定：

```csharp
GameApp.WebRequestModule.UnityWebRequestMgr.Get(url, callback, timeout: 10);
```

---

## 4. 完整示例

```csharp
public void DownloadHotPatch(string url, string savePath)
{
    GameApp.WebRequestModule.UnityWebRequestMgr.Download(
        url,
        savePath,
        (error, progress, uwr) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                Log.Error($"下载失败: {error}");
                return;
            }
            if (progress >= 1f)
            {
                Log.Info("下载完成");
            }
        },
        60);
}
```

---

## 5. 最佳实践

- 所有网络请求通过 `WebRequestModule` 发起，便于统一加签、超时、重试。
- 大文件下载设置合理的超时时间，并在 UI 上显示进度。
- 上传/下载完成后检查 `uwr.result` 是否为 `Success`。
