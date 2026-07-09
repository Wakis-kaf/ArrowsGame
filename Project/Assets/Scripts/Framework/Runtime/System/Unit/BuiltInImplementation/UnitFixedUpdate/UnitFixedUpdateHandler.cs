using Framework.Runtime.LogSystem;
using Framework.Runtime.UnitSystem.Base;

using System;
using System.Collections.Generic;

namespace Framework.Runtime.UnitSystem.BIInterfaces
{
    public class UnitBehaviourFixedUpdateHandler : UnitBehaviourHandler<IUnitFixedUpdate>
    {
        private List<IUnitFixedUpdate> mFixedUpdates = new List<IUnitFixedUpdate>(10000);
        public override int Priority => 600;

        public override void OnAssignUnitDeRegister(IUnitFixedUpdate unit)
        {
            base.OnAssignUnitDeRegister(unit);
            mFixedUpdates.Remove(unit);
        }

        public override void OnAssignUnitRegister(IUnitFixedUpdate unit)
        {
            base.OnAssignUnitRegister(unit);
            mFixedUpdates.Add(unit);
        }

        public override void OnUnitModuleFixedUpdate()
        {
            base.OnUnitModuleFixedUpdate();
            int count = mFixedUpdates.Count;
            for (int i = 0; i < count; i++)
            {
                IUnitFixedUpdate ctr = mFixedUpdates[i];
                IBehaviourUnit behaviourUnit = ctr.OwnerBehaviourUnit;
                if (ReferenceEquals(behaviourUnit, null) || !behaviourUnit.IsUnitEnable) continue;
                try
                {
                    ctr.OnUnitFixedUpdate();
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
            mFixedUpdates.Clear();
            mFixedUpdates = null;
        }
    }
}