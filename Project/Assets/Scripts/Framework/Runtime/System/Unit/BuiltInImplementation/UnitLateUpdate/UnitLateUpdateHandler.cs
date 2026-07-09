using Framework.Runtime.LogSystem;
using Framework.Runtime.UnitSystem.Base;

using System;
using System.Collections.Generic;

namespace Framework.Runtime.UnitSystem.BIInterfaces
{
    public class UnitLateUpdateHandler : UnitBehaviourHandler<IUnitLateUpdate>
    {
        private List<IUnitLateUpdate> mLateUpdates = new List<IUnitLateUpdate>(10000);
        public override int Priority => 500;

        public override void OnAssignUnitDeRegister(IUnitLateUpdate unit)
        {
            base.OnAssignUnitDeRegister(unit);
            mLateUpdates.Remove(unit);
        }

        public override void OnAssignUnitRegister(IUnitLateUpdate unit)
        {
            base.OnAssignUnitRegister(unit);
            mLateUpdates.Add(unit);
        }

        public override void OnUnitModuleLateUpdate()
        {
            base.OnUnitModuleLateUpdate();
            int count = mLateUpdates.Count;
            for (int i = 0; i < count; i++)
            {
                IUnitLateUpdate ctr = mLateUpdates[i];
                IBehaviourUnit behaviourUnit = ctr.OwnerBehaviourUnit;
                if (ReferenceEquals(behaviourUnit, null) || !behaviourUnit.IsUnitEnable) continue;
                try
                {
                    ctr.OnUnitLateUpdate();
                }
                catch (Exception e)
                {
                    Log.Fatal(e);
                }
            }
        }

        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            mLateUpdates.Clear();
            mLateUpdates = null;
        }
    }
}