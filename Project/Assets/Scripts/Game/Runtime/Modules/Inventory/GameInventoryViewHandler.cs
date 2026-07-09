using Framework.Runtime;
using Framework.Runtime.MGameModule;
using System;
using System.Collections.Generic;

namespace Game.Modules.GModuleInventory
{
    public class GameInventoryViewHandler : GameModuleViewHandler
    {
        protected override void OnHandlerAwake()
        {
        }

        protected override void OnHandlerDestroy()
        {
        }

        protected override void OnHandlerStart()
        {
            MessageDispatcher.Ins.Subscribe<List<Item>>(MessageCode.msg_open_itemGetTip_panel, OnOpenItemGetTip);
            MessageDispatcher.Ins.Subscribe<GoodsInfoData>(MessageCode.msg_open_goodsInfo_panel, OnOpenGoodsInfoPanel);
            MessageDispatcher.Ins.Subscribe(MessageCode.msg_close_goodsInfo_panel, OnCloseGoodsInfoPanel);
        }
        private void OnCloseGoodsInfoPanel()
        {
            ClosePanel<GoodsInfoPanel>();
        }
        private void OnOpenGoodsInfoPanel(GoodsInfoData infoData)
        {
            var itemInfo = GameInventoryDataHandler.Ins.GetItemInfoCfg(infoData.itemId);
            if (itemInfo == null) return;
            var panel =  OpenPanel<GoodsInfoPanel>("");
            panel.SetData(infoData);
        }
        
        private void OnOpenItemGetTip(List<Item> items)
        {
            var  panel = OpenPanel<ItemGetPanel>("");
            panel.SetData(items);
        }
    }
}