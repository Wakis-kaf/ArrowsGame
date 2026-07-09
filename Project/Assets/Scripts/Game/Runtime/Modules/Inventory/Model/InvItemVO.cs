using Framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Game.Modules.GModuleInventory.Inventory;

namespace Game.Modules.GModuleInventory
{
    public class ItemUniqueData
    {
        public int itemId; // 通用id
        public long uniqueItemId; // 唯一id
        public List<KV> kvDatas = new List<KV>();
        public void AddKV(string key, string val)
        {
            kvDatas.Add(new KV() { key = key, val = val });
        }
        public void SetKV(string key, int val)
        {
            var find = GetValue(key);
            if (find != null)
            {
                find.iVal = val;
            }
            else
            {
                AddKV(key, val);
            }
        }
        public void SetKV(string key, string val)
        {
            var find = GetValue(key);
            if (find != null)
            {
                find.val = val;
            }
            else
            {
                AddKV(key, val);
            }
        }
        public void AddKV(string key, int val)
        {
            kvDatas.Add(new KV() { key = key, iVal = val });
        }
        public KV GetValue(string key)
        {
            for (int i = 0; i < kvDatas.Count; i++)
            {
                if (kvDatas[i].key == key)
                {
                    return kvDatas[i];
                }
            }
            return null;
        }
    }
    public struct InvUniqItemMeta
    {
        public int id;
        public long uId;
        public KV[] kvs;
    }
    public class ItemUniqueGroup
    {
        public List<ItemUniqueData> itemUniqueDatas;
        public int storageIndex = 0;
    }
    
    public partial class Inventory
    {
        public class GridData
        {
            private int m_Count;
            public int Count => m_Count;
            private LinkedList<ItemUniqueData> m_ItemUniqIdList;
            public LinkedList<ItemUniqueData> ItemUniqDataList => m_ItemUniqIdList;
            public ItemUniqueData GetItemUniqueData(int index)
            {
                if (index < m_ItemUniqIdList.Count){
                    return m_ItemUniqIdList.ElementAt(index);
                }
                return null;
            }
            public List<long> GetUniqItemIds()
            {
                List<long> ids = new List<long>();
                foreach (var item in m_ItemUniqIdList)
                {
                    ids.Add(item.uniqueItemId);
                }
                return ids;
            }
            public GridData(InvItemVO ownItemVo)
            {
                OwnItemVO = ownItemVo;
                m_ItemUniqIdList = new LinkedList<ItemUniqueData>();
            }

            public InvItemVO OwnItemVO { get; private set; }

            private void TakeOutUnique(int getCount, ref List<ItemUniqueData> uniqueDatas)
            {
                int max = Mathf.Min(getCount, m_ItemUniqIdList.Count);
                int i = 0;
                while (i<max)
                {
                    uniqueDatas.Add(m_ItemUniqIdList.ElementAt(0));
                    uniqueDatas.RemoveAt(0);
                    i++;
                }
            }

          
            public void SetCount(int operateCount,bool isUnique,  ItemUniqueGroup uniqueGroup)
            {
                m_Count = operateCount;
                m_Count = Mathf.Max(0, m_Count);
                m_ItemUniqIdList.Clear();
                StorageUnique(operateCount, isUnique, uniqueGroup);
            }
            private void StorageUnique(int operateCount,bool isUnique, ItemUniqueGroup uniqueGroup)
            {
                if (!isUnique) return;
                int storageCount = 0;
                if (uniqueGroup != null && uniqueGroup.itemUniqueDatas != null)
                {
                    int index = uniqueGroup.storageIndex;
                    for (int i = 0; i < operateCount; i++)
                    {
                        if (index<uniqueGroup.itemUniqueDatas.Count )
                        {
                            var item = uniqueGroup.itemUniqueDatas[index];
                            m_ItemUniqIdList.AddLast(item);
                            storageCount++;
                        }
                        else 
                        {
                            break;
                        }
                        index++;
                        uniqueGroup.storageIndex = index;
                    }
                }
                int needCreate = Mathf.Max(0, operateCount - storageCount);
                for (int i = 0; i < needCreate; i++)
                {
                    ItemUniqueData item = new ItemUniqueData();
                    item.uniqueItemId = Utility.IDGenerator.GetSnowflakeID();
                    item.itemId = OwnItemVO.itemId;
                    m_ItemUniqIdList.AddLast(item);
                    
                    uniqueGroup.itemUniqueDatas.Add(item);
                    uniqueGroup.storageIndex++;
                }


            }
            public void AddCount(int operateCount,bool isUnique, ItemUniqueGroup uniqueGroup = null)
            {
                m_Count += operateCount;
                StorageUnique(operateCount, isUnique, uniqueGroup);
            }
            public ItemUniqueData TakeOutUnique(long uId)
            {
                int i = 0;
                while (i < m_ItemUniqIdList.Count)
                {
                    var item = m_ItemUniqIdList.ElementAt(i);
                    if(item.uniqueItemId == uId)
                    {
                        m_Count -= 1;
                        m_Count = Mathf.Max(0, m_Count);
                        return item;
                    }
                    i++;
                }
                return null;
            }
            public void TakeOutCount(int getCount, bool isUnique, ref List<ItemUniqueData> uniqueDatas)
            {
                m_Count -= getCount;
                m_Count = Mathf.Max(0, m_Count);
                if (isUnique)
                {
                    TakeOutUnique(getCount, ref uniqueDatas);
                }
                
            }

