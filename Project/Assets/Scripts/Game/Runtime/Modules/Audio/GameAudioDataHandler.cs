using Framework.Runtime.LogSystem;
using Framework.Runtime.MGameModule;
using Game.Modules.GModuleSceneUnit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Modules.GModuleAudio
{
    public class GameAudioDataHandler : GameConfigDataHandler
    {
        public static GameAudioDataHandler Ins => GetModuleHandlerIns<GameAudioDataHandler>();
        private CfgAudioTable m_CfgAudioTable;
        public CfgAudioTable GetCfgAudioTable()
        {
            if (m_CfgAudioTable != null) return m_CfgAudioTable;
            if (TryReadConfig<CfgAudioTable>("cfg_audio", out m_CfgAudioTable))
            {
                Log.Info("读取 cfg_audio 成功");
                return m_CfgAudioTable;
            }
            Log.Error("读取 cfg_audio 失败");
            return null;
        }
        public CfgAudioPath GetCfgAudioPath(string audioId)
        {
            var cfg = GetCfgAudioTable();
            if(cfg.audiosCfg.TryGetValue(audioId,out var audioCfg))
            {
                return audioCfg;
            }
            return null;

        }

    }
}
