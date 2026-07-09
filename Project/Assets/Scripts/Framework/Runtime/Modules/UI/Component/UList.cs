//#define OLD_LIST

using Cysharp.Threading.Tasks;
using Framework.Runtime.LogSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
// using Unity.Loading;
using UnityEditor;
using UnityEngine;
using UnityEngine.Scripting;

namespace Framework.Runtime.UI
{
    public enum FlexSizeType
    {
        DataSize = 1,
        MinViewSize = 2,
        ViewSize = 3,
    }

    [Preserve]
    public class UList : UContainer
    {
        [SerializeField] private bool m_GetItemFromPool = true;
        [SerializeField] private LayoutEfficiency m_LayoutEfficiency = LayoutEfficiency.ShowFirst;
        [SerializeField] private Padding m_Padding = new Padding();
        [SerializeField] private Vector2 m_Spacing = Vector2.zero;
        [SerializeField] private Corner m_StartCorner = Corner.UpperLeft;
        [SerializeField] private FlexSizeType m_XFlexSizeType = FlexSizeType.MinViewSize;
        [SerializeField] private FlexSizeType m_YFlexSizeType = FlexSizeType.MinViewSize;
        [SerializeField] private Axis m_Axis;
        [SerializeField] private ChildAlignment m_Alignment;
        [SerializeField] private Constraint m_Constraint;
        [SerializeField] private float m_LoadingInterval = 0.0015f;
        [SerializeField] private int m_FixedCount;
        [SerializeField] private bool m_IsCustomItemSize;
        [SerializeField] private Vector2 m_ItemSize;
        [SerializeField] private Vector2 m_ChildPivot = new Vector2(0, 1);
        [SerializeField] private bool m_AllowMultiSelect = false; // 是否允许多选
        [SerializeField] private bool m_AllowSwitchOff = true; // 是否允许单次开关
        // 有一定的性能消耗
        [SerializeField] private bool m_SyncRenderNameSync = false;

        [SerializeField] private UIBaseRender m_ListRenderPrefab;

        [Header("动效设置")]
        [SerializeField] private bool m_EnableVisibleAnimation = false;
        [SerializeField] private float m_VisibleAnimationDuration = 0.1f;
        [SerializeField] private float m_CanvasGroupFadeDuration = 0.1f;

        public bool EnableVisibleAnimation => m_EnableVisibleAnimation;
        public float VisibleAnimationDuration => m_VisibleAnimationDuration;
        public float CanvasGroupFadeDuration => m_CanvasGroupFadeDuration;

        private AutoLayoutManager m_LayoutManager = new AutoLayoutManager();
        private ListOption m_ListOption;
        private Vector3 m_ItemScale = Vector3.one;
        public Vector3 ItemScale { get => m_ItemScale; set => m_ItemScale = value; }
        public int SelectedIndex
        {
            get => m_LayoutManager.GetSelectIndex(0);
            set
            {
                m_LayoutManager.ClearAllSelectIndex();
                m_LayoutManager.SetItemSelect(value, true);
            }
        }
        public ListOption ListOption
        {
            get => m_ListOption; set
            {
                m_ListOption = value;
                ResetOption(m_ListOption);
            }
        }

        public Type ListRenderType
        {
            get { return m_ListDisplayType; }
            set
            {
                if (value.IsSubclassOf(typeof(UListDisplayUnit)))
                {
                    m_ListDisplayType = value;
                }
                else
                {

                    Log.Error($"目标类请继承 UListDisplayUnit ，当前类{value.GetType().Name}");

                }
            }
        }

        private void ResetOption(ListOption option)
        {
            m_ItemScale = option.itemScale;
            for (int i = 0; i < m_LayoutManager.ItemCount; i++)
            {
                var item = m_LayoutManager.GetItemAt(i);
                if (item == null) continue;
                (item as UListDisplayUnit).SetListOption(option);
            }
        }
        public UListDisplayUnit GetVisibleItem(int visibleIndex)
        {
            var item = m_LayoutManager.GetVisibleItem(visibleIndex);
            if (item == null) return null;
            return (item as UListDisplayUnit);
        }

