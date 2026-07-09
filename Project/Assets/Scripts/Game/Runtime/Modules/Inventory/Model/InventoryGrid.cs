using System;
using static Game.Modules.GModuleInventory.Inventory;

namespace Game.Modules.GModuleInventory
{
    public class InventoryGrid
    {
        private InventoryGridChangCb m_OnChangedCb;

        public InventoryGrid(Inventory ownInventory, int inventoryIndex)
        {
            this.Index = inventoryIndex;
            this.OwnInventory = ownInventory;
            this.OnClear();
        }

        public delegate void InventoryGridChangCb(InventoryGrid grid);

        public GridData GridItemData => OwnInventory.GetGridData(this);
        public bool HasData()
        {
            return GridItemData != null;
        }
        public int Index { get; private set; }
        public bool IsDirty { get; private set; }
        public bool IsFree => Status == InventoryGridStatus.Free;
        public bool IsUsing => Status == InventoryGridStatus.Using;
        public Inventory OwnInventory { get; }
        public InventoryGridStatus Status
        { get { return OwnInventory.GetGridStatus(this); } }

        public void AddChangeListener(InventoryGridChangCb cb)
        {
            m_OnChangedCb -= cb;
            m_OnChangedCb += cb;
        }

        public Item GetItem()
        {
            if (!IsUsing) return default;
            Item item = new Item();
            item.itemId = GridItemData.OwnItemVO.itemCfg.itemId;
            item.itemName = GridItemData.OwnItemVO.itemCfg.itemName;
            item.enableStack = GridItemData.OwnItemVO.itemCfg.enableStack;
            item.maxStackCount = GridItemData.OwnItemVO.itemCfg.maxStackCount;
            item.maxPerStackCount = GridItemData.OwnItemVO.itemCfg.maxPerStackCount;
            item.maxHoldCount = GameInventoryDataHandler.Ins.GetItemHoldMaxCount( GridItemData.OwnItemVO.itemCfg);
            item.iconSpritePath = GridItemData.OwnItemVO.itemCfg.iconSpritePath;
            item.iconTexPath = GridItemData.OwnItemVO.itemCfg.iconTexPath;
            item.count = GridItemData.Count;
            return item;
        }

        public void OnClear()
        {
        }

        public void OnUpdate()
        {
            IsDirty = true;
        }

        public void Refresh()
        {
            if (IsDirty)
            {
                IsDirty = false;
                m_OnChangedCb?.Invoke(this);
            }
        }

        public void RemveChangeListener(InventoryGridChangCb cb)
        {
            m_OnChangedCb -= cb;
        }


    }
    public partial class Inventory
    {
        public enum InventoryGridStatus
        {
            Free,
            Using,
            Disbale
        }

        
    }
}