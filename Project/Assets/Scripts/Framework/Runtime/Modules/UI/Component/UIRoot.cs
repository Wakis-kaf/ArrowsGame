using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Framework.Runtime.UI
{
    public enum UIMode
    {
        Phone,
        Pc
    }

    /// <summary>
    /// UI 的根组件
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasScaler))]
    [RequireComponent(typeof(GraphicRaycaster))]
    [AddComponentMenu("UnitUI/UIRoot")]
    public class UIRoot : MonoBehaviour
    {
        public Vector2 DefaultPcSize = new Vector2(1920, 1080);

        public Vector2 DefaultPhoneSize = new Vector2(750, 1334);

        [OnValueChanged("OnUIModeChanged")]
        public UIMode uiMode = UIMode.Pc;

        private static CanvasScaler m_CanvasScaler;

        private static Vector2 m_DesignSize;

        private static UIRoot m_Instance;

        private static RectTransform m_RectTransform;

        // 默认尺寸
        private static UIRoot m_Root;

        private static UIRootCamera m_RootCamera;

        private static Canvas m_RootCanvas;

        private static Transform m_Transform;
        public static bool IsPcUI()
        {
            return Instance.uiMode == UIMode.Pc;
        }
        public static bool IsPhoneUI()
        {
            return Instance.uiMode == UIMode.Phone;
        }
        public static CanvasScaler CanvasScaler
        {
            get
            {
                if (m_CanvasScaler == null)
                {
                    m_CanvasScaler = Root.GetComponent<CanvasScaler>();
                }

                return m_CanvasScaler;
            }
        }

        /// <summary>
        /// 画布尺寸
        /// </summary>
        public static Vector2 CanvasSize => m_DesignSize;

        public static UIRoot Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = FindObjectOfType<UIRoot>();
                }
                return m_Instance;
            }
            set => m_Instance = value;
        }

        // 根节点
        public static UIRoot Root
        {
            get
            {
                if (m_Root == null)
                    m_Root = FindObjectOfType<UIRoot>();
                if (UIRoot.Instance == null && m_Root != null)
                {
                    UIRoot.Instance = m_Root;
                }
                if (m_Root == null)
                {
                    CheckRoot();
                }
                CheckCanvas();
                CheckRootCamera();

                return m_Root;
            }
            set { m_Root = value; }
        }

        public static UIRootCamera RootCamera
        {
            get
            {
                if (m_RootCamera == null)
                {
                    m_RootCamera = FindObjectOfType<UIRootCamera>();
                }

                return m_RootCamera;
            }
            set { m_RootCamera = value; }
        }

        public static Canvas RootCanvas
        {
            get
            {
                if (m_RootCanvas == null)
                {
                    m_RootCanvas = Root.GetComponent<Canvas>();
                }

                return m_RootCanvas;
            }
        }

        public static RectTransform RootRectTransform
        {
            get
            {
                if (m_RectTransform == null)
                {
                    m_RectTransform = Root.GetComponent<RectTransform>();
                }

                return m_RectTransform;
            }
        }

        public static Transform RootTransform
        {
            get
            {
                if (m_Transform == null)
                {
                    m_Transform = Root.transform;
                }

                return m_Transform;
            }
        }

        public static Vector2 ScreenSize => new Vector2(Screen.width, Screen.height);

        /// <summary>
        /// 获取屏幕宽高比
        /// </summary>
        /// <returns></returns>
        public float GetScreenAspectRatio()
        {
            return Screen.width / Screen.height;
        }

        public static void CheckCanvas()
        {
            if (m_RootCanvas == null)
            {
                m_RootCanvas = m_Root.GetComponent<Canvas>();
                m_RootCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                m_RootCanvas.worldCamera = UIRootCamera.Camera;
            }

            if (m_CanvasScaler == null)
            {
                m_CanvasScaler = m_Root.GetComponent<CanvasScaler>();
                m_CanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                ResetCanvasResolution();
            }
        }

        private static UIRoot CheckRoot()
        {
            if (m_Root == null)
            {
                m_Root = FindObjectOfType<UIRoot>();
                if (m_Root == null)
                {
                    m_Root = new GameObject("UIRoot").AddComponent<UIRoot>();
                }
                UIRoot.Instance = m_Root;

                m_Root.gameObject.layer = 5;
                m_Root.name = "UIRoot";
            }

            return m_Root;
        }

        private static UIRootCamera CheckRootCamera()
        {
            if (m_RootCamera == null)
            {
                m_RootCamera = FindObjectOfType<UIRootCamera>();
                if (m_RootCamera == null)
                {
                    m_RootCamera = new GameObject("UIRootCamera").AddComponent<UIRootCamera>();
                }

                m_RootCamera.name = "UIRootCamera";
                m_RootCamera.transform.SetParent(m_Root.transform, false);
            }

            return UIRootCamera.Root;
        }

        private static void ResetCanvasResolution()
        {
            if (UIRoot.Instance.uiMode == UIMode.Phone)
            {
                CanvasScaler.referenceResolution = UIRoot.Instance.DefaultPhoneSize;
            }
            else
            {
                CanvasScaler.referenceResolution = UIRoot.Instance.DefaultPcSize;
            }
            m_DesignSize = CanvasScaler.referenceResolution;
        }

        private void Awake()
        {
            UIRoot uiRoot = FindObjectOfType<UIRoot>();
            if (uiRoot != this)
            {
                Destroy(uiRoot.gameObject);
                return;
            }
            m_Root = uiRoot;
            DontDestroyOnLoad(gameObject);
            CheckCanvas();
        }
        private void OnDestroy()
        {
            if (m_Instance != this) return;
            m_Root = null;
            m_RootCamera = null;
            m_RootCanvas = null;
            m_CanvasScaler = null;
            m_RectTransform = null;
            m_Transform = null;
            m_Instance = null;
        }

        private void OnDisable()
        {
        }

        private void OnEnable()
        {
        }

        private void OnUIModeChanged()
        {
            ResetCanvasResolution();
        }
    }
}