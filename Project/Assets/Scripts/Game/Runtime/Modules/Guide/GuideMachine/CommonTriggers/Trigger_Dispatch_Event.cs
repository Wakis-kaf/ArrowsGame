using Framework.Runtime;
using Game.Modules.GModuleGuid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Modules.GModuleGuid
{
    /// <summary>
    /// 触发器发布事件
    /// </summary>
    public class Trigger_Dispatch_Event : GuideHandler
    {
        public string triggerEventCode;
        public string doneEventCode;
        public override bool OnTrigger()
        {
            // 取出active参数
            string eventCode = GetParam<string>("triggerEventCode");
            MessageDispatcher.Ins.Dispatch(eventCode);
            return true;
        }
    }
}
