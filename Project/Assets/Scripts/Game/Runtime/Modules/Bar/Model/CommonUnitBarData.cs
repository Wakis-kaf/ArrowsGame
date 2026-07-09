using Framework.Runtime.Config;
using UnityEngine;
namespace Game.Modules.GModuleBar
{

    public class CommonUnitBarData : BarOption
    {
        public CommonUnitBarData()
        {
            this.layer = GlobalConstant.LAYER_INSTRUCTION;
            this.prefabLink = "";
            this.barType = typeof(CommonUnitBar);
            this.popAnchorPos = new Vector2(0, -200);
            this.size = Vector2.zero;
            this.data = null;
            this.usePoolBar = true;
            this.autoPut = true;
        }
    }
}