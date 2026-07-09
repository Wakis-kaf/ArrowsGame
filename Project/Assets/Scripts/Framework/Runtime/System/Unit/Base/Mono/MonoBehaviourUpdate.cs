using Framework.Runtime.UnitSystem.BIInterfaces;

namespace Framework.Runtime.UnitSystem.MonoBase
{
    public class MonoBehaviourUpdate : MonoBehaviourUnit, IUnitUpdate
    {
        public virtual string ControllerName
        {
            get => UnitName;
        }

        public virtual void OnUnitUpdate()
        {
        }
    }
}