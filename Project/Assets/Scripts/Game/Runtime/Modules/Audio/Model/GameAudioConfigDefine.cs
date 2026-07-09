using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Modules.GModuleAudio
{
    public class CfgAudioTable
    {
        public Dictionary<string, CfgAudioPath> audiosCfg;
    }
    public class CfgAudioPath
    {
        public int audioId;
        public string audioName;
        public string audioClipPath;
        public string audioDescription;
        public float startTime;
        public float volume;
        public int priority;
    }
}
