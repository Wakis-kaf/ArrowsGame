using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_PIPELINE_URP
using UnityEngine.Rendering.Universal;
#endif

namespace Framework.Runtime.CameraManage
{
    public class CameraStackManager
    {
        private static CameraStackManager m_Instance;
        public static CameraStackManager Instance => m_Instance ??= new CameraStackManager();

        private Camera m_BaseCamera;

#if UNITY_PIPELINE_URP
        private UniversalAdditionalCameraData m_BaseCameraData;
#endif

        private List<Camera> m_BaseHistoryStack = new List<Camera>();
        private List<CameraRegistration> m_RegisteredCameras = new List<CameraRegistration>();

        private struct CameraRegistration
        {
            public Camera Camera;
            public int Priority;
        }

        private struct CameraState
        {
            public CameraClearFlags originalClearFlags;
            public float originalDepth;
        }

        private Dictionary<Camera, CameraState> m_OriginalStates = new Dictionary<Camera, CameraState>();
        private bool m_IsURP;

        public CameraStackManager()
        {
            if (m_Instance == null)
            {
                m_Instance = this;
            }

            m_IsURP = GraphicsSettings.currentRenderPipeline != null;

            if (GraphicsSettings.currentRenderPipeline != null)
            {
                m_IsURP = GraphicsSettings.currentRenderPipeline.GetType().Name.Contains("Universal");
            }
        }

        private void RecordState(Camera camera)
        {
            if (camera == null || m_OriginalStates.ContainsKey(camera)) return;
            m_OriginalStates[camera] = new CameraState
            {
                originalClearFlags = camera.clearFlags,
                originalDepth = camera.depth
            };
        }

        private void RestoreState(Camera camera)
        {
            if (camera == null) return;
            if (m_OriginalStates.TryGetValue(camera, out var state))
            {
                camera.clearFlags = state.originalClearFlags;
                camera.depth = state.originalDepth;
                m_OriginalStates.Remove(camera);
            }
        }

        public void RegisterCamera(Camera camera, int priority = 0)
        {
            if (camera == null) return;

            if (m_BaseCamera == null && m_BaseHistoryStack.Count == 0)
            {
                SetBaseCamera(camera);
                return;
            }

            if (camera == m_BaseCamera) return;

            RecordState(camera);

            if (m_IsURP)
            {
#if UNITY_PIPELINE_URP
                var cameraData = camera.GetUniversalAdditionalCameraData();
                cameraData.renderType = CameraRenderType.Overlay;
#endif
            }
            else
            {
                camera.clearFlags = CameraClearFlags.Depth;
            }

            m_RegisteredCameras.RemoveAll(r => r.Camera == camera);
            m_RegisteredCameras.Add(new CameraRegistration { Camera = camera, Priority = priority });

            UpdateStack();
        }

        public void UnregisterCamera(Camera camera)
        {
            if (camera == null) return;

            m_BaseHistoryStack.Remove(camera);
            m_RegisteredCameras.RemoveAll(r => r.Camera == camera);

            if (camera == m_BaseCamera)
            {
                m_BaseCamera = null;
#if UNITY_PIPELINE_URP
                m_BaseCameraData = null;
#endif
                RestoreState(camera);
                TryRestoreOrFindNewBase();
            }
            else
            {
                RestoreState(camera);
                UpdateStack();
            }
        }

        public void SetBaseCamera(Camera baseCamera)
        {
            if (baseCamera == null) return;

            if (m_BaseCamera != null && m_BaseCamera != baseCamera)
            {
                RecordState(m_BaseCamera);

                if (m_IsURP)
                {
#if UNITY_PIPELINE_URP
                    var oldData = m_BaseCamera.GetUniversalAdditionalCameraData();
                    oldData.renderType = CameraRenderType.Overlay;
#endif
                }
                else
                {
                    m_BaseCamera.clearFlags = CameraClearFlags.Depth;
                }

                if (!m_BaseHistoryStack.Contains(m_BaseCamera))
                {
                    m_BaseHistoryStack.Add(m_BaseCamera);
                }
            }

            m_BaseHistoryStack.Remove(baseCamera);
            ApplyBaseCamera(baseCamera);
        }

        private void ApplyBaseCamera(Camera baseCamera)
        {
            m_BaseCamera = baseCamera;
            RecordState(baseCamera);

            if (m_IsURP)
            {
#if UNITY_PIPELINE_URP
                m_BaseCameraData = m_BaseCamera.GetUniversalAdditionalCameraData();
                m_BaseCameraData.renderType = CameraRenderType.Base;
#endif
            }
            else
            {
                if (m_OriginalStates.TryGetValue(baseCamera, out var state))
                {
                    m_BaseCamera.clearFlags = state.originalClearFlags;
                }
                else if (m_BaseCamera.clearFlags == CameraClearFlags.Depth)
                {
                    m_BaseCamera.clearFlags = CameraClearFlags.SolidColor;
                }
            }

            m_RegisteredCameras.RemoveAll(r => r.Camera == baseCamera);
            UpdateStack();
        }

        private void TryRestoreOrFindNewBase()
        {
            m_BaseHistoryStack.RemoveAll(c => c == null);

            if (m_BaseHistoryStack.Count > 0)
            {
                int lastIndex = m_BaseHistoryStack.Count - 1;
                Camera previousBase = m_BaseHistoryStack[lastIndex];
                m_BaseHistoryStack.RemoveAt(lastIndex);
                ApplyBaseCamera(previousBase);
                return;
            }

            if (m_RegisteredCameras.Count > 0)
            {
                Camera candidate = m_RegisteredCameras[0].Camera;
                if (candidate != null)
                {
                    ApplyBaseCamera(candidate);
                }
            }
        }

        private void UpdateStack()
        {
            if (m_BaseCamera == null) return;

            m_RegisteredCameras.RemoveAll(r => r.Camera == null);
            m_BaseHistoryStack.RemoveAll(c => c == null);

            if (m_IsURP)
            {
#if UNITY_PIPELINE_URP
                if (m_BaseCameraData == null) m_BaseCameraData = m_BaseCamera.GetUniversalAdditionalCameraData();
                if (m_BaseCameraData == null) return;

                var stack = m_BaseCameraData.cameraStack;
                stack.Clear();

                foreach (var cam in m_BaseHistoryStack)
                {
                    if (cam != m_BaseCamera) stack.Add(cam);
                }

                m_RegisteredCameras.Sort((a, b) => a.Priority.CompareTo(b.Priority));
                foreach (var reg in m_RegisteredCameras)
                {
                    if (reg.Camera != m_BaseCamera) stack.Add(reg.Camera);
                }
#endif
            }
            else
            {
                float currentDepth = m_BaseCamera.depth;

                foreach (var cam in m_BaseHistoryStack)
                {
                    if (cam != m_BaseCamera)
                    {
                        RecordState(cam);
                        currentDepth += 1f;
                        cam.depth = currentDepth;
                    }
                }

                m_RegisteredCameras.Sort((a, b) => a.Priority.CompareTo(b.Priority));
                foreach (var reg in m_RegisteredCameras)
                {
                    if (reg.Camera != m_BaseCamera)
                    {
                        RecordState(reg.Camera);
                        currentDepth += 1f;
                        reg.Camera.depth = currentDepth;
                    }
                }
            }
        }
    }
}