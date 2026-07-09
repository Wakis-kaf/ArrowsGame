using Framework.Runtime.Modules.UI.PrefabBind;
using Framework.Runtime.UI;
using Game.Modules.GModuleInventory;
using static Game.Modules.GModuleInventory.Inventory;

namespace Game.Modules
{
    public class UListGridRender : UListDisplayUnit
    {
        #region PrefabBinder 自动引用区域 开始

        private UnityEngine.RectTransform transGoodsImage;
        private Framework.Runtime.UI.UText utxtContent;

        #endregion PrefabBinder 自动引用区域 开始

        private GoodsImage m_ItemGoodsImage;

        /// <summary>
        /// 当绑定UI被删除时回调(子类重写)
        /// </summary>
        public override void OnDestroy()
        {
        }

        protected override void AutoExtractPrefabBinderComponent(PrefabBinder prefabBinder)
        {
            this.transGoodsImage = prefabBinder.GetObj<UnityEngine.RectTransform>("transGoodsImage");
            this.utxtContent = prefabBinder.GetObj<Framework.Runtime.UI.UText>("utxtContent");
        }

        protected override void OnGUI(object data)
        {
            if (data is InventoryGrid grid)
            {
                grid.AddChangeListener(UpdateInventoryGrid);
                UpdateInventoryGrid(data as InventoryGrid);
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
            m_ItemGoodsImage = GoodsImage.CreateGoodsImage(this.transGoodsImage);
            m_ItemGoodsImage.Show();
        }

        /// <summary>
        /// 当绑定UI加载完成且UI显示回调(子类重写)
        /// </summary>
        protected override void OnShow()
        {
            base.OnShow();
        }

        /// <summary>
        /// 当绑定UI加载完成且数据更新的时候调用(子类重写)
        /// </summary>
        /// <param name="data"></param>
        private void UpdateInventoryGrid(InventoryGrid grid)
        {
            this.utxtContent.text = grid.Index + "";
            if (!grid.IsUsing)
            {
                this.m_ItemGoodsImage.Hide();
            }
            else
            {
                this.m_ItemGoodsImage.Show();
                this.m_ItemGoodsImage.SetData(grid.GetItem());
            }
        }
    }
}