            public ItemUniqueData FindUniqueData(long uid)
            {
                foreach (var item in m_ItemUniqIdList)
                {
                    if (item.uniqueItemId == uid) return item;
                }
                return null;
            }
        }

        public class ItemParamKey
        {
            public const string Level = "level";
            public const string Quaility = "quality";
        }

        
    }
    public class InvItemVO
    {
        private Dictionary<InventoryGrid, GridData> m_Grid2DataMap;
        private List<InventoryGrid> m_HoldGrids;
        private DataManager m_DataManager;
        private Action m_ItemVOChangedCb;
        public int itemId { get; private set; }
        public InvItemVO(Inventory ownInventory, CfgItemInfo itemCfg, CfgItemParam itemParamCfg)
        {
            this.OwnInventory = ownInventory;
            this.itemId = itemCfg?.itemId ?? 0;
            this.itemCfg = itemCfg;
            m_DataManager = new DataManager();
            m_HoldGrids = new List<InventoryGrid>();
            m_Grid2DataMap = new Dictionary<InventoryGrid, GridData>();
            this.InitParams(itemParamCfg);
        }

        public CfgItemInfo itemCfg { get; private set; }

        public Dictionary<InventoryGrid, GridData> Grid2DataMap => m_Grid2DataMap;
        public List<InventoryGrid> HoldGrids => m_HoldGrids;
        // 类型id
        public Inventory OwnInventory { get; private set; }

        public int UId { get; } // 唯一id

        public void AllocateGrid(InventoryGrid grid)
        {
            GridData itemVOGridData = new GridData(this);
            m_Grid2DataMap.Add(grid, itemVOGridData);
            this.OwnInventory.SetGridStatus(grid, InventoryGridStatus.Using);
            m_HoldGrids.Add(grid);
            grid.OnUpdate();
        }
        public ItemUniqueData FindUniqueData(int uid)
        {
            for (int i = 0; i < m_HoldGrids.Count; i++)
            {
                var gridItem = GetGridData(m_HoldGrids[i]);
                if (gridItem == null) continue;
                var find = gridItem.FindUniqueData(uid);
                if(find != null) return find;
            }
            return null;
        }

        public void Clear(InventoryGrid grid)
        {
            OwnInventory.ClearGrid(grid);
        }

        public void DisposeFreeGrid()
        {
            for (int i = m_HoldGrids.Count - 1; i >= 0; i--)
            {
                if (m_HoldGrids[i].IsFree)
                {
                    m_Grid2DataMap.Remove(m_HoldGrids[i]);
                    m_HoldGrids.RemoveAt(i);
                }
            }
        }

