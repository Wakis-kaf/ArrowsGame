using Framework.Runtime.Config;
using Framework.Runtime.MAsset;
using Framework.Runtime.Modules.UI.PrefabBind;
using Framework.Runtime.UI;
using Game.Modules.GModuleInventory;

using Game.Runtime.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Modules
{
    public struct GoodsInfoData
    {
        public int itemId;
        public RectTransform rectFrom;
    }
    public class GoodsInfoPanel : Panel
    {
        #region PrefabBinder 自动引用区域 开始
		private UnityEngine.RectTransform transTipSafeArea;
		private Framework.Runtime.UI.UTMPText utmpTxtHas;
		private Framework.Runtime.UI.UTMPText utmpTxtDesc;
		private Framework.Runtime.UI.UTMPText utmpTxtName;
		private Framework.Runtime.UI.USprite uspIcon;
		private Framework.Runtime.UI.USprite uspIconBg;
		private Framework.Runtime.UI.USprite uspInfoTip;
		private Framework.Runtime.UI.UButton ubtnClose;

		#endregion PrefabBinder 自动引用区域 结束

        protected override void AutoExtractPrefabBinderComponent(PrefabBinder prefabBinder)
        {
			this.transTipSafeArea = prefabBinder.GetObj<UnityEngine.RectTransform>("transTipSafeArea");
			this.utmpTxtHas = prefabBinder.GetObj<Framework.Runtime.UI.UTMPText>("utmpTxtHas");
			this.utmpTxtDesc = prefabBinder.GetObj<Framework.Runtime.UI.UTMPText>("utmpTxtDesc");
			this.utmpTxtName = prefabBinder.GetObj<Framework.Runtime.UI.UTMPText>("utmpTxtName");
			this.uspIcon = prefabBinder.GetObj<Framework.Runtime.UI.USprite>("uspIcon");
			this.uspIconBg = prefabBinder.GetObj<Framework.Runtime.UI.USprite>("uspIconBg");
			this.uspInfoTip = prefabBinder.GetObj<Framework.Runtime.UI.USprite>("uspInfoTip");
			this.ubtnClose = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnClose");

		}
        public override int GetOpenLayer(int externalLayer)
        {
			return GlobalConstant.LAYER_HIGH_PANEL;

		}

        public override string GetAssetLink(string outAssetLink)
        {
			string assetPath = "Assets/AddressableResources/UI/Inventory/Prefabs/GoodsInfoPanel.prefab";
			return AssetPathEncoder.EncodeEnvAssetLink(assetPath, AssetType.PrefabAsset);

		}
        /// <summary>
        /// 子类重写，构造函数中调用
        /// </summary>
        protected override void OnInit()
        {

        }
        /// <summary>
        /// 显示对象初始化UI,当绑定的预制体加载完成后回调(子类重写)
        /// </summary>
        protected override void OnInitUI()
        {

        }
        /// <summary>
        /// 当绑定UI加载完成且UI显示回调(子类重写)
        /// </summary>
        protected override void OnShow()
        {

        }
        /// <summary>
        /// 当绑定UI加载完成且UI隐藏回调(子类重写)    
        /// </summary>
        protected override void OnHide()
        {

        }
        /// <summary>
        /// 当绑定UI加载完成且数据更新的时候调用(子类重写)
        /// </summary>
        /// <param name="data"></param>

        protected override void OnGUI(object data)
        {
            if(data is GoodsInfoData infoData)
            {
                ShowItemInfo(infoData);
            }
        }
        private void ShowItemInfo(GoodsInfoData infoData)
        {
            UIPopupUtil.GetPopAnchorPos(infoData.rectFrom, this.uspInfoTip.rectTransform,
                UIRootCamera.Camera, out Vector2 anchorPos, this.transTipSafeArea);
            this.uspInfoTip.rectTransform.anchoredPosition = anchorPos;
            int itemId = infoData.itemId;
            var itemCfg = GameInventoryDataHandler.Ins.GetItemInfoCfg(itemId);
            if (itemCfg.quality <= 0)
            {
                uspIconBg.Path = $"CommonUI.game_panel_award";
            }
            else
            {
                uspIconBg.Path = $"CommonUI.first_build_grid{itemCfg.quality}";
            }
            this.uspIcon.Path = itemCfg.iconSpritePath;
            this.utmpTxtName.text = itemCfg.itemName;
            this.utmpTxtHas.text = $"拥有：{StringFormatUtil.FormatCurrency(GameInventoryDataHandler.Ins.GetItemHasCount(itemId))}";
            this.utmpTxtDesc.text = itemCfg.description;
            //var uSelect = UIEventUtil.GetOrAddUSelect(this.uspInfoTip.transform);
            //uSelect.SetSelect(OnOutSideSelect);
            //UIUtil.SetSelectedGameObject(this.uspInfoTip.gameObject);
        }
        private void OnOutSideSelect(bool isSelect)
        {
            if (!isSelect)
            {
                CloseTipInfo();
            }
        }
        private void CloseTipInfo()
        {
            CloseWindow();
        }


        /// <summary>
        /// 当绑定UI被删除时回调(子类重写)
        /// </summary>
        public override void OnDestroy()
        {

        }
    }
}




