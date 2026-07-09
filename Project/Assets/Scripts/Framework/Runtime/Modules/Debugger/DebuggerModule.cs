using System;
using Framework.Runtime.Config;
using Framework.Runtime.MDebugger.UIView;
using Framework.Runtime.Module.Core;
using Framework.Runtime.UI;

namespace Framework.Runtime.MDebugger
{
    public class DebuggerModule : ModuleUnit
    {
        public Action OnDebuggerPanelLoaded;

        public DebuggerPanel GetDebuggerPanel()
        {
            return PanelManager.Ins.FindPanel<DebuggerPanel>();
        }
        public void FoldDebuggerPanel()
        {
            GetDebuggerPanel()?.Fold();
        }

        public override void OnAppUpdate(GameAppMessage appMessage)
        {
            base.OnAppUpdate(appMessage);
            if (appMessage.MessageCode == GameAppMessage.code_mainGameState_changed
                && MainGame.Instance.IsAssetModuleLoadSuc)
            {
                if (GameEnv.IsInDevlopMode() && FrameworkSetting.Instance.isDefaultOpenDebugger)
                {
                    OpenDebuggerPanel();
                }
            }
        }
        public void CloseDebuggerPanel()
        {
            if (!GameEnv.IsInDevlopMode()) return;
            PanelManager.Ins.ClosePanel<DebuggerPanel>();
        }
        public void OpenDebuggerPanel()
        {
            if (!GameEnv.IsInDevlopMode()) return;
            string path = UIRoot.Instance.uiMode == UIMode.Pc ? GlobalConstant.PcDebuggerPanelLink :
                    GlobalConstant.PhoneDebuggerPanelLink;
            PanelManager.Ins.OpenPanel<DebuggerPanel>(path,
                GlobalConstant.LAYER_DEBUGGER);
        }
        public bool IsDebuggrPanelShowing()
        {
            return GetDebuggerPanel()?.IsShow ?? false;
        }

        protected override void OnModuleConstructed()
        {
            base.OnModuleConstructed();
        }
        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            OnDebuggerPanelLoaded = null;
        }
        //protected override void OnConstructed()
        //{
        //    base.OnConstructed();
        //    EventDriven.GetChanel<UFrameEventRootQueue>(UFrameEventRootConstant.CHANEL_FRAME_GLOBAL)
        //        ?.AddMessageListener(UFrameEventRootConstant.MESSAGE_UI_MODULE_INIT_OVER, OnUIModuleInit, true);
        //}
    }
}