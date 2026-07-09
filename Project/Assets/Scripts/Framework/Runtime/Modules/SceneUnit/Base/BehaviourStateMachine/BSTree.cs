using System;

namespace Framework.Runtime.MSceneUnit.BSM
{
    public class BSTree : BSTreeNode
    {
        public string startStateName;

        public BSTreeNode[] subTreeNodes = Array.Empty<BSTreeNode>();
        private Type m_StateType = typeof(BehaviourStateMachine);

        public override Type stateType
        {
            get { return m_StateType; }
            set { m_StateType = value; }
        }
    }
}