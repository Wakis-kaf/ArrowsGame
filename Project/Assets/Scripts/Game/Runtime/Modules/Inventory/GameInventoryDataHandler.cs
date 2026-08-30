using Framework.Runtime;
using Framework.Runtime.LogSystem;
//using Game.Modules.GModulePlayer;
using System;
using System.Collections.Generic;

namespace Game.Modules.GModuleInventory
{
    public class GameInventoryDataHandler : GameConfigDataHandler
    {
        private CfgItemTable m_ItemCfgTable;
        private Dictionary<int, Inventory> m_Type2Inventory;
        public static GameInventoryDataHandler Ins => GetModuleHandlerIns<GameInventoryDataHandler>();
        public void ClearAllInventory()
        {
            foreach (var item in m_Type2Inventory)
            {
                item.Value.ClearInventory();
            }
            MessageDispatcher.Ins.Dispatch(MessageCode.msg_on_inventory_changed);
        }
        public void ClearGameInInventory()
        {
            GetInventory(GameInventoryConstant.InvType_InGameCurrency).ClearInventory();
            GetInventory(GameInventoryConstant.InvType_InGameCommon).ClearInventory();
            MessageDispatcher.Ins.Dispatch(MessageCode.msg_on_inventory_changed);
        }
        public void AddItemChangedListener(int itemId, Action changeCb)
        {
            var itemInfoCfg = GetItemInfoCfg(itemId);
            if (itemInfoCfg == null) return;
            Inventory inventory =  GetInventory(itemInfoCfg.inventoryType);
            if (inventory == null)
            {
                return;
            }
            inventory.AddItemChangedListener(itemId,changeCb);
        }
        public Inventory GetInventory(int inventoryType)
        {
            if (m_Type2Inventory.ContainsKey(inventoryType))
            {
                return m_Type2Inventory[inventoryType];
            }
            return null;
        }

        public int GetItemHasCount(int itemId)
        {
            var cfg = GetItemInfoCfg(itemId);
            if (cfg == null) return 0;
            Inventory inventory = GetInventory(cfg.inventoryType);
            if (inventory == null) return 0;
            return inventory.GetItemHasCount(itemId);
        }
        

        public CfgItemInfo GetItemInfoCfg(int itemId)
        {
            if(m_ItemCfgTable.itemInfoCfg!=null && m_ItemCfgTable.itemInfoCfg.TryGetValue(itemId,out var cfg))
            {
                if(cfg.maxStackCount == -1)
                {
                    cfg.maxStackCount = int.MaxValue;
                }
                if(cfg.maxPerStackCount == -1)
                {
                    cfg.maxPerStackCount = int.MaxValue;
                }
                //if(cfg.maxHoldCount == -1)
                //{
                //    cfg.maxHoldCount = int.MaxValue;
                //}
                return cfg;
            }
           
            return null;
        }

        public CfgItemParam GetItemParamCfg(int itemId)
        {
            if (m_ItemCfgTable.itemParamsCfg != null && m_ItemCfgTable.itemParamsCfg.TryGetValue(itemId, out var cfg))
            {
                return cfg;
            }
            return null;
        }

        public InventoryOperation StoreItem(int itemId, int count,ItemUniqueGroup uniqueGroup = null)
        {
            InventoryOperation inventoryOperation = new InventoryOperation();
            var cfg = GetItemInfoCfg(itemId);
            if (cfg == null)
            {
                inventoryOperation.errMessage = "物品不存在";
                return inventoryOperation;
            }
            Inventory inventory = GetInventory(cfg.inventoryType);
            if (inventory == null)
            {
                inventoryOperation.errMessage = "背包不存在";
                return inventoryOperation;
            }
            var opt= inventory.StoreItem(itemId, count, uniqueGroup);
            return opt;
        }

