using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Runtime.MSceneUnit.BSM
{
    public class BehaviourStateMachine : BehaviourState
    {
        private List<BehaviourState> m_ChildBSList;
        private BehaviourState m_CurrentState;

        public BehaviourStateMachine()
        {
            m_ChildBSList = new List<BehaviourState>();
        }

        public T AddState<T>() where T : BehaviourState
        {
            return AddState<T>(Activator.CreateInstance<T>());
        }

        public T AddState<T>(T state) where T : BehaviourState
        {
            if (m_ChildBSList.Contains(state)) return state;
            m_ChildBSList.Add(state);
            state.SetOwnerMachine(this);
            return state;
        }

        public T FindState<T>(string stateName = "") where T : BehaviourState
        {
            for (int i = 0; i < m_ChildBSList.Count; i++)
            {
                var state = m_ChildBSList[i];
                if (state.Type == typeof(T) && state.StateName == stateName) return state as T;
            }

            return null;
        }

        public override void InputBind(Func<string, BSInputCondition> conditionGetter)
        {
            base.InputBind(conditionGetter);
            for (int i = 0; i < m_ChildBSList.Count; i++)
            {
                var state = m_ChildBSList[i];
                state.InputBind(conditionGetter);
            }
        }

        public override void OnUpdating()
        {
            base.OnUpdating();
            for (int i = 0; i < m_ChildBSList.Count; i++)
            {
                var state = m_ChildBSList[i];
                state.Updating();
            }
        }

        public override void RegisterStateTree(BSTreeNode bsTreeNode)
        {
            if (bsTreeNode is BSTree bsTree)
            {
                InitStateMachine(bsTree);
            }
            else
            {
                base.RegisterStateTree(bsTreeNode);
            }
        }

        public void SwitchState<T>(string stateName = "") where T : BehaviourState
        {
            T state = FindState<T>(stateName);
            if (state == null)
            {
                Debug.LogError($"Switch State error ! not found state typeof {typeof(T)}");
                return;
            }

            SwitchPreprocess(state);
            SwitchProcess(state);
        }

        private void GenerateTreeDeep(BehaviourStateMachine parent, BSTree tree)
        {
            //machine.LuaStateType = tree.luaStateType;
            BehaviourState state = null;
            for (int i = 0; i < tree.subTreeNodes.Length; i++)
            {
                var subTree = tree.subTreeNodes[i];
                state = Activator.CreateInstance(subTree.stateType) as BehaviourState;
                parent.AddState(state);
                state.RootBehaviourState = parent.RootBehaviourState;
                state.RegisterStateTree(subTree);
            }
        }

        private void InitStateMachine(BSTree bsTree)
        {
            InitState(bsTree);
            GenerateTreeDeep(this, bsTree);
        }

        private bool SwitchPreprocess(BehaviourState targetState)
        {
            if (m_CurrentState == null) return true;
            if (m_CurrentState.CanExit() && targetState.CanEnter())
            {
                return true;
            }

            return false;
        }

        private void SwitchProcess(BehaviourState targetState)
        {
            m_CurrentState?.Exit();
            m_CurrentState = targetState;
            m_CurrentState?.Enter();
        }
    }
}