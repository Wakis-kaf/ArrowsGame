using Framework.Runtime.LogSystem;
using Framework.Runtime.MGameModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Modules.GModuleGuid
{
    public class GameGuideDataHandler : GameConfigDataHandler
    {
        protected override void OnHandlerAwake()
        {

        }
        protected override void OnHandlerStart()
        {
            
            
        }
        protected override void OnHandlerDestroy()
        {
           
        }
        public  CfgGuide GetCfgGuide()
        {
            if (TryReadConfig<CfgGuide>("cfg_guide", out var gameGuideCfg))
            {
                var guideNodes = gameGuideCfg.guideNodes;

                Log.Info("加载新手引导配置表成功;");
            }
            else
            {
                Log.Error("加载新手引导配置表失败;");
            }
            return gameGuideCfg;
        }
    }
}
