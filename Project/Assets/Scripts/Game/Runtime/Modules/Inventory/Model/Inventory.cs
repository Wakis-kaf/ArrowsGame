using Framework.Runtime.LogSystem;
using Framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using static Game.Modules.GModuleInventory.Inventory;

namespace Game.Modules.GModuleInventory

{
    public enum InventoryOperationStatus
    {
        Waiting,
        Success,

        //PartSuccess,
        Fail
    }

    public struct InventoryInitOption
    {
        public bool autoExpand;// 自动扩容

        public bool autoTidy;

        public int id; // 背包id
        public int initGridCount;

        // 初始背包格子数量
        public int maxGridCount;

        public string Name; // 背包名称
                            // 最大背包格子数量 自动整理
    }

    public struct InventoryOperation
    {
        public string errMessage;
        public int operateCount;
        public List<ItemUniqueData> uniqueDatas;
        public ItemUniqueGroup uniqueStoragePut;
    }
    public struct InvItemMeta
    {
        public InvGridMeta[] gridMetas;
        public int itemId;
    }
    public struct InvMeta
    {
        public int id;
        public int uId;
        public InvItemMeta[] itemArcs;
    }

    public struct InvGridMeta
    {
        public int idx;
        public InventoryGridStatus gridStat;
        public int count;
        public InvUniqItemMeta[] uniqs;
    }
    public static class InventoryOpt
    {
        public const int Register = 0;
        public const int Tidy = 1;
        public const int Clear = 2;
        internal static int Store = 3;
        internal static int SwitchGrid = 4;
        internal static int Refresh = 5;
        internal static int TakeOut = 6;
    }
    public partial class Inventory
    {
        private DataManager m_DataManager;
        private Dictionary<int, InventoryGridStatus> m_GridIndex2Status;
        private List<InventoryGrid> m_Grids;
        private InventoryInitOption m_InventoryInitOption;
        private Dictionary<int, InvItemVO> m_ItemId2ItemVOMap;
        private Action<Inventory, int> m_OnInventoryChanged;
        public delegate bool GridsFilter(InventoryGrid grid);
        public Dictionary<int, InvItemVO> ItemId2ItemVOMap => m_ItemId2ItemVOMap;

        public Inventory(InventoryInitOption initOption)
        {
            this.InitInventory(initOption);
        }
        public List<int> GetInvAllItemIds()
        {
            return ItemId2ItemVOMap.Keys.ToList();
        }
        public int CurrentGridCount { get => m_Grids.Count; }

        public int Id { get; private set; }

        public int UId { get; private set; }

        public void AddItemChangedListener(int itemId, Action changeCb)
        {
            GetItemVO(itemId).AddChangedListener(changeCb);
        }
        public void AddChangeListener(Action<Inventory, int> listener)
        {

            m_OnInventoryChanged -= listener;
            m_OnInventoryChanged += listener;
            listener?.Invoke(this, InventoryOpt.Register);
        }
        public void RemoveChangeListener(Action<Inventory, int> listener)
        {
            m_OnInventoryChanged -= listener;
        }

        public void ClearGrid(InventoryGrid grid)
        {
            SetGridStatus(grid, InventoryGridStatus.Free);
            grid.OnClear();
            grid.OnUpdate();
        }

        public void ClearInventory()
        {
            for (int i = 0; i < m_Grids.Count; i++)
            {
                m_GridIndex2Status[i] = InventoryGridStatus.Free;
            }
            foreach (var item in m_ItemId2ItemVOMap)
            {
                item.Value.DisposeFreeGrid();
                item.Value.CheckRefesh();
            }
            m_OnInventoryChanged?.Invoke(this, InventoryOpt.Clear);
        }

        public GridData GetGridData(InventoryGrid inventoryGrid)
        {
            foreach (var item in m_ItemId2ItemVOMap)
            {
                var findData = item.Value.GetGridData(inventoryGrid);
                if (findData != null)
                {
                    return findData;
                }
            }
            return null;
        }

