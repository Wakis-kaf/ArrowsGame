using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Framework.Runtime.UI
{
    /// <summary>
    /// 能够限制面板的拖动,自动滚动到某个位置, 监听事件
    /// </summary>
    public class UContainer : ScrollRect, IDragEventPass
    {
        private const float scrollBarHideTolerence = 0.001f;

        [Header("水平滚动条设置")][SerializeField] private bool m_HorizontalScrollBarShow = true; // 是否显示水平滚动条
        [SerializeField] private bool m_HorizontalAutoHide = true;
        [SerializeField] private float m_HorizontalAutoHideTimer = 0.3f;
        [SerializeField] private float m_HorizontalShowDuration = 0.1f;
        [SerializeField] private float m_HorizontalHideDuration = 0.1f;
        [Header("垂直滚动条设置")][SerializeField] private bool m_VerticalScrollBarShow = true; // 是否显示垂直滚动条
        [SerializeField] private bool m_VerticalAutoHide = true;
        [SerializeField] private float m_VerticalAutoHideTimer = 0.3f;
        [SerializeField] private float m_VerticalShowDuration = 0.1f;
        [SerializeField] private float m_VerticalHideDuration = 0.1f;
        [Header("滚动条吸附阈值")][SerializeField] private float m_AutoTolerance = 0.1f; // 移动到顶部或者底部吸附阈值
        [SerializeField]
        private bool m_EnableDragEventPass = false;

        private UScrollRectDragPass m_UScrollRectDragPass;
        public UScrollRectDragPass DragPass
        {
            get
            {
                if(m_UScrollRectDragPass == null)
                {
                    m_UScrollRectDragPass = gameObject.GetOrAddComponent<UScrollRectDragPass>();
                }
                return m_UScrollRectDragPass;
            }
        }
        public bool HorizontalScrollBarShow
        {
            get { return m_HorizontalScrollBarShow; }
            set
            {
                m_HorizontalScrollBarShow = value;
                if (m_HorizontalScrollBarShow)
                    ShowHorizontalScrollBar();
                else
                {
                    HideHorizontalScrollBar();
                }
            }
        }

        public bool VerticalScrollBarShow
        {
            get { return m_HorizontalScrollBarShow; }
            set
            {
                m_VerticalScrollBarShow = value;
                if (m_VerticalScrollBarShow)
                    ShowVerticalScrollBar();
                else
                {
                    HideVerticalScrollBar();
                }
            }
        }

        public float AutoTolerance
        {
            get => m_AutoTolerance;
            set => m_AutoTolerance = value;
        }

        private RectTransform m_ContentRT;
        private RectTransform m_RectTransform;
        private bool m_IsDrag;
        private Vector2 m_LastNormalPos;
        
        public Vector2 ContentSize
        {
            get { return contentRT.rect.size; }
            set
            {
                if (contentRT.rect.size != value)
                {
                    contentRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value.x);
                    contentRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value.y);
                }
            }
        }

        public Vector2 Size
        {
            get => RectTransform.sizeDelta;
            set
            {
                RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value.x);
                RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value.y);
            }
        }

        public RectTransform contentRT
        {
            get
            {
                if (m_ContentRT == null)
                    m_ContentRT = content;
                return m_ContentRT;
            }
        }

        public RectTransform RectTransform
        {
            get
            {
                if (m_RectTransform == null)
                {
                    m_RectTransform = GetComponent<RectTransform>();
                }

                return m_RectTransform;
            }
        }

        public bool HorizontalDrag
        {
            get { return horizontal; }
            set { horizontal = value; }
        }

        public bool VerticalDrag
        {
            get { return horizontal; }
            set { horizontal = value; }
        }

        private Tweener horTweener;
        private Tweener verTweener;
        private Vector2 scrollBarTimer;
        private bool m_EnableDrag = true;
        private bool m_IsDestroying = false;
        private bool m_UpdateScrollBar = true;

        public bool EnableDrag => m_EnableDrag;
        
        //private GameObject m_DragEventPassTarget;
        public bool enableDragEventPass { get => m_EnableDragEventPass; set => m_EnableDragEventPass = value; }
        public GameObject dragEventPassTarget { get => gameObject; set { } }

        protected override void Awake()
        {
            base.Awake();
            //m_DragEventPassTarget = gameObject;
            m_LastNormalPos = normalizedPosition;
            HorizontalScrollBarShow = m_HorizontalScrollBarShow;
            VerticalScrollBarShow = m_VerticalScrollBarShow;
            onValueChanged.AddListener(OnScrollBarValueChange);
        }

        private void OnScrollBarValueChange(Vector2 normalPos)
        {
            m_UpdateScrollBar = true;
        }

        protected override void LateUpdate()
        {
            if (m_IsDestroying) return;
            if (!isActiveAndEnabled) return;
            base.LateUpdate();
            
            if (m_UpdateScrollBar)
            {
                bool xUpdate = true;
                bool yUpdate = true;
                Vector2 normalPos = normalizedPosition;
                if (m_HorizontalAutoHide)
                {
                    if (Math.Abs(normalPos.x - m_LastNormalPos.x) > GetScrollBarHideTolerence(0))
                    {
                        scrollBarTimer.x = Time.time;
                    }  
                    if (Time.time - scrollBarTimer.x > m_HorizontalAutoHideTimer)
                    {
                        HorizontalScrollBarShow = false;
                        xUpdate = false;
                    }
                    else
                    {
                        HorizontalScrollBarShow = true;

                    }
                }

                if (m_VerticalAutoHide)
                {
                    if ((Math.Abs(normalPos.y - m_LastNormalPos.y) > GetScrollBarHideTolerence(1)))
                    {
                        scrollBarTimer.y = Time.time;
                    }
                    if (Time.time - scrollBarTimer.y > m_VerticalAutoHideTimer)
                    {
                        VerticalScrollBarShow = false;
                        yUpdate = false;
                    }
                    else
                    {
                        VerticalScrollBarShow = true;

                    }
                }
                m_LastNormalPos = normalPos;
                if (!xUpdate && !yUpdate)
                {
                    m_UpdateScrollBar = false;
                }
                
            }
        }

        protected virtual float GetScrollBarHideTolerence(int axis)
        {
            return scrollBarHideTolerence;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            m_IsDestroying = true;
            StopMove();
            horTweener?.Kill(true);
            verTweener?.Kill(true);
            this.DOKill(true);
            horTweener = null;
            verTweener = null;
        }

        /// <summary>
        /// 使得水平和垂直滚动条移动到某个位置
        /// </summary>
        /// <param name="position"></param>
        public void MoveToDirectly(Vector2 position)
        {
            normalizedPosition = position;
        }

        public void MoveTo(Vector2 position, float duration = 0.5f)
        {
            MoveTo(position.x, position.y, duration);
        }

        public void MoveTo(float horNormalPosition, float verNormalPosition, float duration = 0.5f)
        {
            horTweener?.Kill();
            verTweener?.Kill();
            if (duration == 0)
            {
                HorziontalDTSetter(horNormalPosition);
                VerticalDTSetter(verNormalPosition);
            }
            else
            {
                horTweener = DOTween.To(HorziontalDTGetter, HorziontalDTSetter,
                    horNormalPosition, duration);
                verTweener = DOTween.To(VerticalDTGetter, VerticalDTSetter,
                    verNormalPosition, duration);
            }
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (!EnableDrag) return;
            base.OnBeginDrag(eventData);
            if (m_EnableDragEventPass)
            {
                DragPass.OnBeginDrag(eventData);
            }
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (!EnableDrag)
            {
                OnEndDrag(eventData);
                return;
            }
            
            base.OnDrag(eventData);
            m_IsDrag = true;
            if (m_EnableDragEventPass)
            {
                DragPass.OnDrag(eventData);
            }
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);
            m_IsDrag = false;
            if (m_EnableDragEventPass)
            {
                DragPass.OnEndDrag(eventData);
            }
        }

        private float HorziontalDTGetter()
        {
            return horizontalNormalizedPosition;
        }

        private void HorziontalDTSetter(float x)
        {
            horizontalNormalizedPosition = x;
            
        }

        private float VerticalDTGetter()
        {
            return verticalNormalizedPosition;
        }

        private void VerticalDTSetter(float y)
        {
            verticalNormalizedPosition = y;
            
        }

        public void ShowHorizontalScrollBar()
        {
            (horizontalScrollbar as UScrollbar)?.ShowSmooth(m_HorizontalShowDuration);
        }

        public void HideHorizontalScrollBar()
        {
            (horizontalScrollbar as UScrollbar)?.HideSmooth(m_HorizontalHideDuration);
        }

        public void ShowVerticalScrollBar()
        {
            (verticalScrollbar as UScrollbar)?.ShowSmooth(m_VerticalShowDuration);
        }

        public void HideVerticalScrollBar()
        {
            (verticalScrollbar as UScrollbar)?.HideSmooth(m_VerticalHideDuration);
        }

