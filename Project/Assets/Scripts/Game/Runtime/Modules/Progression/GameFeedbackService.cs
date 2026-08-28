using Framework.Runtime;
using Game.Modules.GModuleManage;
using UnityEngine;

namespace Game.Modules.GModuleProgression
{
    public static class GameFeedbackService
    {
        public static void OnWrongAction()
        {
            MessageDispatcher.Ins.Dispatch(MessageCode.msg_on_wrong_arrow_click);
            if (GameArchive.Main != null && GameArchive.Main.RoleArchive != null && GameArchive.Main.RoleArchive.GetShakeOpen()) Handheld.Vibrate();
        }
    }
}
