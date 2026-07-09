using Framework.Runtime.LogSystem;
using Framework.Utils;
using System;
using System.Collections.Generic;
using System.Security.Principal;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Framework.Runtime.UI
{
    public class UCheckBoxGroup : UContainer, IExternalAutoLayoutManager
    {
        [SerializeField] private UCheckBox m_CheckBoxPrefab;
        [SerializeField] private List<string> m_Tabs = new List<string>();
        [SerializeField] private bool m_GetItemFromPool = true;
        [SerializeField] private Padding m_Padding = new Padding();
        [SerializeField] private Vector2 m_Spacing = Vector2.zero;
        [SerializeField] private Corner m_StartCorner = Corner.UpperLeft;
        [SerializeField] private Axis m_Axis;
        [SerializeField] private ChildAlignment m_Alignment;
        [SerializeField] private Constraint m_Constraint;
        [SerializeField] private float m_LoadingInterval = 0.0015f;
        [SerializeField] private int m_FixedCount;
        [SerializeField] private bool m_IsCustomItemSize;
        [SerializeField] private Vector2 m_ItemSize;
        [SerializeField] private Vector2 m_ChildPivot = new Vector2(0, 1);
        [SerializeField] private bool m_AllowMultiSelect = true; // 是否允许多选
        [SerializeField] private bool m_AllowSwitchOff = true; // 是否允许单次开关

        //[SerializeField] private bool m_FlexContentSize = true;
        [SerializeField] private FlexSizeType m_XFlexSizeType = FlexSizeType.MinViewSize;
        [SerializeField] private FlexSizeType m_YFlexSizeType = FlexSizeType.MinViewSize;

        // 有一定的性能消耗
        [SerializeField] private bool m_SyncRenderName = false;

        private Dictionary<IAutoLayoutItem, UCheckBox> m_DisplayUnit2RenderMap =
            new Dictionary<IAutoLayoutItem, UCheckBox>(1024);

        [SerializeField] private List<UCheckBox> m_CheckboxList = new List<UCheckBox>();
        [SerializeField] private List<int> m_SelectedIndexList = new List<int>();
        public int TabCount => m_Tabs.Count;
        private List<UCheckBox> CheckboxList
        {
            get
            {
                if (m_CheckboxList == null) m_CheckboxList = new List<UCheckBox>();
                return m_CheckboxList;
            }
        }

        private int[] OldSelectedIndexList;
        private AutoLayoutManager m_LayoutManager = new AutoLayoutManager();
        private bool m_AssemplyChanged = true;
        private LayoutData m_LayoutData => m_LayoutManager.LayoutData;
        private Action<int> m_OnSelect;
        private Action<int> m_OnDeSelect;

        public int SelectedIndex
        {
            get => m_SelectedIndexList.Count > 0 ? m_SelectedIndexList[0] : -1;
            set
            {
                m_LayoutManager.ClearAllSelectIndex();
                m_LayoutManager.SetItemSelect(value, true);
            }
        }

        protected virtual UCheckBox BaseRenderPrefab
        {
            get { return m_CheckBoxPrefab; }
            set
            {
                if (value == null) m_CheckBoxPrefab = null;
                else
                {
                    m_CheckBoxPrefab = value;
                }
            }
        }

        private Vector2 ItemSize
        {
            get
            {
                if (m_IsCustomItemSize) return m_ItemSize;
                if (BaseRenderPrefab != null)
                {
                    return BaseRenderPrefab.RectTransform.sizeDelta * ItemScale;
                }
                return Vector2.one;
            }
        }
        private Vector3 m_ItemScale = Vector3.one;
        public Vector3 ItemScale { get => m_ItemScale; set => m_ItemScale = value; }
        private void InitAssemplyCheckBox()
        {
            if (m_AssemplyChanged)
            {
                for (int i = 0; i < CheckboxList.Count; i++)
                {
                    var checkBox = CheckboxList[i];
                    checkBox.onValueChanged.AddListener((isOn) =>
                    {
                        m_LayoutManager.SetItemSelect(checkBox.Index, isOn);
                    });
                }
            }
        }

        private void BindListenerCheck()
        {
            m_LayoutManager.SetExternalAutoLayoutManager(this);
            m_LayoutManager.SetItemLoadAgent(ItemLoad);
            m_LayoutManager.SetItemPositionSetAgent(ItemPositionSet);
            m_LayoutManager.SetItemSelectSetAgent(ItemSelectSet);
            m_LayoutManager.SetItemVisibleSetAgent(ItemVisibleSet);
            m_LayoutManager.SetItemIndexSetAgent(ItemIndexSet);
            m_LayoutManager.SetItemNameSetAgent(ItemNamSet);
        }

        protected override void Awake()
        {
            base.Awake();
            var select = new List<int>(m_SelectedIndexList);
            onValueChanged.AddListener(OnScrollValueChanged);
            m_LayoutManager.ClearAllSelectIndex();
            BindListenerCheck();
            for (int i = 0; i < m_CheckboxList.Count; i++)
            {
                var ucheckBox = m_CheckboxList[i];
                RegisterCheckBox(ucheckBox);
                //ucheckBox.AddPreSelect((isOn) =>
                //{
                //    OnTabValueChanged(ucheckBox, isOn);
                //    m_LayoutManager.SetItemSelect(ucheckBox.Index, ucheckBox.IsSelected);
                //});
            }
            for (int i = 0; i < select.Count; i++)
            {
                m_LayoutManager.SetItemSelect(select[i], true);
            }
        }
        private void RegisterCheckBox(UCheckBox ucheckBox)
        {
            ucheckBox.AddPreSelect((isOn) =>
            {
                OnTabValueChanged(ucheckBox, isOn);
                m_LayoutManager.SetItemSelect(ucheckBox.Index, ucheckBox.IsSelected);
            });
        }
        public void SetTabLabel(int tabIndex, string tabContent, bool isAdd = true)
        {
            if (isAdd)
            {
                AddTabTo(tabIndex);
            }
            UCheckBox simpleTabBar = GetTabBar(tabIndex);
            m_Tabs[tabIndex] = tabContent;
            simpleTabBar?.SetLabel(tabContent);
        }

#if UNITY_EDITOR

        protected override void OnValidate()
        {
            //if (!Application.isPlaying || !isActiveAndEnabled) return;
            base.OnValidate();
            m_LayoutManager.IsLayoutDirty = true;
        }

#endif

        public UCheckBox GetTabBar(int index)
        {
            return CheckboxList[index];
        }

        public void AddSelect(int tabIndex, Action<bool> onTabChange)
        {
            GetTabBar(tabIndex)?.AddValueChanged(onTabChange);
        }
        public void RemoveSelect(int tabIndex, Action<bool> onTabDeSelect)
        {
            GetTabBar(tabIndex)?.RemoveValueChanged(onTabDeSelect);
        }
        public void SwitchTab(int tabIndex)
        {
            if (m_Tabs.Count <= tabIndex)
            {
                Log.Error("switch tab index over current tabs count!");
                return;
            }

            SelectedIndex = tabIndex;
        }
        public void RefreshLayoutImmediate()
        {
            m_ContentSizeDirty = true;
            m_LayoutManager.IsLayoutDirty = true;
            if (m_ContentSizeDirty)
            {
                m_ContentSizeDirty = false;
                RefreshLayoutSize();
            }
            if (m_LayoutManager.IsLayoutDirty)
            {
                RebuildLayoutImmediate();
            }
        }
        public void RefreshLayout(bool isForce = false)
        {
            if (!Application.isPlaying && !isForce) return;
            if (isForce)
            {
                RefreshLayoutImmediate();
            }
            else
            {
                BindListenerCheck();
                UpdateLayoutOption();
                m_LayoutManager.IsLayoutDirty = true;
            }
        }
        private Vector2 m_ContentSizeCalculate;
        private bool m_ContentSizeDirty = true;

        protected override void LateUpdate()
        {
            if (!isActiveAndEnabled) return;
            base.LateUpdate();
            InitAssemplyCheckBox();
            m_AssemplyChanged = false;
            if (m_ContentSizeDirty)
            {
                m_ContentSizeDirty = false;
                RefreshLayoutSize();
            }
            if (m_LayoutManager.IsLayoutDirty && CheckSizeValidate())
            {
                RebuildLayoutImmediate();
            }
        }
        private bool CheckSizeValidate()
        {
            Vector3 scale = transform.lossyScale;
            if (scale.y <= 0.001f || scale.x <= 0.001f || scale.z <= 0.001f)
            {
                return false;
            }
            return true;
        }
        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            ReCalculateView(normalizedPosition, true);
        }
        private void RebuildLayoutImmediate()
        {
            m_LayoutManager.IsLayoutDirty = true;
            BindListenerCheck();
            UpdateLayoutOption();
            m_LayoutManager.CalculateLayout();
            m_LayoutManager.UpdateLayout();
        }
        private void RefreshLayoutSize()
        {
            m_ContentSizeCalculate = viewRect.rect.size;
            UpdateLayoutOption();
            m_LayoutManager.CalculateLayout();
            ResetContentSize();
            ApplyContentSize();
        }
        private void ResetContentSize()
        {
            Vector2 newSize = m_ContentSizeCalculate;
            newSize.x = ResetAxisSize(m_XFlexSizeType, m_LayoutData.flexContentSize.x, m_ContentSizeCalculate.x);
            newSize.y = ResetAxisSize(m_YFlexSizeType, m_LayoutData.flexContentSize.y, m_ContentSizeCalculate.y);
            m_ContentSizeCalculate = newSize;
        }
        private void ApplyContentSize()
        {
            ContentSize = m_ContentSizeCalculate;
        }
        private float ResetAxisSize(FlexSizeType flexSizeType, float dataSize, float viewSize)
        {
            if (flexSizeType == FlexSizeType.MinViewSize)
            {
                return Mathf.Max(dataSize, viewSize);
            }
            if (flexSizeType == FlexSizeType.DataSize)
            {
                return dataSize;
            }
            if (flexSizeType == FlexSizeType.ViewSize)
            {
                return viewSize;
            }
            return dataSize;
        }


        private void UpdateLayoutOption()
        {
            m_LayoutData.itemSize = ItemSize;
            m_LayoutData.alignment = m_Alignment;
            m_LayoutData.padding = m_Padding;
            m_LayoutData.spacing = m_Spacing;
            m_LayoutData.corner = m_StartCorner;
            m_LayoutData.axis = m_Axis;
            m_LayoutData.axisFixedCount = m_FixedCount;
            m_LayoutData.layoutConstraint = m_Constraint;
            m_LayoutData.containerSize = ContentSize;
            m_LayoutData.itemCount = m_Tabs.Count;
            m_LayoutData.viewSize = viewport.rect.size;

            m_LayoutData.customOffset = GetItemPivotOffset();
            m_LayoutManager.getRenderFromPool = m_GetItemFromPool;
            m_LayoutManager.allowMultiSelect = m_AllowMultiSelect;
            m_LayoutManager.allowSwitchOff = m_AllowSwitchOff;
            m_LayoutManager.loadingInterval = m_LoadingInterval;
        }

        private Vector2 GetItemPivotOffset()
        {
            var rectTransform = BaseRenderPrefab.RectTransform;
            Vector2 size = ItemSize;
            Vector2 pivotOffset = rectTransform.pivot - m_ChildPivot;
            return new Vector2(pivotOffset.x * size.x, pivotOffset.y * size.y);
        }

        private void ItemLoad(Action<IAutoLayoutItem> cb)
        {
            var ucheckBox = CreateCheckBox();
            m_CheckboxList.Add(ucheckBox);
            cb?.Invoke(ucheckBox);
        }

        private UCheckBox CreateCheckBox()
        {
#if UNITY_EDITOR
            var ucheckBox = PrefabUtility.InstantiatePrefab(BaseRenderPrefab) as UCheckBox;
            if (ucheckBox == null)
            {
                ucheckBox = GameObject.Instantiate(BaseRenderPrefab).GetComponent<UCheckBox>();
            }
            //var ucheckBox = GameObject.Instantiate(BaseRenderPrefab).GetComponent<UCheckBox>();

#else
            var ucheckBox = GameObject.Instantiate(BaseRenderPrefab).GetComponent<UCheckBox>();
#endif
            ucheckBox.gameObject.SetActive(true);


            AddChild(ucheckBox.RectTransform);
            RegisterCheckBox(ucheckBox);
            //ucheckBox.onValueChanged.AddListener((isOn) =>
            //{
            //    OnTabValueChanged(ucheckBox, isOn);
            //    m_LayoutManager.SetItemSelect(ucheckBox.Index, isOn);
            //});
            return ucheckBox;
        }

        private void OnTabValueChanged(UCheckBox tabBar, bool value)
        {
            int index = CheckboxList.IndexOf(tabBar);
            if (value)
            {
                m_OnSelect?.Invoke(index);
            }
            else
                m_OnDeSelect?.Invoke(index);
        }

        private void AddChild(RectTransform rt)
        {
            rt.SetParent(contentRT, false);
            rt.SetAnchor(AnchorPresets.TopLeft); // 设置anchor
            rt.anchoredPosition = Vector2.zero;
        }

        private void ItemPositionSet(IAutoLayoutItem item, Vector2 pos)
        {
            (item as UCheckBox).RectTransform.anchoredPosition = pos;
        }

        private void ItemSelectSet(IAutoLayoutItem item, int index, bool isSelect, bool isEvent = true)
        {
            if (item == null && isEvent)
            {
                if (isSelect)
                {
                    m_OnSelect?.Invoke(index);
                }
                else if (!isSelect)
                {
                    m_OnDeSelect?.Invoke(index);
                }
                return;
            }
            var render = item as UCheckBox;
            if (isEvent)
            {
                if (isSelect)
                {
                    render?.DoSelect(true);
                }
                else if (!isSelect)
                {
                    render?.DoDeSelect(true);
                }
            }
            item.IsSelected = isSelect;

        }

        private void ItemIndexSet(IAutoLayoutItem item, int index)
        {
            item.Index = index;
        }

        private void ItemVisibleSet(IAutoLayoutItem item, bool visible)
        {
            var checkBox = (item as UCheckBox);
            GameObjectUtil.SetActive(checkBox, visible);

            if (visible)
            {
                checkBox.Text = GetDataAt(checkBox.Index);
            }
        }

        private string GetDataAt(int index)
        {
            if (m_Tabs.Count > index && index >= 0)
            {
                return m_Tabs[index];
            }

            return null;
        }

        private void ItemNamSet(IAutoLayoutItem item, string name)
        {
            if (!m_SyncRenderName) return;
            (item as UCheckBox).name = name;
        }
        private void ReCalculateView(Vector2 normalPosition, bool isRedirectAnchor = false)
        {
            if (!CheckSizeValidate()) return;
            Vector2 contentSize = contentRT.rect.size;
            Vector2 viewRectSize = viewport.rect.size;
            m_LayoutData.viewRelativeStart.x = normalPosition.x * (contentSize.x - viewRectSize.x);
            m_LayoutData.viewRelativeStart.y = (1 - normalPosition.y) * (contentSize.y - viewRectSize.y);
            if (isRedirectAnchor)
            {
                Vector2 anchorPos = contentRT.anchoredPosition;
                if (!horizontal)
                {
                    anchorPos.x = m_LayoutData.viewRelativeStart.x;
                }
                if (!vertical)
                {
                    anchorPos.y = m_LayoutData.viewRelativeStart.y;
                }
                contentRT.anchoredPosition = anchorPos;
            }
            m_LayoutManager.IsLayoutDirty = true;
        }
        private void OnScrollValueChanged(Vector2 normalPosition)
        {
            ReCalculateView(normalPosition, false);
        }

        int IExternalAutoLayoutManager.ItemCount
        {
            get => CheckboxList.Count;
        }

        void IExternalAutoLayoutManager.SaveItem(IAutoLayoutItem item)
        {
            if (!CheckboxList.Contains(item as UCheckBox))
            {
                CheckboxList.Add(item as UCheckBox);
            }
        }

        IAutoLayoutItem IExternalAutoLayoutManager.GetItem(int index)
        {
            if (CheckboxList.Count <= index) return null;
            if (CheckboxList[index].gameObject == null)
            {
                CheckboxList[index] = CreateCheckBox();
            }
            return CheckboxList[index];
        }
        IAutoLayoutItem IExternalAutoLayoutManager.GetItemByIndex(int index)
        {
            for (int i = 0; i < CheckboxList.Count; i++)
            {
                if (CheckboxList[i].Index == index)
                {
                    return CheckboxList[i];
                }
            }
            return null;
        }

        void IExternalAutoLayoutManager.SaveItemVisibleIndex(IAutoLayoutItem item, int index)
        {
            item.Index = index;
        }

        bool IExternalAutoLayoutManager.GetItemVisibleIndex(IAutoLayoutItem item, out int index)
        {
            index = item.Index;
            return item.Index != -1;
        }

        void IExternalAutoLayoutManager.SaveItemIndexVisible(int index, IAutoLayoutItem item)
        {
            item.Index = index;
        }

        bool IExternalAutoLayoutManager.GetItemIndexVisible(int index, out IAutoLayoutItem item)
        {
            for (int i = 0; i < CheckboxList.Count; i++)
            {
                var curIndex = CheckboxList[i].Index;
                if (curIndex == index && curIndex != -1)
                {
                    item = CheckboxList[i];
                    return true;
                }
            }

            item = null;
            return false;
        }

        void IExternalAutoLayoutManager.RemoveIndexVisible(int index)
        {
            for (int i = 0; i < CheckboxList.Count; i++)
            {
                var curIndex = CheckboxList[i].Index;
                if (curIndex == index)
                {
                    CheckboxList[i].Index = -1;
                    return;
                }
            }
        }
        public void AddTabTo(int tabIndex)
        {
            int start = m_Tabs.Count;
            int create = tabIndex - start + 1;
            for (int i = 0; i < create; i++)
            {
                m_Tabs.Add("New Tab");
                m_ContentSizeDirty = true;
            }
            int tabNeedCreateCount = tabIndex - m_CheckboxList.Count + 1;
            for (int i = 0; i < tabNeedCreateCount; i++)
            {
                var ucheckBox = CreateCheckBox();
                m_CheckboxList.Add(ucheckBox);
            }

            RefreshLayout();
        }
        void IExternalAutoLayoutManager.ClearAllSelectedIndex()
        {
            int count = CheckboxList.Count;
            for (int i = 0; i < count; i++)
            {
                var item = CheckboxList[i];
                ItemSelectSet(item, item.Index, false, true);
            }
            m_SelectedIndexList.Clear();
        }

        void IExternalAutoLayoutManager.SaveSelectIndex(int index)
        {
            if (!m_SelectedIndexList.Contains(index))
                m_SelectedIndexList.Add(index);
        }

        void IExternalAutoLayoutManager.RemoveSelectIndex(int index)
        {
            m_SelectedIndexList.Remove(index);
        }

        bool IExternalAutoLayoutManager.IsSelectIndex(int index)
        {
            return m_SelectedIndexList.Contains(index);
        }

        int IExternalAutoLayoutManager.SelectedCount
        {
            get => m_SelectedIndexList.Count;
        }

        public void AddSelect(Action<int> listener)
        {
            m_OnSelect -= listener;
            m_OnSelect += listener;
        }

        public void RemoveSelect(Action<int> listener)
        {
            m_OnSelect -= listener;
        }

        public void RemoveDeSelect(Action<int> listener)
        {
            m_OnDeSelect -= listener;
        }

        public void AddDeSelect(Action<int> listener)
        {
            m_OnDeSelect -= listener;
            m_OnDeSelect += listener;
        }

        public void DeleteCheckBox(int index)
        {
        }

        public void ClearAllCheckBox()
        {
            m_Tabs.Clear();
            m_ContentSizeDirty = true;
            m_CheckboxList.Clear();
            m_SelectedIndexList.Clear();
            m_LayoutManager.ClearAllSelectIndex();
            m_LayoutManager.ClearLoadingCount();
            if (Application.isPlaying)
            {
                GameObjectUtil.DestroyChilds(contentRT);
            }
            else
            {
                GameObjectUtil.DestroyChildsImmediate(contentRT);
            }
        }
    }
}