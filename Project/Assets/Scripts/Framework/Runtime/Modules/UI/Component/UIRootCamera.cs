using UnityEngine;

namespace Framework.Runtime.UI
{
    [RequireComponent(typeof(Camera))]
    public class UIRootCamera : MonoBehaviour
    {
        private static Camera m_Camera;
        private static RectTransform m_RectTransform;
        private static UIRootCamera m_Root;
        private static Transform m_Transform;

        public static Camera Camera
        {
            get
            {
                CheckRoot();
                CheckRootCamera();
                return m_Camera;
            }
        }

        public static UIRootCamera Root
        {
            get
            {
                CheckRoot();
                CheckRootCamera();
                return m_Root;
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

        private static UIRootCamera CheckRoot()
        {
            if (m_Root == null)
            {
                m_Root = FindObjectOfType<UIRootCamera>();
                if (m_Root == null)
                {
                    m_Root = new GameObject("UIRootCamera").AddComponent<UIRootCamera>();
                }

                m_Root.name = "UIRootCamera";
                m_Root.gameObject.layer = 5;
                m_Root.transform.SetParent(UIRoot.RootTransform, false);
            }

            return m_Root;
        }

        private static void CheckRootCamera()
        {
            if (m_Camera == null)
            {
                m_Camera = m_Root.GetComponent<Camera>();
                m_Camera.orthographic = true;
                m_Camera.orthographicSize = 1;
                m_Camera.clearFlags = CameraClearFlags.Depth;
                m_Camera.depth = 10;
                m_Camera.farClipPlane = 60;
                m_Camera.nearClipPlane = -60;
                m_Camera.cullingMask = 1 << 5;
            }
        }
    }
}