        public GridData GetGridData(InventoryGrid inventoryGrid)
        {
            if (m_Grid2DataMap.ContainsKey(inventoryGrid))
                return m_Grid2DataMap[inventoryGrid];
            return null;
        }
        public InventoryOperation GetUniqItemFromHoldGrid(int uid)
        {
            InventoryOperation operation = new InventoryOperation();
            
            for (int i = 0; i < m_HoldGrids.Count; i++)
            {
                var holdGrid = m_HoldGrids[i];
                var gridData = GetGridData(holdGrid);
                var itemUniqueData = gridData.TakeOutUnique(uid);
                if (itemUniqueData != null)
                {
                    operation.operateCount = 1;
                    operation.uniqueDatas = new List<ItemUniqueData>() { itemUniqueData };
                    if (gridData.Count == 0)
                    {
                        Clear(m_HoldGrids[i]);
                    }
                    holdGrid.OnUpdate();
                    return operation;
                }
            }
            operation.errMessage = "物品不存在";
            return operation;
        }
        public InventoryOperation GetItemFromHoldGrid(int count)
        {
            int needGetCount = count;
            InventoryOperation operation = new InventoryOperation();
            List<ItemUniqueData> uniqueDatas = null;
            if (itemCfg.isUnique)
            {
                uniqueDatas = new List<ItemUniqueData>();
            }
            for (int i = 0; i < m_HoldGrids.Count; i++)
            {
                var holdGrid = m_HoldGrids[i];
                var gridData = GetGridData(holdGrid);
                int maxPerStackCount = this.itemCfg.maxPerStackCount == -1?int.MaxValue:itemCfg.maxPerStackCount;
                int hasCount = gridData.Count;
                int getCount = Mathf.Min(hasCount, needGetCount);
                gridData.TakeOutCount(getCount, itemCfg.isUnique, ref uniqueDatas);
                needGetCount = Mathf.Max(needGetCount - hasCount, 0);
                if (gridData.Count <= 0)
                {
                    Clear(m_HoldGrids[i]);
                }
                holdGrid.OnUpdate();
                if (needGetCount <= 0)
                {  // 已经全部取出
                    break;
                }
            }
            operation.operateCount = count - needGetCount;
            operation.uniqueDatas = uniqueDatas;
            return operation;
        }

        public int GetHasCount()
        {
            int total = 0;
            for (int i = 0; i < m_HoldGrids.Count; i++)
            {
                total += GetGridData(m_HoldGrids[i]).Count;
            }
            return total;
        }

        public bool IsManaged(InventoryGrid grid)
        {
            return m_HoldGrids.Contains(grid);
        }

        public InventoryOperation PutItemToGrid(InventoryGrid grid, int count, ItemUniqueGroup uniqueGroup = null)
        {
            count = Mathf.Max(count, 0);
            int curStackCount = m_HoldGrids.Count;
            itemCfg.maxStackCount = itemCfg.maxStackCount == -1 ? int.MaxValue : itemCfg.maxStackCount;
            int maxStackCount = itemCfg.maxStackCount;
            InventoryOperation operation;
            if (curStackCount >= maxStackCount)
            {
                operation = new InventoryOperation();
                operation.errMessage = "该物品已经超出堆数量";
                operation.operateCount = 0;
                operation.uniqueStoragePut = uniqueGroup;
                return operation;
            }

            int curCount = this.GetHasCount();
            int maxCount = GameInventoryDataHandler.Ins.GetItemHoldMaxCount(itemCfg);
            if (curCount >= maxCount)
            {
                operation = new InventoryOperation();
                operation.errMessage = "背包持有该物品已超出上线";
                operation.operateCount = 0;
                operation.uniqueStoragePut = uniqueGroup;
                return operation;
            }
            itemCfg.maxPerStackCount = itemCfg.maxPerStackCount == -1 ? int.MaxValue : itemCfg.maxPerStackCount;
            int maxPerStackCount = itemCfg.enableStack ? itemCfg.maxPerStackCount : 1;
            int maxPutCount = Mathf.Min(count, maxPerStackCount);
            AllocateGrid(grid);
            var gridData = grid.GridItemData;
            gridData.SetCount(maxPutCount, itemCfg.isUnique, uniqueGroup);
            operation = new InventoryOperation();
            operation.operateCount = maxPutCount;
            operation.uniqueStoragePut = uniqueGroup;
            return operation;
        }

