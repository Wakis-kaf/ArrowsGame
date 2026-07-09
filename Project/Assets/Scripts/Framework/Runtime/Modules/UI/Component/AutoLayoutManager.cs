using Cysharp.Threading.Tasks;
using Framework.Runtime.LogSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;



//using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Framework.Runtime.UI
{
    public enum LayoutEfficiency
    {
        /// <summary>
        /// 性能优先
        /// </summary>
        SpeedFirst, // 性能优先

        /// <summary>
        /// 效果优先
        /// </summary>
        ShowFirst //观赏优先
    }

    public interface IAutoLayoutItem
    {
        int Index { get; set; }
        bool IsSelected { get; set; }
    }

    public interface IExternalAutoLayoutManager
    {
        int ItemCount { get; }
        int SelectedCount { get; }

        void ClearAllSelectedIndex();

        IAutoLayoutItem GetItem(int index);
        IAutoLayoutItem GetItemByIndex(int index);

        bool GetItemIndexVisible(int index, out IAutoLayoutItem item);

        bool GetItemVisibleIndex(IAutoLayoutItem item, out int index);

        bool IsSelectIndex(int index);

        void RemoveIndexVisible(int index);

        void RemoveSelectIndex(int index);

        void SaveItem(IAutoLayoutItem item);

        void SaveItemIndexVisible(int index, IAutoLayoutItem item);

        void SaveItemVisibleIndex(IAutoLayoutItem item, int index);

        void SaveSelectIndex(int index);
    }

    [Serializable]
    public class AutoLayoutManager
    {
        public bool allowMultiSelect;
        public float loadingInterval = 0.01f;
        public bool allowSwitchOff;
        public bool getRenderFromPool = true; // 重复使用item
        private IExternalAutoLayoutManager m_IExternalAutoLayoutManager;
        private Dictionary<int, IAutoLayoutItem> m_IndexVisibleMap;
        private bool m_IsLayoutDirty;
        private Action<IAutoLayoutItem, int> m_ItemIndexSetAgent;
        private Action<Action<IAutoLayoutItem>> m_ItemLoadAgent;
        private Action<IAutoLayoutItem, string> m_ItemNameSetAgent;
        private Action<IAutoLayoutItem, Vector2> m_ItemPositionSetAgent;
        private List<IAutoLayoutItem> m_Items;
        /// <summary>
        /// 元素，下标，是否选中，是否触发事件
        /// </summary>
        private Action<IAutoLayoutItem, int, bool, bool> m_ItemSelectSetAgent;
        private Dictionary<IAutoLayoutItem, int> m_ItemVisibleIndexMap;
        private Action<IAutoLayoutItem, bool> m_ItemVisibleSetAgent;
        private LayoutController m_LayoutController;
        private LayoutData m_LayoutData;
        private LayoutEfficiency m_LayoutEfficiency = LayoutEfficiency.ShowFirst;
        private int m_LoadingCount = 0;
        private HashSet<int> m_SelectedIndexSet;
        public HashSet<int> SelectedIndexSet => m_SelectedIndexSet;
        public int GetVisibleIndexCount()
        {
            return m_LayoutData.visibleItemSet.Count;
        }
        public int GetVisibleIndex(int index)
        {
            if (m_LayoutData.visibleItemSet.Count <= index)
            {
                return -1;
            }
            return m_LayoutData.visibleItemSet[index];
        }

        public AutoLayoutManager()
        {
            m_LayoutData = new LayoutData();
            m_LayoutController = new LayoutController();
            m_Items = new List<IAutoLayoutItem>(16);
            m_ItemVisibleIndexMap = new Dictionary<IAutoLayoutItem, int>(16);
            m_IndexVisibleMap = new Dictionary<int, IAutoLayoutItem>(16);
            m_SelectedIndexSet = new HashSet<int>(16);
        }

        public bool IsLayoutDirty
        {
            get => m_IsLayoutDirty;
            set => m_IsLayoutDirty = value;
        }

        public LayoutData LayoutData => m_LayoutData;

        public LayoutEfficiency LayoutEfficiency
        {
            get => m_LayoutEfficiency;
            set => m_LayoutEfficiency = value;
        }

        public int ItemCount
        {
            get
            {
                if (m_IExternalAutoLayoutManager != null)
                {
                    return m_IExternalAutoLayoutManager.ItemCount;
                }

                return m_Items.Count;
            }
        }

        private int SelectedCount
        {
            get
            {
                if (m_IExternalAutoLayoutManager != null) return m_IExternalAutoLayoutManager.SelectedCount;
                return m_SelectedIndexSet.Count;
            }
        }

        public void ClearAllSelectIndex()
        {
            if (m_IExternalAutoLayoutManager != null)
            {
                m_IExternalAutoLayoutManager.ClearAllSelectedIndex();
                return;
            }

            int count = ItemCount;
            for (int i = 0; i < count; i++)
            {
                var item = GetItemAt(i);
                m_ItemSelectSetAgent?.Invoke(item, item.Index, false, true);
            }
            m_SelectedIndexSet.Clear();
        }

        public bool TryGetIndexVisible(int curShowIndex, out IAutoLayoutItem item)
        {
            if (m_IExternalAutoLayoutManager != null)
            {
                return m_IExternalAutoLayoutManager.GetItemIndexVisible(curShowIndex, out item);
            }

            return m_IndexVisibleMap.TryGetValue(curShowIndex, out item);
        }

        public int GetVisibleItemIndex(IAutoLayoutItem item)
        {
            if (m_IExternalAutoLayoutManager != null)
            {
                if (m_IExternalAutoLayoutManager.GetItemVisibleIndex(item, out int index))
                {
                    return index;
                }

                return -1;
            }

            if (m_ItemVisibleIndexMap.TryGetValue(item, out int res))
            {
                return res;
            }

            return -1;
        }

        public void SetExternalAutoLayoutManager(IExternalAutoLayoutManager exter)
        {
            m_IExternalAutoLayoutManager = exter;
        }

        public void SetItemIndexSetAgent(Action<IAutoLayoutItem, int> agent)
        {
            m_ItemIndexSetAgent = agent;
        }

        public void SetItemLoadAgent(Action<Action<IAutoLayoutItem>> agent)
        {
            m_ItemLoadAgent = agent;
        }

        public void SetItemNameSetAgent(Action<IAutoLayoutItem, string> agent)
        {
            m_ItemNameSetAgent = agent;
        }

        public void SetItemPositionSetAgent(Action<IAutoLayoutItem, Vector2> agent)
        {
            m_ItemPositionSetAgent = agent;
        }

        public void SetItemSelect(int index, bool isSelect)
        {
            int curSelectCount = SelectedCount;
            if (!allowMultiSelect && isSelect && curSelectCount > 0 && !IsSelected(index))
            {
                ClearAllSelectIndex();
                SaveSelectedIndex(index);
                m_IsLayoutDirty = true;
                return;
                //if (!IsSelected(index))
                //{
                //    ClearAllSelectIndex();
                //    SaveSelectedIndex(index);
                //    m_IsLayoutDirty = true;
                //}
                //return;
            }

            if (!allowSwitchOff && curSelectCount == 1 && IsSelected(index) && !isSelect)
            {
                var item = GetItemByIndex(index);
                if (item == null) return;
                m_ItemSelectSetAgent?.Invoke(GetItemByIndex(index), index, true, true);
                m_IsLayoutDirty = true;
                return;
            }

            if (isSelect && !IsSelected(index))
            {
                SaveSelectedIndex(index);
                m_IsLayoutDirty = true;
            }
            else if (!isSelect && IsSelected(index))
            {
                RemoveSelectedIndex(index);
                m_IsLayoutDirty = true;
            }


        }

        public void SetItemSelectSetAgent(Action<IAutoLayoutItem, int, bool, bool> agent)
        {
            m_ItemSelectSetAgent = agent;
        }

        public void SetItemVisibleSetAgent(Action<IAutoLayoutItem, bool> agent)
        {
            m_ItemVisibleSetAgent = agent;
        }

        public void ShowItem(IAutoLayoutItem item, Vector2 position, int index, bool record = true)
        {

            bool isVisibleChanged = false;
            if (item.Index != index || item.IsSelected != IsSelected(index))
            {
                isVisibleChanged = true;
            }
            m_ItemIndexSetAgent?.Invoke(item, index);
            m_ItemNameSetAgent?.Invoke(item, index + "");
            m_ItemSelectSetAgent?.Invoke(item, index, IsSelected(index), isVisibleChanged);
            m_ItemPositionSetAgent?.Invoke(item, position);
            m_ItemVisibleSetAgent?.Invoke(item, true);
            if (record) RecordShow(item, index);
        }
        private bool IsVisible(int index)
        {
            return m_LayoutData.IsVisible(index);
        }
        public void CalculateLayout()
        {
            m_LayoutController.CalculateLayout(m_LayoutData);
        }
        public void UpdateLayout()
        {
            if (!m_IsLayoutDirty) return;
            m_IsLayoutDirty = false;
            try
            {
                while (m_LoadedItemQueue.Count > 0)
                {
                    var item = m_LoadedItemQueue.Dequeue();
                    MoveNewLoadItem(item);
                }

                CalculateLayout();
                //m_LayoutController.CalculateLayout(m_LayoutData);

                // 先回收当前使用中但是在本轮中不显示的Item
                int count = ItemCount;
                for (int i = 0; i < count; i++)
                {
                    var item = GetItemAt(i);
                    if (HasShow(item, out var index))
                    {
                        if (m_LayoutEfficiency == LayoutEfficiency.ShowFirst)
                        {
                            HideItem(item, false);
                        }
                        else
                        {
                            if (IsVisible(index))
                            {
                                // 设置位置就行
                                ShowItem(item, m_LayoutController.GetPositionByIndex(m_LayoutData, index), index, false);
                            }
                            else
                            {
                                HideItem(item);
                            }
                        }
                    }
                    else if (m_LayoutEfficiency == LayoutEfficiency.SpeedFirst && m_LayoutData.IsVisible(index))
                    {
                        ShowItem(item, m_LayoutController.GetPositionByIndex(m_LayoutData, index), index);
                    }
                }

                for (int i = 0; i < m_LayoutData.visibleItemSet.Count; i++)
                {
                    var visibleIndex = m_LayoutData.visibleItemSet.ElementAt(i);
                    if (!HasShowAt(visibleIndex, out IAutoLayoutItem item))
                    {
                        ShowFreeItem(m_LayoutController.GetPositionByIndex(m_LayoutData, visibleIndex), visibleIndex,
                            getRenderFromPool);
                    }
                }

                if (!allowSwitchOff && SelectedCount == 0)
                {
                    SetItemSelect(0, true);
                }
            }
            catch (Exception e)
            {
                Log.Fatal("更新布局失败!!" + e.Message + " " + e.StackTrace);
            }
        }
        public Vector2 GetNormalizedPosition(int index)
        {
            Vector2 viewportSize = m_LayoutData.viewSize;
            Vector2 containerSize = m_LayoutData.containerSize;
            var position = m_LayoutController.GetPositionByIndex(m_LayoutData, index);
            var elementSize = m_LayoutData.itemSize;

            // 计算视口中心应该对准的位置
            float viewCenterX = viewportSize.x * 0.5f;
            float viewCenterY = -viewportSize.y * 0.5f;
            float moveX = viewCenterX - position.x;
            float moveY = viewCenterY - position.y;
            float offsetX = (containerSize.x - viewportSize.x);
            float offsetY = (containerSize.y - viewportSize.y);
            float x = offsetX == 0 ? 0 : -moveX / offsetX;
            float y = offsetY == 0 ? 0 : moveY / offsetY;
            x = Mathf.Clamp01(x);
            y = 1 - Mathf.Clamp01(y);

            return new Vector2(x, y);
        }
        private void AddItem(IAutoLayoutItem item)
        {
            if (m_IExternalAutoLayoutManager != null)
            {
                m_IExternalAutoLayoutManager.SaveItem(item);
                return;
            }

            m_Items.Add(item);
        }
        public IAutoLayoutItem GetVisibleItem(int visibleIndex)
        {
            if (TryGetIndexVisible(visibleIndex, out var item))
            {
                return item;
            }
            return null;
        }
        public IAutoLayoutItem GetItemAt(int index)
        {
            if (m_IExternalAutoLayoutManager != null)
            {
                return m_IExternalAutoLayoutManager.GetItem(index);
            }

            if (index < m_Items.Count)
            {
                return m_Items[index];
            }

            return null;
        }
        public IAutoLayoutItem GetItemByIndex(int index)
        {
            if (m_IExternalAutoLayoutManager != null)
            {
                return m_IExternalAutoLayoutManager.GetItemByIndex(index);
            }
            for (int i = 0; i < m_Items.Count; i++)
            {
                if (m_Items[i].Index == index)
                {
                    return m_Items[i];
                }
            }

            return null;
        }

        private int GetLoadingAllowCount()
        {
            int total = m_LayoutData.itemCount;
            if (getRenderFromPool)
            {
                // 获取当前布局最大需要的个数
                total = m_LayoutData.maxVisibleItemCount;
            }

            return total - ItemCount - m_LoadingCount;
        }

        private bool HasShow(IAutoLayoutItem item, out int curShowIndex)
        {
            if (m_IExternalAutoLayoutManager != null)
            {
                return m_IExternalAutoLayoutManager.GetItemVisibleIndex(item, out curShowIndex) && curShowIndex != -1;
            }

            return m_ItemVisibleIndexMap.TryGetValue(item, out curShowIndex) && curShowIndex != -1;
        }

        private bool HasShowAt(int curShowIndex, out IAutoLayoutItem item)
        {
            return TryGetIndexVisible(curShowIndex, out item);
        }

        private void HideItem(IAutoLayoutItem item, bool clearSelect = true)
        {
            RecordHide(item);
            int index = GetVisibleItemIndex(item);
            m_ItemIndexSetAgent?.Invoke(item, index);
            RecordVisibleIndex(item, -1);
            if (clearSelect)
                m_ItemSelectSetAgent?.Invoke(item, index, false, false);
            m_ItemVisibleSetAgent?.Invoke(item, false);
        }

        public bool IsSelected(int index)
        {
            if (m_IExternalAutoLayoutManager != null)
            {
                return m_IExternalAutoLayoutManager.IsSelectIndex(index);
            }
            return m_SelectedIndexSet.Contains(index);
        }

        private async UniTask Loading()
        {
            m_LoadingCount++;
            await UniTask.WaitForSeconds(m_LoadingCount * loadingInterval, true);
            m_ItemLoadAgent?.Invoke(OnItemLoadCompletely);
        }
        private void LoadingNow()
        {
            m_LoadingCount++;
            m_ItemLoadAgent?.Invoke(OnItemLoadCompletely);
        }
        private Queue<IAutoLayoutItem> m_LoadedItemQueue = new Queue<IAutoLayoutItem>();
        private void MoveNewLoadItem(IAutoLayoutItem item)
        {
            AddItem(item);
            m_LoadingCount = m_LoadingCount - 1;
            m_LoadingCount = Mathf.Max(m_LoadingCount, 0);
            RecordVisibleIndex(item, -1);
            m_ItemIndexSetAgent?.Invoke(item, -1);
            m_ItemVisibleSetAgent?.Invoke(item, false);
        }
        private void OnItemLoadCompletely(IAutoLayoutItem item)
        {
            m_LoadedItemQueue.Enqueue(item);
            RecordVisibleIndex(item, -1);
            m_ItemIndexSetAgent?.Invoke(item, -1);
            m_ItemVisibleSetAgent?.Invoke(item, false);
            m_IsLayoutDirty = true;
            // 判断是否需要展示
            //AddItem(item);
            //m_LoadingCount = m_LoadingCount - 1;
            //m_LoadingCount = Mathf.Max(m_LoadingCount, 0);
            //m_IsLayoutDirty = true; // 需要刷新视图
            //RecordVisibleIndex(item, -1);
            //m_ItemIndexSetAgent?.Invoke(item, -1);
            //m_ItemVisibleSetAgent?.Invoke(item, false);
            //if (ItemCount <= m_LayoutData.visibleItemSet.Count)
            //{
            //    this.UpdateLayout();
            //    m_IsLayoutDirty = true;
            //}
        }

        private void RecordHide(IAutoLayoutItem item)
        {
            int index = GetVisibleItemIndex(item);
            if (index != -1)
            {
                if (m_IExternalAutoLayoutManager != null)
                {
                    m_IExternalAutoLayoutManager.RemoveIndexVisible(index);
                }
                else
                {
                    m_IndexVisibleMap.Remove(index);
                }
            }
        }

        private void RecordShow(IAutoLayoutItem item, int visibleIndex)
        {
            SaveIndexVisible(visibleIndex, item);
            //m_IndexVisibleMap.Add(visibleIndex,item);
            RecordVisibleIndex(item, visibleIndex);
        }

        private void RecordVisibleIndex(IAutoLayoutItem item, int visibleIndex)
        {
            if (m_IExternalAutoLayoutManager != null)
            {
                m_IExternalAutoLayoutManager.SaveItemVisibleIndex(item, visibleIndex);
            }
            else
            {
                m_ItemVisibleIndexMap[item] = visibleIndex;
            }
        }

        private void RemoveSelectedIndex(int index)
        {
            if (m_IExternalAutoLayoutManager != null)
            {
                m_IExternalAutoLayoutManager.RemoveSelectIndex(index);
            }
            else
            {
                m_SelectedIndexSet.Remove(index);
            }
            if (!IsVisible(index))
            {
                m_ItemSelectSetAgent?.Invoke(null, index, false, true);
            }
        }

        private void SaveIndexVisible(int visibleIndex, IAutoLayoutItem item)
        {
            if (m_IExternalAutoLayoutManager != null)
            {
                m_IExternalAutoLayoutManager.SaveItemIndexVisible(visibleIndex, item);
            }
            else
            {
                m_IndexVisibleMap.Add(visibleIndex, item);
            }

        }

        private void SaveSelectedIndex(int index)
        {
            if (m_IExternalAutoLayoutManager != null)
            {
                m_IExternalAutoLayoutManager.SaveSelectIndex(index);
            }
            else
            {
                m_SelectedIndexSet.Add(index);
            }
            if (!IsVisible(index))
            {
                m_ItemSelectSetAgent?.Invoke(null, index, true, true);
            }
            else
            {
                m_ItemSelectSetAgent?.Invoke(GetItemByIndex(index), index, true, true);
            }
        }

        private void ShowFreeItem(Vector2 position, int visibleIndex, bool isFromPool)
        {
            if (!isFromPool && ItemCount > visibleIndex)
            {
                var itemFind = GetItemAt(visibleIndex);
                ShowItem(itemFind, position, visibleIndex);
                return;
            }

            if (isFromPool && TryGetFreeItemFromPool(out IAutoLayoutItem poolFreeItem))
            {
                ShowItem(poolFreeItem, position, visibleIndex);
                return;
            }

            bool isIgnoreLoading = !isFromPool && ItemCount + m_LoadingCount > visibleIndex;
            if (GetLoadingAllowCount() > 0)
            {
                if (!isIgnoreLoading)
                {
                    // 重新加载一个对象
                    if (Application.isPlaying)
                    {
                        Loading().Forget();
                    }
                    else
                    {
                        LoadingNow();
                    }
                }
            }
        }

        private bool TryGetFreeItemFromPool(out IAutoLayoutItem item)
        {
            for (int i = 0; i < ItemCount; i++)
            {
                if (!HasShow(GetItemAt(i), out var index))
                {
                    item = GetItemAt(i);
                    return true;
                }
            }

            item = null;
            return false;
        }

        public int GetSelectIndex(int index)
        {
            if (SelectedIndexSet == null || SelectedIndexSet.Count <= index)
            {
                return -1;

            }
            return SelectedIndexSet.ElementAt(index);
        }

        public void ClearLoadingCount()
        {
            m_LoadingCount = 0;
        }
    }
}