        public List<InventoryGrid> GetFilterGirds(GridsFilter filter)
        {
            var list = new List<InventoryGrid>();
            for (int i = 0; i < m_Grids.Count; i++)
            {
                if (filter(m_Grids[i]))
                {
                    list.Add(m_Grids[i]);
                }
            }
            return list;
        }
        public List<InventoryGrid> GetGrids()
        {
            var list = new List<InventoryGrid>();
            for (int i = 0; i < m_Grids.Count; i++)
            {
                list.Add(m_Grids[i]);
            }
            return list;
        }

        public InventoryGridStatus GetGridStatus(InventoryGrid grid)
        {
            return m_GridIndex2Status[grid.Index];
        }

        public InventoryGrid GetInventoryGridByIndex(int index)
        {
            if (index < m_Grids.Count)
            {
                return m_Grids[index];
            }
            return null;
        }

        public int GetItemHasCount(int itemTypeId)
        {
            CfgItemInfo itemCfg = GameInventoryModule.GetHandlerIns<GameInventoryDataHandler>().GetItemInfoCfg(itemTypeId);
            if (itemCfg == null)
            {
                return 0;
            }
            InvItemVO itemVO = GetItemVO(itemTypeId);
            return itemVO.GetHasCount();
        }

        // 唯一ID
        public InvItemVO GetOwnItemVO(InventoryGrid inventoryGrid)
        {
            foreach (var item in m_ItemId2ItemVOMap)
            {
                if (item.Value.IsManaged(inventoryGrid))
                {
                    return item.Value;
                }
            }
            return null;
        }

        public void RemoveItemChangedListener(int itemId, Action changeCb)
        {
            GetItemVO(itemId)?.RemoveChangedListener(changeCb);
        }

        public void SetGridStatus(InventoryGrid grid, InventoryGridStatus status)
        {
            m_GridIndex2Status[grid.Index] = status;
            grid.OnUpdate();
        }

        public InventoryOperation StoreItem(int itemTypeId, int count, ItemUniqueGroup uniqueGroup = null)
        {
            CfgItemInfo itemCfg = GameInventoryModule.GetHandlerIns<GameInventoryDataHandler>().GetItemInfoCfg(itemTypeId);
            if (itemCfg == null)
            {
                InventoryOperation operation = new InventoryOperation();
                operation.errMessage = $"未找到id为{itemTypeId}物品配置";
                return operation;
            }
            if (itemCfg.isUnique && uniqueGroup == null)
            {
                uniqueGroup = new ItemUniqueGroup();
                uniqueGroup.itemUniqueDatas = new List<ItemUniqueData>();
                uniqueGroup.storageIndex = 0;
            }

            InventoryOperation option = TryPutItem(itemTypeId, count, uniqueGroup);
            m_OnInventoryChanged?.Invoke(this, InventoryOpt.Store);
            return option;
        }

        public void SwitchGrdiData(int index, int index2)
        {
            SwitchGridData(GetInventoryGridByIndex(index), GetInventoryGridByIndex(index2));
        }

