using BehaviorDesigner.Runtime.Tasks.Unity.UnityTransform;
using Framework.Runtime;
using Framework.Runtime.MAsset;
using Framework.Runtime.Modules.UI.PrefabBind;
using Framework.Runtime.UI;
using Framework.Utils;
using Game.Modules.GModuleInventory;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Modules
{
  
    public class GoodsImage : UListDisplayUnit
    {
        #region PrefabBinder 自动引用区域 开始
		private Framework.Runtime.UI.UImage uimgFlow;
		private Framework.Runtime.UI.USprite uspDouble;
		private Framework.Runtime.UI.USprite uspRed;
		private Framework.Runtime.UI.UTMPText utmpTxtCount;
		private Framework.Runtime.UI.USprite uspGot;
		private Framework.Runtime.UI.UText utxtName;
		private Framework.Runtime.UI.USprite uspIcon;
		private Framework.Runtime.UI.USprite uspIconBg;
		private Framework.Runtime.UI.UTexture utexIcon;

		#endregion PrefabBinder 自动引用区域 结束
        private GoodsImageOption m_GoodsImageOption;
        public static GoodsImage CreateGoodsImage(Transform rootTransform)
        {
            string link = AssetPathEncoder.EncodeEnvAssetLink("Assets/AddressableResources/UI/Inventory/Prefabs/GoodsImage.prefab");
            GoodsImage goodsImage = UIWindow.Ins.GetDisplayUnitAsync<GoodsImage>(link);
            goodsImage.ParentTransform = rootTransform;
            return goodsImage;
        }
        public override void OnOptionSet(ListOption option)
        {
            if(option is GoodsImageOption goodsImageOption)
            {
                m_GoodsImageOption = goodsImageOption;
                isClickShowInfo = goodsImageOption.isClickShowInfo;
                DisplayGO.transform.localScale = goodsImageOption.itemScale;
                if (CanvasGroup != null)
                {
                    isShowRayCaster = goodsImageOption.isRayCaster;
                    CanvasGroup.blocksRaycasts = isShowRayCaster;
                }
                this.UIBaseRender.raycastTarget = isShowRayCaster;
                    
            }
        }
        public bool isShowRayCaster = true;
        public bool isClickShowInfo = true;
        /// <summary>
        /// 当绑定UI被删除时回调(子类重写)
        /// </summary>
        public override void OnDestroy()
        {
        }
        public override string GetAssetLink(string outAssetLink)
        {
			string assetPath = "Assets/AddressableResources/UI/Inventory/Prefabs/GoodsImage.prefab";
			return AssetPathEncoder.EncodeEnvAssetLink(assetPath, AssetType.PrefabAsset);

		}
        public override int GetOpenLayer(int externalLayer)
        {
			return externalLayer;

		}
        protected override void AutoExtractPrefabBinderComponent(PrefabBinder prefabBinder)
        {
			this.uimgFlow = prefabBinder.GetObj<Framework.Runtime.UI.UImage>("uimgFlow");
			this.uspDouble = prefabBinder.GetObj<Framework.Runtime.UI.USprite>("uspDouble");
			this.uspRed = prefabBinder.GetObj<Framework.Runtime.UI.USprite>("uspRed");
			this.utmpTxtCount = prefabBinder.GetObj<Framework.Runtime.UI.UTMPText>("utmpTxtCount");
			this.uspGot = prefabBinder.GetObj<Framework.Runtime.UI.USprite>("uspGot");
			this.utxtName = prefabBinder.GetObj<Framework.Runtime.UI.UText>("utxtName");
			this.uspIcon = prefabBinder.GetObj<Framework.Runtime.UI.USprite>("uspIcon");
			this.uspIconBg = prefabBinder.GetObj<Framework.Runtime.UI.USprite>("uspIconBg");
			this.utexIcon = prefabBinder.GetObj<Framework.Runtime.UI.UTexture>("utexIcon");

		}

        protected override void OnGUI(object data)
        {
            if (!(data is Item item))
            {
                return;
            }
            var infoCfg = GameInventoryDataHandler.Ins.GetItemInfoCfg(item.itemId);
            if (infoCfg.quality <= 0)
            {
                uspIconBg.Path = $"CommonUI.game_panel_award";
            }
            else
            {
                uspIconBg.Path = $"CommonUI.first_build_grid{infoCfg.quality}";
            }
                
            GameObjectUtil.SetActive(uspRed, item.showRed);
            GameObjectUtil.SetActive(uspDouble, item.showDouble);
            GameObjectUtil.SetActive(uimgFlow, item.showFlow);
            if (item.status == Item.ItemStatus_Got)
            {
                GameObjectUtil.SetActive(this.uspGot, true);
            }
            else
            {
                GameObjectUtil.SetActive(this.uspGot, false);
            }

            if (!string.IsNullOrEmpty(item.iconTexPath))
            {
                this.utexIcon.gameObject.SetActive(true);
                this.utexIcon.TexPath = item.iconTexPath;
            }
            else
            {
                this.utexIcon.gameObject.SetActive(false);
            }
            if (!string.IsNullOrEmpty(item.iconSpritePath))
            {
                this.uspIcon.gameObject.SetActive(true);
                this.uspIcon.Path = item.iconSpritePath;
            }
            else
            {
                this.uspIcon.gameObject.SetActive(false);
            }

            this.utxtName.text = item.itemName;
            if (m_GoodsImageOption!=null && !m_GoodsImageOption.isShowCount)
            {
                SetActive(utmpTxtCount, false);
            }
            else
            {
                SetActive(utmpTxtCount, true);
                this.utmpTxtCount.text = "x" + item.count;
            }
                
        }

        /// <summary>
        /// 当绑定UI加载完成且UI隐藏回调(子类重写)
        /// </summary>
        protected override void OnHide()
        {
            base.OnHide();
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
            this.UIBaseRender.AddPointerDown(OnRenderPointDown);
            this.UIBaseRender.AddPointerUp(OnRenderPointUp);
        }
        private void OnRenderPointUp(PointerEventData evtData)
        {
            DispatchEevent(MessageCode.msg_close_goodsInfo_panel);
        }
        private void OnRenderPointDown(PointerEventData evtData)
        {
            if (!isClickShowInfo || !(this.Data is Item item)) return;
            var data = new GoodsInfoData();
            data.itemId = item.itemId;
            data.rectFrom = this.rectTransform;
            DispatchEevent(MessageCode.msg_open_goodsInfo_panel, data);
        }
        /// <summary>
        /// 当绑定UI加载完成且UI显示回调(子类重写)
        /// </summary>
        protected override void OnShow()
        {
            base.OnShow();
            CanvasGroup.blocksRaycasts = isShowRayCaster;
        }

        /// <summary>
        /// 当绑定UI加载完成且数据更新的时候调用(子类重写)
        /// </summary>
        /// <param name="data"></param>
    }

    public class GoodsImageOption : ListOption
    {
        public bool isClickShowInfo = true;
        public bool isRayCaster = true;
        public bool isShowCount = true;
    }
}






