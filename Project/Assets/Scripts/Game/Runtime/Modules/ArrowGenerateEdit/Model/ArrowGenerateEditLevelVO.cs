using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Framework.Runtime;
using Framework.Runtime.LogSystem;
using Framework.Utils;
using Game.Modules.GModuleAudio;
using Game.Modules.GModuleSceneUnit;
using Game.Modules.GModuleStage;
using UnityEngine;
using static Game.Modules.GModuleArrows.ArrowLineSceneUnit;

namespace Game.Modules.GModuleArrows
{

    public class ArrowGenerateEditLevelVO : LevelVO
    {
        protected override void OnGamePlayPreparedOver()
        {
            SwitchStatus(LevelStatus.Playing);
            GameStage.PlayStage();
        }


    }
}
