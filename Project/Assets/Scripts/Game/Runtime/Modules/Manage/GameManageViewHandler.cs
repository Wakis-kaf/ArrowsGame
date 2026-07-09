using Framework.Runtime;
using Framework.Runtime.Config;
using Framework.Runtime.MGameModule;
using Framework.Runtime.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Modules.GModuleManage
{
    public class GameManageViewHandler : GameModuleViewHandler
    {
        public static GameManageViewHandler Ins => GetModuleHandlerIns<GameManageViewHandler>();

        protected override void OnHandlerAwake()
        {

        }
        protected override void OnHandlerEnable()
        {

        }
        protected override void OnHandlerStart()
        {
            var debuggerPanel = GameApp.Debugger.GetDebuggerPanel();
            if (debuggerPanel != null && debuggerPanel.IsModelLoaded)
            {
                OnDebuggerPanelLoaded();
            }
            else
            {

                GameApp.Debugger.OnDebuggerPanelLoaded += OnDebuggerPanelLoaded;
            }


        }

        private void OnDebuggerPanelLoaded()
        {
            if (GameEnv.IsInDevlopMode() && GameEnv.AppStatus == AppStatus.Devlop)
            {
                //string envViewPath = UIRoot.Instance.uiMode == UIMode.Pc ? GlobalConstant.PcDebuggerEnvViewLink
                //    : GlobalConstant.PhoneDebuggerEnvViewLink;
                GameApp.Debugger.GetDebuggerPanel().TabNavView.SetTabNavView<DebugGMView>("", 3, "GM");
            }
        }

        protected override void OnHandlerDestroy()
        {

        }
    }

}