        private LayoutData m_LayoutData => m_LayoutManager.LayoutData;
        private Type m_ListDisplayType = typeof(UListDisplayUnit); // c# 渲染
        private string m_PrefabAssetPath;

        public string PrefabAssetPath
        {
            get { return m_PrefabAssetPath; }
            set
            {
                if (value != m_PrefabAssetPath && !string.IsNullOrEmpty(value))
                {
                    m_PrefabAssetPath = value;
                    m_ListRenderPrefab = LoadRenderPrefab();
                }
            }
        }

        public UIBaseRender ListRenderPrefab
        {
            get
            {
                if (m_ListRenderPrefab == null)
                {
                    m_ListRenderPrefab = LoadRenderPrefab();
                }

                return m_ListRenderPrefab;
            }
        }

        public object GetDataAt(int index)
        {
            if (DataSources.Count > index && index >= 0)
            {
                return DataSources[index];
            }

            return null;
        }

        private UIBaseRender LoadRenderPrefab()
        {
            if (m_ListRenderPrefab == null)
            {
                string path = "";
                if (string.IsNullOrEmpty(path))
                {
                    Log.Error($"Not Found Path key target: {path}");
                    return null;
                }

                var prefab = UIAgent.LoadAssetSync(path).GetAsset() as GameObject;
                m_ListRenderPrefab = prefab?.GetComponent<UIBaseRender>();
            }

            return m_ListRenderPrefab;
        }

        private Vector2 ItemSize
        {
            get
            {
                if (m_IsCustomItemSize) return m_ItemSize;
                if (m_ListRenderPrefab != null)
                {
                    return m_ListRenderPrefab.RectTransform.sizeDelta * ItemScale;
                }

                return Vector2.one;
            }
        }

        private Dictionary<UListDisplayUnit, UIBaseRender> m_DisplayUnit2RenderMap =
            new Dictionary<UListDisplayUnit, UIBaseRender>(1024);

        private List<object> m_Datas = new List<object>(1024);
        private Vector2 m_ContentSizeCalculate;
        private bool m_ContentSizeDirty = true;
        public int GetVisibleIndexCount()
        {
            return m_LayoutManager.GetVisibleIndexCount();
        }
        public int GetVisibleIndexAt(int index)
        {
            return m_LayoutManager.GetVisibleIndex(index);
        }
        public void ForInVisibleItem(Action<UListDisplayUnit> loopCb)
        {
            int count = GetVisibleIndexCount();

            for (int i = 0; i < count; i++)
            {
                int visibleIndex = GetVisibleIndexAt(i);
                IAutoLayoutItem item = GetVisibleItem(visibleIndex);
                if (item != null)
                {
                    loopCb?.Invoke(item as UListDisplayUnit);
                }
            }
        }

        public void SetSelect(int index, bool isSelect)
        {
            m_LayoutManager.SetItemSelect(index, isSelect);
        }

        public List<object> DataSources
        {
            get => m_Datas;
            set
            {
                ClearData();
                m_Datas = value;
                m_ContentSizeDirty = true;
                m_LayoutManager.ClearAllSelectIndex();
                MoveToStart(true, 0);
                ReCalculateView(GetClampedNormalizedPosition(), true);
                m_LayoutManager.IsLayoutDirty = true;
            }
        }
        public Vector2 GetClampedNormalizedPosition(Vector2 originPos)
        {
            return new Vector2(Mathf.Clamp01(originPos.x), Mathf.Clamp01(originPos.y));

        }
        public Vector2 GetClampedNormalizedPosition()
        {
            return GetClampedNormalizedPosition(normalizedPosition);
        }
        public void ClearSelects()
        {
            m_LayoutManager.ClearAllSelectIndex();
        }

        public void MoveTo(int index, float duration = 0.5f, bool refreshImediately = true)
        {
            index = Mathf.Clamp(index, 0, DataSources.Count - 1);
            if (refreshImediately)
            {
                RefreshLayoutImmediate();
            }
            Vector2 normalPostion = m_LayoutManager.GetNormalizedPosition(index);
            MoveTo(normalPostion.x, normalPostion.y, duration);
            if (refreshImediately)
            {
                RebuildLayoutImmediate();
            }

        }

