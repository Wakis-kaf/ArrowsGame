using Framework.Runtime;
using Framework.Runtime.MGameModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Modules.GModuleGuid
{
    public class GameGuideViewHandler : GameModuleViewHandler
    {

        protected override void OnHandlerAwake()
        {
            
        }
        protected override void OnHandlerStart()
        {
            MessageDispatcher.Ins.Subscribe<GuideDialogOption>(MessageCode.msg_open_gameGuide_panel, OpenGuideDialogPanel);
            MessageDispatcher.Ins.Subscribe<GuideHighlightOption>(MessageCode.msg_open_gameGuide_panel, OpenGuideDialogPanel);
            MessageDispatcher.Ins.Subscribe<GuidePointOption>(MessageCode.msg_open_gameGuide_panel, OpenGuideDialogPanel);
            MessageDispatcher.Ins.Subscribe(MessageCode.msg_close_gameGuide_panel, CloseGuideDialogPanel);
        }
        private void CloseGuideDialogPanel()
        {
           ClosePanel<GameGuidePanel>();            
        }
        private void OpenGuideDialogPanel(GuideDialogOption option)
        {
            var panel= OpenPanel<GameGuidePanel>("");
            panel.SetData(option);
        }
        private void OpenGuideDialogPanel(GuideHighlightOption option)
        {
            var panel= OpenPanel<GameGuidePanel>("");
            panel.SetData(option);
        }
        private void OpenGuideDialogPanel(GuidePointOption option)
        {
            var panel= OpenPanel<GameGuidePanel>("");
            panel.SetData(option);
        }
        protected override void OnHandlerDestroy()
        {
            
        }
    }

}