        /// <summary>
        /// 交换背包中的两个格子
        /// </summary>
        /// <param name="grid1"></param>
        /// <param name="grid2"></param>
        public void SwitchGridData(InventoryGrid grid1, InventoryGrid grid2)
        {
            InvItemVO grid1Vo = GetOwnItemVO(grid1);
            InvItemVO grid2Vo = GetOwnItemVO(grid2);
            bool grid1Using = grid1.IsUsing;
            bool grid2Using = grid2.IsUsing;
            GridData data = grid1.GridItemData;
            GridData data2 = grid2.GridItemData;
            InventoryGridStatus status1 = GetGridStatus(grid1);
            InventoryGridStatus status2 = GetGridStatus(grid2);
            if (grid1Using && grid2Using && grid1Vo == grid2Vo)
            {
                grid1Vo.SwitchGrid(grid1, grid2);
            }
            else
            {
                if (grid1Using)
                {
                    grid1Vo.Redirect(grid1, grid2);
                }
                if (grid2Using)
                {
                    grid2Vo.Redirect(grid2, grid1);
                }
            }
            SetGridStatus(grid1, status2);
            SetGridStatus(grid2, status1);
            grid1.OnUpdate();
            grid2.OnUpdate();
            grid1.Refresh();
            grid2.Refresh();
            m_OnInventoryChanged?.Invoke(this, InventoryOpt.SwitchGrid);
        }
        public void Refresh()
        {
            foreach (var item in m_ItemId2ItemVOMap)
            {
                item.Value.CheckRefesh();
            }
            m_OnInventoryChanged?.Invoke(this, InventoryOpt.Refresh);
        }
        public InventoryOperation TakeOutUniqItem(int itemId, int uid)
        {
            var getOperation = TryGetUniqueItem(itemId, uid);
            m_OnInventoryChanged?.Invoke(this, InventoryOpt.TakeOut);
            return getOperation;
        }
        public InventoryOperation TakeOutItem(int itemTypeId, int count)
        {
            CfgItemInfo itemCfg = GameInventoryModule.GetHandlerIns<GameInventoryDataHandler>().GetItemInfoCfg(itemTypeId);
            if (itemCfg == null)
            {
                InventoryOperation operation = new InventoryOperation();
                operation.errMessage = $"未找到id为{itemTypeId}物品配置";
                return operation;
            }
            var getOperation = TryGetItem(itemTypeId, count);
            m_OnInventoryChanged?.Invoke(this, InventoryOpt.TakeOut);
            return getOperation;
        }

        /// <summary>
        /// 整理背包
        /// </summary>
        public void TidyInventory(Comparison<InventoryGrid> sortComparison = null)
        {
            if (sortComparison == null)
            {
                SortInventoryByDefault();
            }
            else
            {
                SortByComparison(sortComparison);
            }
            m_OnInventoryChanged?.Invoke(this, InventoryOpt.Tidy);

        }

        private void SortByComparison(Comparison<InventoryGrid> sortComparison = null)
        {
            int count = m_Grids.Count;
            if (count <= 1) return;

            // 1. 预计算排序后的顺序（不移动物理格子）
            // 仅创建一个索引数组，避免对整个对象列表进行内存重排
            int[] sortedIndices = new int[count];
            for (int i = 0; i < count; i++) sortedIndices[i] = i;

            // 使用局部变量引用，减少闭包开销
            var grids = m_Grids;
            Array.Sort(sortedIndices, (a, b) => {
                InventoryGrid g1 = grids[a];
                InventoryGrid g2 = grids[b];

                // 这里的排序逻辑建议：有数据的永远排在前面
                bool h1 = g1.HasData();
                bool h2 = g2.HasData();
                if (h1 != h2) return h1 ? -1 : 1;
                if (!h1) return 0;

                return sortComparison != null ? sortComparison(g1, g2) : OrderSort(g1, g2);
            });

            // 2. 原位环形置换算法 (In-place Permutation)
            // 性能最高：每个格子的数据最多只被移动一次，且不触发大规模的 Clear 操作
            bool[] visited = new bool[count];
            for (int i = 0; i < count; i++)
            {
                if (visited[i] || sortedIndices[i] == i) continue;

                int current = i;
                while (!visited[current])
                {
                    visited[current] = true;
                    int next = sortedIndices[current];
                    if (next == i) break;

                    // 执行物理数据交换，但不交换格子对象本身
                    // 这里调用你已有的 SwitchGridData 逻辑，它负责处理 VO 指向更新
                    SwitchGridData(grids[current], grids[next]);
                    current = next;
                }
            }
        }
        private void SortInventoryByDefault()
        {
            List<InventoryGrid> sortedGrids = new List<InventoryGrid>();
            Dictionary<InventoryGrid, int> originalIndices = new Dictionary<InventoryGrid, int>();
            for (int i = 0; i < m_Grids.Count; i++)
            {
                originalIndices.Add(m_Grids[i], i);
                sortedGrids.Add(m_Grids[i]);
            }
            sortedGrids.Sort(OrderSort);


            for (int i = 0; i < sortedGrids.Count; i++)
            {
                int putIndex = i;
                var curInGrid = m_Grids[putIndex]; // 当前仍占用空间的格子
                var putGrid = sortedGrids[putIndex]; // 待放入的格子
                var inGridIndex = originalIndices[putGrid];// 待放入的格子在原列表中的索引
                SwitchGrdiData(inGridIndex, putIndex);
                originalIndices[curInGrid] = inGridIndex;
                originalIndices[putGrid] = putIndex;

            }
        }
        private int GetGridSortOrder(InventoryGrid grid)
        {
            if (!grid.IsUsing)
            {
                return -1000;
            }
            int hashCode = grid.GridItemData.OwnItemVO.itemCfg.itemName[0].GetHashCode();
            int count = grid.GridItemData.Count;
            return count;
        }

