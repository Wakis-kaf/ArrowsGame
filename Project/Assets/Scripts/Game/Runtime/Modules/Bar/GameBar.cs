using Game.Modules;
using Game.Modules.GModuleTip;
using UnityEngine;
namespace Game.Modules.GModuleBar
{

    public class GameBar : GameModuleBaseInstance<GameBar>
    {
        public BarPool BarPool { get; private set; }
        private BarOption commonBarOption;
        // 默认弹出消息
        public Bar GetBar(object data = null, BarOption option = null)
        {
            if (option == null)
            {
                option = GetDefaultCommonBarOption();
            }
            option.data = data;
            return PopBar(option);
        }
        protected override void OnConstructed()
        {
            base.OnConstructed();
            BarPool = new BarPool();
        }

        public BarOption GetDefaultCommonBarOption()
        {
            if (commonBarOption != null)
            {
                return commonBarOption;
            }
            commonBarOption = new CommonUnitBarData();
            return commonBarOption;
        }
        public Bar PopBar(BarOption option)
        {
            Bar bar = BarPool.GetOrCreateBar(option, (barInstance) =>
            {
                barInstance.SetOption(option);
                barInstance.SetData(option.data ?? barInstance.Data);
            });
            return bar;
        }
        public void PutBar(Bar bar)
        {
            BarPool.PutBar(bar);
            
        }

    }
}