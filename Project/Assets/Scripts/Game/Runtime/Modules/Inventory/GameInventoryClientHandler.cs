using Framework.Runtime;
using Framework.Runtime.MGameModule;
using Game.Modules.GModuleManage;
using System;
using System.Collections.Generic;

namespace Game.Modules.GModuleInventory
{
    public class GameInventoryClientHandler : GameModuleLogicHandler
    {
        protected override void OnHandlerAwake()
        {
            
            MessageDispatcher.Ins.Subscribe<GameArchive>(MessageCode.msg_on_mainArchiveLoaded, CheckArchive);
            MessageDispatcher.Ins.Subscribe<Inventory>(MessageCode.msg_on_inventory_added, OnNewInventoryAdded);
        }

        protected override void OnHandlerDestroy()
        {
        }

        protected override void OnHandlerStart()
        {
            
        }
        private List<int> m_ArchiveableInventoryId = new List<int>
        {
            GameInventoryConstant.InvType_GlobalEquip,
            GameInventoryConstant.InvType_GlobalCurrency,
            GameInventoryConstant.InvType_GlobalRoleEquip,
            GameInventoryConstant.InvType_GlobalCommon,
            GameInventoryConstant.InvType_GlobalBag,
            GameInventoryConstant.InvType_InGameBag, // TODO:  记得删除
        };
        public void AddAchiveSaveAble(int invId)
        {
            var inventory = GameInventoryDataHandler.Ins.GetInventory(invId);
            inventory?.AddChangeListener(OnInventoryChanged);
            if (!m_ArchiveableInventoryId.Contains(invId))
            {
                m_ArchiveableInventoryId.Add(invId);
            }
            
        }
        private void OnNewInventoryAdded(Inventory inventory)
        {
            if (m_ArchiveableInventoryId.Contains(inventory.Id))
            {
                AddAchiveSaveAble(inventory.Id);
                if(GameArchive.Main!=null && GameArchive.Main.InventoryArchive.TryGetInvMeta(inventory.Id,out var invMeta))
                {
                    inventory.ImportMeta(invMeta);
                }
            }
            
        }
        private void CheckArchive(GameArchive archive)
        {
            ImportInventoryMeta();
            CheckInventoryArchiveable();
            //var inventory = GameInventoryDataHandler.Ins.GetInventory(GameInventoryConstant.InvType_GlobalEquip);
            //inventory.AddChangeListener(OnInventoryChanged);
            //inventory = GameInventoryDataHandler.Ins.GetInventory(GameInventoryConstant.InvType_GlobalCurrency);
            //inventory.AddChangeListener(OnInventoryChanged);
            //inventory = GameInventoryDataHandler.Ins.GetInventory(GameInventoryConstant.InvType_GlobalRoleEquip);
            //inventory.AddChangeListener(OnInventoryChanged);
            //inventory = GameInventoryDataHandler.Ins.GetInventory(GameInventoryConstant.InvType_GlobalCommon);
            //inventory.AddChangeListener(OnInventoryChanged);
        }
        private void CheckInventoryArchiveable()
        {
            foreach (var invId in m_ArchiveableInventoryId)
            {
                AddAchiveSaveAble(invId);
            }
        }

        private void OnInventoryChanged(Inventory inventory, int opt)
        {
            if (opt == InventoryOpt.Refresh ||
                opt == InventoryOpt.SwitchGrid ||
                opt == InventoryOpt.Register ||
                opt == InventoryOpt.Tidy)
            {
                return;
            }
            if (GameArchive.Main != null)
            {
                GameArchive.Main.InventoryArchive.SaveInventory(inventory.Id, inventory.ExportMeta());
            }
        }
        private void ImportInventoryMeta()
        {
            var invMeta = GameArchive.Main?.InventoryArchive.InvMeta;
            if (invMeta == null) return;
            foreach (var item in invMeta)
            {
                var inventory = GameInventoryDataHandler.Ins.GetInventory(item.Key);
                if (inventory == null) continue;
                inventory.ImportMeta(item.Value);
            }
        }
    }
}