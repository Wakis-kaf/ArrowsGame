using Framework.Runtime;
using Framework.Runtime.LogSystem;
using Framework.Runtime.MAsset;
using Framework.Runtime.MAudio;
using UnityEngine.Audio;

namespace Game.Modules.GModuleAudio
{
    public class GameAudioModule : GameModuleBaseInstance<GameAudioModule>
    {
        /// <summary>
        /// 构造函数中调用，托管对象可以在这初始化
        /// </summary>
        protected override void OnConstructed()
        {
            
        }
        /// <summary>
        /// 注册所有的处理类
        /// </summary>
        protected override void GenerateHandlers()
        {
            RegisterHandler<GameAudioClientHandler>();
            RegisterHandler<GameAudioDataHandler>();
            RegisterHandler<GameAudioViewHandler>();
        }
        /// <summary>
        /// 当所有游戏模块刚被构建的时候回传触发
        /// </summary>
        protected override void OnModuleAwake()
        {
         
        }
        /// <summary>
        /// 当所有游戏模块已被创建成功的时候回传触发
        /// </summary>
        protected override void OnModuleStart()
        {
          
        }

        /// <summary>
        /// 当游戏模块被销毁的时候回传触发
        /// </summary>
        protected override void OnModuleDestroy()
        {
            
        }
        protected override void OnCheckModuleLoad()
        {
            LoadMasterMixer();
        }
        private void LoadMasterMixer()
        {
            string masetMixer = AssetPathEncoder.EncodeEnvAssetLink("Assets/AddressableResources/Audios/MasterMixer.mixer");
            AudioAgent.LoadAssetAsync(masetMixer, OnMasterMixerLoaded);
        }

        private void OnMasterMixerLoaded(IAssetVO vO)
        {
            var masterMixer = vO.GetAsset() as AudioMixer;
            if(masterMixer == null)
            {
                Log.Error("加载音频混响失败失败");
                OnModuleLoaded();
                return;
            }
            GameApp.AudioModule.SetMasterMixer(AudioModule.TYPE_SOUND_BGM,
                masterMixer.FindMatchingGroups("Master/Music")[0]);
            GameApp.AudioModule.SetMasterMixer(AudioModule.TYPE_SOUND_EFFECT,
                masterMixer.FindMatchingGroups("Master/Effect")[0]);
            OnModuleLoaded();
        }

    }

}
