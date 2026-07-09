using Framework.Runtime.UnitSystem.Base;
using Framework.Utils;
using System.Collections.Generic;

namespace Framework.Runtime.UnitSystem.BIInterfaces
{
    public class UnitBehaviourStartHandler : UnitBehaviourHandler<IUnitStart>
    {
        private List<IUnitStart> m_StartWaitingUnits = new List<IUnitStart>(1000);
        public override int Priority => 800;

        public override void OnAssignUnitDeRegister(IUnitStart assignUnit)
        {
            base.OnAssignUnitDeRegister(assignUnit);
            m_StartWaitingUnits.Remove(assignUnit);
        }

        public override void OnAssignUnitRegister(IUnitStart assignUnit)
        {
            base.OnAssignUnitRegister(assignUnit);
            // 加入等待队列
            m_StartWaitingUnits.Add(assignUnit);
        }

        public override void OnUnitModuleUpdate()
        {
            base.OnUnitModuleUpdate();
            int count = m_StartWaitingUnits.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                var item = m_StartWaitingUnits[i];
                if (item.OwnerBehaviourUnit.IsUnitEnable)
                {
                    // 移除队列
                    m_StartWaitingUnits[i] = m_StartWaitingUnits[count - 1];
                    m_StartWaitingUnits.RemoveAt(count - 1);
                    count -= 1;
                    FunctionUtility.SafeCall(item.OnUnitStart);
                    //item.OnUnitStart();
                }
            }
        }

        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            m_StartWaitingUnits.Clear();
            m_StartWaitingUnits = null;
        }
    }
}