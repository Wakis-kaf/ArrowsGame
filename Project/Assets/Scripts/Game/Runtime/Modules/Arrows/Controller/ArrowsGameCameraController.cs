using Framework.Runtime;
using Framework.Runtime.LogSystem;
using Framework.Runtime.MGameModule;
using Game.Modules.GModuleStage;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Modules.GModuleArrows
{
    public class ArrowsGameCameraController : MonoBehaviour
    {
        [SerializeField] private Camera m_TargetCamera;
        [SerializeField] private ArrowsGameCameraInput m_CameraInput;
        public ArrowsGameCameraInput CameraInput => m_CameraInput;

        [Header("Zoom Scale Settings")]
        [SerializeField] private float m_MinZoomScale = 0.4f;
        [SerializeField] private float m_StartZoomScale = 0.6f;
        [SerializeField] private float m_MaxZoomScale = 1f;
        [SerializeField] private float m_ScrollSpeed = 5f;
        [SerializeField] private float m_ZoomSmoothSpeed = 12f;
        [SerializeField] private float m_ZoomStableDelay = 0.2f;

        [Header("Move Inertia Settings")]
        [SerializeField] private float m_PanDeceleration = 8f;
        [SerializeField] private float m_PanSmoothSpeed = 15f;
        [SerializeField] private float m_CenterSnapSpeed = 10f;

        [Header("Border Padding (Proportion)")]
        [SerializeField] private float m_PaddingLeft = 0.05f;
        [SerializeField] private float m_PaddingRight = 0.05f;
        [SerializeField] private float m_PaddingTop = 0.05f;
        [SerializeField] private float m_PaddingBottom = 0.05f;

        [Header("Border Margin (Proportion)")]
        [SerializeField] private float m_MarginLeft = 0.05f;
        [SerializeField] private float m_MarginRight = 0.05f;
        [SerializeField] private float m_MarginTop = 0.05f;
        [SerializeField] private float m_MarginBottom = 0.05f;

        private float m_InitialSize;
        private Vector3 m_CenterPosition;

        private Vector2 m_MinWorldBounds;
        private Vector2 m_MaxWorldBounds;
        private float m_CurrentZoomScale = 1.0f;
        private float m_TargetZoomScale = 1.0f;
        public float TargetZoomScale => m_TargetZoomScale;
        private float m_StableTimer = 0f;
        private Vector3 m_PanVelocity = Vector3.zero;

        private Vector3 m_TargetPosition;
        private bool m_IsMovingToTarget = false;

        public bool IsDragging => m_CameraInput != null && m_CameraInput.IsDragging;
        public bool IsZooming => m_CameraInput != null && m_CameraInput.IsZooming;
        public bool IsZoomingStopStabled => Mathf.Abs(m_CurrentZoomScale - m_TargetZoomScale) <= 0.0001f && m_StableTimer >= m_ZoomStableDelay;

        private void Awake()
        {
            if (m_CameraInput == null)
            {
                m_CameraInput = gameObject.GetComponent<ArrowsGameCameraInput>();
                if (m_CameraInput == null)
                {
                    m_CameraInput = gameObject.AddComponent<ArrowsGameCameraInput>();
                }
            }
        }

        public void BindCamera(Camera gameCamera)
        {
            m_TargetCamera = gameCamera;
            if (m_TargetCamera != null)
            {
                m_TargetCamera.orthographic = true;
                m_CameraInput.Init(m_TargetCamera, m_ScrollSpeed);
            }
        }

        public void SetZoomSpeed(float speed)
        {
            m_ZoomSmoothSpeed = speed;
        }

        public void SetArgs(float minZoomScale, float startZoomScale, float maxZoomScale, float scrollSpeed)
        {
            m_MinZoomScale = minZoomScale;
            m_StartZoomScale = startZoomScale;
            m_MaxZoomScale = maxZoomScale;
            m_ScrollSpeed = scrollSpeed;
        }

        public void SetWorldArea(float minAreaX, float maxAreaX, float minAreaY, float maxAreaY)
        {
            if (m_TargetCamera == null) return;

            m_MinWorldBounds = new Vector2(minAreaX, minAreaY);
            m_MaxWorldBounds = new Vector2(maxAreaX, maxAreaY);

            float width = maxAreaX - minAreaX;
            float height = maxAreaY - minAreaY;

            m_CenterPosition = new Vector3(minAreaX + width * 0.5f, minAreaY + height * 0.5f, m_TargetCamera.transform.position.z);
            m_TargetCamera.transform.position = m_CenterPosition;

            float halfPadX = width * (m_PaddingLeft + m_PaddingRight) * 0.5f;
            float halfPadY = height * (m_PaddingTop + m_PaddingBottom) * 0.5f;

            float widthForZoom = width + halfPadX * 2f;
            float heightForZoom = height + halfPadY * 2f;

            float requiredSizeByHeight = heightForZoom * 0.5f;
            float requiredSizeByWidth = (widthForZoom * 0.5f) / m_TargetCamera.aspect;
            m_InitialSize = Mathf.Max(requiredSizeByHeight, requiredSizeByWidth);

            m_CurrentZoomScale = m_StartZoomScale;
            m_TargetZoomScale = m_StartZoomScale;
            m_TargetCamera.orthographicSize = ConvertScaleToSize(m_CurrentZoomScale);
            m_StableTimer = 0f;
            m_PanVelocity = Vector3.zero;
            m_IsMovingToTarget = false;

            ClampCameraPosition();

            MessageDispatcher.Ins.Dispatch(MessageCode.msg_on_cameraZoom_changed, m_CurrentZoomScale, m_TargetZoomScale);
        }

        public void SetTargetZoom(float targetScale, bool immediate = false)
        {
            m_TargetZoomScale = Mathf.Clamp(targetScale, m_MinZoomScale, m_MaxZoomScale);
            m_StableTimer = 0f;

            if (immediate)
            {
                m_CurrentZoomScale = m_TargetZoomScale;
                if (m_TargetCamera != null)
                {
                    m_TargetCamera.orthographicSize = ConvertScaleToSize(m_CurrentZoomScale);
                    ClampCameraPosition();
                }
                MessageDispatcher.Ins.Dispatch(MessageCode.msg_on_cameraZoom_changed, m_CurrentZoomScale, m_TargetZoomScale);
            }
        }

        public void SnapToCenter(bool immediate = false)
        {
            if (m_TargetCamera == null) return;

            m_PanVelocity = Vector3.zero;

            if (immediate)
            {
                m_IsMovingToTarget = false;
                m_TargetCamera.transform.position = new Vector3(m_CenterPosition.x, m_CenterPosition.y, m_TargetCamera.transform.position.z);
                ClampCameraPosition();
            }
            else
            {
                m_TargetPosition = new Vector3(m_CenterPosition.x, m_CenterPosition.y, m_TargetCamera.transform.position.z);
                m_IsMovingToTarget = true;
            }
        }

        private float ConvertScaleToSize(float scale)
        {
            float invertedScale = m_MaxZoomScale + m_MinZoomScale - scale;
            return m_InitialSize * invertedScale;
        }

        private void Update()
        {
            if (m_TargetCamera == null || m_CameraInput == null) return;

            m_CameraInput.TickInput();

            if (m_CameraInput.IsInputEnabled && Mathf.Abs(m_CameraInput.ZoomDelta) > 0.001f)
            {
                m_TargetZoomScale += m_CameraInput.ZoomDelta;
                m_TargetZoomScale = Mathf.Clamp(m_TargetZoomScale, m_MinZoomScale, m_MaxZoomScale);
            }

            if (Mathf.Abs(m_CurrentZoomScale - m_TargetZoomScale) > 0.0001f)
            {
                m_StableTimer = 0f;
                m_CurrentZoomScale = Mathf.Lerp(m_CurrentZoomScale, m_TargetZoomScale, Time.deltaTime * m_ZoomSmoothSpeed);
                m_TargetCamera.orthographicSize = ConvertScaleToSize(m_CurrentZoomScale);
                ClampCameraPosition();
                MessageDispatcher.Ins.Dispatch(MessageCode.msg_on_cameraZoom_changed, m_CurrentZoomScale, m_TargetZoomScale);
            }
            else
            {
                if (m_StableTimer < m_ZoomStableDelay)
                {
                    m_StableTimer += Time.deltaTime;
                }
            }

            if (m_CameraInput.IsZooming)
            {
                m_PanVelocity = Vector3.zero;
                m_IsMovingToTarget = false;
            }
            else if (m_CameraInput.IsInputEnabled && m_CameraInput.IsActivelyDragging)
            {
                Vector3 instantVelocity = m_CameraInput.PanWorldDelta / Mathf.Max(Time.deltaTime, 0.0001f);
                m_PanVelocity = Vector3.Lerp(m_PanVelocity, instantVelocity, Time.deltaTime * m_PanSmoothSpeed);
                m_IsMovingToTarget = false;
            }
            else
            {
                m_PanVelocity = Vector3.Lerp(m_PanVelocity, Vector3.zero, Time.deltaTime * m_PanDeceleration);
            }

            if (m_IsMovingToTarget)
            {
                Vector3 currentPos = m_TargetCamera.transform.position;
                Vector3 nextPos = Vector3.Lerp(currentPos, m_TargetPosition, Time.deltaTime * m_CenterSnapSpeed);
                m_TargetCamera.transform.position = new Vector3(nextPos.x, nextPos.y, currentPos.z);
                ClampCameraPosition();

                if (Vector2.SqrMagnitude(new Vector2(nextPos.x - m_TargetPosition.x, nextPos.y - m_TargetPosition.y)) < 0.0001f)
                {
                    m_IsMovingToTarget = false;
                }
            }
            else if (m_PanVelocity.sqrMagnitude > 0.0001f)
            {
                Vector3 targetPos = m_TargetCamera.transform.position + m_PanVelocity * Time.deltaTime;
                m_TargetCamera.transform.position = new Vector3(targetPos.x, targetPos.y, m_TargetCamera.transform.position.z);
                ClampCameraPosition();
            }
        }

        private void ClampCameraPosition()
        {
            if (m_TargetCamera == null) return;

            float camHeight = m_TargetCamera.orthographicSize;
            float camWidth = camHeight * m_TargetCamera.aspect;

            float zoomFactor = camHeight / m_InitialSize;

            float width = m_MaxWorldBounds.x - m_MinWorldBounds.x;
            float height = m_MaxWorldBounds.y - m_MinWorldBounds.y;

            float halfPadX = width * (m_PaddingLeft + m_PaddingRight) * 0.5f;
            float halfPadY = height * (m_PaddingTop + m_PaddingBottom) * 0.5f;
            float halfMarginX = width * (m_MarginLeft + m_MarginRight) * 0.5f;
            float halfMarginY = height * (m_MarginTop + m_MarginBottom) * 0.5f;

            float dynamicTotalX = (halfPadX + halfMarginX) * zoomFactor;
            float dynamicTotalY = (halfPadY + halfMarginY) * zoomFactor;

            float worldMinX = m_MinWorldBounds.x - dynamicTotalX;
            float worldMaxX = m_MaxWorldBounds.x + dynamicTotalX;
            float worldMinY = m_MinWorldBounds.y - dynamicTotalY;
            float worldMaxY = m_MaxWorldBounds.y + dynamicTotalY;

            float totalWidth = worldMaxX - worldMinX;
            float totalHeight = worldMaxY - worldMinY;

            float minX, maxX, minY, maxY;

            if (camWidth * 2f >= totalWidth)
            {
                minX = maxX = m_CenterPosition.x;
            }
            else
            {
                minX = worldMinX + camWidth;
                maxX = worldMaxX - camWidth;
            }

            if (camHeight * 2f >= totalHeight)
            {
                minY = maxY = m_CenterPosition.y;
            }
            else
            {
                minY = worldMinY + camHeight;
                maxY = worldMaxY - camHeight;
            }

            Vector3 pos = m_TargetCamera.transform.position;
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
            m_TargetCamera.transform.position = pos;
        }
    }
}