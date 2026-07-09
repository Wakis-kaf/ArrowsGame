using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Modules.GModuleGuid
{
    public static class CommonTriggerAgents
    {
        /// <summary>
        /// 触发器-打开面板
        /// </summary>
        public const string trigger_dispatch_event = "trigger_dispatch_event";
        public static void InjectAgents()
        {
            GuideFactoryTable.RegisterGuideTriggerType<Trigger_Dispatch_Event>(trigger_dispatch_event);
        }





    }
}
