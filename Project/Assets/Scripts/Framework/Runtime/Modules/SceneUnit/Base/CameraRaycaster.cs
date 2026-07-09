using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Runtime.MSceneUnit
{
    public class CameraRaycaster : MonoBehaviour
    {
        public LayerMask defaultEventMask;
        public bool enableDebug;
        public float maxDistance = 100;
        public RayHitEvent onRayHit;
        private GameObject _lastObject;

        [SerializeField, ShowIf("@this.enableDebug == true")]
        private GameObject debugObject;

        private RaycastHit hitInfo;
        private bool m_Hit;
        [SerializeField] private Camera m_TargetCamera;

        private Dictionary<LayerMask, RayHitEvent> OnRayHitLayer = new Dictionary<LayerMask, RayHitEvent>();

        public delegate void RayHitEvent(RaycastHit hitInfo, bool isHit);

        public RaycastHit HitInfo
        {
            get => hitInfo;
            set => hitInfo = value;
        }

        public bool IsHit => m_Hit;

        public bool isMousePosition { get; set; } = true;

        public Camera TargetCamera
        {
            get => m_TargetCamera;
            set => m_TargetCamera = value;
        }

        public void AddLayerHitListener(LayerMask layer, RayHitEvent ctx)
        {
            if (OnRayHitLayer.ContainsKey(layer))
            {
                OnRayHitLayer[layer] += ctx;
            }
            else
            {
                OnRayHitLayer[layer] = ctx;
            }
        }

        public T GetHit<T>() where T : Component
        {
            if (hitInfo.collider == null) return default;
            return hitInfo.collider.gameObject.GetComponent<T>();
        }

        public Component GetHit(Type type)
        {
            if (hitInfo.collider == null) return default;
            return hitInfo.collider.gameObject.GetComponent(type);
        }

        public void RemoveLayerHitListener(LayerMask layer, RayHitEvent ctx)
        {
            if (OnRayHitLayer.ContainsKey(layer))
            {
                OnRayHitLayer[layer] -= ctx;
                if (OnRayHitLayer[layer] == null)
                {
                    OnRayHitLayer.Remove(layer);
                }
            }
        }

        public void SetLayerMask(int layer)
        {
            defaultEventMask = layer;
        }

        public void SetLayerMaskNot(int layer)
        {
            defaultEventMask = ~layer;
        }

        public void TickUpdate()
        {
            LayerRayCheck();
            Ray ray;
            if (isMousePosition)
            {
                ray = m_TargetCamera.ScreenPointToRay(Input.mousePosition);
            }
            else
            {
                ray = m_TargetCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            }
            //Ray ray = _targetCamera.ScreenPointToRay(new Vector3(GetMouseInput().x, GetMouseInput().y, 0));
            m_Hit = Physics.Raycast(ray, out hitInfo, maxDistance, defaultEventMask);
            if (enableDebug)
            {
                Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.red);
                DebugShow(m_Hit, hitInfo);
            }

            if (m_Hit)
            {
                onRayHit?.Invoke(hitInfo, true);
                // 如果击中物体，判断物体是否是上次击中的
                if (_lastObject != null && _lastObject == hitInfo.transform.gameObject)
                {
                    return;
                }

                // 第一次击中或者击中新目标 为目标发送进入消息
                hitInfo.transform.gameObject.SendMessage("OnMouseRayEnter", SendMessageOptions.DontRequireReceiver);
                // 如果不是第一次击中就发送离开消息
                if (_lastObject != null)
                    _lastObject.transform.gameObject.SendMessage("OnMouseRayExit",
                        SendMessageOptions.DontRequireReceiver);
                // 更新上一次击中点
                _lastObject = hitInfo.transform.gameObject;
            }
            // 如果射线没有击中
            else
            {
                onRayHit?.Invoke(hitInfo, false);
                // 发送离开消息
                if (_lastObject != null)
                    _lastObject.transform.gameObject.SendMessage("OnMouseRayExit",
                        SendMessageOptions.DontRequireReceiver);
                _lastObject = null;
            }
        }

        public bool TryGetHit<T>(out T hitComponent) where T : Component
        {
            hitComponent = default;
            if (hitInfo.collider == null) return false;
            return hitInfo.collider.gameObject.TryGetComponent(out hitComponent);
        }

        private void DebugShow(bool isHit, RaycastHit hitInfo)
        {
            if (!enableDebug || debugObject == null) return;
            if (!isHit) debugObject?.SetActive(false);
            else
            {
                if (debugObject == null) return;
                debugObject?.SetActive(true);
                debugObject.transform.position = hitInfo.point;
            }
        }

        private void LayerRayCheck()
        {
            foreach (LayerMask layer in OnRayHitLayer.Keys)
            {
                RaycastHit hitInfo;
                Ray ray = m_TargetCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

                if (Physics.Raycast(ray, out hitInfo, maxDistance, layer))
                {
                    //Debug.Log(layer + "hit");
                    OnRayHitLayer[layer]?.Invoke(hitInfo, true);
                }
                else
                {
                    //Debug.Log(layer + "not hit");
                    OnRayHitLayer[layer]?.Invoke(hitInfo, false);
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (enableDebug && m_Hit)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(hitInfo.point, 0.15f);
            }
        }

        private void Start()
        {
            if (m_TargetCamera == null)
                m_TargetCamera = GetComponent<Camera>();
        }

        private void Update()
        {
            TickUpdate();
        }
    }
}