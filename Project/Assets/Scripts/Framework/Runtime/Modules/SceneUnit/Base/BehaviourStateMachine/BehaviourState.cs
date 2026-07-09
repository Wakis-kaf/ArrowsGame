using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Runtime.MSceneUnit.BSM
{
    public class BehaviourState
    {
        private Func<BehaviourState, bool> m_CanEnterCond;
        private Func<BehaviourState, bool> m_CanExitCond;
        private Func<BehaviourState, bool> m_CanPauseCond;
        private Func<BehaviourState, bool> m_CanPlayCond;

        private Func<BehaviourState, bool> m_CanUpdateCond;
        private bool m_IsPlaying;
        private bool m_IsStaying;

        private BehaviourStateMachine m_OwnerMachine;
        private SceneUnit m_OwnerSceneUnit;
        private BehaviourState m_RootBehaviourState;
        private string m_StateName;
        private Type m_Type;

        public bool IsPlaying
        {
            get => m_IsPlaying;
        }

        public bool IsStaying
        {
            get => m_IsStaying;
        }

        public BehaviourStateMachine OwnerMachine
        {
            get => m_OwnerMachine;
        }

        public SceneUnit OwnerSceneUnit
        {
            get { return m_OwnerSceneUnit; }
            set { m_OwnerSceneUnit = value; }
        }

        public BehaviourState RootBehaviourState
        {
            get
            {
                if (m_RootBehaviourState == null)
                    m_RootBehaviourState = this;
                return m_RootBehaviourState;
            }
            set { m_RootBehaviourState = value; }
        }

        public string StateName
        {
            get { return m_StateName; }
            set { m_StateName = value; }
        }

        public Type Type
        {
            get
            {
                if (m_Type == null)
                    m_Type = GetType();
                return m_Type;
            }
        }

        public static void LogError(object msg)
        {
            Debug.LogError(msg);
        }

        public void BindCanEnterCond(Func<BehaviourState, bool> input)
        {
            m_CanEnterCond = input;
        }

        public void BindCanExitCond(Func<BehaviourState, bool> input)
        {
            m_CanExitCond = input;
        }

        public void BindCanPauseCond(Func<BehaviourState, bool> input)
        {
            m_CanPauseCond = input;
        }

        public void BindCanPlayCond(Func<BehaviourState, bool> input)
        {
            m_CanPlayCond = input;
        }

        public void BindCanUpdatingCond(Func<BehaviourState, bool> input)
        {
            m_CanUpdateCond = input;
        }

        public virtual void BindInputTree(BSInputMap inputMap)
        {
            Dictionary<string, BSInputCondition> condDict = new Dictionary<string, BSInputCondition>();
            for (int i = 0; i < inputMap.inputConditions.Length; i++)
            {
                var condition = inputMap.inputConditions[i];
                condDict.Add(condition.targetState, condition);
            }

            Func<string, BSInputCondition> func = (name) =>
            {
                if (string.IsNullOrEmpty(name)) return null;
                condDict.TryGetValue(name, out var cnd);
                return cnd;
            };
            InputBind(func);
        }

        public virtual bool CanEnter()
        {
            return !IsStaying;
        }

        public virtual bool CanExit()
        {
            return IsStaying;
        }

        public virtual bool CanPause()
        {
            return IsPlaying;
        }

        public virtual bool CanPlay()
        {
            return !IsPlaying;
        }

        public virtual bool CanUpdating()
        {
            return IsStaying && IsPlaying &&
                   (m_CanUpdateCond == null || m_CanUpdateCond.Invoke(this));
        }

        public void Enter()
        {
            if (m_IsStaying) return;
            m_IsStaying = true;
            OnEnter();
        }

        public void Exit()
        {
            if (!m_IsStaying) return;
            m_IsStaying = false;
            OnExit();
        }

        public virtual void InputBind(Func<string, BSInputCondition> conditionGetter)
        {
            var condition = conditionGetter.Invoke(StateName);
            if (condition?.canEnterCondition != null)
                BindCanEnterCond(condition.canEnterCondition);
            if (condition?.canExitCondition != null)
                BindCanExitCond(condition.canExitCondition);
            if (condition?.canPlayCondition != null)
                BindCanPlayCond(condition.canPlayCondition);
            if (condition?.canPauseCondition != null)
                BindCanPauseCond(condition.canPauseCondition);
            if (condition?.canUpdateCondition != null)
                BindCanUpdatingCond(condition.canUpdateCondition);
        }

        public virtual void OnEnter()
        {
        }

        public virtual void OnExit()
        {
        }

        public virtual void OnPause()
        {
        }

        public virtual void OnPlay()
        {
        }

        public virtual void OnUpdating()
        {
        }

        public void Pause()
        {
            if (!m_IsPlaying) return;
            m_IsPlaying = false;
            OnPause();
        }

        public void Play()
        {
            if (m_IsPlaying) return;
            m_IsPlaying = true;
            OnPlay();
        }

        public virtual void RegisterStateTree(BSTreeNode bsTreeNode)
        {
            InitState(bsTreeNode);
        }

        public void SetOwnerMachine(BehaviourStateMachine ownerMachine)
        {
            m_OwnerMachine = ownerMachine;
        }

        public void Updating()
        {
            CheckInput();
            if (CanUpdating())
            {
                OnUpdating();
            }
        }

        protected BehaviourState InitState(BSTreeNode bsTreeNode)
        {
            if (bsTreeNode.awakeEnter)
                Enter();
            if (bsTreeNode.awakePlay)
                Play();
            StateName = string.IsNullOrEmpty(bsTreeNode.stateName) ? StateName : bsTreeNode.stateName;
            return this;
        }

        private void CheckInput()
        {
            if (CanEnter() && m_CanEnterCond != null && m_CanEnterCond.Invoke(this))
            {
                Enter();
            }

            if (CanPlay() && m_CanPlayCond != null && m_CanPlayCond.Invoke(this))
            {
                Play();
            }

            if (CanPause() && m_CanPauseCond != null && m_CanPauseCond.Invoke(this))
            {
                Pause();
            }

            if (CanExit() && m_CanExitCond != null && m_CanExitCond.Invoke(this))
            {
                Exit();
            }
        }
    }
}