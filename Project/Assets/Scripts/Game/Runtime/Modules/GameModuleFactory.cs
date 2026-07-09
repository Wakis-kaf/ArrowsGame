
using Game.Modules.GModuleArrowGenerateEdit;
using Game.Modules.GModuleArrows;
using Game.Modules.GModuleAudio;
using Game.Modules.GModuleBar;
using Game.Modules.GModuleFX;
using Game.Modules.GModuleGuid;
using Game.Modules.GModuleInput;
using Game.Modules.GModuleInventory;
using Game.Modules.GModuleManage;
using Game.Modules.GModuleModelCapture;
using Game.Modules.GModulePlay;
using Game.Modules.GModulePlayer;
using Game.Modules.GModuleScene;
using Game.Modules.GModuleSceneUnit;
using Game.Modules.GModuleStage;
using Game.Modules.GModuleTask;
using Game.Modules.GModuleTip;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Modules
{
    public class GameModuleFactory
    {
        public static List<Type> GameModuleList = new List<Type>()
        {
            typeof(GameConfigModule),
            typeof(GameRedPointModule),
            typeof(GameGuidModule),
            typeof(GameInventoryModule),
            // typeof(GameTaskModule),
            typeof(GameSceneUnitModule),
            typeof(GameInputModule),
            typeof(GameTip),
            typeof(GameBar),
            typeof(GameFX),
            typeof(GameAudioModule),
            typeof(GameModelCapture),
            typeof(GameManageModule),
            typeof(GamePlayModule),
            typeof(GameStageModule),
            typeof(GameSceneModule),
            typeof(GamePlayerModule),
            typeof(GameArrowsModule),
            typeof(GameArrowGenerateEditModule),

        };
    }
}