        public InventoryOperation TakeOutItem(int itemId, int count)
        {
            InventoryOperation inventoryOperation = new InventoryOperation();
            var cfg = GetItemInfoCfg(itemId);
            if (cfg == null)
            {
                inventoryOperation.errMessage = "物品不存在";
                return inventoryOperation;
            }
            Inventory inventory = GetInventory(cfg.inventoryType);
            if (inventory == null)
            {
                inventoryOperation.errMessage = "背包不存在";
                return inventoryOperation;
            }
            return inventory.TakeOutItem(itemId, count);
        }


        protected override void OnHandlerAwake()
        {
            m_Type2Inventory = new Dictionary<int, Inventory>();
            InitInventory();
        }

        protected override void OnHandlerDestroy()
        {
        }

        protected override void OnHandlerEnable()
        {
            if (TryReadConfig<CfgItemTable>("cfg_items", out m_ItemCfgTable))
            {
                Log.Info("读取道具配置成功");
            }
            else
            {
                Log.Error("读取道具配置失败");
            }
        }
        
        private Item CreateItem(int itemId)
        {
            var itemCfg = GetItemInfoCfg(itemId);
            var item = new Item();
            item.count = 0;
            item.enableStack = itemCfg.enableStack;
            item.isUnique = itemCfg.isUnique;
            item.iconSpritePath = itemCfg.iconSpritePath;
            item.iconTexPath = itemCfg.iconTexPath;
            item.itemId = itemCfg.itemId;
            item.itemName = itemCfg.itemName;
            item.maxHoldCount = GetItemHoldMaxCount(itemCfg);
            item.maxPerStackCount = itemCfg.maxPerStackCount;
            item.maxStackCount = itemCfg.maxStackCount;
            return item;
        }
        public List<Item> GetItems(int itemId,int count, bool useProduce = true)
        {
            if (!useProduce || !IsProduceEquiptItem(itemId))
            {
                var item = GetItem(itemId,false);
                item.count = count; 
                return new List<Item>() { item};
            }
       
            List<Item> items = new List<Item>();
            for (int i = 0; i < count; i++)
            {
                var item = GetItem(itemId, true);
                item.count = 1;
                items.Add(item);
            }
            
            return items;
        }


        public Item GetItem(int itemId,bool useProduce = true)
        {
            if (!useProduce)
            {
                return CreateItem(itemId);
            }
            if (IsProduceEquiptItem(itemId))
            {
                //int stage = itemId % 10;
                //stage = stage == 0 ? -1 : stage;
                //var uniqueMap = GamePlayerEquipClientHandler.Ins.GetRdmEquipUniqueMap(stage);
                //var equipItem = CreateItem(uniqueMap.itemUniqueDatas[0].itemId);
                //equipItem.uniqueGroup = uniqueMap;
                //return equipItem;
                throw new NotImplementedException("未实现的方法");
            }
            return CreateItem(itemId);
        }
        private bool IsProduceItem(int itemId)
        {
            var itemCfg = GetItemInfoCfg(itemId);
            int bgType = itemCfg.itemType % 100;
            return bgType == 99;
        }
        private bool IsProduceEquiptItem(int itemId)
        {
            var itemCfg = GetItemInfoCfg(itemId);
            return itemCfg.itemType == 9901;
        }


        public void AddInventory(int inventoryType, Inventory inventory)
        {
            if (!m_Type2Inventory.ContainsKey(inventoryType))
            {
                m_Type2Inventory.Add(inventoryType, inventory);
            }
            MessageDispatcher.Ins.Dispatch(MessageCode.msg_on_inventory_added, inventory);
            MessageDispatcher.Ins.Dispatch(MessageCode.msg_on_inventory_changed);
        }

