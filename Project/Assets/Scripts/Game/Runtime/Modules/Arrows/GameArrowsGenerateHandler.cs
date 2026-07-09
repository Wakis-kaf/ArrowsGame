using Framework.Runtime.MGameModule;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Modules.GModuleArrows
{
    public class GameArrowsGenerateHandler : GameModuleLogicHandler
    {

        public static GameArrowsGenerateHandler Ins => GetModuleHandlerIns<GameArrowsGenerateHandler>();

        internal LevelPointLayout GeneratePointLayoutByPointPresets(LevelPointPresets pointPresets)
        {
            throw new NotImplementedException();
        }
    }

}
