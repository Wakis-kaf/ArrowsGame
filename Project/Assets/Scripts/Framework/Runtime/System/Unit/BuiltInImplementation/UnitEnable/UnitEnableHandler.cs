using Framework.Runtime.UnitSystem.Base;
using Framework.Utils;

namespace Framework.Runtime.UnitSystem.BIInterfaces
{
    public class UnitBehaviourEnableHandler : UnitBehaviourHandler<IUnitEnable>
    {
        /// <summary>
        /// Awake 优先级
        /// </summary>
        public override int Priority => 900;

        public override void OnAssignUnitEnable(IUnitEnable assignUnit)
        {
            base.OnAssignUnitEnable(assignUnit);
            FunctionUtility.SafeCall(assignUnit.OnUnitEnable);
            //assignUnit.OnUnitEnable();
        }
    }
}