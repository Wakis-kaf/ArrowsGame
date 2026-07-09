using Framework.Runtime.UnitSystem.BIInterfaces;
using UnityEngine;

namespace Framework.Runtime.MSceneUnit
{
    // 移动平面枚举（独立于相机的跟踪平面）
    public enum MovePlane
    {
        XY,  // 2D平面（X左右，Y上下）
        XZ   // 类3D顶视平面（X左右，Z前后，Y输入映射到Z轴）
    }

    public class RbMoveController : UnitMoveController, IUnitFixedUpdate
    {
        // 私有移动向量（存储当前输入）
        private Vector2 _moveInputVector;

        [Header("移动配置")]
        [SerializeField] private MovePlane _movePlane = MovePlane.XZ;

        [SerializeField] private bool _normalizeDiagonalMovement = true;

        // 外部可访问的移动向量（带getter和setter）
        public Vector2 MoveInputVector
        {
            get => _moveInputVector;
            set
            {
                // 设置时自动处理移动逻辑
                _moveInputVector = value;
                OnMoveChange(_moveInputVector);
                //ProcessMoveInput(_moveInputVector);
            }
        }

        public override Vector3 GetVelocity()
        {
            return Rigidbody.velocity;
        }

        public void OnUnitFixedUpdate()
        {
            //Debug.Log("FixedUpdate" +(int)Time.time);
            if (IsMoveEnable)
            {
                ProcessMoveInput(_moveInputVector);
            }
            else
            {
                ProcessMoveInput(Vector2.zero);
            }
        }

        /// <summary>
        /// 基类接口：通过Vector2输入移动（内部调用处理方法）
        /// </summary>
        public override void SetMoveInput(Vector2 input)
        {
            MoveInputVector = input; // 复用setter逻辑
        }

        public override void SetMoveToPosition(Vector3 position)
        {
            Vector3 dirToTarget = position - UnitEntityTransform.position;
            dirToTarget = _movePlane switch
            {
                MovePlane.XY => new Vector3(dirToTarget.x, dirToTarget.y, 0).normalized,
                MovePlane.XZ => new Vector3(dirToTarget.x, 0, dirToTarget.z).normalized,
                _ => Vector3.zero
            };

            Vector2 input = _movePlane switch
            {
                MovePlane.XY => new Vector2(dirToTarget.x, dirToTarget.y),
                MovePlane.XZ => new Vector2(dirToTarget.x, dirToTarget.z),
                _ => Vector2.zero
            };

            MoveInputVector = input; // 使用setter更新移动
        }

        public override void StopMove()
        {
            Rigidbody.velocity = Vector2.Lerp(
                Rigidbody.velocity,
                Vector2.zero,
                m_Acceleration * Time.fixedDeltaTime
            );
            _moveInputVector = Vector2.zero; // 重置输入向量
        }

        protected override void OnComponentInit()
        {
            base.OnComponentInit();

            //Rigidbody.bodyType = RigidbodyType2D.Dynamic;
            //Rigidbody.angularDrag = 0;
            Rigidbody.interpolation = RigidbodyInterpolation.Interpolate; ;
            _moveInputVector = Vector2.zero;
        }

        /// <summary>
        /// 转换输入向量到世界空间
        /// </summary>
        private Vector3 ConvertInputToWorldDir(Vector2 input)
        {
            return _movePlane switch
            {
                MovePlane.XY => new Vector3(input.x, input.y, 0),
                MovePlane.XZ => new Vector3(input.x, 0, input.y),
                _ => Vector3.zero
            };
        }

        /// <summary>
        /// 处理移动输入的核心方法
        /// </summary>
        private void ProcessMoveInput(Vector2 input)
        {
            // 归一化斜向输入
            var processedInput = _normalizeDiagonalMovement ? input.normalized : input;

            // 转换为世界方向
            Vector3 worldDir = ConvertInputToWorldDir(processedInput);

            // 计算目标速度并应用加速度
            Vector3 targetVelocity = new Vector3(worldDir.x, worldDir.y, worldDir.z) * m_MoveSpeed;
            Rigidbody.velocity = targetVelocity;
        }
    }
}