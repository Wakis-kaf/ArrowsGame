using Framework.Runtime.UnitSystem.Base;
using Framework.Utils;

namespace Framework.Runtime.UnitSystem.BIInterfaces
{
    public class UnitBehaviourDisableHandler : UnitBehaviourHandler<IUnitDisable>
    {
        public override int Priority => 400;

        public override void OnAssignUnitDisable(IUnitDisable assignUnit)
        {
            //assignUnit.OnUnitDisable();
            FunctionUtility.SafeCall(assignUnit.OnUnitDisable);
        }
    }
}