        private bool m_IsRuntiming = false;

        private void RecordDisplayUnit2BaseRender(UListDisplayUnit displayBehaviourUnit,
            UIBaseRender baseRender)
        {
            m_DisplayUnit2RenderMap.Add(displayBehaviourUnit, baseRender);
        }
        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            m_ContentSizeDirty = true;
            ReCalculateView(GetClampedNormalizedPosition(), true);
        }
        private UIBaseRender GetBaseRender(IAutoLayoutItem item)
        {
            if (item != null && m_DisplayUnit2RenderMap.ContainsKey(item as UListDisplayUnit))
            {
                return m_DisplayUnit2RenderMap[item as UListDisplayUnit];
            }
            return null;
        }

        private UIBaseRender GetBaseRender(UListDisplayUnit displayUnit)
        {
            return m_DisplayUnit2RenderMap[displayUnit];
        }

        public void ClearData()
        {
            if (m_Datas.Count == 0) return;
            //m_LastDatas = null;
            m_Datas = new List<object>();
            m_ContentSizeDirty = true;
            m_LayoutManager.ClearAllSelectIndex();
            MoveToStart(true, 0);
            ReCalculateView(GetClampedNormalizedPosition(), true);
            //m_LayoutManager.IsLayoutDirty = true;
            RebuildLayoutImmediate();
        }

        public void RemoveData(object data) // 删除一条数据
        {
            m_Datas.Remove(data);
            m_ContentSizeDirty = true;
            m_LayoutManager.IsLayoutDirty = true;
        }

        public void AddData(object data) // 插入一条数据
        {
            m_Datas.Add(data);
            m_LayoutManager.IsLayoutDirty = true;
            m_ContentSizeDirty = true;
        }

        [ContextMenu("重新刷新")]
        public void RefreshLayout()
        {
            UpdateLayoutOption();
            m_LayoutManager.IsLayoutDirty = true;
        }

        public void RemoveData(int index) // 删除一条数据
        {
            if (index >= m_Datas.Count) return;
            m_Datas.RemoveAt(index);
            m_LayoutManager.IsLayoutDirty = true;
        }
        public void SetDataSources<T>(IEnumerable<T> datas)
        {
            if (datas == null)
            {
                ClearData();
                return;
            }
            if (gameObject == null) return;
            List<object> dataSources = new List<object>();
            foreach (var item in datas)
            {
                dataSources.Add(item);
            }
            DataSources = dataSources;
        }

        //public void SetDataSources<T>(T[] datas)
        //{
        //    if (datas == null)
        //    {
        //        DataSources = new List<object>();
        //        return;
        //    }

        //    if (gameObject == null) return;
        //    // 可能会被裁剪
        //    List<object> dataSources = new List<object>();
        //    for (int i = 0; i < datas.Length; i++)
        //    {
        //        dataSources.Add(datas[i]);
        //    }

        //    DataSources = dataSources;
        //}

        private void CheckRuntime()
        {
#if UNITY_EDITOR
            bool isPrefab = UnityEditor.PrefabUtility.IsPartOfPrefabAsset(gameObject);
            if (isPrefab)
            {
                m_IsRuntiming = false;
                return;
            }
#endif
            m_IsRuntiming = false;
            if (!gameObject.scene.IsValid())
            {
                // 脚本挂载在预制体上，禁用逻辑执行
                return;
            }
            // 检查是否在编辑器模式下运行
            if (Application.isPlaying)
            {
                m_IsRuntiming = true;
                m_LayoutManager.IsLayoutDirty = true;
                contentRT.SetAnchor(AnchorPresets.TopLeft);
            }
        }

