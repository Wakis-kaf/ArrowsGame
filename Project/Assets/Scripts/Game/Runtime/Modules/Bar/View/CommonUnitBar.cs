using Framework.Runtime.MAsset;
using Framework.Runtime.Modules.UI.PrefabBind;
using Framework.Runtime.UI;
using Framework.Utils;
using Game.Modules.GModuleBar;
using UnityEngine;
namespace Game.Modules
{
   
    public class CommonUnitBar : Bar
    {
        public enum BarColor
        {
            Red,
            Green
        }
       public class CommonUnitBarOption
        {
            public int hp;
            public int maxHp;
            public bool showTxt;
            public BarColor barColor = BarColor.Green;
        }
        public CommonUnitBarOption BarOption { get; private set; }
        
        #region PrefabBinder 自动引用区域 开始
        private Framework.Runtime.UI.UText utxtContent;
		private Framework.Runtime.UI.USprite uspFillRed;
		private Framework.Runtime.UI.USprite uspFillGreen;
		private Framework.Runtime.UI.UProgressBar upbProgress;

        #endregion PrefabBinder 自动引用区域 结束
        
        protected override void AutoExtractPrefabBinderComponent(PrefabBinder prefabBinder)
        {
			this.utxtContent = prefabBinder.GetObj<Framework.Runtime.UI.UText>("utxtContent");
			this.uspFillRed = prefabBinder.GetObj<Framework.Runtime.UI.USprite>("uspFillRed");
			this.uspFillGreen = prefabBinder.GetObj<Framework.Runtime.UI.USprite>("uspFillGreen");
			this.upbProgress = prefabBinder.GetObj<Framework.Runtime.UI.UProgressBar>("upbProgress");

		}
        //private bool m_IsStyleChanged = false;
        protected override void OnInit()
        {
            base.OnInit();
            BarOption = new CommonUnitBarOption();
            this.Data = BarOption;
        }

        public override string GetAssetLink(string outAssetLink)
        {
			string assetPath = "Assets/AddressableResources/UI/Bar/Prefabs/CommonUnitBar.prefab";
			return AssetPathEncoder.EncodeEnvAssetLink(assetPath, AssetType.PrefabAsset);

		}
        public override void ResetByOption(BarOption option)
        {
            base.ResetByOption(option);
            //m_IsStyleChanged = false;
        }
        protected override void OnShow()
        {
            base.OnShow();
            //m_IsStyleChanged = false;
            RefreshStyle();
        }
        public override void Hide()
        {
            base.Hide();
            //m_IsStyleChanged = false;
        }
        public void RefreshStyle()
        {
            if (BarOption.barColor == BarColor.Green)
            {
                GameObjectUtil.SetActive(uspFillGreen, true);
                GameObjectUtil.SetActive(uspFillRed, false);
                this.upbProgress.fillRect = uspFillGreen.rectTransform;
            }
            else
            {
                GameObjectUtil.SetActive(uspFillGreen, false);
                GameObjectUtil.SetActive(uspFillRed, true);
                this.upbProgress.fillRect = uspFillRed.rectTransform;
            }
        }
        protected override void OnGUI(object data)
        {
            RefreshStyle();
            if (!(data is CommonUnitBarOption barOption)) return;
            // 这里需要根据实际的data类型进行转换
            // 假设data有hp, maxHp, name属性
            barOption.hp = Mathf.Clamp(barOption.hp, 0, barOption.maxHp);
            this.upbProgress.value = (float)barOption.hp / (float)barOption.maxHp;
            if (barOption.showTxt)
            {
                GameObjectUtil.SetActive(this.utxtContent, true);
                this.utxtContent.text = barOption.hp + "/" + barOption.maxHp;
            }
            else
            {
                GameObjectUtil.SetActive(this.utxtContent, false);
            }
            
            
        }
  
    }
}



