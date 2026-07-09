using Framework.Runtime.MGameModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Modules.GModuleTask
{
    public class GameTaskClientHandler : GameModuleLogicHandler
    {
        private TaskManager m_TaskManager;
        protected override void OnHandlerAwake()
        {
            m_TaskManager = new TaskManager();
        }
        protected override void OnHandlerEnable()
        {
            base.OnHandlerEnable();
        }
        protected override void OnHandlerStart()
        {
            var cfgTaskTable = GetModuleInHandler<GameTaskDataHandler>().GetGameTaskCfg();
            if (cfgTaskTable != null)
            {
                m_TaskManager.InitTaskByCfg(cfgTaskTable);
            }
        }
        protected override void OnHandlerDestroy()
        {
            
        }
    }

}
