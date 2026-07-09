
using System;

namespace Framework.Runtime.MSceneUnit.BSM
{
    public class BSTreeNode
    {
        public bool awakeEnter = true;
        public bool awakePlay = true;
        public string stateName;
        private Type m_StateType = typeof(BehaviourState);

        public virtual Type stateType
        {
            get { return m_StateType; }
            set { m_StateType = value; }
        }
    }
}