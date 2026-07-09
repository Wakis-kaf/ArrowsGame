using Framework.Runtime.LogSystem;
using Framework.Runtime.MGameModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Modules
{
    public class GameRedPointDataHandler : GameConfigDataHandler
    {
        protected override void OnHandlerAwake()
        {

        }
        protected override void OnHandlerStart()
        {
            // 读取配置文件并注册
            if (TryReadConfig<CfgGameRedPoint>("cfg_redpoint", out var gameRedPointCfg))
            {
                Log.Info("加载红点配置表成功;");
                for (int i = 0; i < gameRedPointCfg.cfgRedPoints.Count; i++)
                {
                    GameRedPointModule.GetIns().RegisterRedPoint(gameRedPointCfg.cfgRedPoints[i]);
                }
            }
            else
            {
                Log.Error("加载红点配置表失败;");
            }
        }
        protected override void OnHandlerDestroy()
        {

        }
    }
}