        protected override void Awake()
        {
            CheckRuntime();
            base.Awake();
            onValueChanged.AddListener(OnScrollValueChanged);
            m_LayoutManager.SetItemLoadAgent(ItemLoad);
            m_LayoutManager.SetItemPositionSetAgent(ItemPositionSet);
            m_LayoutManager.SetItemSelectSetAgent(ItemSelectSet);
            m_LayoutManager.SetItemVisibleSetAgent(ItemVisibleSet);
            m_LayoutManager.SetItemIndexSetAgent(ItemIndexSet);
            m_LayoutManager.SetItemNameSetAgent(ItemNamSet);
        }

#if UNITY_EDITOR

        protected override void OnValidate()
        {
            if (!m_IsRuntiming) return;
            base.OnValidate();
        }

#endif

        private void InitDragPass(UIBaseRender baseRender)
        {
            IDragEventPass[] dragEventPass = baseRender.GetComponentsInChildren<IDragEventPass>();
            for (int i = 0; i < dragEventPass.Length; i++)
            {
                var scrollRectrDragPass = dragEventPass[i].dragEventPassTarget.GetOrAddComponent<UScrollRectDragPass>();
                scrollRectrDragPass.ParentScrollRect = this;
            }
        }

        private void DisplayPrefabLoadedCallback(GameObject go, IDisplayUnit displayUnit)
        {
            if (displayUnit == null) return;
            UListDisplayUnit displayBehaviourUnit = displayUnit as UListDisplayUnit;
            if (displayUnit == null) return;
            displayBehaviourUnit.BindList = this;
            if (go == null) return;
            UIBaseRender baseRender = go.GetComponent<UIBaseRender>();
            InitDragPass(baseRender);
            RecordDisplayUnit2BaseRender(displayBehaviourUnit, baseRender);
            AddChild(baseRender.rectTransform);

            baseRender.IsSelect = m_LayoutManager.IsSelected(displayBehaviourUnit.Index);
            baseRender.AddSelectChanged((isSelect) =>
            {
                m_LayoutManager.SetItemSelect(displayBehaviourUnit.Index, isSelect);
            });
            baseRender.AddClick((data) =>
            {
                m_OnClick?.Invoke(displayBehaviourUnit.Index);
            });
            baseRender.AddPointerDown((data) =>
            {
                m_OnPointerDown?.Invoke(displayBehaviourUnit.Index);
            });
            baseRender.AddPointerUp((data) =>
            {
                m_OnPointerUp?.Invoke(displayBehaviourUnit.Index);
            });
        }

        private void AddChild(RectTransform rt)
        {
            if (!Application.isPlaying) return;
            rt.SetParent(contentRT, false);
            rt.SetAnchor(AnchorPresets.TopLeft); // 设置anchor
            //rt.pivot = new Vector2(0,1);
            rt.anchoredPosition = Vector2.zero;
        }

        private void ItemLoad(Action<IAutoLayoutItem> cb)
        {
            if (ListRenderPrefab != null && ListRenderPrefab.gameObject != null)
            {
                IDisplayUnit displayUnit = UIWindow.Ins.CreateDisplayUnitByPrefab(m_ListDisplayType, ListRenderPrefab.gameObject);
                if (displayUnit is UListDisplayUnit uListDisplayUnit)
                {
                    uListDisplayUnit.BindList = this;
                    uListDisplayUnit.SetListOption(ListOption);
                }
                DisplayPrefabLoadedCallback(displayUnit.DisplayGO, displayUnit);
                cb?.Invoke(displayUnit as IAutoLayoutItem);
            }
        }

        private void ItemPositionSet(IAutoLayoutItem item, Vector2 pos)
        {
            GetBaseRender(item).rectTransform.anchoredPosition = pos;
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
            var render = GetBaseRender(item);
            if (isEvent)
            {
                item.IsSelected = isSelect;
                if (isSelect)
                {

                    render?.DoSelect(true);
                    m_OnSelect?.Invoke(item.Index);
                }
                else if (!isSelect)
                {
                    render?.DoDeSelect(true);
                    m_OnDeSelect?.Invoke(item.Index);
                }
            }
            else
            {
                item.IsSelected = isSelect;
            }

        }
        public int GetInVisibleSetIndex(int visibleIndex)
        {
            return m_LayoutManager.LayoutData.visibleItemSet.IndexOf(visibleIndex);
        }
        private void ItemIndexSet(IAutoLayoutItem item, int index)
        {
            UListDisplayUnit displayUnit = item as UListDisplayUnit;
            displayUnit.Index = index;
        }

