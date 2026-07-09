using Framework.Runtime.LogSystem;
using Framework.Runtime.MAsset;
using Framework.Runtime.Module;
using Framework.Runtime.Module.Core;

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Framework.Runtime.MAudio
{
    public class AudioModule : ModuleUnit
    {
        public const int PRIORITY_BGM = 160;
        public const int PRIORITY_EFFECT = 128;

        public const int TYPE_SOUND_BGM = 1;
        public const int TYPE_SOUND_DEFULT = 0;
        public const int TYPE_SOUND_EFFECT = 2;
        private Dictionary<string, IAssetVO> _clipCache = new Dictionary<string, IAssetVO>();
        private Dictionary<string, AudioSound> _id2Sound = new Dictionary<string, AudioSound>(50);
        private List<AudioSound> _sounds = new List<AudioSound>(50);
        private Dictionary<int, float> _soundVolumnMultipier = new Dictionary<int, float>();
        private Dictionary<int, AudioMixerGroup> m_Type2MixerGroup = new Dictionary<int, AudioMixerGroup>();
        private AudioMixer m_MasterMixer;
        public GameObject rootGO { get; private set; }

        public void CacheClipAssetVO(string resLink, IAssetVO audioClipAssetVO)
        {
            if (audioClipAssetVO == null) return;
            _clipCache.Add(resLink, audioClipAssetVO);
        }

        public void ChangSoundTypeVolumnMultiplier(int type, float multipier)
        {
            if (_soundVolumnMultipier.ContainsKey(type))
            {
                _soundVolumnMultipier[type] = multipier;
            }
            else
            {
                _soundVolumnMultipier.Add(type, multipier);
            }

            foreach (var sound in _sounds)
            {
                if (sound.targetSoundOption.soundType == type)
                {
                    sound.targetSoundOption.SetVolumeMolutiper(multipier);
                }
            }
        }

        public AudioSound CreateSound()
        {
            string name = "New Sound" + AudioSound.soundInstanceCount;
            return CreateSound(name);
        }

        public AudioSound CreateSound(string name)
        {
            int count = 1;
            while (_id2Sound.ContainsKey(name))
            {
                name = "New Sound" + count;
                count++;
            }

            GameObject soundModle = new GameObject(name);
            soundModle.transform.SetParent(rootGO.transform);
            AudioSound audioSound = soundModle.AddComponent<AudioSound>();
            audioSound.SetTag(name);
            _sounds.Add(audioSound);
            _id2Sound.Add(audioSound.soundID, audioSound);
            return SoundGetInit(audioSound);
        }

        public void DestroySound(AudioSound audioSound)
        {
            if (audioSound != null)
            {
                audioSound.Destroy();
            }
        }

        public void DestroySound(string tag)
        {
            if (_id2Sound.ContainsKey(tag))
            {
                DestroySound(_id2Sound[tag]);
            }
        }

        public List<AudioSound> FindAudioSoundsByType(int targetType)
        {
            List<AudioSound> sounds = new List<AudioSound>();
            foreach (var sound in _sounds)
            {
                if (sound.targetSoundOption.soundType == targetType)
                {
                    sounds.Add(sound);
                }
            }
            return sounds;
        }

        public bool FindCacheClipAssetVO(string resLink, out IAssetVO audioClipAssetVO)
        {
            audioClipAssetVO = null;
            if (_clipCache.ContainsKey(resLink))
            {
                audioClipAssetVO = _clipCache[resLink];
                return true;
            }
            return false;
        }

        public AudioSound FindFreeSound()
        {
            foreach (var sound in _sounds)
            {
                if (sound.IsFree) return sound;
            }

            return null;
        }

        public AudioSound FindSound(string name)
        {
            if (_id2Sound.ContainsKey(name))
            {
                return _id2Sound[name];
            }
            return null;
        }

        public AudioSound GetFreeSound()
        {
            AudioSound get = FindFreeSound();
            if (get == null)
                get = CreateSound();
            return get;
        }

        public AudioSound GetSound(string name)
        {
            AudioSound find = FindSound(name);
            if (find == null)
            {
                return CreateSound(name);
            }

            return find;
        }

        public float GetSoundTypeVolumnMolutipier(int type)
        {
            if (_soundVolumnMultipier.ContainsKey(type))
            {
                return _soundVolumnMultipier[type];
            }
            return 1;
        }

        public void LoopSoundWhere(Action<AudioSound> cb)
        {
            for (int i = 0; i < _sounds.Count; i++)
            {
                cb?.Invoke(_sounds[i]);
            }
        }
        public void SetBgmVolume(float volume)
        {
            AudioSound sound = GetSound("AUDIO_BGM");
            if (sound == null || !sound.IsPlaying) return;
            sound.targetSoundOption.SetVolume(volume);
        }
        public float GetBgmVolume()
        {
            AudioSound sound = GetSound("AUDIO_BGM");
            if (sound == null || !sound.IsPlaying) return 0;
            return sound.targetSoundOption.volume;
        }
        public void StopBgm()
        {
            AudioSound sound = GetSound("AUDIO_BGM");
            sound?.Stop();
        }
        public bool IsBgmPlay(string clipLink)
        {
            AudioSound sound = GetSound("AUDIO_BGM");
            return sound != null && sound.IsPlaying && sound.targetSoundOption.audioLink == clipLink;
        }
        public AudioSound PlayBgm(string clipLink, float volume = 1)
        {
            AudioSound sound = GetSound("AUDIO_BGM");
            sound.targetSoundOption.output = GetAudioMixerGroup(TYPE_SOUND_BGM);
            sound.targetSoundOption.soundType = TYPE_SOUND_BGM;
            sound.targetSoundOption.audioClip = null;
            sound.targetSoundOption.priority = PRIORITY_BGM;
            sound.targetSoundOption.audioLink = clipLink;
            sound.targetSoundOption.loop = true;
            sound.targetSoundOption.volume = volume;
            sound.Play();
            return sound;
        }
        private AudioMixerGroup FindMixerGroup(string groupName)
        {
            if (m_MasterMixer == null)
            {
                return null;
            }
            return m_MasterMixer.FindMatchingGroups(groupName)[0];
        }

        public AudioSound PlayBgm(AudioClip audioClip, float volume = 1)
        {
            AudioSound sound = GetSound("AUDIO_BGM");
            sound.targetSoundOption.soundType = TYPE_SOUND_BGM;
            sound.targetSoundOption.output = GetAudioMixerGroup(TYPE_SOUND_BGM);
            sound.targetSoundOption.audioClip = audioClip;
            sound.targetSoundOption.priority = PRIORITY_BGM;
            sound.targetSoundOption.audioLink = "";
            sound.targetSoundOption.loop = true;
            sound.targetSoundOption.volume = volume;
            sound.Play();
            return sound;
        }

        public AudioSound PlayEffect(string clipLink, float volume = 1, float startTime = 0)
        {
            AudioSound sound = GetFreeSound();
            sound.targetSoundOption.output = GetAudioMixerGroup(TYPE_SOUND_EFFECT);
            sound.targetSoundOption.soundType = TYPE_SOUND_EFFECT;
            sound.targetSoundOption.priority = PRIORITY_EFFECT;
            sound.targetSoundOption.audioClip = null;
            sound.targetSoundOption.audioLink = clipLink;
            sound.targetSoundOption.volume = volume;
            sound.targetSoundOption.startTime = startTime;
            sound.Play();
            return sound;
        }

        public AudioSound PlayEffect(AudioClip audioClip, float volume = 1, float startTime = 0)
        {
            AudioSound sound = GetFreeSound();
            sound.targetSoundOption.soundType = TYPE_SOUND_EFFECT;
            sound.targetSoundOption.priority = PRIORITY_EFFECT;
            sound.targetSoundOption.audioClip = audioClip;
            sound.targetSoundOption.audioLink = "";
            sound.targetSoundOption.volume = volume;
            sound.targetSoundOption.startTime = startTime;
            sound.Play();
            return sound;
        }

        protected override void OnModuleConstructed()
        {
            base.OnModuleConstructed();
            rootGO = new GameObject("Audio Root");
            var audioListeners = Transform.FindObjectsOfType<AudioListener>();
            if (audioListeners != null)
            {
                foreach (var audioListener in audioListeners)
                {
                    GameObject.Destroy(audioListener);
                }
            }

            rootGO.AddComponent<AudioListener>();
            rootGO.transform.SetParent(GameApp.Ins.GameAppShell.transform);
            var assetManager = GameApp.AssetManager;
            if (assetManager == null)
            {
                Log.Fatal("asset module not found!");
                return;
            }
            Log.Info("音频模块绑定 资源加载系统成功");
            AudioAgent.SetAssetLoadAsyncAgent(assetManager.LoadAssetAsync);
            AudioAgent.SetAssetLoadSyncAgent(assetManager.LoadAssetSync);

        }
        private AudioMixerGroup GetAudioMixerGroup(int soundType)
        {
            if (m_Type2MixerGroup.TryGetValue(soundType, out var group)) return group;
            return null;
        }
        private void AddAudioMixerGroup(int soundType, AudioMixerGroup mixerGroup)
        {
            if (m_Type2MixerGroup.ContainsKey(soundType))
            {
                m_Type2MixerGroup[soundType] = mixerGroup;
                return;
            }
            m_Type2MixerGroup.Add(soundType, mixerGroup);
        }
        public void SetMasterMixer(int soundType, AudioMixerGroup mixerGroup)
        {
            AddAudioMixerGroup(soundType, mixerGroup);
            for (int i = 0; i < _sounds.Count; i++)
            {
                if (!_sounds[i].IsPlaying) continue;
                if (_sounds[i].targetSoundOption.soundType == TYPE_SOUND_EFFECT)
                {
                    _sounds[i].targetSoundOption.output = GetAudioMixerGroup(TYPE_SOUND_EFFECT);
                }
                else if (_sounds[i].targetSoundOption.soundType == TYPE_SOUND_BGM)
                {
                    _sounds[i].targetSoundOption.output = GetAudioMixerGroup(TYPE_SOUND_BGM);
                }
            }
        }

        private AudioSound SoundGetInit(AudioSound sound)
        {
            sound.targetSoundOption.soundType = TYPE_SOUND_DEFULT;
            return sound;
        }


    }
}