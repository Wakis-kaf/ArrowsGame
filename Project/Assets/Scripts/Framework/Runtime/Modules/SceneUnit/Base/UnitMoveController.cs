using DG.Tweening;
using System;
using UnityEngine;

namespace Framework.Runtime.MSceneUnit
{
    public abstract class UnitMoveController : SceneUnitComponent
    {
        protected float m_Acceleration = 2;
        protected float m_MoveSpeed = 2;
        protected float m_StoppingDistance = 0.1f;
        protected float m_TurnTime = 0.3f;
        public bool IsMoveEnable { get; protected set; } = true;
        private Rigidbody m_RigidBody;
        private Rigidbody2D m_RigidBody2D;
        private Action<Vector3> m_MoveChangeCb;
        public virtual float Acceleration
        {
            get => m_Acceleration;
            set => m_Acceleration = value;
        }

        public virtual float MoveSpeed
        {
            get => m_MoveSpeed;
            set => m_MoveSpeed = value;
        }

        public virtual Rigidbody Rigidbody
        {
            get
            {
                if (m_RigidBody == null)
                {
                    m_RigidBody = UnitEntityTransform.gameObject.GetOrAddComponent<Rigidbody>();
                    m_RigidBody.useGravity = false;
                }

                return m_RigidBody;
            }
            set
            {
                m_RigidBody = value;
            }
        }
        public virtual Rigidbody2D Rigidbody2D
        {
            get
            {
                if (m_RigidBody2D == null)
                {
                    m_RigidBody2D = UnitEntityTransform.gameObject.GetOrAddComponent<Rigidbody2D>();
                    m_RigidBody2D.gravityScale = 0;
                }

                return m_RigidBody2D;
            }
            set
            {
                m_RigidBody2D = value;
            }
        }

        public virtual float StoppingDistance
        {
            get => m_StoppingDistance;
            set => m_StoppingDistance = value;
        }

        public virtual float TurnTime
        {
            get => m_TurnTime;
            set => m_TurnTime = value;
        }

        public void DisableMove()
        {
            IsMoveEnable = false;
        }

        public void EnableMove()
        {
            IsMoveEnable = true;
        }

        public virtual Vector3 GetVelocity()
        {
            return Vector3.zero;
        }

        public virtual bool IsGrounded()
        {
            return true;
        }

        public virtual void LookTowards(Vector3 dir, float turnTime = 0)
        {
            UnitEntityTransform.DOLookAt(UnitEntityTransform.position + dir, turnTime);
        }

        public virtual void LookTowardsTarget(Vector3 pos, float turnTime = 0)
        {
            Vector3 dir = pos - UnitEntityTransform.position;
            LookTowards(dir, turnTime);
        }

        public virtual void SetCameraRot(Quaternion quaternion)
        {
        }

        public virtual void SetCrouch(bool isCrouch)
        {
        }

        public virtual void SetJump(bool isJump)
        {
        }

        public virtual void SetMoveInput(Vector2 input)
        {
        }

        public virtual void SetMoveInput(Vector3 worldInput)
        {
        }

        public virtual void SetMoveToPosition(Vector3 position)
        {
        }

        public virtual void SetPosition(Vector3 position)
        {
        }

        public virtual void StopLoopAndRot()
        {
            UnitEntityTransform.DOKill();
        }

        public virtual void StopMove()
        {
        }
        
        public void AddMoveChangeListener(Action<Vector3> input)
        {
            m_MoveChangeCb -= input;
            m_MoveChangeCb += input;
        }
        public void OnMoveChange(Vector3 moveInput)
        {
            m_MoveChangeCb?.Invoke(moveInput);
        }
    }
}