        private void ItemVisibleSet(IAutoLayoutItem item, bool visible)
        {
            UListDisplayUnit displayUnit = item as UListDisplayUnit;
            if (visible)
            {
                displayUnit?.Show();
                displayUnit.SetData(GetDataAt(displayUnit.Index));
            }
            else
            {
                displayUnit?.Hide();
            }
        }

        private void ItemNamSet(IAutoLayoutItem item, string name)
        {
            UListDisplayUnit listDisplayUnit = item as UListDisplayUnit;
            if (m_SyncRenderNameSync)
            {
                listDisplayUnit.DisplayGO.name = name;
            }
        }

        private void OnScrollValueChanged(Vector2 normalPosition)
        {
            ReCalculateView(GetClampedNormalizedPosition(normalPosition));
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
                RedirectAnchor();
            }
            m_LayoutManager.IsLayoutDirty = true;
        }
        private bool CheckSizeValidate()
        {
            Vector3 scale = transform.localScale;
            if (scale.y <= 0.001f || scale.x <= 0.001f || scale.z <= 0.001f)
            {
                return false;
            }
            return true;
        }
        private void RedirectAnchor()
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


        private void UpdateLayoutOption()
        {
            OnScrollValueChanged(GetClampedNormalizedPosition());
            m_LayoutData.itemSize = ItemSize;
            m_LayoutData.alignment = m_Alignment;
            m_LayoutData.padding = m_Padding;
            m_LayoutData.spacing = m_Spacing;
            m_LayoutData.corner = m_StartCorner;
            m_LayoutData.axis = m_Axis;
            m_LayoutData.axisFixedCount = m_FixedCount;
            m_LayoutData.layoutConstraint = m_Constraint;
            m_LayoutData.containerSize = m_ContentSizeCalculate;
            m_LayoutData.itemCount = DataSources.Count;
            m_LayoutData.viewSize = viewport.rect.size;

            m_LayoutData.customOffset = GetItemPivotOffset();
            m_LayoutManager.getRenderFromPool = m_GetItemFromPool;
            m_LayoutManager.allowMultiSelect = m_AllowMultiSelect;
            m_LayoutManager.allowSwitchOff = m_AllowSwitchOff;
            m_LayoutManager.loadingInterval = m_LoadingInterval;
            m_LayoutManager.LayoutEfficiency = m_LayoutEfficiency;
        }

        private Vector2 GetItemPivotOffset()
        {
            // 0.5 0.5 0 1 0.5 -0.5
            var rectTransform = ListRenderPrefab.rectTransform;
            Vector2 size = ItemSize;
            Vector2 pivotOffset = rectTransform.pivot - m_ChildPivot;
            return new Vector2(pivotOffset.x * size.x, pivotOffset.y * size.y);
        }

        public bool AllowMultiSelect
        {
            get { return m_AllowMultiSelect; }
            set { m_AllowMultiSelect = value; }
        }

        public bool AllowSwitchOff
        {
            get { return m_AllowSwitchOff; }
            set { m_AllowSwitchOff = value; }
        }

        protected override void LateUpdate()
        {
            if (!m_IsRuntiming) return;
            if (!isActiveAndEnabled) return;
            base.LateUpdate();
            if (m_ContentSizeDirty)
            {
                m_ContentSizeDirty = false;
                RefreshLayoutSize();
                RedirectAnchor();
            }
            if (m_LayoutManager.IsLayoutDirty && CheckSizeValidate())
            {
                RebuildLayoutImmediate();
            }
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

        private void ResetContentSize()
        {
            Vector2 newSize = m_ContentSizeCalculate;
            newSize.x = ResetAxisSize(m_XFlexSizeType, m_LayoutData.flexContentSize.x, m_ContentSizeCalculate.x);
            newSize.y = ResetAxisSize(m_YFlexSizeType, m_LayoutData.flexContentSize.y, m_ContentSizeCalculate.y);
            m_ContentSizeCalculate = newSize;
        }

        private void RefreshLayoutSize()
        {
            m_ContentSizeCalculate = viewRect.rect.size;
            UpdateLayoutOption();
            m_LayoutManager.CalculateLayout();
            ResetContentSize();
            ApplyContentSize();
        }

        private void RebuildLayoutImmediate()
        {
            m_LayoutManager.IsLayoutDirty = true;
            UpdateLayoutOption();
            m_LayoutManager.CalculateLayout();
            m_LayoutManager.UpdateLayout();
        }

        private void ApplyContentSize()
        {
            ContentSize = m_ContentSizeCalculate;
        }

        private Action<int> m_OnSelect;
        private Action<int> m_OnDeSelect;
        private Action<int> m_OnClick;
        private Action<int> m_OnPointerUp;
        private Action<int> m_OnPointerDown;
        private Action<int[]> m_OnSelectListChanged;

        public void AddSelect(Action<int> listener)
        {
            m_OnSelect -= listener;
            m_OnSelect += listener;
        }

        public void SetSelect(Action<int> listener)
        {
            m_OnSelect = listener;
        }

        public void RemoveSelect(Action<int> listener)
        {
            m_OnSelect -= listener;
        }

        public void AddClick(Action<int> listener)
        {
            m_OnClick -= listener;
            m_OnClick += listener;
        }

        public void SetClick(Action<int> listener)
        {
            m_OnClick = listener;
        }

        public void RemoveClick(Action<int> listener)
        {
            m_OnClick -= listener;
        }

        public void SetDeSelect(Action<int> listener)
        {
            m_OnDeSelect = listener;
        }

        public void AddDeSelect(Action<int> listener)
        {
            m_OnDeSelect -= listener;
            m_OnDeSelect += listener;
        }

        public void RemovePointerDown(Action<int> listener)
        {
            m_OnPointerDown -= listener;
        }

        public void SetPointerDown(Action<int> listener)
        {
            m_OnPointerDown = listener;
        }

        public void AddPointerDown(Action<int> listener)
        {
            m_OnPointerDown += listener;
        }

        public void RemovePointerUp(Action<int> listener)
        {
            m_OnPointerUp -= listener;
        }

        public void SetPointerUp(Action<int> listener)
        {
            m_OnPointerUp = listener;
        }

        public void AddPointerUp(Action<int> listener)
        {
            m_OnPointerUp += listener;
        }

        public void RemoveDeSelect(Action<int> listener)
        {
            m_OnDeSelect -= listener;
        }

        public void AddSelectListChange(Action<int[]> listener)
        {
            m_OnSelectListChanged += listener;
        }

        public void RemoveSelectListChange(Action<int[]> listener)
        {
            m_OnSelectListChanged -= listener;
        }

        public object GetData(int index)
        {
            if (DataSources != null && DataSources.Count > index)
            {
                return DataSources[index];
            }

            return null;
        }
    }

    #region define

    [Serializable]
    public class Padding
    {
        public float left;
        public float top;
        public float right;
        public float bottom;
    }

    public enum Constraint
    {
        AutoExpand, // 自动扩充容器
        FixedSize, //固定画布大小,超过就自动换行
        FixedCount // 固定数量, 会自动换行并扩充容器
    }

    public enum Axis
    {
        Horizontal = 0,
        Vertical = 1
    }

    public enum Corner
    {
        UpperLeft = 1,
        UpperRight = 2,
        LowerLeft = 3,
        LowerRight = 4
    }

    public enum ChildAlignment
    {
        UpperLeft = 0,
        UpperCenter = 1,
        UpperRight = 2,
        MiddleLeft = 3,
        MiddleCenter = 4,
        MiddleRight = 5,
        LowerLeft = 6,
        LowerCenter = 7,
        LowerRight = 8
    }

    #endregion define
}