using Framework.Runtime;
using Framework.Runtime.LogSystem;
using Framework.Runtime.MAsset;
using Framework.Runtime.MAudio;
using Framework.Runtime.MGameModule;
using Framework.Runtime.UI;
using Game.Modules.GModuleManage;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Game.Modules.GModuleAudio
{
    public class GameAudioClientHandler : GameModuleLogicHandler
    {

        private string m_TouchSound = GameAudioConstant.Eff_ScreenClick;
        public void EnablePlayTouchSound(string soundName)
        {
            m_TouchSound = soundName;
            GameApp.Ins.LoopManager.AddLoop(CheckPointerClick);
        }
        public void DisablePlayTouchSound()
        {
            m_TouchSound = "";
            GameApp.Ins.LoopManager.RemoveLoop(CheckPointerClick);
        }
        protected override void OnHandlerStart()
        {
            base.OnHandlerStart();
            //UIAgent.SetAudioPlayAgent(OnUIPlayAudio);
            //GameApp.Ins.LoopManager.AddLoop(CheckPointerClick);
            MessageDispatcher.Ins.Subscribe<GameArchive>(MessageCode.msg_on_mainArchiveLoaded, OnArchiveLoaded);
        }
        private void OnArchiveLoaded(GameArchive archive)
        {
            float musicVolume = GetMusicVolume();
            float effectVolume = GetEffectVolume();
            ChangeMusicVolMultiplier(musicVolume);
            ChangeEffectVolMultiplier(effectVolume);
        }
        private void CheckPointerClick()
        {
            if (Input.GetMouseButtonDown(0) && ShouldPlayScreenSound())
            {
                PlayTouchSound();
            }

        }
        public void SaveMusicVolume(float value)
        {
            GameArchive.Main.RoleArchive.SetMusicVolume(value);
        }
        public void SaveEffectVolume(float value)
        {
            GameArchive.Main.RoleArchive.SetEffectVolume(value);
        }
        public float GetMusicVolume()
        {
            return GameArchive.Main.RoleArchive.GetMusicVolume(0.5f);
        }
        public float GetEffectVolume()
        {
            return GameArchive.Main.RoleArchive.GetEffectVolume(0.5f);
        }
        private bool ShouldPlayScreenSound()
        {
            // 1. 如果连 EventSystem 都没有，直接允许播放
            if (EventSystem.current == null) return true;

            // 2. 获取当前鼠标位置下所有的 UI 射线结果
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            // 3. 遍历点到的所有 UI
            foreach (var result in results)
            {
                // 检查该物体或其父物体上是否有“可交互”组件
                // 常见的交互组件：Button, Toggle, Slider, Scrollbar, Dropdown
                Selectable selectable = result.gameObject.GetComponentInParent<Selectable>();

                // 如果点到了可交互的组件，且该组件是启用的，则认为触发了“功能按钮”
                if (selectable != null && selectable.interactable)
                {
                    return false; // 找到了交互按钮，不触发屏幕音
                }
            }

            // 如果遍历完都没找到 Selectable，说明点到的是空地或者是纯图片
            return true;
        }
        private void PlayTouchSound()
        {
            if (string.IsNullOrEmpty(m_TouchSound)) return;
            PlayEffect(m_TouchSound);
        }
        private void OnUIPlayAudio(object fromer, int audioType)
        {
            if (audioType == UIAudioType.NormalButtonClick)
            {
                GameAudioClientHandler.Ins.PlayEffect(GameAudioConstant.Eff_NormalClick);
            }
            else if (audioType == UIAudioType.MainTabClick)
            {
                GameAudioClientHandler.Ins.PlayEffect(GameAudioConstant.Eff_MainTapClick);
            }
        }

        public static GameAudioClientHandler Ins => GetModuleHandlerIns<GameAudioClientHandler>();
        public bool IsBgmPlay(string bgmId)
        {
            var path = GameAudioDataHandler.Ins.GetCfgAudioPath(bgmId);
            if (path == null)
            {
                Log.Error($"未找到 id:{bgmId}的音频配置");
                return false;
            }
            string link = AssetPathEncoder.EncodeEnvAssetLink(path.audioClipPath);
            return GameApp.AudioModule.IsBgmPlay(link);
        }
        public void PlayBgm(string bgmId)
        {
            var path = GameAudioDataHandler.Ins.GetCfgAudioPath(bgmId);
            if (path == null)
            {
                Log.Error($"未找到 id:{bgmId}的音频配置");
                return;
            }
            string link = AssetPathEncoder.EncodeEnvAssetLink(path.audioClipPath);
            GameApp.AudioModule.PlayBgm(link, path.volume);
        }
        public void PlayEffect(string effId)
        {
            var path = GameAudioDataHandler.Ins.GetCfgAudioPath(effId);
            if (path == null)
            {
                Log.Error($"未找到 id:{effId}的音频配置");
                return;
            }
            //Log.Error($"播放音频 id:{effId}的音频");
            string link = AssetPathEncoder.EncodeEnvAssetLink(path.audioClipPath);
            var sound = GameApp.AudioModule.PlayEffect(link, path.volume);
        }

        public void PlayEntryBgm()
        {
            PlayBgm(GameAudioConstant.Bgm_EntryId);
        }
        public void PlayFightPreBgm()
        {
            PlayBgm(GameAudioConstant.Bgm_GameFightReadyId);
        }
        public void PlayFightBgm()
        {
            PlayBgm(GameAudioConstant.Bgm_GameFightId);
        }

        public void PlaySwordAttackEffect()
        {
            PlayEffect(GameAudioConstant.Eff_SwordAttackId);
        }
        public void PlayBombAttackEffect()
        {
            PlayEffect(GameAudioConstant.Eff_BombAttackId);
        }
        public void ChangeMusicVolMultiplier(float value)
        {
            GameApp.AudioModule.ChangSoundTypeVolumnMultiplier(AudioModule.TYPE_SOUND_BGM, value);
        }
        public void ChangeEffectVolMultiplier(float value)
        {
            GameApp.AudioModule.ChangSoundTypeVolumnMultiplier(AudioModule.TYPE_SOUND_EFFECT, value);
        }

        public void StopBgm()
        {
            GameApp.AudioModule.StopBgm();
        }
    }

}
