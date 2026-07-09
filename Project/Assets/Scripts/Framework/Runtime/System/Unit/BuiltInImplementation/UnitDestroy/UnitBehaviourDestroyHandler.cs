using Framework.Runtime.UnitSystem.Base;
using Framework.Utils;

namespace Framework.Runtime.UnitSystem.BIInterfaces
{
    public class UnitBehaviourDestroyHandler : UnitBehaviourHandler<IUnitDestroy>
    {
        public override int Priority => 300;

        public override void OnAssignUnitDeRegister(IUnitDestroy assignUnit)
        {
            base.OnAssignUnitDeRegister(assignUnit);
            FunctionUtility.SafeCall(assignUnit.OnUnitDestroy);
            //assignUnit.OnUnitDestroy();
        }
    }
}