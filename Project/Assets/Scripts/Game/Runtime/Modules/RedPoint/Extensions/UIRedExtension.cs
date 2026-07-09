using Game.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Framework.Runtime.UI
{
    public static class UIRedExtension
    {
       
        public static void RegisterTabViewReds(this UckbTabNavView tabNavigation,
            List<string[]> reds)
        {
            for (int i = 0; i < reds.Count; i++)
            {
                tabNavigation.RegisterTabViewReds(i, reds[i]);
            }
        }
        public static void RegisterTabViewReds(this UckbTabNavView tabNavigation,
            int tabIndex,
            string[] reds)
        {
            if (reds == null || reds.Length == 0) 
                return;
            tabNavigation.RegisterTabReds(tabIndex, reds);

            for (int i = 0; i < reds.Length; i++)
            {
                var redPointVO = GameRedPointModule.Ins.GetRedPointVO(reds[i]);
                redPointVO.AddChangeCallBack((redPointVO) =>
                {
                    if (tabNavigation.TryGetRedTabs(tabIndex, out var reds))
                    {
                        bool isShow = false;
                        for (int i = 0; i < reds.Length; i++)
                        {
                            if (GameRedPointModule.Ins.GetRedPointState(reds[i]))
                            {
                                isShow = true;
                                break;
                            }
                        }
                        var tab = tabNavigation.TabNavigation.TagGroup.GetTabBar(tabIndex);
                        tab.SetRedVisible(isShow);
                    }
                });
            }
        }
    }
}