        public InventoryOperation PutItemToHoldGrid(int totalCount, ItemUniqueGroup uniqueGroup = null)
        {
            int needStackCount = totalCount;
            InventoryOperation operation = new InventoryOperation();
            for (int i = 0; i < m_HoldGrids.Count; i++)
            {
                var holdGrid = m_HoldGrids[i];
                var gridData = GetGridData(holdGrid);
                int maxPerStackCount = this.itemCfg.maxPerStackCount == -1?int.MaxValue: this.itemCfg.maxPerStackCount;
                int hasCount = gridData.Count;
                int canStackCount = maxPerStackCount - hasCount;
                int operateBeforeCount = needStackCount;
                needStackCount = Mathf.Max(needStackCount - canStackCount, 0);
                int operateCount = operateBeforeCount - needStackCount;
                gridData.AddCount(operateCount, itemCfg.isUnique, uniqueGroup);
                holdGrid.OnUpdate();
                if (needStackCount <= 0)
                {  // 已经全部堆放完成
                    break;
                }
            }
            operation.operateCount = totalCount - needStackCount;
            operation.uniqueStoragePut = uniqueGroup;
            return operation;
        }

        public void Redirect(InventoryGrid ownGrid, InventoryGrid otherGrid)
        {
            if (!m_HoldGrids.Contains(ownGrid)) return;
            GridData data = null;
            if (m_HoldGrids.Contains(ownGrid))
            {
                data = m_Grid2DataMap[ownGrid];
                m_HoldGrids.Remove(ownGrid);
                m_Grid2DataMap.Remove(ownGrid);
            }
            if (!m_HoldGrids.Contains(otherGrid))
            {
                m_HoldGrids.Add(otherGrid);
                m_Grid2DataMap.Add(otherGrid, data);
            }
            else
            {
                m_Grid2DataMap[otherGrid] = data;
            }
        }

        public void SwitchGrid(InventoryGrid ownGrid, InventoryGrid otherGrid)
        {
            if (!m_HoldGrids.Contains(ownGrid) || !m_HoldGrids.Contains(otherGrid))
            {
                return;
            }
            var data1 = m_Grid2DataMap[ownGrid];
            var data2 = m_Grid2DataMap[otherGrid];
            m_Grid2DataMap[ownGrid] = data2;
            m_Grid2DataMap[otherGrid] = data1;
        }

        public void AddChangedListener(Action changeCb)
        {
            m_ItemVOChangedCb -= changeCb;
            m_ItemVOChangedCb += changeCb;
        }

        public void CheckRefesh()
        {
            for (int i = 0; i < m_HoldGrids.Count; i++)
            {
                if (m_HoldGrids[i].IsDirty)
                {
                    m_HoldGrids[i].Refresh();
                }
            }
            m_ItemVOChangedCb?.Invoke();
        }

        internal void RemoveChangedListener(Action changeCb)
        {
            m_ItemVOChangedCb -= changeCb;
        }

        private void InitParams(CfgItemParam paramCfg)
        {
            if (paramCfg == null) return;
            for (int i = 0; i < paramCfg.itemParams.Count; i++)
            {
                var param = paramCfg.itemParams[i];
                this.m_DataManager.SetDataAsString(param.paramName, param.paramStringValue);
            }
        }

        //public void ImportItemMeta(InvItemMeta itemMeta)
        //{
        //    if (itemMeta.gridMetas == null) return;
        //    for (int i = 0; i < itemMeta.gridMetas.Length; i++)
        //    {
        //        ImportGridMeta(itemMeta.gridMetas[i]);
        //    }
        //}
        //private void ImportGridMeta(InvGridMeta gridMeta)
        //{

        //}
    }
}