#if UNITY_EDITOR

        protected override void OnValidate()
        {
            base.OnValidate();
            if (!Application.isPlaying) return;
            if (m_HorizontalScrollBarShow)
                ShowHorizontalScrollBar();
            else HideHorizontalScrollBar();
            if (m_VerticalScrollBarShow)
                ShowVerticalScrollBar();
            else HideVerticalScrollBar();
        }

#endif

        public void StopMove()
        {
            StopMovement();
            horTweener?.Kill();
            verTweener?.Kill();
        }

        public void MoveToEnd(bool force = false, float duration = 0.5f)
        {
            if (CanMoveToEnd(force))
                MoveTo(1, 0, duration);
        }

        private bool CanMoveToEnd(bool force)
        {
            if (gameObject == null) return false;
            if (contentRT.gameObject == null) return false;
            Vector2 norPos = m_LastNormalPos;
            float tolerance = m_AutoTolerance;
            if (force || (!m_IsDrag && Math.Abs(norPos.x - GetHorizontalMax()) < tolerance &&
                          Math.Abs(norPos.y - GetVerticalMax()) < tolerance))
            {
                return true;
            }

            return false;
        }

        private float GetHorizontalMax()
        {
            return contentRT.rect.size.x <= viewRect.rect.size.x ? 0 : 1;
        }

        private float GetHorizontalMin()
        {
            return 0;
        }

        private float GetVerticalMax()
        {
            return 0;
        }

        private float GetVerticalMin()
        {
            return contentRT.rect.size.y <= viewRect.rect.size.y ? 0 : 1;
        }

        public void MoveToStart(bool force = true, float duration = 0.5f)
        {
            if (CanMoveToStart(force))
                MoveTo(0, 1, duration);
        }

        private bool CanMoveToStart(bool force)
        {
            Vector2 norPos = m_LastNormalPos;
            float tolerance = m_AutoTolerance;
            if (force || (!m_IsDrag && Math.Abs(norPos.x - GetHorizontalMin()) < tolerance &&
                          Math.Abs(norPos.y - GetVerticalMin()) < tolerance))
            {
                return true;
            }

            return false;
        }
    }
}