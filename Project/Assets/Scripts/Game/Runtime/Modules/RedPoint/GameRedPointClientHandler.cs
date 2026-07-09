using Framework.Runtime;
using Framework.Runtime.MGameModule;

namespace Game.Modules
{
    public class GameRedPointClientHandler : GameModuleLogicHandler
    {

        protected override void OnHandlerAwake()
        {
            
        }
        protected override void OnHandlerStart()
        {

            ReCheckAllRedPoint();
            GameApp.Ins.LoopManager.AddSecond(SecondRedCheck,1);
        }
        private void ReCheckAllRedPoint()
        {

        }
        private void SecondRedCheck()
        {

        }

        protected override void OnHandlerDestroy()
        {
            
        }
    }

}
