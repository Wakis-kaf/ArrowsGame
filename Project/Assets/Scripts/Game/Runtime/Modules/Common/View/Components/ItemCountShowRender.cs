using Framework.Runtime;
using Framework.Runtime.LogSystem;
using Framework.Runtime.MAsset;
using Framework.Runtime.Modules.UI.PrefabBind;
using Framework.Runtime.UI;
using Framework.Utils;
using Game.Modules.GModuleInventory;
using Game.Modules.GModuleTip;
using Game.Runtime.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Modules
{
    public class ItemCountShowRenderOption
    {
        public bool isAutoUpdate;
        public bool isCustomShowTxt;
        public string customTxt;
        public bool isMultiStyle;
        public bool isStreatchAll;
        public int targetItemId;
        public int countTxtSize = -1;
        public Vector2 iconSize = Vector2.zero;
    }
    public class ItemCountShowRender : DisplayUnit
    {
        #region PrefabBinder 自动引用区域 开始
		private Framework.Runtime.UI.UText utxtCount;
		private Framework.Runtime.UI.USprite uspIcon;

		#endregion PrefabBinder 自动引用区域 结束
        private int m_Count;
        public ItemCountShowRenderOption Option { get; private set; }
        private CfgItemInfo m_itemInfoCfg;

        protected override void AutoExtractPrefabBinderComponent(PrefabBinder prefabBinder)
        {
			this.utxtCount = prefabBinder.GetObj<Framework.Runtime.UI.UText>("utxtCount");
			this.uspIcon = prefabBinder.GetObj<Framework.Runtime.UI.USprite>("uspIcon");

		}

        public override string GetAssetLink(string outAssetLink)
        {
            string name = string.IsNullOrEmpty(outAssetLink)?"ItemCountShowRender":outAssetLink;
			string assetPath = $"Assets/AddressableResources/UI/Common/Prefabs/{name}.prefab";
			return AssetPathEncoder.EncodeEnvAssetLink(assetPath, AssetType.PrefabAsset);

		}

        /// <summary>
        /// 子类重写，构造函数中调用
        /// </summary>
        protected override void OnInit()
        {
            Option = new ItemCountShowRenderOption();
        }
        /// <summary>
        /// 显示对象初始化UI,当绑定的预制体加载完成后回调(子类重写)
        /// </summary>
        protected override void OnInitUI()
        {
            m_itemInfoCfg = GameModuleHandler.GetModuleHandlerIns<GameInventoryDataHandler>().
                GetItemInfoCfg(this.Option.targetItemId);
            SubscribeEvent(MessageCode.msg_on_inventory_changed, BindInventory);
            BindInventory();
            if (this.Option.isStreatchAll) {
                UIUtil.SetAnchor(RectTransform, AnchorPresets.StretchAll);
                UIUtil.SetOffsetZero(RectTransform);
            }
            
        }
        private void BindInventory()
        {
            if (!Option.isAutoUpdate) return;
            Inventory inventory = GameModuleHandler.GetModuleHandlerIns<GameInventoryDataHandler>().
                GetInventory(m_itemInfoCfg.inventoryType);
            if (inventory == null)
            {
                Log.Error($"未找到背包类型为{m_itemInfoCfg.inventoryType} 的背包");
                return;
            }
            // TODO:添加道具回调
            if (Option.isAutoUpdate)
            {
                inventory.AddItemChangedListener(m_itemInfoCfg.itemId, UpdateGUI);
            }
            else
            {
                inventory.RemoveItemChangedListener(m_itemInfoCfg.itemId, UpdateGUI);
            }
        }
        /// <summary>
        /// 当绑定UI加载完成且UI显示回调(子类重写)
        /// </summary>
        protected override void OnShow()
        {
            uspIcon.Path = this.m_itemInfoCfg.iconSpritePath;   
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
            if (Option.isAutoUpdate)
            {
                this.m_Count = GameInventoryDataHandler.Ins.GetItemHasCount(this.Option.targetItemId);
            }
            string txtStr = "";
            if (!Option.isCustomShowTxt)
            {
                string txt = StringFormatUtil.FormatCurrency(this.m_Count);
                if (m_itemInfoCfg.isShowMax)
                {
                    txt += "/"+GameInventoryDataHandler.Ins.GetItemHoldMaxCount( m_itemInfoCfg);
                }
                txtStr = txt;
            }
            else
            {
                txtStr = Option.customTxt;
            }
            if (Option.isMultiStyle)
            {
                txtStr = "x" + txtStr;
            }
            this.utxtCount.text = txtStr;
            if (Option.countTxtSize > 0)
            {
                this.utxtCount.fontSize = Option.countTxtSize;
            }
            if (Option.iconSize.magnitude >= 1)
            {
                uspIcon.rectTransform.sizeDelta = Option.iconSize;
            }
        }
        /// <summary>
        /// 当绑定UI被删除时回调(子类重写)
        /// </summary>
        public override void OnDestroy()
        {

        }
        public static ItemCountShowRender CreateRenderByGO(int itemId, GameObject go, bool isAutoUpdate = true)
        {

            ItemCountShowRender countShowRender = UIWindow.Ins.GetDisplayUnitDirect<ItemCountShowRender>(go);
            countShowRender.Option.targetItemId = itemId;
            countShowRender.Option.isAutoUpdate = isAutoUpdate;
            countShowRender.Show();
            return countShowRender;

        }

        public static ItemCountShowRender CreateRender(int itemId, Transform parentTransform, bool isAutoUpdate = true,string path = "")
        {

            ItemCountShowRender countShowRender = UIWindow.Ins.GetDisplayUnitAsync<ItemCountShowRender>(path);
            countShowRender.ParentTransform = parentTransform;
            countShowRender.Option.targetItemId = itemId;
            countShowRender.Option.isAutoUpdate = isAutoUpdate;
            countShowRender.Show();
            return countShowRender;

        }
        public void ShowText(string txt)
        {
            Option.isCustomShowTxt = true;
            Option.customTxt = txt;
            this.UpdateGUI();
        }
        public void SetCount(int count)
        {
            Option.isCustomShowTxt = false;
            m_Count = count;
            this.UpdateGUI();
        }

    
        public void SetShowMultiStyle(bool isMultiStyle)
        {
            Option.isMultiStyle = isMultiStyle;
            UpdateGUI();
        }
    }

}



