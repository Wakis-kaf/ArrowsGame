using Framework.Runtime.MAsset;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace Framework.Runtime.MAudio
{
    public enum SoundStatus
    {
        Disabled, // 禁用状态
        Free,     // 空闲状态
        Playing,  // 播放状态
        Waiting,  // 资源加载中
    }

    [RequireComponent(typeof(AudioSource))]
    public class AudioSound : MonoBehaviour
    {
        public bool isAutoFree = true;
        private Coroutine _stopCoroutine;
        private SoundOption _targetSoundOption;
        private bool isResLoading = false;
        private string loadingPath = "";

        // 针对超短音频的逻辑判定阈值
        private const float SHORT_CLIP_THRESHOLD = 0.1f;

        public static int soundInstanceCount { get; private set; } = 0;
        public GameObject attachGO { get; private set; }
        public AudioSource attahAS { get; private set; }
        public bool IsFree => soundStatus == SoundStatus.Free;
        public bool IsPlaying => soundStatus == SoundStatus.Playing;
        public string soundID { get; private set; }

        public SoundStatus soundStatus { get; private set; }

        public SoundOption targetSoundOption
        {
            get
            {
                if (_targetSoundOption == null)
                {
                    _targetSoundOption = new SoundOption();
                    _targetSoundOption.BindSound(this);
                }
                return _targetSoundOption;
            }
            private set { _targetSoundOption = value; }
        }

        public AudioSound()
        {
            soundInstanceCount++;
            soundStatus = SoundStatus.Free;
        }

        private void Awake()
        {
            attachGO = gameObject;
            attahAS = GetComponent<AudioSource>();
            Disable();
        }

        // --- 补全管理方法 ---
        public void Destroy()
        {
            Stop();
            GameObject.Destroy(gameObject);
        }

        public string SetTag(string tag)
        {
            this.soundID = tag;
            return tag;
        }

        public void Pause() => attahAS.Pause();
        public void Resume() => attahAS.UnPause();

        // --- 播放逻辑 ---
        public void Play()
        {
            if (targetSoundOption.audioClip != null)
            {
                RealPlay();
                return;
            }

            string path = targetSoundOption.audioLink;
            if (string.IsNullOrEmpty(path)) return;

            if (isResLoading && loadingPath == path) return;

            isResLoading = true;
            loadingPath = path;
            soundStatus = SoundStatus.Waiting;

            Action<IAssetVO> audioAssetLoadCb = null;
            audioAssetLoadCb = (assetVo) =>
            {
                if (!isResLoading || loadingPath != path) return;

                var audioClip = assetVo.GetAsset() as AudioClip;
                assetVo.RemoveAssetLoadCallback(audioAssetLoadCb);
                isResLoading = false;
                loadingPath = "";

                if (audioClip != null)
                {
                    targetSoundOption.audioClip = audioClip;
                    if (soundStatus == SoundStatus.Waiting)
                        RealPlay();
                }
                else
                {
                    Stop();
                }
            };

            if (GameApp.AudioModule.FindCacheClipAssetVO(path, out IAssetVO assetVo))
            {
                if (assetVo.IsLoaded)
                {
                    isResLoading = false;
                    targetSoundOption.audioClip = assetVo.GetAsset<AudioClip>();
                    RealPlay();
                }
                else
                {
                    assetVo.AddAssetLoadCallback(audioAssetLoadCb);
                }
            }
            else
            {
                GameApp.AudioModule.CacheClipAssetVO(path, AudioAgent.LoadAssetAsync(path, audioAssetLoadCb));
            }
        }

        public void Play(SoundOption option)
        {
            ResetByOption(option);

            if (attahAS.clip != null)
            {
                Enable();

                // 解决 0.032s 音频播放逻辑
                if (attahAS.clip.length < SHORT_CLIP_THRESHOLD)
                {
                    // 1. 使用 OneShot 保证不掉音
                    attahAS.PlayOneShot(attahAS.clip, option.volume);
                    soundStatus = SoundStatus.Playing;

                    // 2. 依然需要开启协程来回收 AudioSound，但给一定的缓冲时间防止竞态
                    if (_stopCoroutine != null) StopCoroutine(_stopCoroutine);
                    _stopCoroutine = StartCoroutine(Stop_Timeout(attahAS.clip.length + 0.05f));
                }
                else
                {
                    attahAS.Play();
                    if (_stopCoroutine != null) StopCoroutine(_stopCoroutine);
                    if (!attahAS.loop)
                    {
                        _stopCoroutine = StartCoroutine(Stop_Timeout(attahAS.clip.length));
                    }
                }
            }
        }

        public void Stop()
        {
            if (attahAS != null) attahAS.Stop();

            isResLoading = false;
            loadingPath = "";

            if (_stopCoroutine != null)
            {
                StopCoroutine(_stopCoroutine);
                _stopCoroutine = null;
            }

            if (isAutoFree)
            {
                soundStatus = SoundStatus.Free;
                targetSoundOption.BindSound(null);
            }
            else
            {
                soundStatus = SoundStatus.Disabled;
            }
            Disable();
        }

        private void RealPlay()
        {
            soundStatus = SoundStatus.Playing;
            Play(targetSoundOption);
        }

        private IEnumerator Stop_Timeout(float time)
        {
            float pitch = Mathf.Abs(attahAS.pitch) < 0.01f ? 1.0f : attahAS.pitch;
            // 增加最小等待时间保护，确保协程不会在同一帧结束
            yield return new WaitForSeconds(Mathf.Max(0.05f, time / pitch));
            _stopCoroutine = null;
            Stop();
        }

        public void ResetByOption(SoundOption option)
        {
            targetSoundOption = option;
            targetSoundOption.BindSound(this);

            attahAS.clip = option.audioClip;
            attahAS.outputAudioMixerGroup = option.output;
            attahAS.mute = option.mute;
            attahAS.bypassEffects = option.byPassEffects;
            attahAS.bypassListenerEffects = option.bypassListenerEffects;
            attahAS.bypassReverbZones = option.byPassReverbZones;
            attahAS.playOnAwake = option.playOnAwake;
            attahAS.loop = option.loop;
            attahAS.priority = option.priority;
            // 注意：OneShot 模式下 volume 是在 PlayOneShot 参数里传的，这里设置的是 AS 的基础音量
            attahAS.volume = option.volume * GameApp.AudioModule.GetSoundTypeVolumnMolutipier(option.soundType);
            attahAS.pitch = option.pitch;
            attahAS.panStereo = option.panStereo;
            attahAS.spatialBlend = option.spatialBlend;
            attahAS.reverbZoneMix = option.reverbZoneMix;
            attahAS.dopplerLevel = option.dopplerLevel;
            attahAS.spread = option.spread;
            attahAS.rolloffMode = option.rolloffMode;
            attahAS.minDistance = option.minDistance;
            attahAS.maxDistance = option.maxDistance;
            attahAS.time = option.startTime;
            gameObject.transform.position = option.worldPosition;
        }

        public void Disable() => gameObject.SetActive(false);
        public void Enable() => gameObject.SetActive(true);
    }

    // --- SoundOption 类保持不变 ---
    public class SoundOption
    {
        public AudioClip audioClip;
        public string audioLink;
        public bool byPassEffects;
        public bool bypassListenerEffects;
        public bool byPassReverbZones;
        public float dopplerLevel = 1;
        public bool loop;
        public float maxDistance = 500;
        public float startTime=0;
        public float minDistance = 1;
        public bool mute;
        public AudioMixerGroup output;
        public float panStereo = 0;
        public float pitch = 1;
        public bool playOnAwake;
        public int priority = 128;
        public float reverbZoneMix = 0;
        public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;
        public int soundType = 0;
        public float spatialBlend = 0;
        public float spread = 1;
        public float volume = 1f;
        public Vector3 worldPosition;
        private AudioSound bindSound;
        private float volumnMolutiper = 1f;

        public SoundOption() { Reset(); }

        public void ApplyChanged() => this.bindSound?.ResetByOption(this);

        public void BindSound(AudioSound audioSound)
        {
            if (audioSound == this.bindSound) return;
            this.bindSound = audioSound;
            if (audioSound == null) Reset();
        }

        public void Reset()
        {
            audioClip = null;
            audioLink = "";
            soundType = 0;
            byPassEffects = false;
            bypassListenerEffects = false;
            byPassReverbZones = false;
            playOnAwake = false;
            loop = false;
            priority = 128;
            volume = 1f;
            pitch = 1;
            panStereo = 0;
            spatialBlend = 0;
            reverbZoneMix = 0;
            dopplerLevel = 1;
            spread = 1;
            minDistance = 1;
            maxDistance = 500;
            rolloffMode = AudioRolloffMode.Logarithmic;
            volumnMolutiper = 1f;
            startTime = 0;
        }

        public void SetVolume(float newVolumn)
        {
            this.volume = newVolumn;
            this.SetVolumeMolutiper(this.volumnMolutiper);
        }

        public void SetVolumeMolutiper(float molutiper)
        {
            this.volumnMolutiper = molutiper;
            if (this.bindSound == null || this.bindSound.soundStatus != SoundStatus.Playing) return;
            this.bindSound.attahAS.volume = this.volume * molutiper;
        }
    }
}