using Framework.Runtime;
using Framework.Runtime.MSceneUnit;
using Framework.Runtime.UI;
using Game.Modules.GModuleSceneUnit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Game.Modules.GModuleFX
{
    public class FXSceneUnit : SceneUnit
    {
        private struct FxTask
        {
            internal string argName;
            internal FxType fxType;
            internal string triggerName;
            internal float endTimeout;
            internal bool isEndDispose;
            internal Action endCb;
        }
        public enum FxType
        {
            AnimatorTrigger,
        }
        private bool m_IsFollow = false;
        private Vector3 m_Follow;
        private Transform m_FollowTrans;
        private Vector3 m_Offset;
        private float m_DisposeTimout = -1;
        private Animator m_Animator;
        private Queue<FxTask> m_Tasks = new Queue<FxTask>();
        public Animator GetAnimator()
        {

            if(m_Animator == null)
            {
                m_Animator = RootTransform.GetComponentInChildren<Animator>();
            }
            return m_Animator;
        }
        public void PlayAnimTrigger(string triggerName,float endTimeout, Action endCb,bool isEndDispose=true)
        {
            var task = new FxTask()
            {
                endCb = endCb,
                isEndDispose = isEndDispose,
                endTimeout = endTimeout,
                triggerName = triggerName,
                fxType = FxType.AnimatorTrigger,
                argName = triggerName
            };
            if (!IsModelLoaded())
            {
                m_Tasks.Enqueue(task);
                return;
            }
            else
            {
                HandleAnimTrigger(task);
            }
        }
        private void EndPlay()
        {
            m_IsFollow = false;
            GameApp.Ins.LoopManager.RemoveTimeout(EndPlay);
            GameSceneUnitClientHandler.Ins.GameSceneUnitPool.PutSceneUnit(this.Id,this);
        }
        public override void OnGetFromPool()
        {
            base.OnGetFromPool();
        }
        public override void OnPutToPool()
        {
            base.OnPutToPool();
            m_IsFollow = false;
        }

        protected override void OnSceneUnitGUI(object data)
        {
            base.OnSceneUnitGUI(data);
            UpdatePos();
            HandleFx();

        }
        private void HandleFx()
        {
            while (m_Tasks.Count>0)
            {
                var task = m_Tasks.Dequeue();
                HandleFx(task);
            }
        }
        private void HandleFx(FxTask task)
        {
            if(task.fxType == FxType.AnimatorTrigger)
            {
                HandleAnimTrigger(task);
            }
        }
        private void HandleAnimTrigger(FxTask task)
        {
            Animator animator = GetAnimator();
            if (animator != null)
            {
                animator.SetTrigger(task.triggerName);
            }
            Action cb = null;
            cb = () =>
            {
                GameApp.Ins.LoopManager.RemoveTimeout(cb);
                if (task.isEndDispose)
                {
                    GameSceneUnitClientHandler.Ins.GameSceneUnitPool.PutSceneUnit(this.Id, this);
                }
                task.endCb?.Invoke();
            };
            if (task.endTimeout > 0) {
                GameApp.Ins.LoopManager.AddTimeout(cb, task.endTimeout);
            }else{
                cb();
            }
        }
        public void BindFollow(Transform followTrans, Vector3 offset = default)
        {
            m_IsFollow = true;
            m_FollowTrans = followTrans;
            m_Follow = followTrans.position;
            m_Offset = offset;
            if (UnitModelGo != null)
            {
                UpdatePos();
            }

        }
        public void BindFollow(Vector3 follow, Vector3 offset = default)
        {
            m_IsFollow = true;
            m_FollowTrans = null;
            m_Follow = follow;
            m_Offset = offset;
            if (UnitModelGo != null)
            {
                UpdatePos();
            }
        }
        private void UpdatePos()
        {
            if (!m_IsFollow) return;
            SetPosition(CalculatePos());

        }
        private Vector2 CalculatePos()
        {
            if (m_FollowTrans != null)
            {
                m_Follow = m_FollowTrans.position;
            }  
            return m_Follow + m_Offset;
        }
    }
}
