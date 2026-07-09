using Framework.Runtime.LogSystem;
using Framework.Runtime.MGameModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Modules.GModuleTask
{
    public class GameTaskDataHandler : GameConfigDataHandler
    {
        private CfgGameTaskTable m_CfgTaskTable;
        protected override void OnHandlerAwake()
        {

        }
        protected override void OnHandlerEnable()
        {
            if(TryReadConfig<CfgGameTaskTable>("cfg_task",out m_CfgTaskTable)){
                Log.Info("加载任务配置表成功");
            }
            else
            {
                Log.Error("加载任务配置表失败");
            }

        }
        public CfgGameTaskTable GetGameTaskCfg()
        {
            return m_CfgTaskTable;
        }
        protected override void OnHandlerStart()
        {

        }
        protected override void OnHandlerDestroy()
        {

        }
    }
}