        private int OrderSort(InventoryGrid grid1, InventoryGrid grid2)
        {
            if (grid1.HasData() && !grid2.HasData())
            {
                return -1;
            }
            if (!grid1.HasData() && grid2.HasData())
            {
                return 1;
            }
            if (!grid1.HasData() && !grid2.HasData())
            {
                return 0;
            }
            var cfg = grid1.GridItemData.OwnItemVO.itemCfg;
            var cfg2 = grid2.GridItemData.OwnItemVO.itemCfg;
            int id1 = cfg.itemId;
            int id2 = cfg2.itemId;

            // 3. 等阶也相同，比较ID（升序：ID小的排前面）
            return id1.CompareTo(id2);
        }
        private void AddNewGrid(InventoryGrid grid)
        {
            m_Grids.Add(grid);
            m_GridIndex2Status.Add(grid.Index, InventoryGridStatus.Free);
        }
        public InventoryGrid GetGridByIndex(int index)
        {
            if (index < m_Grids.Count) return m_Grids[index];
            return null;
        }
        private InventoryGrid GetFreeGrid()
        {
            for (int i = 0; i < m_Grids.Count; i++)
            {
                if (m_Grids[i].IsFree)
                {
                    return m_Grids[i];
                }
            }
            if (this.m_InventoryInitOption.autoExpand && GetUsingGridCount() < this.m_InventoryInitOption.maxGridCount)
            {
                int index = this.CurrentGridCount;
                InventoryGrid grid = new InventoryGrid(this, index);
                AddNewGrid(grid);
                return grid;
            }
            return null;
        }
        private InventoryGrid CreateGridTo(int indexTo)
        {
            var findGrid = GetGridByIndex(indexTo);
            if (findGrid != null) return findGrid;

            int currentCount = this.CurrentGridCount;
            int maxCount = this.m_InventoryInitOption.maxGridCount;
            if (maxCount == -1)
            {
                maxCount = int.MaxValue;
            }
            int maxEnableCreateCount = maxCount - currentCount;
            int needCreateCount = indexTo - currentCount + 1;
            needCreateCount = Mathf.Min(maxEnableCreateCount, needCreateCount);
            for (int i = 0; i < needCreateCount; i++)
            {
                int index = this.CurrentGridCount;
                InventoryGrid grid = new InventoryGrid(this, index);
                AddNewGrid(grid);
            }
            return GetGridByIndex(indexTo);
        }

        private InvItemVO GetItemVO(int itemTypeId)
        {
            if (m_ItemId2ItemVOMap.TryGetValue(itemTypeId, out var itemVO))
            {
                return itemVO;
            }
            CfgItemInfo itemCfg = GameInventoryDataHandler.GetModuleHandlerIns<GameInventoryDataHandler>().GetItemInfoCfg(itemTypeId);
            CfgItemParam paramCfg = GameInventoryDataHandler.GetModuleHandlerIns<GameInventoryDataHandler>().GetItemParamCfg(itemTypeId);
            itemVO = new InvItemVO(this, itemCfg, paramCfg);
            m_ItemId2ItemVOMap.Add(itemTypeId, itemVO);
            return itemVO;
        }