        private void InitInventory()
        {
            AddInventory(GameInventoryConstant.InvType_InGameCurrency, new Inventory(new InventoryInitOption()
            {
                Name = "游戏局内货币背包",
                id = GameInventoryConstant.InvType_InGameCurrency,
                autoTidy = true,
                initGridCount = 10,
                autoExpand = true,
                maxGridCount = int.MaxValue
            }));

            AddInventory(GameInventoryConstant.InvType_GlobalCurrency, new Inventory(new InventoryInitOption()
            {
                Name = "全局货币背包",
                id = GameInventoryConstant.InvType_GlobalCurrency,
                autoTidy = true,
                initGridCount = 10,
                autoExpand = true,
                maxGridCount = int.MaxValue
            }));
            AddInventory(GameInventoryConstant.InvType_GlobalCommon, new Inventory(new InventoryInitOption()
            {
                Name = "全局通用背包",
                id = GameInventoryConstant.InvType_GlobalCommon,
                autoTidy = true,
                initGridCount = 99,
                autoExpand = true,
                maxGridCount = int.MaxValue
            }));
            AddInventory(GameInventoryConstant.InvType_InGameCommon, new Inventory(new InventoryInitOption()
            {
                Name = "局内通用背包",
                id = GameInventoryConstant.InvType_InGameCommon,
                autoTidy = true,
                initGridCount = 10,
                autoExpand = true,
                maxGridCount = int.MaxValue
            }));
            AddInventory(GameInventoryConstant.InvType_GlobalEquip, new Inventory(new InventoryInitOption()
            {
                Name = "全局装备背包",
                id = GameInventoryConstant.InvType_GlobalEquip,
                autoTidy = true,
                initGridCount = 0,
               
                autoExpand = true,
                maxGridCount = 200
            }));
            AddInventory(GameInventoryConstant.InvType_GlobalRoleEquip, new Inventory(new InventoryInitOption()
            {
                Name = "GlobalRoleEquip",
                id = GameInventoryConstant.InvType_GlobalRoleEquip,
                autoTidy = true,
                initGridCount = 30,
                autoExpand = true,
                maxGridCount = 200
            }));
        }

        public void StoreItems(List<Item> items, bool showGetTip = true)
        {
            for (int i = 0; i < items.Count; i++)
            {
                StoreItem(items[i],false);
            }
            if (showGetTip)
            {
                MessageDispatcher.Ins.Dispatch(MessageCode.msg_open_itemGetTip_panel,items);
            }
        }
        public void StoreItem(Item item,bool showGetTip = true)
        {
            StoreItem(item.itemId, item.count, item.uniqueGroup);
            if (showGetTip)
            {
                MessageDispatcher.Ins.Dispatch(MessageCode.msg_open_itemGetTip_panel, new List<Item>() { item });
            }
        }
        public int GetItemHoldMaxCount(CfgItemInfo infoCfg)
        {
            if (infoCfg.maxHoldCount == -1)
            {
                return int.MaxValue;
            }
            return infoCfg.maxHoldCount;
        }
        public int GetItemHoldMaxCount(int itemId)
        {
            var infoCfg = GetItemInfoCfg(itemId);
            return GetItemHoldMaxCount(infoCfg);
        }
    }

    public class CfgItemTable
    {
        public Dictionary<int,CfgItemInfo> itemInfoCfg;
        public Dictionary<int, CfgItemParam> itemParamsCfg;
    }

    public class CfgItemInfo
    {
        public bool enableStack;
        public bool isUnique;
        public bool isShowMax;
        public string iconSpritePath;
        public string iconTexPath;
        public string description;
        public int inventoryType;
        public int itemId;
        public string itemName;
        public int itemType;
        public int maxHoldCount;
        // 配置表可配置：每次恢复间隔（秒），0 表示不自动恢复。
        public int recoverySeconds;
        public int maxPerStackCount;
        public int quality; 
        public int maxStackCount; // 最大对数 -1为无穷
                                  // 每堆最大数量，-1为无穷 最大持有数量，-1为无穷
    }

    public class CfgItemParam
    {
        public int itemId;
        public List<CfgParamData> itemParams;
    }

    public class CfgParamData
    {
        public string paramName;
        public string paramStringValue;
    }
}
