using Framework.Runtime.Base;

namespace Framework.Runtime.UnitSystem.Base
{
    public interface IUnitBehaviourHandler : IUnitObject
    {
        public int Priority { get; }

        public void OnUnitHandleEventTrigger(IBehaviourUnit behaviourUnit, UnitHandleType handleType);

        public void OnUnitModuleFixedUpdate();

        public void OnUnitModuleLateUpdate();

        public void OnUnitModuleQuit();

        public void OnUnitModuleUpdate();
    }
}