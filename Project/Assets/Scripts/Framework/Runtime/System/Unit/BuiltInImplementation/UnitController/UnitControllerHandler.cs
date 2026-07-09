using Framework.Runtime.LogSystem;
using Framework.Runtime.UnitSystem.Base;
using Framework.Utils;
using System;
using System.Collections.Generic;

namespace Framework.Runtime.UnitSystem.BIInterfaces
{
    public class UnitBehaviourControllerHandler : UnitBehaviourHandler<IUnitUpdate>
    {
        private List<IUnitUpdate> m_Controllers = new List<IUnitUpdate>(10000);
        public override int Priority => 700;

        public override void OnAssignUnitDeRegister(IUnitUpdate unit)
        {
            base.OnAssignUnitDeRegister(unit);
            m_Controllers.Remove(unit);
        }

        public override void OnAssignUnitRegister(IUnitUpdate unit)
        {
            base.OnAssignUnitRegister(unit);
            m_Controllers.Add(unit);
        }

        public override void OnUnitModuleUpdate()
        {
            base.OnUnitModuleUpdate();
            int count = m_Controllers.Count;
            for (int i = 0; i < count; i++)
            {
                if (i >= m_Controllers.Count) continue;
                if (!m_Controllers[i].OwnerBehaviourUnit.IsUnitEnable) continue;
                FunctionUtility.SafeCall(m_Controllers[i].OnUnitUpdate);
            }
        }

        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            m_Controllers.Clear();
            m_Controllers = null;
        }
    }
}