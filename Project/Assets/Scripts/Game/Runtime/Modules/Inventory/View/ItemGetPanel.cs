using Framework.Runtime.Config;
using Framework.Runtime.MAsset;
using Framework.Runtime.Modules.UI.PrefabBind;
using Framework.Runtime.UI;
using Game.Modules.GModuleAudio;
using Game.Modules.GModuleInventory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Modules
{
    public class ItemGetPanel : Panel
    {
        #region PrefabBinder 自动引用区域 开始
		private Framework.Runtime.UI.UButton ubtnClose;
		private Framework.Runtime.UI.UList ulistRewards;
		private Framework.Runtime.UI.USprite uspContainer;
		private Framework.Runtime.UI.UButton ubtnBgClose;

		#endregion PrefabBinder 自动引用区域 结束

        protected override void AutoExtractPrefabBinderComponent(PrefabBinder prefabBinder)
        {
			this.ubtnClose = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnClose");
			this.ulistRewards = prefabBinder.GetObj<Framework.Runtime.UI.UList>("ulistRewards");
			this.uspContainer = prefabBinder.GetObj<Framework.Runtime.UI.USprite>("uspContainer");
			this.ubtnBgClose = prefabBinder.GetObj<Framework.Runtime.UI.UButton>("ubtnBgClose");

		}
        public override int GetOpenLayer(int externalLayer)
        {
			return GlobalConstant.LAYER_PANEL;

		}

        public override string GetAssetLink(string outAssetLink)
        {
			string assetPath = "Assets/AddressableResources/UI/Inventory/Prefabs/ItemGetPanel.prefab";
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
            this.ulistRewards.ListRenderType = typeof(GoodsImage);
            //this.ubtnClose.AddClick(this.CloseWindow);
        }
        /// <summary>
        /// 当绑定UI加载完成且UI显示回调(子类重写)
        /// </summary>
        protected override void OnShow()
        {
            GameAudioClientHandler.Ins.PlayEffect(GameAudioConstant.Eff_ItemGetTip);
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

        protected override void OnDataChanged(object oldData,object newData)
        {
            if(newData is List<Item> items)
            {
                ShowView(items);
            }
        }
        private void ShowView(List<Item> items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                item.status = Item.ItemStatus_UnLock;
                item.showFlow = true;
                items[i] = item;
            }
            this.ulistRewards.SetDataSources(items);
        }
        /// <summary>
        /// 当绑定UI被删除时回调(子类重写)
        /// </summary>
        public override void OnDestroy()
        {

        }
    }
}



