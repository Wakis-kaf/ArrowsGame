// CommonMsgTip.cs
using Framework.Runtime.UI;
using Game.Modules.GModuleTip;
namespace Game.Modules.GModuleTip
{
    public class BounceMsgTip : Tip
    {
        public UText utxtTip = null;
        protected override void OnInitUI()
        {
            base.OnInitUI();
            this.utxtTip = GetBindObject<UText>("utxtTip");
        }
        protected override void OnGUI(object data)
        {
            string msg = data as string;
            this.utxtTip.text = msg;
        }
    }
}