        private int GetUsingGridCount()
        {
            int count = 0;
            for (int i = 0; i < m_Grids.Count; i++)
            {
                if (m_Grids[i].IsUsing)
                {
                    count++;
                }
            }
            return count;
        }

        private void InitGrids()
        {
            for (int i = 0; i < this.m_InventoryInitOption.initGridCount; i++)
            {
                InventoryGrid grid = new InventoryGrid(this, i);
                AddNewGrid(grid);
            }
        }

        // 通用Id 通用Id
        private void InitInventory(InventoryInitOption option)
        {
            this.m_InventoryInitOption = option;
            this.Id = option.id;
            this.UId = Utility.IDGenerator.GetIntGuidID();
            this.m_DataManager = new DataManager();
            this.m_Grids = new List<InventoryGrid>();
            this.m_ItemId2ItemVOMap = new Dictionary<int, InvItemVO>();
            this.m_GridIndex2Status = new Dictionary<int, InventoryGridStatus>();
            InitGrids();
        }

        private InventoryOperation PutItemToNewGrid(int itemTypeId, int count, ItemUniqueGroup uniqueGroup = null)
        {
            InvItemVO itemVO = GetItemVO(itemTypeId);
            InventoryGrid freeGrid = GetFreeGrid();
            if (freeGrid == null)
            {
                InventoryOperation operation = new InventoryOperation();
                operation.errMessage = "背包已满,无法添加";
                operation.operateCount = 0;
                operation.uniqueStoragePut = uniqueGroup;
                return operation;
            }
            InventoryOperation purOperation = itemVO.PutItemToGrid(freeGrid, count, uniqueGroup);
            itemVO.CheckRefesh();
            return purOperation;
        }
        private InventoryOperation TryGetUniqueItem(int itemTypeId, int uid)
        {
            InvItemVO itemVO = GetItemVO(itemTypeId);
            InventoryOperation operation = itemVO.GetUniqItemFromHoldGrid(uid);
            itemVO.CheckRefesh();
            itemVO.DisposeFreeGrid();
            return operation;
        }
        private InventoryOperation TryGetItem(int itemTypeId, int count)
        {
            InvItemVO itemVO = GetItemVO(itemTypeId);
            InventoryOperation operation = itemVO.GetItemFromHoldGrid(count);
            itemVO.CheckRefesh();
            itemVO.DisposeFreeGrid();
            return operation;
        }

        private InventoryOperation TryPutItem(int itemTypeId, int count, ItemUniqueGroup uniqueDatas = null)
        {
            InvItemVO itemVO = GetItemVO(itemTypeId);
            InventoryOperation operation = itemVO.PutItemToHoldGrid(count, uniqueDatas);
            int optCount = operation.operateCount;
            int remainCount = count - optCount;
            while (remainCount > 0 && string.IsNullOrEmpty(operation.errMessage))
            {
                operation = PutItemToNewGrid(itemTypeId, remainCount, uniqueDatas);
                remainCount -= operation.operateCount;
            }
            operation.operateCount = count - remainCount;
            itemVO.CheckRefesh();
            return operation;
        }
    }

    #region 处理存档导出和读取
    public partial class Inventory
    {
        public InvMeta ExportMeta()
        {
            InvMeta archive = new InvMeta();
            archive.id = this.Id;
            archive.uId = this.UId;
            archive.itemArcs = GenerateItemMeta();
            return archive;
        }
        private InvItemMeta[] GenerateItemMeta()
        {
            InvItemMeta[] itemArchive = new InvItemMeta[m_ItemId2ItemVOMap.Count];
            int index = 0;
            foreach (var item in m_ItemId2ItemVOMap)
            {
                var archive = GenerateItemMeta(item.Value);
                itemArchive[index] = archive;
                index++;
            }
            return itemArchive;
        }
        private InvItemMeta GenerateItemMeta(InvItemVO itemVO)
        {
            InvItemMeta archive = new InvItemMeta();
            archive.itemId = itemVO.itemId;
            archive.gridMetas = new InvGridMeta[itemVO.HoldGrids.Count];
            for (int i = 0; i < itemVO.HoldGrids.Count; i++)
            {
                archive.gridMetas[i] = GenerateGridMeta(itemVO.HoldGrids[i]);
            }
            return archive;
        }

