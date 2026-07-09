# 音频模块使用说明

> 本文介绍 UnitFramework 的音频系统，包括背景音乐、音效、AudioMixer 分组与音量控制。

---

## 1. 模块入口

音频模块入口为 [`AudioModule`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/Audio/AudioModule.cs)。

快捷访问：

```csharp
GameApp.AudioModule
```

---

## 2. 核心概念

### 2.1 声音类型

```csharp
public const int TYPE_SOUND_BGM = 1;      // 背景音乐
public const int TYPE_SOUND_EFFECT = 2;   // 音效
public const int TYPE_SOUND_DEFULT = 0;   // 默认
```

### 2.2 AudioSound

[`AudioSound`](file:///d:/UnityGame/UnitFramework/Project/Assets/Scripts/Framework/Runtime/Modules/Audio/AudioSound.cs) 是单个声音实例，内部持有 `AudioSource`，支持：

- 空闲/播放中/等待加载状态
- 自动回收
- 资源异步加载
- 超短音频保护逻辑

### 2.3 SoundOption

用于配置一次播放的参数：AudioClip、AudioLink、音量、循环、MixerGroup、优先级等。

---

## 3. 常用 API

### 3.1 播放背景音乐

```csharp
GameApp.AudioModule.PlayBgm("Assets/AddressableResources/Audio/Bgm/main.mp3", 0.8f);
```

### 3.2 播放音效

```csharp
GameApp.AudioModule.PlayEffect("Assets/AddressableResources/Audio/Effect/click.wav", 1f);
```

### 3.3 停止背景音乐

```csharp
GameApp.AudioModule.StopBgm();
```

### 3.4 音量控制

```csharp
// 设置 BGM 音量
GameApp.AudioModule.SetBgmVolume(0.5f);

// 获取 BGM 音量
float vol = GameApp.AudioModule.GetBgmVolume();

// 按声音类型设置倍率（影响所有该类型的声音）
GameApp.AudioModule.ChangSoundTypeVolumnMultiplier(AudioModule.TYPE_SOUND_EFFECT, 0.7f);
```

### 3.5 判断 BGM 是否播放

```csharp
bool isPlaying = GameApp.AudioModule.IsBgmPlay("audio_link");
```

---

## 4. 进阶使用

### 4.1 创建/获取声音

```csharp
AudioSound sound = GameApp.AudioModule.GetSound("AUDIO_BGM");
AudioSound freeSound = GameApp.AudioModule.GetFreeSound();
```

### 4.2 销毁声音

```csharp
GameApp.AudioModule.DestroySound("MySound");
GameApp.AudioModule.DestroySound(sound);
```

### 4.3 设置 Master Mixer

```csharp
GameApp.AudioModule.SetMasterMixer(AudioModule.TYPE_SOUND_BGM, bgmMixerGroup);
GameApp.AudioModule.SetMasterMixer(AudioModule.TYPE_SOUND_EFFECT, effectMixerGroup);
```

### 4.4 遍历声音

```csharp
GameApp.AudioModule.LoopSoundWhere((sound) =>
{
    if (sound.targetSoundOption.soundType == AudioModule.TYPE_SOUND_EFFECT)
    {
        sound.Stop();
    }
});
```

---

## 5. 业务封装

通常在 `GameAudioClientHandler` 中封装项目音频接口：

```csharp
public static class GameAudioConstant
{
    public const string Bgm_GameEntry1 = "$env:mp3&Assets/AddressableResources/Audio/Bgm/GameEntry1";
    public const string Effect_ButtonClick = "$env:wav&Assets/AddressableResources/Audio/Effect/ButtonClick";
}

public class GameAudioClientHandler : GameModuleLogicHandler
{
    public static GameAudioClientHandler Ins => GetModuleHandlerIns<GameAudioClientHandler>();

    public void PlayBgm(string bgmLink)
    {
        GameApp.AudioModule.PlayBgm(bgmLink);
    }

    public void PlayEffect(string effectLink)
    {
        GameApp.AudioModule.PlayEffect(effectLink);
    }
}
```

---

## 6. 最佳实践

- 使用业务 `Constant` 集中管理音频路径。
- BGM 与 Effect 建议分别使用 MixerGroup，方便独立调音。
- 大量短音效使用 `PlayEffect`，模块内部会自动复用 `AudioSound` 实例。
- 切换场景或界面时，在 ViewHandler 中调用 `StopBgm()` 或切换 BGM。
