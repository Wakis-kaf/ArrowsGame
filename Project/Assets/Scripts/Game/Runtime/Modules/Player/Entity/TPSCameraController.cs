using Framework.Runtime;
using Framework.Runtime.MAsset;
using Framework.Runtime.MSceneUnit;
using Framework.Runtime.UnitSystem.BIInterfaces;
using UnityEngine;

namespace Game.Modules.GModulePlayer
{
    public class TPSCameraController : UnitCameraController, IUnitLateUpdate
    {
        [Header("跟随目标")]
        [SerializeField] private Transform m_TargetTransform;

        [Header("俯视视角配置")]
        [Range(10f, 85f)]
        [SerializeField] private float m_PitchAngle = 45f;    // 俯视角度 (X轴旋转)
        [SerializeField] private float m_YawAngle = 0f;      // 偏航角度 (Y轴旋转)
        [SerializeField] private float m_Distance = 15f;     // 与目标的距离
        [SerializeField] private Vector3 m_TargetOffset = new Vector3(0, 1.0f, 0); // 目标中心偏移(如看人物头部)

        [Header("平滑参数")]
        [SerializeField] private float m_SmoothSpeed = 8f;   // 跟随平滑度
        [SerializeField] private bool m_UseSmoothDamp = true;

        [Header("地图边界")]
        [SerializeField] private bool m_EnableMapBounds = false;
        [SerializeField] private Bounds m_MapBounds = new Bounds(Vector3.zero, new Vector3(100, 0, 100));

        private Vector3 m_CurrentVelocity;
        private Vector3 m_DesiredPosition;

        #region Properties
        public Transform TargetTransform { get => m_TargetTransform; set => m_TargetTransform = value; }
        public float PitchAngle { get => m_PitchAngle; set => m_PitchAngle = value; }
        public float Distance { get => m_Distance; set => m_Distance = value; }
        public float SmoothSpeed { get => m_SmoothSpeed; set => m_SmoothSpeed = value; }
        #endregion

        public void OnUnitLateUpdate()
        {
            if (m_TargetTransform == null || Camera == null) return;

            // 1. 计算相机旋转
            Quaternion rotation = Quaternion.Euler(m_PitchAngle, m_YawAngle, 0);

            // 2. 根据旋转和距离计算相机应处于的相对位置
            // 在饥荒视角中，相机是在目标后上方，所以是 rotation * -Vector3.forward
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -m_Distance);
            Vector3 position = rotation * negDistance + (m_TargetTransform.position + m_TargetOffset);

            // 3. 边界限制 (可选)
            if (m_EnableMapBounds)
            {
                position = ClampToMapBounds(position);
            }

            m_DesiredPosition = position;

            // 4. 执行平滑跟随
            ApplySmoothing(rotation);
        }

        private void ApplySmoothing(Quaternion targetRotation)
        {
            if (m_UseSmoothDamp)
            {
                // 1f / m_SmoothSpeed 这个写法会导致 smoothTime 随 Speed 增大而变小
                // 建议直接定义一个 m_SmoothTime 变量 (如 0.1f)，这样更直观
                float smoothTime = Mathf.Max(0.01f, 1f / m_SmoothSpeed);

                Camera.transform.position = Vector3.SmoothDamp(
                    Camera.transform.position,
                    m_DesiredPosition,
                    ref m_CurrentVelocity,
                    smoothTime
                );
            }
            else
            {
                // Lerp 逻辑在不固定帧率下表现不如 SmoothDamp
                Camera.transform.position = Vector3.Lerp(
                    Camera.transform.position,
                    m_DesiredPosition,
                    Time.deltaTime * m_SmoothSpeed
                );
            }

            Camera.transform.rotation = targetRotation;
        }

        private Vector3 ClampToMapBounds(Vector3 pos)
        {
            // 简单的点限位，若需根据视口宽度限位，可保留原有的 viewWidth 计算逻辑
            float clampedX = Mathf.Clamp(pos.x, m_MapBounds.min.x, m_MapBounds.max.x);
            float clampedZ = Mathf.Clamp(pos.z, m_MapBounds.min.z, m_MapBounds.max.z);
            return new Vector3(clampedX, pos.y, clampedZ);
        }

        public override void SetFollowTransform(Transform target)
        {
            m_TargetTransform = target;
            if (m_TargetTransform != null)
            {
                // 立即切过去防止开局拉洞
                OnUnitLateUpdate();
                Camera.transform.position = m_DesiredPosition;
            }
        }

        public override void ResetCamera()
        {
            m_CurrentVelocity = Vector3.zero;
        }

        public override bool IsLoaded() => true;

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (m_EnableMapBounds)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(m_MapBounds.center, m_MapBounds.size);
            }

            if (m_TargetTransform != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(Camera.transform.position, m_TargetTransform.position + m_TargetOffset);
            }
        }
#endif
    }
}