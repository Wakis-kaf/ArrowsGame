using UnityEngine;

namespace Framework.Runtime.MSceneUnit
{
    public abstract class UnitCameraController : SceneUnitComponent
    {
        [SerializeField] private Camera m_Camera;

        private CameraRaycaster m_CameraRaycaster;
        private bool m_IsMain;

        public Camera Camera
        {
            get
            {
                if (m_Camera == null)
                    m_Camera = GetCamera();
                if (IsMain)
                {
                    m_Camera.gameObject.tag = "MainCamera";
                }

                return m_Camera;
            }
            set { m_Camera = value; }
        }

        public Vector3 CameraPositoin
        {
            get { return Camera.transform.position; }
        }

        public CameraRaycaster CameraRayCaster
        {
            get
            {
                if (m_CameraRaycaster == null && Camera != null)
                {
                    m_CameraRaycaster = gameObject.AddComponent<CameraRaycaster>();
                    m_CameraRaycaster.TargetCamera = Camera;
                }

                return m_CameraRaycaster;
            }
        }

        public Quaternion CameraRotation
        {
            get { return Camera.transform.rotation; }
        }

        public bool IsMain
        {
            get => m_IsMain;
            set
            {
                m_IsMain = value;
                if (m_IsMain)
                {
                    Camera.gameObject.tag = "MainCamera";
                }
            }
        }

        public virtual void DisableCameraRayCaster()
        {
            CameraRayCaster.enabled = false;
        }

        public virtual void EnableCameraRayCaster()
        {
            CameraRayCaster.enabled = true;
        }

        public virtual Camera GetCamera()
        {
            GameObject cameraGo = new GameObject("Camera_" + OwnSceneUnit.UnitName);
            cameraGo.transform.SetParent(transform);
            Camera camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            return camera;
        }
        public virtual void ResetCamera()
        {

        }

        public virtual void SetFollowTransform(Transform target)
        {
        }

        public virtual void SetLookAtTransform(Transform lookAt)
        {
        }

        public virtual void SetOption(object option)
        {
        }

        public virtual void UpdateWithInput(float deltaTime, float zoomInput, Vector3 rotationInput)
        {
        }
    }
}