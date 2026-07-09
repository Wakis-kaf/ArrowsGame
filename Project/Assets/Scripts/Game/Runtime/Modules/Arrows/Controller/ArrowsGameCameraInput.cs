using System.Collections.Generic;
using Framework.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Modules.GModuleArrows
{
    public class ArrowsGameCameraInput : MonoBehaviour
    {
        public static ArrowsGameCameraInput Ins => LevelVO.Current?.GameStage?.arrowsGameCameraController?.CameraInput;
        private Camera m_TargetCamera;

        [SerializeField] private float m_DragActiveBuffer = 0.1f;
        [SerializeField] private float m_ZoomActiveBuffer = 0.15f;
        [SerializeField] private float m_DragThreshold = 5f;
        [SerializeField] private bool m_EnableUIBlock = true;

        public bool IsDragging => m_DragBufferTimer > 0f;
        public bool IsZooming => m_ZoomBufferTimer > 0f;
        public bool IsActivelyDragging => m_IsRealDragging;

        public Vector3 PanWorldDelta { get; private set; }
        public float ZoomDelta { get; private set; }

        private Vector2 m_DragStartScreenPos;
        private Vector2 m_LastFrameScreenPos;
        private bool m_IsRealDragging = false;
        private bool m_IsPanBlockedByUI = false;
        private float m_ScrollSpeed = 5f;

        private float m_DragBufferTimer = 0f;
        private float m_ZoomBufferTimer = 0f;
        private bool m_InputEnable = false;
        public bool IsInputEnabled => m_InputEnable;

        public void EnbaleInput()
        {
            GameApp.Ins.LoopManager.AddTimeout(() =>
            {
                m_InputEnable = true;
                m_IsPanBlockedByUI = false;
            }, 0.1f);
        }

        public void DisableInput()
        {
            m_InputEnable = false;
            ClearInputs();
        }

        public void Init(Camera targetCamera, float scrollSpeed = 5f)
        {
            m_TargetCamera = targetCamera;
            m_ScrollSpeed = scrollSpeed;
        }

        public void ClearInputs()
        {
            m_DragBufferTimer = 0f;
            m_ZoomBufferTimer = 0f;
            m_IsRealDragging = false;
            m_IsPanBlockedByUI = false;
            ZoomDelta = 0;
            PanWorldDelta = Vector3.zero;
        }

        public void TickInput()
        {
            if (m_TargetCamera == null) return;

            if (m_DragBufferTimer > 0f) m_DragBufferTimer -= Time.deltaTime;
            if (m_ZoomBufferTimer > 0f) m_ZoomBufferTimer -= Time.deltaTime;

            HandleZoomInput();
            HandlePanInput();
        }

        private void HandleZoomInput()
        {
            float zoomDelta = 0f;
            bool currentZooming = false;
            if (!IsInputEnabled)
            {
                ZoomDelta = 0f;
                return;
            }

            if (Input.touchCount == 2)
            {
                if (m_EnableUIBlock && !ShouldCheckNullPoint())
                {
                    ZoomDelta = 0f;
                    return;
                }

                currentZooming = true;
                Touch touchZero = Input.GetTouch(0);
                Touch touchOne = Input.GetTouch(1);

                Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

                float deltaMagnitudeDiff = touchDeltaMag - prevTouchDeltaMag;
                zoomDelta = (deltaMagnitudeDiff / Mathf.Min(Screen.width, Screen.height)) * m_ScrollSpeed;
                ZoomDelta = zoomDelta;
            }
            else
            {
                float scroll = Input.GetAxis("Mouse ScrollWheel");
                if (Mathf.Abs(scroll) > 0.001f)
                {
                    if (m_EnableUIBlock && !ShouldCheckNullPoint())
                    {
                        ZoomDelta = 0f;
                        return;
                    }

                    zoomDelta = scroll * m_ScrollSpeed * 0.1f;
                    currentZooming = true;
                    ZoomDelta = zoomDelta;
                }
                else
                {
                    ZoomDelta = 0f;
                }
            }

            if (currentZooming)
            {
                m_ZoomBufferTimer = m_ZoomActiveBuffer;
            }
        }

        public bool ShouldCheckNullPoint()
        {
            if (EventSystem.current == null) return true;

            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (var result in results)
            {
                if (result.gameObject == null) continue;

                Selectable selectable = result.gameObject.GetComponentInParent<Selectable>();
                if (selectable != null && selectable.interactable)
                {
                    return false;
                }

                Graphic graphic = result.gameObject.GetComponent<Graphic>();
                if (graphic != null && graphic.raycastTarget)
                {
                    return false;
                }
            }

            return true;
        }

        private void HandlePanInput()
        {
            if (!IsInputEnabled || IsZooming || Input.touchCount > 2)
            {
                m_IsRealDragging = false;
                PanWorldDelta = Vector3.zero;
                return;
            }

            Vector3 currentWorldDelta = Vector3.zero;

            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    m_DragStartScreenPos = touch.position;
                    m_LastFrameScreenPos = touch.position;
                    m_IsRealDragging = false;
                    m_IsPanBlockedByUI = m_EnableUIBlock && !ShouldCheckNullPoint();
                }
                else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                {
                    if (!m_IsPanBlockedByUI)
                    {
                        if (!m_IsRealDragging)
                        {
                            if (Vector2.Distance(touch.position, m_DragStartScreenPos) > m_DragThreshold)
                            {
                                m_IsRealDragging = true;
                                m_LastFrameScreenPos = touch.position;
                            }
                        }

                        if (m_IsRealDragging)
                        {
                            Vector3 lastWorldPos = m_TargetCamera.ScreenToWorldPoint(new Vector3(m_LastFrameScreenPos.x, m_LastFrameScreenPos.y, m_TargetCamera.nearClipPlane));
                            Vector3 currentWorldPos = m_TargetCamera.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, m_TargetCamera.nearClipPlane));
                            currentWorldDelta = lastWorldPos - currentWorldPos;
                            m_LastFrameScreenPos = touch.position;
                        }
                    }
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    m_IsRealDragging = false;
                    m_IsPanBlockedByUI = false;
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    m_DragStartScreenPos = Input.mousePosition;
                    m_LastFrameScreenPos = Input.mousePosition;
                    m_IsRealDragging = false;
                    m_IsPanBlockedByUI = m_EnableUIBlock && !ShouldCheckNullPoint();
                }
                else if (Input.GetMouseButton(0))
                {
                    if (!m_IsPanBlockedByUI)
                    {
                        if (!m_IsRealDragging)
                        {
                            if (Vector2.Distance(Input.mousePosition, m_DragStartScreenPos) > m_DragThreshold)
                            {
                                m_IsRealDragging = true;
                                m_LastFrameScreenPos = Input.mousePosition;
                            }
                        }

                        if (m_IsRealDragging)
                        {
                            Vector3 lastWorldPos = m_TargetCamera.ScreenToWorldPoint(new Vector3(m_LastFrameScreenPos.x, m_LastFrameScreenPos.y, m_TargetCamera.nearClipPlane));
                            Vector3 currentWorldPos = m_TargetCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, m_TargetCamera.nearClipPlane));
                            currentWorldDelta = lastWorldPos - currentWorldPos;
                            m_LastFrameScreenPos = Input.mousePosition;
                        }
                    }
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    m_IsRealDragging = false;
                    m_IsPanBlockedByUI = false;
                }
            }

            PanWorldDelta = currentWorldDelta;

            if (m_IsRealDragging)
            {
                m_DragBufferTimer = m_DragActiveBuffer;
            }
        }
    }
}