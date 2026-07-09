using Framework.Runtime.UnitSystem.Base;

namespace Framework.Runtime.Module.Core
{
    public abstract class ModuleUnit : BehaviourUnit
    {
        public ModuleUnit()
        {
            OnInit();
        }

        public string ControllerName { get; }

        public void DoConstruct()
        {
            OnModuleConstructed();
        }

        public virtual void OnAppPopupUpdate(GameAppMessage appMessage)
        {
        }

        public virtual void OnAppUpdate(GameAppMessage appMessage)
        {
        }

        protected virtual void OnInit()
        {
        }

        protected virtual void OnModuleConstructed()
        {
        }
    }
}