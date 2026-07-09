using Framework.Runtime;
using Framework.Runtime.MGameModule;
using UnityEngine;
namespace Game.Modules.GModuleInput
{
    public class GameInputViewHandler : GameModuleViewHandler
    {
        //private JoyStickInputPanel m_JoyStickInputPanel;

        protected override void OnHandlerAwake()
        {
            
        }
        protected override void OnHandlerEnable()
        {
            
        }
        protected override void OnHandlerStart()
        {
            MessageDispatcher.Ins.Subscribe(MessageCode.msg_open_joystick_panel,OpenJoyStickPanel);
            MessageDispatcher.Ins.Subscribe(MessageCode.msg_close_joystick_panel, CloseJoyStickPanel);
        }
        protected override void OnHandlerDestroy()
        {
            
        }
        private void CloseJoyStickPanel()
        {
            ClosePanel<JoyStickInputPanel>();
            //m_JoyStickInputPanel?.CloseWindow();
        }
        private void OpenJoyStickPanel()
        {
             OpenPanel<JoyStickInputPanel>("");
        }
    }

}
