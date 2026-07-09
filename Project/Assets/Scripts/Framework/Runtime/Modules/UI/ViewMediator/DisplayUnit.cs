using Cysharp.Threading.Tasks;
using Framework.Runtime.Base;
using Framework.Runtime.Config;
using Framework.Runtime.MAsset;
using Framework.Runtime.Modules.UI.PrefabBind;
using Framework.Runtime.UI.UIAnimae;
using Framework.Runtime.UnitSystem.Base;
using Framework.Runtime.UnitSystem.BIInterfaces;
using Framework.Runtime.UnitSystem.MonoBase;
using Framework.Utils;
using Game.Modules;
using System;
using System.Drawing;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Framework.Runtime.UI
{
    public class DisplayUnit : UnitObject, IDisplayUnit, IMessageSubscriber
    {
        private int m_CurLayer = -1;
        private bool m_ActiveCheckIng = false;
        private GOAttributeCache<bool> m_ActiveSelf;
        private Canvas m_Canvas;
        private CanvasGroup m_CanvasGroup;
        private object m_Data = null;
        private bool m_DirtyDataGUIMute = false;
        private bool m_DisActiveCheckIng = false;
        private GameObject m_DisplayObj;
        private DisplayUnitListener m_DisplayUnitListener;
        private GraphicRaycaster m_GraphicRaycaster;
        private bool m_IsDirtyData;
        private bool m_IsGap = true;
        private bool m_IsHideEffectIng;
        private bool m_IsShowEffectIng;
        private bool m_IsLoading = false;
        private bool m_IsModelLoaded = false;

        private bool m_IsUIInited = false;
        private bool m_LastIsRealShow;
        private Action<IDisplayUnit> m_LoadedListeners;
        private GOAttributeCache<Vector3> m_LocalPos;
        private GOAttributeCache<Vector3> m_LocalRot;
        private GOAttributeCache<Vector3> m_LocalScale;
        private GOAttributeCache<Vector3> m_Rot;
        private GOAttributeCache<Transform> m_Parent;
        private GOAttributeCache<Vector3> m_Pos;
        private GOAttributeCache<Vector2> m_Size;
        private GOAttributeCache<Vector3> m_AnchorPos;
        private PrefabBinder m_PrefabBinder;
        private RectTransform m_RectTransform;

        private int m_SortOrder;
        private Transform m_Transform;
        private UIAnimator m_UiAnimator;
        public bool IsShowEffectIng
        {
            get => m_IsShowEffectIng;
        }
        public bool IsHideEffectIng
        {
            get => m_IsHideEffectIng;
        }
        public void RefreshLayout()
        {
            if (DisplayGO != null)
            {
                UIUtil.RefreshLayoutDelay(RectTransform);
            }
        }
        public DisplayUnit()
        {
            m_ActiveSelf = new GOAttributeCache<bool>(GOActiveSetter, GOActiveGetter);
            m_LocalPos = new GOAttributeCache<Vector3>(GOLocalPosSetter, GOLocalPosGetter);
            m_LocalRot = new GOAttributeCache<Vector3>(GOLocalRotSetter, GOLocalRotGetter);
            m_Pos = new GOAttributeCache<Vector3>(GOPosSetter, GOPosGetter);
            m_Size = new GOAttributeCache<Vector2>(GOSizeSetter, GOSizeGetter);
            m_AnchorPos = new GOAttributeCache<Vector3>(GOAnchorPosSetter, GOAnchorPosGetter);
            m_Rot = new GOAttributeCache<Vector3>(GORotSetter, GORotGetter);
            m_LocalScale = new GOAttributeCache<Vector3>(GOScaleSetter, GOScaleGetter);
            m_Parent = new GOAttributeCache<Transform>(ParentSetter, ParentGetter);
            OnInit();
        }

        protected delegate void ActiveUpdateComplete();
        private UIBaseRender m_BaseRender;
        public UIBaseRender UIBaseRender
        {
            get
            {
                if (m_BaseRender == null)
                {
                    m_BaseRender = DisplayGO?.GetComponent<UIBaseRender>();
                }
                return m_BaseRender;
            }
        }
        public IAssetVO AssetVo { get; set; }

        // 自动600秒未使用销毁
        public virtual float AutoDisposeTime { get; set; } = 600f;

        /// <summary>
        /// 视图优先级，相同优先级按照渲染顺序来决定，不同则按照从小到大一次排序
        /// </summary>
        /// <returns></returns>
        public virtual int GetVisiblePriority()
        {
            return 0;
        }
        public Canvas Canvas
        {
            get
            {
                if (IsAddCanvas && m_Canvas == null && IsModelLoaded)
                {
                    m_Canvas = DisplayGO.AddComponent<Canvas>();
                    ResetCanvas();
                }
                return m_Canvas;
            }
        }
        private bool m_IsAddCanvas = false;
        public bool IsAddCanvas
        {
            get
            {
                return m_IsAddCanvas;
            }
            set
            {
                m_IsAddCanvas = value;
                if (m_IsAddCanvas && m_Canvas == null && IsModelLoaded)
                {
                    m_Canvas = DisplayGO.AddComponent<Canvas>();
                    ResetCanvas();
                }
            }
        }

        public CanvasGroup CanvasGroup
        {
            get { return m_CanvasGroup; }
        }

        public int CurLayer
        {
            get => m_CurLayer;
            set => m_CurLayer = value;
        }

        public object Data
        {
            get { return m_Data; }
            set
            {
                if (IsUIInited && !m_DirtyDataGUIMute && IsRealShow)
                {
                    if (m_Data != value || IsDirtyData)
                    {
                        var oldData = m_Data;
                        FunctionUtility.SafeCall(OnGUIOld, m_Data);
                        m_Data = value;
                        FunctionUtility.SafeCall(OnGUI, m_Data);
                        FunctionUtility.SafeCall(OnDataChanged, oldData, m_Data);
                    }
                    else
                    {
                        FunctionUtility.SafeCall(OnGUI, value);
                    }
                    IsDirtyData = false;
                }
                else
                {
                    m_Data = value;
                    IsDirtyData = true;
                }
            }
        }

        public GameObject DisplayGO
        {
            get { return m_DisplayObj; }
        }

        public DisplayUnitListener DisplayUnitListener => m_DisplayUnitListener;

        public GraphicRaycaster GraphicRaycaster
        {
            get
            {
                if (m_GraphicRaycaster == null && DisplayGO != null)
                {
                    m_GraphicRaycaster = DisplayGO.GetOrAddComponent<GraphicRaycaster>();
                }

                return m_GraphicRaycaster;
            }
        }

        //public object InitOption
        //{
        //    get => m_InitOption;
        //    set => m_InitOption = value;
        //}

        public virtual bool IsActiveChangeParent { get; set; } = true;
        public virtual bool IsAutoDispose { get; set; } = true;
        public bool IsDirtyData { get => m_IsDirtyData; set => m_IsDirtyData = value; }

        public bool IsGap
        {
            get => m_IsGap;
            set => m_IsGap = value;
        }

        public bool IsLoading
        {
            get => m_IsLoading;
            set
            {
                m_IsLoading = value;
            }
        }

        public bool IsModelLoaded
        {
            get => m_IsModelLoaded;
        }

        public bool IsRaycastLocationValid1
        {
            get => DisplayUnitListener ? DisplayUnitListener.IsRaycastLocationValid1 : true;
            set
            {
                if (DisplayUnitListener)
                {
                    DisplayUnitListener.IsRaycastLocationValid1 = value;
                }
            }
        }

        public virtual bool IsRealShow
        {
            get
            {
                if (DisplayGO == null)
                {
                    return false;
                }
                return activeSelf && DisplayGO.activeInHierarchy;
            }
        }

        public virtual bool IsShow
        {
            get => activeSelf;
        }

        public bool IsUIInited { get => m_IsUIInited; }

        public Vector3 LocalPosition
        {
            get { return m_LocalPos.GetValue(DisplayGO); }
            set { m_LocalPos.SetValue(value, DisplayGO); }
        }

        public Vector3 LocalRotation
        {
            get { return m_LocalRot.GetValue(DisplayGO); }
            set { m_LocalRot.SetValue(value, DisplayGO); }
        }

        public Vector3 LocalScale
        {
            get { return m_LocalScale.GetValue(DisplayGO); }
            set { m_LocalScale.SetValue(value, DisplayGO); }
        }

        public Transform ParentTransform
        {
            get { return m_Parent.GetValue(DisplayGO); }
            set { m_Parent.SetValue(value, DisplayGO); }
        }
        public Vector2 Size
        {
            get { return m_Size.GetValue(DisplayGO); }
            set
            {
                m_Size.SetValue(value, DisplayGO);

            }
        }
        public Vector3 Position
        {
            get { return m_Pos.GetValue(DisplayGO); }
            set { m_Pos.SetValue(value, DisplayGO); }
        }
        public Vector3 AnchorPosition
        {
            get { return m_AnchorPos.GetValue(DisplayGO); }
            set { m_AnchorPos.SetValue(value, DisplayGO); }
        }

        public PrefabBinder PrefabBinder => m_PrefabBinder;
        public DisplayUnitReference m_DisplayUnitReference;
        public DisplayUnitReference DisplayUnitReference
        {
            get
            {
                if (m_DisplayUnitReference == null && DisplayGO != null)
                {
                    m_DisplayUnitReference = DisplayGO.GetOrAddComponent<DisplayUnitReference>();
                }
                return m_DisplayUnitReference;

            }
        }

        public RectTransform RectTransform
        {
            get
            {
                if (m_RectTransform == null)
                {
                    m_RectTransform = DisplayGO?.GetOrAddComponent<RectTransform>();
                }

                return m_RectTransform;
            }
        }

        public Vector3 Rotation
        {
            get { return m_Rot.GetValue(DisplayGO); }
            set { m_Rot.SetValue(value, DisplayGO); }
        }

        public Transform SelfTransform
        {
            get
            {
                if (m_Transform == null && DisplayGO != null)
                    m_Transform = DisplayGO.transform;
                return m_Transform;
            }
        }



        public int SortOrder
        {
            get => m_SortOrder;
            set
            {
                m_SortOrder = value;
                if (Canvas != null)
                {
                    if (m_EnableOverrideSorting)
                    {
                        Canvas.overrideSorting = true;
                        Canvas.sortingOrder = m_SortOrder;
                    }
                    else
                    {
                        Canvas.overrideSorting = false;
                    }

                }
                FunctionUtility.SafeCall(OnSortOrderReset, m_SortOrder);
            }
        }

        private bool m_EnableOverrideSorting = true;
        public bool IsEnableOverrideSorting
        {
            get
            {
                return m_EnableOverrideSorting;
            }
            set
            {
                m_EnableOverrideSorting = value;
                SortOrder = m_SortOrder;
            }
        }

        public UIAnimator UIAnimator => m_UiAnimator;

        private bool activeSelf
        {
            get { return m_ActiveSelf.GetValue(DisplayGO); }
            set { m_ActiveSelf.SetValue(value, DisplayGO); }
        }

        private bool activeWorld
        {
            get
            {
                if (DisplayGO == null) return false;
                return DisplayGO.activeInHierarchy;
            }
        }

        public bool IsActiveSubscriber => IsRealShow;

        public void AddLoadedCb(Action<IDisplayUnit> cb)
        {
            if (!IsLoading) cb?.Invoke(this);
            m_LoadedListeners += cb;
        }

        public virtual void CloseWindow()
        {
            UIWindow.Ins.Close(this);
        }

        public virtual void Destroy()
        {
            UIWindow.Ins.DestroyWindow(this);
        }

        public Vector2 GetAnchoredPosition()
        {
            return RectTransform.anchoredPosition;
        }
        public Vector2 GetPosition()
        {
            return RectTransform?.position ?? Vector3.zero;
        }

        public virtual string GetAssetLink(string outAssetLink)
        {
            return outAssetLink;
        }

        public T GetBindObject<T>(string key) where T : Object
        {
            if (PrefabBinder == null) return default;
            return PrefabBinder.GetObj<T>(key);
        }

        public Object GetBindObject(string key)
        {
            if (PrefabBinder == null) return default;
            return PrefabBinder.GetObj<Object>(key);
        }

        public T GetComponentInChildren<T>(string componentName)
        {
            return SelfTransform.GetComponentInChild<T>(componentName);
        }

        public Component GetComponentInChildren(Type componentType, string componentName)
        {
            return SelfTransform.GetComponentInChild(componentType, componentName);
        }

        public Component GetComponentInChildren(String componentTypeName, string componentName)
        {
            return SelfTransform.GetComponentInChild(componentTypeName, componentName);
        }

        public RectTransform GetLayerRectTransform()
        {
            return GetCurLayer().RectTransform;
        }
        public WindowLayer GetCurLayer(int notFindLayer = GlobalConstant.LAYER_SLIENCE)
        {
            return WindowLayerManager.Instance.FindLayer(m_CurLayer) ?? WindowLayerManager.Instance.FindLayer(notFindLayer);
        }
        public virtual int GetOpenLayer(int externalLayer)
        {
            return externalLayer;
        }

        public virtual void Hide()
        {
            if (activeSelf)
            {
                activeSelf = false;
            }
        }

        public virtual void OnDestroy()
        {
        }

        public virtual void OnOpenInLayer(int layer)
        {
        }

        public virtual void OnRemoveFromLayer(int layer)
        {
        }

        public virtual void OnSortOrderReset(int sortOrder)
        {
        }

        public virtual void OnUILoaded(GameObject gameObject)
        {
            m_IsModelLoaded = true;
            FunctionUtility.SafeCall(CheckUIInit, gameObject);
            if (!m_IsLoading)
            {
                m_LoadedListeners?.Invoke(this);
                m_LoadedListeners = null;
            }
        }

        public virtual void OpenWindow()
        {
            OpenWindowInLayer(CurLayer);
        }
        public virtual void OpenWindowInLayer(int layer)
        {
            UIWindow.Ins.OpenWindow(this, layer);
        }


        public void RemoveLoadedCb(Action<IDisplayUnit> cb)
        {
            m_LoadedListeners -= cb;
        }

        public Vector3 ScreenPointToLayerPoint(Vector2 screenPoint)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(GetLayerRectTransform(), screenPoint, UIRootCamera.Camera, out var point);
            return point;
        }

        public void SetAnchoredPosition(Vector3 anchorPos)
        {
            AnchorPosition = anchorPos;
        }
        public void SetPosition(Vector3 pos)
        {
            Position = pos;
        }
        public void SetRotation(Vector3 rot)
        {
            Rotation = rot;
        }
        public void SetLocalRotation(Vector3 rot)
        {
            LocalRotation = rot;
        }
        public void SetData(object data = null, bool guiMute = false)
        {
            m_DirtyDataGUIMute = guiMute;
            this.Data = data;
        }

        public void SetSortOrder(int layer)
        {
            SortOrder = layer;
        }

        public virtual void Show()
        {
            if (!activeSelf)
            {
                activeSelf = true;
            }
        }
        public void DispatchEevent(string messageId)
        {
            MessageDispatcher.Ins.Dispatch(messageId);
        }

        public void DispatchEevent<T>(string messageId, T arg1)
        {
            MessageDispatcher.Ins.Dispatch(messageId, arg1);
        }

        public void DispatchEevent<T, T2>(string messageId, T arg1, T2 arg2)
        {
            MessageDispatcher.Ins.Dispatch(messageId, arg1, arg2);
        }

        public void DispatchEevent<T, T2, T3>(string messageId, T arg1, T2 arg2, T3 arg3)
        {
            MessageDispatcher.Ins.Dispatch(messageId, arg1, arg2, arg3);
        }

        public void DispatchEevent<T, T2, T3, T4>(string messageId, T arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            MessageDispatcher.Ins.Dispatch(messageId, arg1, arg2, arg3, arg4);
        }

        public void SubscribeEvent(string messageId, Action callback)
        {
            MessageDispatcher.Ins.Subscribe(messageId, callback, this);
        }

        public void SubscribeEvent<T>(string messageId, Action<T> callback, IUnitObject caller = null)
        {
            MessageDispatcher.Ins.Subscribe(messageId, callback, this);
        }

        public void SubscribeEvent<T, T2>(string messageId, Action<T, T2> callback, IUnitObject caller = null)
        {
            MessageDispatcher.Ins.Subscribe(messageId, callback, this);
        }

        public void SubscribeEvent<T, T2, T3>(string messageId, Action<T, T2, T3> callback, IUnitObject caller = null)
        {
            MessageDispatcher.Ins.Subscribe(messageId, callback, this);
        }

        public void SubscribeEvent<T, T2, T3, T4>(string messageId, Action<T, T2, T3, T4> callback, IUnitObject caller = null)
        {
            MessageDispatcher.Ins.Subscribe(messageId, callback, this);
        }

        public void UnsubscribeEvent(string messageId, Action callback)
        {
            MessageDispatcher.Ins.Subscribe(messageId, callback, this);
        }

        public void UnsubscribeEvent<T>(string messageId, Action<T> callback)
        {
            MessageDispatcher.Ins.Subscribe(messageId, callback, this);
        }

        public void UnsubscribeEvent<T, T2>(string messageId, Action<T, T2> callback)
        {
            MessageDispatcher.Ins.Subscribe(messageId, callback, this);
        }

        public void UnsubscribeEvent<T, T2, T3>(string messageId, Action<T, T2, T3> callback)
        {
            MessageDispatcher.Ins.Subscribe(messageId, callback, this);
        }

        public void UnsubscribeEvent<T, T2, T3, T4>(string messageId, Action<T, T2, T3, T4> callback)
        {
            MessageDispatcher.Ins.Subscribe(messageId, callback, this);
        }

        public void UpdateGUI()
        {
            this.SetData(this.Data);
        }

        protected virtual void AutoExtractPrefabBinderComponent(PrefabBinder prefabBinder)
        {
        }

        protected override void DisposeManagedResources()
        {
            m_ActiveSelf?.Clear();
            m_LocalPos?.Clear();
            m_LocalRot?.Clear();
            m_AnchorPos?.Clear();
            m_Pos?.Clear();
            m_Rot?.Clear();
            m_Size?.Clear();
            m_LocalScale?.Clear();
            m_UiAnimator = null;
            m_PrefabBinder = null;
            m_Data = null;
            base.DisposeManagedResources();
        }

        protected override void DisposeUnManagedResources()
        {
            OnDestroy();
            m_PrefabBinder?.Clear();
            m_UiAnimator?.Clear();
            AssetVo?.UnLoadAsync();
            MessageDispatcher.Ins?.UnsubscribeAll(this);
            base.DisposeUnManagedResources();
        }

        protected virtual void ExtractComponent(GameObject go)
        {
            m_PrefabBinder = go.GetComponent<PrefabBinder>();
            if (m_PrefabBinder != null)
            {
                this.AutoExtractPrefabBinderComponent(m_PrefabBinder);
            }
            m_UiAnimator = go.GetComponent<UIAnimator>();
            m_CanvasGroup = go.GetComponent<CanvasGroup>();
            m_Transform = go.transform;
            m_RectTransform = go.GetOrAddComponent<RectTransform>();
            m_Canvas = IsAddCanvas ? go.GetOrAddComponent<Canvas>() : go.GetComponent<Canvas>();
            ResetCanvas();
        }
        private void ResetCanvas()
        {
            if (m_Canvas != null)
            {
                m_GraphicRaycaster = DisplayGO.GetOrAddComponent<GraphicRaycaster>();
                m_Canvas.overrideSorting = true;
            }
        }

        protected virtual void OnClearHideEffect()
        {
        }

        protected virtual void OnClearShowEffect()
        {
        }

        protected virtual void OnDataChanged(object oldData, object newData)
        {
        }

        protected virtual void OnGODestroy()
        {
            // 回收当前内存
            this.Dispose();
        }

        /// <summary>
        /// 当预知体加载完成且数据加载完成之后回调
        /// </summary>
        /// <param name="data"></param>
        protected virtual void OnGUI(object data)
        {
        }

        protected virtual void OnGUIOld(object oldData)
        {
        }

        protected virtual void OnHide()
        {
        }

        protected virtual void OnInit()
        {
        }

        protected virtual void OnSubscribeMessages()
        {

        }
        protected virtual void OnInitUI()
        {
        }

        protected virtual void OnShow()
        {
        }

        protected virtual void OnStartHideEffect(Action hideCompleted)
        {
            DisvisibleUI();
            hideCompleted?.Invoke();
        }

        protected virtual void OnStartShowEffect(Action ShowCompleteCb)
        {
            VisibleUI();
            ShowCompleteCb?.Invoke();
        }
        protected void DisvisibleUI()
        {
            if (CanvasGroup)
            {
                CanvasGroup.alpha = 0;
                CanvasGroup.blocksRaycasts = false;
                CanvasGroup.interactable = false;

            }
            else
            {
                GameObjectUtil.SetActive(DisplayGO, false);

            }
        }
        protected void VisibleUI()
        {
            if (CanvasGroup)
            {
                CanvasGroup.alpha = 1;
                CanvasGroup.blocksRaycasts = true;
                CanvasGroup.interactable = true;

            }
            else
            {
                GameObjectUtil.SetActive(DisplayGO, true);

            }
        }
        private void ApplyObjProperty(GameObject go)
        {
            m_LocalPos.UpdateValue(go);
            m_LocalRot.UpdateValue(go);
            m_AnchorPos.UpdateValue(go);
            m_Pos.UpdateValue(go);
            m_Size.UpdateValue(go);
            m_Rot.UpdateValue(go);
            m_LocalScale.UpdateValue(go);
            m_Parent.UpdateValue(go);
            m_ActiveSelf.UpdateValue(go);
        }

        private void CheckActive()
        {
            if (m_ActiveCheckIng) return;
            m_ActiveCheckIng = true;
            if (activeSelf && !DisplayGO.activeSelf)
            {
                GameObjectUtil.SetActive(DisplayGO, true);
                //DisplayGO.SetActive(true);
            }
            if (IsRealShow)
            {
                CheckInitUI();
                ClearHideEffect();
                if (!m_IsShowEffectIng)
                {
                    m_IsShowEffectIng = true;
                    OnStartShowEffect(OnShowEffectComplete);
                    if (IsRealShow)
                    {
                        DoShow();
                    }
                }
            }
            m_ActiveCheckIng = false;
        }
        private float m_AutoCloseTime = -1;
        private float m_RemainingTime;
        private float m_TotalTime;
        public void SetAutoCloseTime(float autoCloseTime)
        {
            m_AutoCloseTime = autoCloseTime;
        }
        protected virtual float GetAutoCloseTime()
        {
            return m_AutoCloseTime;
        }
        protected virtual void DoHide()
        {
            StopAutoCloseTimer();
            OnHide();
        }
        protected virtual void DoShow()
        {
            StartAutoCloseTimer();
            OnShow();
            SetData(Data);

        }
        protected void StartAutoCloseTimer()
        {
            StopAutoCloseTimer();
            float autoCloseTime = GetAutoCloseTime();
            if (autoCloseTime <= 0) return;
            m_TotalTime = m_RemainingTime = autoCloseTime;
            GameApp.Ins.LoopManager.AddLoop(OnAutoCloseTick);
        }
        private void OnAutoClose()
        {
            StopAutoCloseTimer();
            CloseWindow();
        }
        protected virtual void OnAutoCloseTimeTick(float autoCloseRemainTime)
        {

        }
        private void OnAutoCloseTick()
        {
            m_RemainingTime -= Time.deltaTime;
            m_RemainingTime = Mathf.Max(0, m_RemainingTime);
            OnAutoCloseTimeTick(m_RemainingTime);
            if (m_RemainingTime <= 0)
            {
                OnAutoClose();
            }
        }
        private void StopAutoCloseTimer()
        {
            GameApp.Ins.LoopManager.RemoveLoop(OnAutoCloseTick);
        }
        private void CheckDisActive()
        {
            if (m_DisActiveCheckIng) return;
            m_DisActiveCheckIng = true; ;
            if (!activeSelf && activeWorld)
            {
                ClearShowEffect();
                if (!m_IsHideEffectIng)
                {
                    m_IsHideEffectIng = true;
                    OnStartHideEffect(OnHideEffectComplete);
                }
            }
            if (activeSelf && !activeWorld)
            {
                ClearShowEffect();
                OnHideEffectComplete();
            }
            m_DisActiveCheckIng = false;
        }

        protected virtual void CheckInitUI()
        {
            if (m_IsUIInited) return;
            m_IsUIInited = true;
            OnInitUI();
            OnSubscribeMessages();


        }

        private void CheckUIInit(GameObject gameObject)
        {
            GameObjectUtil.SetActive(gameObject, true);
            //gameObject.SetActive(true);
            m_DisplayObj = gameObject;
            ExtractComponent(DisplayGO);
            ApplyObjProperty(DisplayGO);
            GameObjectUtil.SetActive(gameObject, activeSelf);
            //gameObject.SetActive(activeSelf);
            //CheckActive();
            m_DisplayUnitListener = DisplayGO.GetOrAddComponent<DisplayUnitListener>();
            m_DisplayUnitListener.SetOnShow(CheckActive);
            m_DisplayUnitListener.SetOnHide(CheckDisActive);
            m_DisplayUnitListener.SetOnDestroy(OnGODestroy);
        }

        private void ClearHideEffect()
        {
            if (m_IsHideEffectIng)
            {
                m_IsHideEffectIng = false;
                OnClearHideEffect();
            }
        }

        private void ClearShowEffect()
        {
            if (m_IsShowEffectIng)
            {
                m_IsShowEffectIng = false;
                OnClearShowEffect();
            }
        }

        private bool GOActiveGetter(GameObject go)
        {
            return m_ActiveSelf.value;
        }

        private void GOActiveSetter(GameObject go, bool active, bool changed)
        {
            if (active)
            {
                CheckActive();
            }
            else
            {
                CheckDisActive();
            }
        }

        private Vector3 GOLocalPosGetter(GameObject go)
        {
            return SelfTransform.localPosition;
        }

        private void GOLocalPosSetter(GameObject go, Vector3 value, bool changed)
        {
            SelfTransform.localPosition = value;
        }

        private Vector3 GOLocalRotGetter(GameObject go)
        {
            return SelfTransform.localEulerAngles;
        }

        private void GOLocalRotSetter(GameObject go, Vector3 value, bool changed)
        {
            SelfTransform.localEulerAngles = value;
        }

        private Vector3 GOPosGetter(GameObject go)
        {
            return SelfTransform.position;
        }

        private void GOPosSetter(GameObject go, Vector3 value, bool changed)
        {
            SelfTransform.position = value;
        }
        private Vector2 GOSizeGetter(GameObject go)
        {
            return RectTransform.sizeDelta;
        }

        private void GOSizeSetter(GameObject go, Vector2 value, bool changed)
        {
            RectTransform?.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value.x);
            RectTransform?.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value.y);
        }

        private Vector3 GORotGetter(GameObject go)
        {
            return SelfTransform.eulerAngles;
        }
        private void GOAnchorPosSetter(GameObject go, Vector3 value, bool changed)
        {
            RectTransform.anchoredPosition3D = value;
        }

        private Vector3 GOAnchorPosGetter(GameObject go)
        {
            return RectTransform.anchoredPosition3D;
        }

        private void GORotSetter(GameObject go, Vector3 value, bool changed)
        {
            SelfTransform.eulerAngles = value;
        }

        private Vector3 GOScaleGetter(GameObject go)
        {
            return SelfTransform.localScale;
        }

        private void GOScaleSetter(GameObject go, Vector3 value, bool changed)
        {
            SelfTransform.localScale = value;
        }

        private void OnHideEffectComplete()
        {
            m_IsHideEffectIng = false;
            DoHide();
        }

        private void OnShowEffectComplete()
        {
            m_IsShowEffectIng = false;
            if (IsRealShow)
            {
                SetData(Data);
            }
        }

        private Transform ParentGetter(GameObject go)
        {
            return go.transform.parent;
        }

        private void ParentSetter(GameObject go, Transform parent, bool changed)
        {
            go.transform.SetParent(parent);
        }
        public void SetActive(GameObject go, bool active)
        {
            GameObjectUtil.SetActive(go, active);
        }
        public void SetActive(Component cmp, bool active)
        {
            GameObjectUtil.SetActive(cmp, active);
        }
    }

    public class DisplayUnitListener : MonoEvents, ICanvasRaycastFilter
    {
        private bool m_IsRaycastLocationValid = true;

        //默认射线不能穿透物品
        public bool IsRaycastLocationValid1
        {
            get => m_IsRaycastLocationValid;
            set => m_IsRaycastLocationValid = value;
        }

        public void DestroySelf()
        {
            GameObject.Destroy(gameObject);
        }

        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            return IsRaycastLocationValid1;
        }
    }
}