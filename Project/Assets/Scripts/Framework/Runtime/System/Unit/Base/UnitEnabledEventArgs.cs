using Framework.Runtime.SystemEvent;

namespace Framework.Runtime.UnitSystem.Base
{
    public class UnitEnabledEventArgs : FrameworkEventArgs
    {
        public bool enable = false;

        public UnitEnabledEventArgs(IBehaviourUnit behaviourUnit)
        {
            this.behaviourUnit = behaviourUnit;
        }

        public IBehaviourUnit behaviourUnit { get; private set; }
    }
}