        private InvGridMeta GenerateGridMeta(InventoryGrid grid)
        {
            InvGridMeta gridArchive = new InvGridMeta();
            gridArchive.idx = grid.Index;
            gridArchive.gridStat = grid.Status;
            gridArchive.count = grid.GridItemData.Count;
            gridArchive.uniqs = new InvUniqItemMeta[grid.GridItemData.ItemUniqDataList.Count];
            for (int i = 0; i < grid.GridItemData.ItemUniqDataList.Count; i++)
            {
                gridArchive.uniqs[i] = GenerateUniqItemMeta(grid.GridItemData.ItemUniqDataList.ElementAt(i));
            }
            return gridArchive;
        }
        private InvUniqItemMeta GenerateUniqItemMeta(ItemUniqueData uniqueData)
        {
            InvUniqItemMeta archive = new InvUniqItemMeta();
            archive.id = uniqueData.itemId;
            archive.uId = uniqueData.uniqueItemId;
            archive.kvs = new KV[uniqueData.kvDatas.Count];
            for (int i = 0; i < uniqueData.kvDatas.Count; i++)
            {
                archive.kvs[i] = uniqueData.kvDatas[i];
            }
            return archive;
        }

        public void ImportMeta(InvMeta invMeta)
        {
            this.UId = invMeta.uId;
            if (invMeta.itemArcs == null) return;
            for (int i = 0; i < invMeta.itemArcs.Length; i++)
            {
                ImportItemMeta(invMeta.itemArcs[i]);
            }
            m_OnInventoryChanged?.Invoke(this, InventoryOpt.Refresh);
        }
        private void ImportItemMeta(InvItemMeta itemMeta)
        {
            if (itemMeta.gridMetas == null) return;
            var itemVo = GetItemVO(itemMeta.itemId);
            if (itemVo == null || itemVo.itemCfg == null) return;
            for (int i = 0; i < itemMeta.gridMetas.Length; i++)
            {
                ImportGridMeta(itemVo, itemMeta.gridMetas[i]);
            }
            itemVo.CheckRefesh();
        }
        private void ImportGridMeta(InvItemVO itemVO, InvGridMeta gridMeta)
        {
            var grid = GetGridByIndex(gridMeta.idx);
            if (grid == null)
            {
                grid = CreateGridTo(gridMeta.idx);
            }
            SetGridStatus(grid, gridMeta.gridStat);
            ItemUniqueGroup group = null;
            if (itemVO.itemCfg.isUnique && gridMeta.uniqs != null)
            {
                group = new ItemUniqueGroup();
                group.storageIndex = 0;
                group.itemUniqueDatas = ImportUniqueDatas(gridMeta.uniqs);
            }
            itemVO.AllocateGrid(grid);
            itemVO.GetGridData(grid).SetCount(gridMeta.count, itemVO.itemCfg.isUnique, group);


        }
        private List<ItemUniqueData> ImportUniqueDatas(InvUniqItemMeta[] uniqs)
        {
            List<ItemUniqueData> res = new List<ItemUniqueData>();
            for (int i = 0; i < uniqs.Length; i++)
            {
                res.Add(ImportUniqueData(uniqs[i]));
            }
            return res;
        }

        private ItemUniqueData ImportUniqueData(InvUniqItemMeta uniqItemMeta)
        {
            ItemUniqueData itemUniqueData = new ItemUniqueData();
            itemUniqueData.uniqueItemId = uniqItemMeta.uId;
            itemUniqueData.itemId = uniqItemMeta.id;
            if (uniqItemMeta.kvs != null)
            {
                itemUniqueData.kvDatas.AddRange(uniqItemMeta.kvs);
            }
            return itemUniqueData;

        }
    }
    #endregion
}