using Framework.Runtime.UI.UIAnimae;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Framework.Runtime.UI
{
    public class UPanel : MonoBehaviour
    {
        private UImage m_BgMask;
        private Canvas m_BgMaskCanvas;

        private Canvas m_Canvas;

        public UIAnimatorCaller showAnimCaller = new UIAnimatorCaller() { targetSequence = "ShowEffect" };
        public UIAnimatorCaller hideAnimCaller = new UIAnimatorCaller() { targetSequence = "HideEffect" };


        [SerializeField, LabelText("开启背景遮罩")]
        protected bool m_BgMaskEnable = true;
        [ShowIf("m_BgMaskEnable", true)]
        public Color bgMaskColor = new Color(0, 0, 0, 0.8f);
        [SerializeField, LabelText("显示背景遮罩")]
        protected bool m_IsShowBgMask = true;

        [SerializeField, LabelText("是否允许背景遮罩过渡")]
        protected bool m_IsEnableBgMaskTransition = false;
        [SerializeField, ShowIf("m_IsEnableBgMaskTransition")]
        protected Material m_BgMaskTransitionMat;
        [SerializeField, ShowIf("m_IsEnableBgMaskTransition")]
        protected float m_TransitionDuration = 0.2f;


        [SerializeField, LabelText("开启水滴屏放遮挡")]
        private bool m_EnableSaveArea = true;
        [SerializeField, LabelText("使用通用面板开启关闭特效")]
        private bool m_UseCommonVisibleEffect = true;
        [SerializeField, LabelText("是否开启通用缩放动画")]
        private bool m_UseCommonScaleEffect = false;


        [SerializeField, ShowIf("m_EnableSaveArea")]
        private RectTransform m_SafeArea;
        [SerializeField, ShowIf("m_EnableSaveArea")]
        private RectTransform m_UnSafeArea;
        [SerializeField, ShowIf("m_EnableSaveArea")]
        private Padding m_SafeAreaPadding;
        [SerializeField] private UButton m_CloseBtn;





        private int m_SortOrder;
        private RectTransform m_RectTransform;

        public bool UseCommonVisibleEffect
        {
            get => m_UseCommonVisibleEffect;
            set => m_UseCommonVisibleEffect = value;
        }
        public bool UseCommonScaleEffect
        {
            get => m_UseCommonScaleEffect;
            set => m_UseCommonScaleEffect = value;
        }

        private CircularTransitionController m_BgMaskTransController;
        public CircularTransitionController BgMaskTransController
        {
            get
            {
                if (m_BgMaskTransController == null && m_IsEnableBgMaskTransition
                    && BgMask != null)
                {
                    m_BgMaskTransController = BgMask.gameObject.GetOrAddComponent<CircularTransitionController>();
                    m_BgMaskTransController.SetMaterial(m_BgMaskTransitionMat);
                    m_BgMaskTransController.SetColor(bgMaskColor);
                }
                return m_BgMaskTransController;
            }
        }
        public UImage BgMask
        {
            get { return m_BgMask; }
            set
            {
                m_BgMask = value;

                if (m_IsShowBgMask && m_BgMaskEnable)
                {
                    ShowBgMask();
                }
                else
                {
                    HideBgMask();
                }
            }
        }

        public Canvas BgMaskCanvas
        {
            get
            {
                if (m_BgMaskCanvas == null && BgMask != null)
                {
                    m_BgMaskCanvas = BgMask.GetComponent<Canvas>();
                }

                return m_BgMaskCanvas;
            }
        }

        public Canvas Canvas
        {
            get
            {
                return m_Canvas;
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
                    Canvas.overrideSorting = true;
                    Canvas.sortingOrder = m_SortOrder;
                }
            }
        }

        public void Close()
        {
            PanelManager.Ins.ClosePanel(this);
        }

        public void DisableBgMask()
        {
            m_BgMaskEnable = false;
            HideBgMask();
        }

        public void EnableBgMask()
        {
            if (!m_BgMaskEnable) return;
            m_BgMaskEnable = true;
            if (m_IsShowBgMask)
            {
                ShowBgMask();
            }
        }

        public void HideBgMask()
        {
            m_IsShowBgMask = false;
            BgMask?.gameObject.SetActive(false);
        }
        public void ClearBgMask()
        {
            if (BgMask != null && BgMask.gameObject != null)
            {
                HideBgMask();
                Destroy(BgMask.gameObject);
            }
            m_BgMask = null;
        }

        public void SetLayer(int layer)
        {
            Canvas.overrideSorting = true;
            Canvas.sortingOrder = layer;
        }

        public void ShowBgMask()
        {
            if (m_BgMaskEnable)
            {
                m_IsShowBgMask = true;
                BgMask?.gameObject.SetActive(true);
                if (m_IsEnableBgMaskTransition)
                {
                    BgMaskTransController?.SetProgressImmediate(0);
                    BgMaskTransController?.PlayTransition(1f, m_TransitionDuration);
                }
            }
        }

        private void Awake()
        {
            m_CloseBtn?.AddClick(OnCloseClick);
            m_Canvas = gameObject.GetComponent<Canvas>();
            if (m_Canvas != null)
            {
                m_Canvas.overrideSorting = true;
                m_Canvas.sortingOrder = SortOrder;
            }
            ApplySafeArea();
        }
        void ApplySafeArea()
        {
            if (!m_EnableSaveArea) return;
            SetSafeArea();

        }
        private void SetSafeArea()
        {
            if (!m_EnableSaveArea) return;
            if (m_SafeArea == null) return;
            Rect safeArea = Screen.safeArea;
            Padding safeAreaPadding = m_SafeAreaPadding;
            float screeWidth = Screen.width;
            float screenHeight = Screen.height;
            float leftX = safeArea.position.x;
            float rightX = leftX + safeArea.width;
            float bottomY = safeArea.position.y;
            float topY = bottomY + safeArea.height;
            float scaleFactor = UIRoot.RootCanvas.scaleFactor;
            float paddingLeft = safeAreaPadding.left * scaleFactor;
            float paddingRight = safeAreaPadding.right * scaleFactor;
            float paddingTop = safeAreaPadding.top * scaleFactor;
            float paddingBottom = safeAreaPadding.bottom * scaleFactor;
            float leftOffset = leftX;
            float rightOffset = screeWidth - rightX;
            float topOffset = screenHeight - topY;
            float bottomOffset = bottomY;

            float leftOffsetMax = Mathf.Max(leftOffset, paddingLeft);
            float rightOffsetMax = Mathf.Max(rightOffset, paddingRight);
            float topOffsetMax = Mathf.Max(topOffset, paddingTop);
            float bottomOffsetMax = Mathf.Max(bottomOffset, paddingBottom);

            float sizeX = safeArea.width - Mathf.Max(leftOffsetMax - leftOffset, 0) - Mathf.Max(rightOffsetMax - rightOffset, 0);
            float sizeY = safeArea.height - Mathf.Max(bottomOffsetMax - bottomOffset, 0) - Mathf.Max(topOffsetMax - topOffset, 0);

            Vector2 newPos = new Vector2(leftOffsetMax, bottomOffsetMax);
            Vector2 newSize = new Vector2(sizeX, sizeY);

            Vector2 anchorMin = newPos;
            Vector2 anchorMax = newPos + newSize;

            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            m_SafeArea.anchorMin = anchorMin;
            m_SafeArea.anchorMax = anchorMax;

            if (m_UnSafeArea != null)
            {
                float maxOffset = Mathf.Max(leftOffset, rightOffset, topOffset, bottomOffset);
                if (maxOffset == leftOffset && leftOffset > 0)
                {
                    m_UnSafeArea.anchorMin = new Vector2(0, 0);
                    m_UnSafeArea.anchorMax = new Vector2(anchorMin.x, 1);
                }
                else if (maxOffset == rightOffset && rightOffset > 0)
                {
                    m_UnSafeArea.anchorMin = new Vector2(anchorMax.x, 0);
                    m_UnSafeArea.anchorMax = new Vector2(1, 1);
                }
                else if (maxOffset == bottomOffset && bottomOffset > 0)
                {
                    m_UnSafeArea.anchorMin = new Vector2(0, 0);
                    m_UnSafeArea.anchorMax = new Vector2(1, anchorMin.y);
                }
                else
                {
                    m_UnSafeArea.anchorMin = new Vector2(0, anchorMax.y);
                    m_UnSafeArea.anchorMax = new Vector2(1, 1);
                }
                m_UnSafeArea.offsetMin = Vector2.zero;
                m_UnSafeArea.offsetMax = Vector2.zero;
            }
        }

        private void OnCloseClick()
        {
            Close();
        }

    }
}