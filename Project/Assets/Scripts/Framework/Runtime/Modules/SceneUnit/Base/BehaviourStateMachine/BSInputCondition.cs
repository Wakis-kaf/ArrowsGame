
using System;

namespace Framework.Runtime.MSceneUnit.BSM
{
    public class BSInputCondition
    {
        public Func<BehaviourState, bool> canEnterCondition;
        public Func<BehaviourState, bool> canExitCondition;
        public Func<BehaviourState, bool> canPauseCondition;
        public Func<BehaviourState, bool> canPlayCondition;
        public Func<BehaviourState, bool> canUpdateCondition;
        public string targetState;
    }
}